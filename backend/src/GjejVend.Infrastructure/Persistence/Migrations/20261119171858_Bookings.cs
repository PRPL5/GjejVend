using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Bookings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    reference = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'GV' || upper(substr(md5(gen_random_uuid()::text), 1, 8))"),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    space_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vehicle_plate = table.Column<string>(type: "text", nullable: false),
                    vehicle_type = table.Column<string>(type: "text", nullable: false, defaultValue: "car"),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "EUR"),
                    commission_rate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    commission_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    provider_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    pricing_snapshot = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    ticket_token = table.Column<string>(type: "text", nullable: false, defaultValueSql: "replace(gen_random_uuid()::text, '-', '')"),
                    checked_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    checked_out_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_by = table.Column<Guid>(type: "uuid", nullable: true),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.CheckConstraint("ck_bookings_amounts", "total_amount >= 0 AND commission_amount >= 0 AND provider_amount >= 0 AND commission_amount + provider_amount = total_amount");
                    table.CheckConstraint("ck_bookings_commission_rate", "commission_rate BETWEEN 0 AND 1");
                    table.CheckConstraint("ck_bookings_pending_expiry", "status <> 'pending' OR expires_at IS NOT NULL");
                    table.CheckConstraint("ck_bookings_period", "ends_at > starts_at");
                    table.CheckConstraint("ck_bookings_status", "status IN ('pending', 'confirmed', 'active', 'completed', 'cancelled', 'expired', 'no_show')");
                    table.CheckConstraint("ck_bookings_vehicle_type", "vehicle_type IN ('car', 'motorcycle', 'van', 'suv')");
                    table.ForeignKey(
                        name: "fk_bookings_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_parking_spaces_space_id",
                        column: x => x.space_id,
                        principalTable: "parking_spaces",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_profiles_cancelled_by",
                        column: x => x.cancelled_by,
                        principalTable: "profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "booking_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "text", nullable: true),
                    to_status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_status_history", x => x.id);
                    table.CheckConstraint("ck_booking_status_history_from_status", "from_status IN ('pending', 'confirmed', 'active', 'completed', 'cancelled', 'expired', 'no_show')");
                    table.CheckConstraint("ck_booking_status_history_to_status", "to_status IN ('pending', 'confirmed', 'active', 'completed', 'cancelled', 'expired', 'no_show')");
                    table.ForeignKey(
                        name: "fk_booking_status_history_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_booking_status_history_profiles_changed_by",
                        column: x => x.changed_by,
                        principalTable: "profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_booking_status_history_booking_id_changed_at",
                table: "booking_status_history",
                columns: new[] { "booking_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_booking_status_history_changed_by",
                table: "booking_status_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_cancelled_by",
                table: "bookings",
                column: "cancelled_by");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_customer_id_starts_at",
                table: "bookings",
                columns: new[] { "customer_id", "starts_at" });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_expires_at",
                table: "bookings",
                column: "expires_at",
                filter: "status = 'pending'");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_location_id_starts_at",
                table: "bookings",
                columns: new[] { "location_id", "starts_at" });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_reference",
                table: "bookings",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bookings_space_id",
                table: "bookings",
                column: "space_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_ticket_token",
                table: "bookings",
                column: "ticket_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bookings_vehicle_id",
                table: "bookings",
                column: "vehicle_id");

            migrationBuilder.Sql("""
                ALTER TABLE public.bookings
                    ADD CONSTRAINT ex_bookings_space_overlap EXCLUDE USING gist (
                        space_id WITH =,
                        tstzrange(starts_at, ends_at, '[)') WITH &&)
                    WHERE (space_id IS NOT NULL AND status IN ('pending', 'confirmed', 'active'));

                CREATE INDEX ix_bookings_location_period ON public.bookings
                    USING gist (location_id, tstzrange(starts_at, ends_at, '[)'))
                    WHERE status IN ('pending', 'confirmed', 'active');

                CREATE FUNCTION public.guard_booking_status() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                BEGIN
                    IF NEW.status = OLD.status THEN
                        RETURN NEW;
                    END IF;

                    IF NOT (
                        (OLD.status = 'pending' AND NEW.status IN ('confirmed', 'cancelled', 'expired'))
                        OR (OLD.status = 'confirmed' AND NEW.status IN ('active', 'cancelled', 'no_show'))
                        OR (OLD.status = 'active' AND NEW.status = 'completed')
                    ) THEN
                        RAISE EXCEPTION 'booking_invalid_transition: % -> %', OLD.status, NEW.status USING ERRCODE = 'check_violation';
                    END IF;

                    CASE NEW.status
                        WHEN 'cancelled' THEN
                            NEW.cancelled_at := coalesce(NEW.cancelled_at, now());
                            NEW.cancelled_by := coalesce(NEW.cancelled_by, public.current_actor_id());
                        WHEN 'active' THEN
                            NEW.checked_in_at := coalesce(NEW.checked_in_at, now());
                        WHEN 'completed' THEN
                            NEW.checked_out_at := coalesce(NEW.checked_out_at, now());
                        ELSE
                            NULL;
                    END CASE;

                    RETURN NEW;
                END $$;

                CREATE TRIGGER guard_booking_status
                    BEFORE UPDATE OF status ON public.bookings
                    FOR EACH ROW EXECUTE FUNCTION public.guard_booking_status();

                CREATE FUNCTION public.record_booking_status() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                BEGIN
                    IF TG_OP = 'UPDATE' AND NEW.status = OLD.status THEN
                        RETURN NULL;
                    END IF;

                    INSERT INTO public.booking_status_history (booking_id, from_status, to_status, changed_by, reason)
                    VALUES (
                        NEW.id,
                        CASE WHEN TG_OP = 'UPDATE' THEN OLD.status END,
                        NEW.status,
                        public.current_actor_id(),
                        CASE WHEN NEW.status = 'cancelled' THEN NEW.cancellation_reason END);

                    RETURN NULL;
                END $$;

                CREATE TRIGGER record_booking_status
                    AFTER INSERT OR UPDATE OF status ON public.bookings
                    FOR EACH ROW EXECUTE FUNCTION public.record_booking_status();

                CREATE FUNCTION public.expire_pending_bookings(p_location_id uuid DEFAULT NULL) RETURNS integer
                LANGUAGE plpgsql SET search_path = '' AS $$
                DECLARE
                    v_count integer;
                BEGIN
                    UPDATE public.bookings
                    SET status = 'expired'
                    WHERE status = 'pending'
                      AND expires_at <= now()
                      AND (p_location_id IS NULL OR location_id = p_location_id);

                    GET DIAGNOSTICS v_count = ROW_COUNT;
                    RETURN v_count;
                END $$;

                CREATE FUNCTION public.location_is_open(p_location_id uuid, p_starts_at timestamptz, p_ends_at timestamptz) RETURNS boolean
                LANGUAGE sql STABLE SET search_path = '' AS $$
                    WITH location AS (
                        SELECT id, timezone FROM public.parking_locations WHERE id = p_location_id
                    ),
                    regular AS (
                        SELECT tstzrange(
                            (d.day::date + h.opens_at) AT TIME ZONE location.timezone,
                            (d.day::date + h.closes_at + CASE WHEN h.closes_at <= h.opens_at THEN interval '1 day' ELSE interval '0' END) AT TIME ZONE location.timezone,
                            '[)') AS period
                        FROM location
                        CROSS JOIN LATERAL generate_series(
                            ((p_starts_at AT TIME ZONE location.timezone)::date - 1)::timestamp,
                            ((p_ends_at AT TIME ZONE location.timezone)::date)::timestamp,
                            interval '1 day') AS d(day)
                        JOIN public.parking_operating_hours h ON h.location_id = location.id AND h.weekday = extract(isodow FROM d.day)
                    ),
                    overrides AS (
                        SELECT o.kind, tstzrange(o.starts_at, o.ends_at, '[)') AS period
                        FROM public.availability_overrides o
                        WHERE o.location_id = p_location_id
                          AND o.space_id IS NULL
                          AND o.kind IN ('extra_hours', 'closed')
                          AND tstzrange(o.starts_at, o.ends_at, '[)') && tstzrange(p_starts_at, p_ends_at, '[)')
                    )
                    SELECT (
                        coalesce((SELECT range_agg(period) FROM (SELECT period FROM regular UNION ALL SELECT period FROM overrides WHERE kind = 'extra_hours') AS open_periods), '{}'::tstzmultirange)
                        - coalesce((SELECT range_agg(period) FROM overrides WHERE kind = 'closed'), '{}'::tstzmultirange)
                    ) @> tstzrange(p_starts_at, p_ends_at, '[)')
                $$;

                CREATE FUNCTION public.space_is_free(p_space_id uuid, p_starts_at timestamptz, p_ends_at timestamptz, p_exclude_booking_id uuid DEFAULT NULL) RETURNS boolean
                LANGUAGE sql STABLE SET search_path = '' AS $$
                    SELECT NOT EXISTS (
                        SELECT 1 FROM public.bookings b
                        WHERE b.space_id = p_space_id
                          AND b.id IS DISTINCT FROM p_exclude_booking_id
                          AND (b.status IN ('confirmed', 'active') OR (b.status = 'pending' AND b.expires_at > now()))
                          AND tstzrange(b.starts_at, b.ends_at, '[)') && tstzrange(p_starts_at, p_ends_at, '[)'))
                    AND NOT EXISTS (
                        SELECT 1 FROM public.availability_overrides o
                        WHERE o.space_id = p_space_id
                          AND o.kind = 'closed'
                          AND tstzrange(o.starts_at, o.ends_at, '[)') && tstzrange(p_starts_at, p_ends_at, '[)'))
                $$;

                CREATE FUNCTION public.location_free_capacity(p_location_id uuid, p_starts_at timestamptz, p_ends_at timestamptz, p_exclude_booking_id uuid DEFAULT NULL) RETURNS integer
                LANGUAGE plpgsql STABLE SET search_path = '' AS $$
                DECLARE
                    v_location public.parking_locations;
                    v_period tstzrange := tstzrange(p_starts_at, p_ends_at, '[)');
                    v_capacity integer;
                    v_peak integer;
                BEGIN
                    SELECT * INTO v_location FROM public.parking_locations WHERE id = p_location_id;
                    IF NOT FOUND THEN
                        RETURN 0;
                    END IF;

                    IF v_location.inventory_mode = 'spaces' THEN
                        RETURN (
                            SELECT count(*)::integer
                            FROM public.parking_spaces s
                            WHERE s.location_id = p_location_id
                              AND s.status = 'active'
                              AND s.deleted_at IS NULL
                              AND public.space_is_free(s.id, p_starts_at, p_ends_at, p_exclude_booking_id));
                    END IF;

                    SELECT least(v_location.total_capacity, coalesce(min(o.capacity), v_location.total_capacity)) - v_location.offline_occupied
                    INTO v_capacity
                    FROM public.availability_overrides o
                    WHERE o.location_id = p_location_id
                      AND o.space_id IS NULL
                      AND o.kind = 'reduced_capacity'
                      AND tstzrange(o.starts_at, o.ends_at, '[)') && v_period;

                    WITH holding AS (
                        SELECT b.starts_at, tstzrange(b.starts_at, b.ends_at, '[)') AS period
                        FROM public.bookings b
                        WHERE b.location_id = p_location_id
                          AND b.id IS DISTINCT FROM p_exclude_booking_id
                          AND (b.status IN ('confirmed', 'active') OR (b.status = 'pending' AND b.expires_at > now()))
                          AND tstzrange(b.starts_at, b.ends_at, '[)') && v_period
                    )
                    SELECT coalesce(max((SELECT count(*) FROM holding h WHERE h.period @> points.at)), 0)::integer
                    INTO v_peak
                    FROM (SELECT p_starts_at AS at UNION SELECT starts_at FROM holding WHERE starts_at > p_starts_at) AS points;

                    RETURN greatest(v_capacity - v_peak, 0);
                END $$;

                CREATE FUNCTION public.quote_booking(p_location_id uuid, p_space_id uuid, p_starts_at timestamptz, p_ends_at timestamptz)
                RETURNS TABLE (rate_id uuid, currency char(3), minutes integer, total_amount numeric(10, 2))
                LANGUAGE plpgsql STABLE SET search_path = '' AS $$
                DECLARE
                    v_rate public.parking_rates;
                    v_minutes integer := ceil(extract(epoch FROM p_ends_at - p_starts_at) / 60);
                BEGIN
                    SELECT r.* INTO v_rate
                    FROM public.parking_rates r
                    WHERE r.location_id = p_location_id
                      AND (r.space_id = p_space_id OR r.space_id IS NULL)
                      AND tstzrange(r.valid_from, r.valid_to, '[)') @> p_starts_at
                    ORDER BY r.space_id NULLS LAST
                    LIMIT 1;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'booking_no_rate';
                    END IF;

                    IF v_minutes < v_rate.min_minutes OR v_minutes > coalesce(v_rate.max_minutes, v_minutes) THEN
                        RAISE EXCEPTION 'booking_duration_out_of_range';
                    END IF;

                    rate_id := v_rate.id;
                    currency := v_rate.currency;
                    minutes := v_minutes;
                    total_amount := CASE
                        WHEN v_rate.daily_rate IS NULL THEN ceil(v_minutes / 60.0) * v_rate.hourly_rate
                        ELSE (v_minutes / 1440) * v_rate.daily_rate + least(ceil((v_minutes % 1440) / 60.0) * v_rate.hourly_rate, v_rate.daily_rate)
                    END;
                    RETURN NEXT;
                END $$;

                CREATE FUNCTION public.create_booking(
                    p_customer_id uuid,
                    p_location_id uuid,
                    p_starts_at timestamptz,
                    p_ends_at timestamptz,
                    p_vehicle_plate text,
                    p_vehicle_type text,
                    p_space_id uuid DEFAULT NULL,
                    p_vehicle_id uuid DEFAULT NULL) RETURNS public.bookings
                LANGUAGE plpgsql SET search_path = '' AS $$
                DECLARE
                    v_location public.parking_locations;
                    v_quote record;
                    v_commission_rate numeric(5, 4);
                    v_ttl_minutes integer;
                    v_commission numeric(10, 2);
                    v_booking public.bookings;
                BEGIN
                    IF p_ends_at <= p_starts_at OR p_starts_at < now() - interval '5 minutes' THEN
                        RAISE EXCEPTION 'booking_invalid_period';
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM public.profiles WHERE id = p_customer_id AND status = 'active') THEN
                        RAISE EXCEPTION 'booking_customer_inactive';
                    END IF;

                    SELECT * INTO v_location
                    FROM public.parking_locations
                    WHERE id = p_location_id AND deleted_at IS NULL AND listing_status = 'published'
                    FOR NO KEY UPDATE;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'booking_location_unavailable';
                    END IF;

                    PERFORM public.expire_pending_bookings(p_location_id);

                    IF (v_location.inventory_mode = 'spaces') <> (p_space_id IS NOT NULL) THEN
                        RAISE EXCEPTION 'booking_space_mismatch';
                    END IF;

                    IF p_vehicle_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM public.vehicles WHERE id = p_vehicle_id AND user_id = p_customer_id) THEN
                        RAISE EXCEPTION 'booking_vehicle_not_owned';
                    END IF;

                    IF EXISTS (SELECT 1 FROM public.parking_location_vehicle_types WHERE location_id = p_location_id)
                       AND NOT EXISTS (SELECT 1 FROM public.parking_location_vehicle_types WHERE location_id = p_location_id AND vehicle_type = p_vehicle_type) THEN
                        RAISE EXCEPTION 'booking_vehicle_type_not_allowed';
                    END IF;

                    IF NOT public.location_is_open(p_location_id, p_starts_at, p_ends_at) THEN
                        RAISE EXCEPTION 'booking_outside_opening_hours';
                    END IF;

                    IF p_space_id IS NOT NULL THEN
                        IF NOT EXISTS (SELECT 1 FROM public.parking_spaces WHERE id = p_space_id AND location_id = p_location_id AND status = 'active' AND deleted_at IS NULL)
                           OR NOT public.space_is_free(p_space_id, p_starts_at, p_ends_at) THEN
                            RAISE EXCEPTION 'booking_space_unavailable';
                        END IF;
                    ELSIF public.location_free_capacity(p_location_id, p_starts_at, p_ends_at) < 1 THEN
                        RAISE EXCEPTION 'booking_no_capacity';
                    END IF;

                    SELECT * INTO v_quote FROM public.quote_booking(p_location_id, p_space_id, p_starts_at, p_ends_at);
                    SELECT (value #>> '{}')::numeric INTO v_commission_rate FROM public.platform_settings WHERE key = 'commission_rate';
                    SELECT (value #>> '{}')::integer INTO v_ttl_minutes FROM public.platform_settings WHERE key = 'pending_booking_ttl_minutes';
                    v_commission := round(v_quote.total_amount * v_commission_rate, 2);

                    INSERT INTO public.bookings (
                        customer_id, location_id, space_id, vehicle_id, vehicle_plate, vehicle_type, starts_at, ends_at,
                        status, expires_at, total_amount, currency, commission_rate, commission_amount, provider_amount, pricing_snapshot)
                    VALUES (
                        p_customer_id, p_location_id, p_space_id, p_vehicle_id, upper(p_vehicle_plate), p_vehicle_type, p_starts_at, p_ends_at,
                        'pending', now() + make_interval(mins => v_ttl_minutes), v_quote.total_amount, v_quote.currency, v_commission_rate,
                        v_commission, v_quote.total_amount - v_commission,
                        jsonb_build_object('rate_id', v_quote.rate_id, 'minutes', v_quote.minutes, 'commission_rate', v_commission_rate))
                    RETURNING * INTO v_booking;

                    RETURN v_booking;
                END $$;

                CREATE FUNCTION public.extend_booking(p_booking_id uuid, p_ends_at timestamptz) RETURNS public.bookings
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

                DO $$
                BEGIN
                    IF EXISTS (SELECT FROM pg_available_extensions WHERE name = 'pg_cron') THEN
                        CREATE EXTENSION IF NOT EXISTS pg_cron;
                        PERFORM cron.schedule('expire-pending-bookings', '* * * * *', 'SELECT public.expire_pending_bookings()');
                    END IF;
                END $$;
                """);

            migrationBuilder.AddUpdatedAtTriggers("bookings");
            migrationBuilder.RejectModifications("bookings", "DELETE");
            migrationBuilder.RejectModifications("booking_status_history", "UPDATE OR DELETE");
            migrationBuilder.ProtectTables("bookings", "booking_status_history");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF to_regnamespace('cron') IS NOT NULL THEN
                        PERFORM cron.unschedule(jobid) FROM cron.job WHERE jobname = 'expire-pending-bookings';
                    END IF;
                END $$;

                DROP FUNCTION IF EXISTS public.extend_booking(uuid, timestamptz);
                DROP FUNCTION IF EXISTS public.create_booking(uuid, uuid, timestamptz, timestamptz, text, text, uuid, uuid);
                DROP FUNCTION IF EXISTS public.quote_booking(uuid, uuid, timestamptz, timestamptz);
                DROP FUNCTION IF EXISTS public.location_free_capacity(uuid, timestamptz, timestamptz, uuid);
                DROP FUNCTION IF EXISTS public.space_is_free(uuid, timestamptz, timestamptz, uuid);
                DROP FUNCTION IF EXISTS public.location_is_open(uuid, timestamptz, timestamptz);
                DROP FUNCTION IF EXISTS public.expire_pending_bookings(uuid);
                DROP TRIGGER IF EXISTS record_booking_status ON public.bookings;
                DROP TRIGGER IF EXISTS guard_booking_status ON public.bookings;
                DROP FUNCTION IF EXISTS public.record_booking_status();
                DROP FUNCTION IF EXISTS public.guard_booking_status();
                """);

            migrationBuilder.DropTable(
                name: "booking_status_history");

            migrationBuilder.DropTable(
                name: "bookings");
        }
    }
}
