using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Hardening : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "vehicle_type",
                table: "vehicles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "car");

            migrationBuilder.AlterColumn<string>(
                name: "category",
                table: "reports",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "inaccurate");

            migrationBuilder.AlterColumn<string>(
                name: "vehicle_type",
                table: "parking_spaces",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "car");

            migrationBuilder.AlterColumn<string>(
                name: "parking_type",
                table: "parking_locations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "private_space");

            migrationBuilder.AlterColumn<string>(
                name: "entry_type",
                table: "ledger_entries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "charge");

            migrationBuilder.AlterColumn<string>(
                name: "vehicle_type",
                table: "bookings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "car");

            migrationBuilder.AlterColumn<string>(
                name: "to_status",
                table: "booking_status_history",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "pending");

            migrationBuilder.AlterColumn<string>(
                name: "kind",
                table: "availability_overrides",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "closed");

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION public.extend_booking(p_booking_id uuid, p_ends_at timestamptz) RETURNS public.bookings
                LANGUAGE plpgsql SET search_path = '' AS $$
                DECLARE
                    v_location_id uuid;
                    v_booking public.bookings;
                    v_quote record;
                    v_commission numeric(10, 2);
                BEGIN
                    SELECT location_id INTO v_location_id FROM public.bookings WHERE id = p_booking_id;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'booking_not_found';
                    END IF;

                    PERFORM 1 FROM public.parking_locations WHERE id = v_location_id FOR NO KEY UPDATE;
                    PERFORM public.expire_pending_bookings(v_location_id);

                    SELECT * INTO v_booking FROM public.bookings WHERE id = p_booking_id FOR UPDATE;

                    IF v_booking.status NOT IN ('confirmed', 'active') THEN
                        RAISE EXCEPTION 'booking_not_extendable';
                    END IF;

                    IF p_ends_at <= v_booking.ends_at THEN
                        RAISE EXCEPTION 'booking_invalid_period';
                    END IF;

                    IF NOT public.location_is_open(v_booking.location_id, v_booking.starts_at, p_ends_at) THEN
                        RAISE EXCEPTION 'booking_outside_opening_hours';
                    END IF;

                    IF v_booking.space_id IS NOT NULL THEN
                        IF NOT public.space_is_free(v_booking.space_id, v_booking.ends_at, p_ends_at, v_booking.id) THEN
                            RAISE EXCEPTION 'booking_space_unavailable';
                        END IF;
                    ELSIF public.location_free_capacity(v_booking.location_id, v_booking.ends_at, p_ends_at, v_booking.id) < 1 THEN
                        RAISE EXCEPTION 'booking_no_capacity';
                    END IF;

                    SELECT * INTO v_quote FROM public.quote_booking(v_booking.location_id, v_booking.space_id, v_booking.starts_at, p_ends_at);
                    v_commission := round(v_quote.total_amount * v_booking.commission_rate, 2);

                    UPDATE public.bookings
                    SET ends_at = p_ends_at,
                        total_amount = v_quote.total_amount,
                        commission_amount = v_commission,
                        provider_amount = v_quote.total_amount - v_commission,
                        pricing_snapshot = pricing_snapshot || jsonb_build_object('minutes', v_quote.minutes, 'extended_at', now())
                    WHERE id = p_booking_id
                    RETURNING * INTO v_booking;

                    RETURN v_booking;
                END $$;

                CREATE OR REPLACE FUNCTION public.audit_row_change() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                DECLARE
                    v_new jsonb := CASE WHEN TG_OP <> 'DELETE' THEN to_jsonb(NEW) - 'ticket_token' END;
                    v_old jsonb := CASE WHEN TG_OP <> 'INSERT' THEN to_jsonb(OLD) - 'ticket_token' END;
                    v_changes jsonb;
                BEGIN
                    IF TG_OP = 'UPDATE' THEN
                        SELECT jsonb_object_agg(n.key, jsonb_build_object('from', o.value, 'to', n.value))
                        INTO v_changes
                        FROM jsonb_each(v_new) n
                        JOIN jsonb_each(v_old) o USING (key)
                        WHERE n.value IS DISTINCT FROM o.value AND n.key <> 'updated_at';

                        IF v_changes IS NULL THEN
                            RETURN NULL;
                        END IF;
                    END IF;

                    INSERT INTO public.audit_logs (actor_id, action, entity_type, entity_id, metadata)
                    VALUES (
                        public.current_actor_id(),
                        lower(TG_OP),
                        TG_TABLE_NAME,
                        (coalesce(v_new, v_old) ->> 'id')::uuid,
                        coalesce(v_changes, v_new, v_old));

                    RETURN NULL;
                END $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION public.extend_booking(p_booking_id uuid, p_ends_at timestamptz) RETURNS public.bookings
                LANGUAGE plpgsql SET search_path = '' AS $$
                DECLARE
                    v_booking public.bookings;
                    v_quote record;
                    v_commission numeric(10, 2);
                BEGIN
                    PERFORM 1
                    FROM public.parking_locations l
                    JOIN public.bookings b ON b.location_id = l.id
                    WHERE b.id = p_booking_id
                    FOR NO KEY UPDATE OF l;

                    SELECT * INTO v_booking FROM public.bookings WHERE id = p_booking_id FOR UPDATE;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'booking_not_found';
                    END IF;

                    IF v_booking.status NOT IN ('confirmed', 'active') THEN
                        RAISE EXCEPTION 'booking_not_extendable';
                    END IF;

                    IF p_ends_at <= v_booking.ends_at THEN
                        RAISE EXCEPTION 'booking_invalid_period';
                    END IF;

                    IF NOT public.location_is_open(v_booking.location_id, v_booking.starts_at, p_ends_at) THEN
                        RAISE EXCEPTION 'booking_outside_opening_hours';
                    END IF;

                    IF v_booking.space_id IS NOT NULL THEN
                        IF NOT public.space_is_free(v_booking.space_id, v_booking.ends_at, p_ends_at, v_booking.id) THEN
                            RAISE EXCEPTION 'booking_space_unavailable';
                        END IF;
                    ELSIF public.location_free_capacity(v_booking.location_id, v_booking.ends_at, p_ends_at, v_booking.id) < 1 THEN
                        RAISE EXCEPTION 'booking_no_capacity';
                    END IF;

                    SELECT * INTO v_quote FROM public.quote_booking(v_booking.location_id, v_booking.space_id, v_booking.starts_at, p_ends_at);
                    v_commission := round(v_quote.total_amount * v_booking.commission_rate, 2);

                    UPDATE public.bookings
                    SET ends_at = p_ends_at,
                        total_amount = v_quote.total_amount,
                        commission_amount = v_commission,
                        provider_amount = v_quote.total_amount - v_commission,
                        pricing_snapshot = pricing_snapshot || jsonb_build_object('minutes', v_quote.minutes, 'extended_at', now())
                    WHERE id = p_booking_id
                    RETURNING * INTO v_booking;

                    RETURN v_booking;
                END $$;

                CREATE OR REPLACE FUNCTION public.audit_row_change() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                DECLARE
                    v_new jsonb := CASE WHEN TG_OP <> 'DELETE' THEN to_jsonb(NEW) END;
                    v_old jsonb := CASE WHEN TG_OP <> 'INSERT' THEN to_jsonb(OLD) END;
                    v_changes jsonb;
                BEGIN
                    IF TG_OP = 'UPDATE' THEN
                        SELECT jsonb_object_agg(n.key, jsonb_build_object('from', o.value, 'to', n.value))
                        INTO v_changes
                        FROM jsonb_each(v_new) n
                        JOIN jsonb_each(v_old) o USING (key)
                        WHERE n.value IS DISTINCT FROM o.value AND n.key <> 'updated_at';

                        IF v_changes IS NULL THEN
                            RETURN NULL;
                        END IF;
                    END IF;

                    INSERT INTO public.audit_logs (actor_id, action, entity_type, entity_id, metadata)
                    VALUES (
                        public.current_actor_id(),
                        lower(TG_OP),
                        TG_TABLE_NAME,
                        (coalesce(v_new, v_old) ->> 'id')::uuid,
                        coalesce(v_changes, v_new, v_old));

                    RETURN NULL;
                END $$;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "vehicle_type",
                table: "vehicles",
                type: "text",
                nullable: false,
                defaultValue: "car",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "category",
                table: "reports",
                type: "text",
                nullable: false,
                defaultValue: "inaccurate",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "vehicle_type",
                table: "parking_spaces",
                type: "text",
                nullable: false,
                defaultValue: "car",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "parking_type",
                table: "parking_locations",
                type: "text",
                nullable: false,
                defaultValue: "private_space",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "entry_type",
                table: "ledger_entries",
                type: "text",
                nullable: false,
                defaultValue: "charge",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "vehicle_type",
                table: "bookings",
                type: "text",
                nullable: false,
                defaultValue: "car",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "to_status",
                table: "booking_status_history",
                type: "text",
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "kind",
                table: "availability_overrides",
                type: "text",
                nullable: false,
                defaultValue: "closed",
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
