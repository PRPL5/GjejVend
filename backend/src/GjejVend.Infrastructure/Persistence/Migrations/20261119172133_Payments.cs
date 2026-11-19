using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Payments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "EUR"),
                    provider = table.Column<string>(type: "text", nullable: false),
                    provider_reference = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.CheckConstraint("ck_payments_amount", "amount > 0");
                    table.CheckConstraint("ck_payments_status", "status IN ('pending', 'authorized', 'succeeded', 'failed', 'partially_refunded', 'refunded')");
                    table.ForeignKey(
                        name: "fk_payments_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_payments_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "provider_payouts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "EUR"),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    provider_reference = table.Column<string>(type: "text", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_payouts", x => x.id);
                    table.CheckConstraint("ck_provider_payouts_amount", "amount > 0");
                    table.CheckConstraint("ck_provider_payouts_period", "period_end >= period_start");
                    table.CheckConstraint("ck_provider_payouts_status", "status IN ('pending', 'processing', 'paid', 'failed')");
                    table.ForeignKey(
                        name: "fk_provider_payouts_parking_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "parking_providers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    provider_reference = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refunds", x => x.id);
                    table.CheckConstraint("ck_refunds_amount", "amount > 0");
                    table.CheckConstraint("ck_refunds_status", "status IN ('pending', 'succeeded', 'failed')");
                    table.ForeignKey(
                        name: "fk_refunds_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ledger_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    entry_type = table.Column<string>(type: "text", nullable: false, defaultValue: "charge"),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    refund_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payout_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "EUR"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ledger_entries", x => x.id);
                    table.CheckConstraint("ck_ledger_entries_entry_type", "entry_type IN ('charge', 'refund', 'commission', 'provider_earning', 'payout')");
                    table.ForeignKey(
                        name: "fk_ledger_entries_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_ledger_entries_parking_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "parking_providers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_ledger_entries_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_ledger_entries_provider_payouts_payout_id",
                        column: x => x.payout_id,
                        principalTable: "provider_payouts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_ledger_entries_refunds_refund_id",
                        column: x => x.refund_id,
                        principalTable: "refunds",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_booking_id",
                table: "ledger_entries",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_payment_id",
                table: "ledger_entries",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_payout_id",
                table: "ledger_entries",
                column: "payout_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_provider_id_created_at",
                table: "ledger_entries",
                columns: new[] { "provider_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_refund_id",
                table: "ledger_entries",
                column: "refund_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id",
                table: "payments",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_customer_id",
                table: "payments",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_provider_provider_reference",
                table: "payments",
                columns: new[] { "provider", "provider_reference" },
                unique: true,
                filter: "provider_reference IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_provider_payouts_provider_id_period_start",
                table: "provider_payouts",
                columns: new[] { "provider_id", "period_start" });

            migrationBuilder.CreateIndex(
                name: "ix_refunds_payment_id",
                table: "refunds",
                column: "payment_id");

            migrationBuilder.Sql("""
                CREATE FUNCTION public.guard_refund_amount() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                DECLARE
                    v_payment public.payments;
                    v_refunded numeric(10, 2);
                BEGIN
                    SELECT * INTO v_payment FROM public.payments WHERE id = NEW.payment_id FOR UPDATE;

                    IF v_payment.status NOT IN ('succeeded', 'partially_refunded') THEN
                        RAISE EXCEPTION 'refund_payment_not_captured';
                    END IF;

                    SELECT coalesce(sum(amount), 0) INTO v_refunded
                    FROM public.refunds
                    WHERE payment_id = NEW.payment_id AND status <> 'failed' AND id <> NEW.id;

                    IF v_refunded + NEW.amount > v_payment.amount THEN
                        RAISE EXCEPTION 'refund_exceeds_payment';
                    END IF;

                    RETURN NEW;
                END $$;

                CREATE TRIGGER guard_refund_amount
                    BEFORE INSERT OR UPDATE OF amount ON public.refunds
                    FOR EACH ROW EXECUTE FUNCTION public.guard_refund_amount();

                CREATE FUNCTION public.record_payment_ledger() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                DECLARE
                    v_provider_id uuid;
                    v_commission numeric(10, 2);
                BEGIN
                    IF NEW.status <> 'succeeded' OR (TG_OP = 'UPDATE' AND OLD.status NOT IN ('pending', 'authorized', 'failed')) THEN
                        RETURN NULL;
                    END IF;

                    SELECT l.provider_id, round(NEW.amount * b.commission_rate, 2)
                    INTO v_provider_id, v_commission
                    FROM public.bookings b
                    JOIN public.parking_locations l ON l.id = b.location_id
                    WHERE b.id = NEW.booking_id;

                    INSERT INTO public.ledger_entries (entry_type, booking_id, payment_id, provider_id, amount, currency)
                    VALUES
                        ('charge', NEW.booking_id, NEW.id, v_provider_id, NEW.amount, NEW.currency),
                        ('commission', NEW.booking_id, NEW.id, v_provider_id, v_commission, NEW.currency),
                        ('provider_earning', NEW.booking_id, NEW.id, v_provider_id, NEW.amount - v_commission, NEW.currency);

                    RETURN NULL;
                END $$;

                CREATE TRIGGER record_payment_ledger
                    AFTER INSERT OR UPDATE OF status ON public.payments
                    FOR EACH ROW EXECUTE FUNCTION public.record_payment_ledger();

                CREATE FUNCTION public.record_refund_ledger() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                DECLARE
                    v_payment public.payments;
                    v_refunded numeric(10, 2);
                BEGIN
                    IF NEW.status <> 'succeeded' OR (TG_OP = 'UPDATE' AND OLD.status = 'succeeded') THEN
                        RETURN NULL;
                    END IF;

                    SELECT * INTO v_payment FROM public.payments WHERE id = NEW.payment_id;

                    INSERT INTO public.ledger_entries (entry_type, booking_id, payment_id, refund_id, provider_id, amount, currency)
                    SELECT 'refund', v_payment.booking_id, v_payment.id, NEW.id, l.provider_id, NEW.amount, v_payment.currency
                    FROM public.bookings b
                    JOIN public.parking_locations l ON l.id = b.location_id
                    WHERE b.id = v_payment.booking_id;

                    SELECT coalesce(sum(amount), 0) INTO v_refunded
                    FROM public.refunds
                    WHERE payment_id = NEW.payment_id AND status = 'succeeded';

                    UPDATE public.payments
                    SET status = CASE WHEN v_refunded >= v_payment.amount THEN 'refunded' ELSE 'partially_refunded' END
                    WHERE id = NEW.payment_id;

                    RETURN NULL;
                END $$;

                CREATE TRIGGER record_refund_ledger
                    AFTER INSERT OR UPDATE OF status ON public.refunds
                    FOR EACH ROW EXECUTE FUNCTION public.record_refund_ledger();

                CREATE FUNCTION public.record_payout_ledger() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                BEGIN
                    IF NEW.status <> 'paid' OR (TG_OP = 'UPDATE' AND OLD.status = 'paid') THEN
                        RETURN NULL;
                    END IF;

                    INSERT INTO public.ledger_entries (entry_type, payout_id, provider_id, amount, currency)
                    VALUES ('payout', NEW.id, NEW.provider_id, NEW.amount, NEW.currency);

                    RETURN NULL;
                END $$;

                CREATE TRIGGER record_payout_ledger
                    AFTER INSERT OR UPDATE OF status ON public.provider_payouts
                    FOR EACH ROW EXECUTE FUNCTION public.record_payout_ledger();
                """);

            migrationBuilder.AddUpdatedAtTriggers("payments", "refunds", "provider_payouts");
            migrationBuilder.RejectModifications("payments", "DELETE");
            migrationBuilder.RejectModifications("refunds", "DELETE");
            migrationBuilder.RejectModifications("provider_payouts", "DELETE");
            migrationBuilder.RejectModifications("ledger_entries", "UPDATE OR DELETE");
            migrationBuilder.ProtectTables("payments", "refunds", "provider_payouts", "ledger_entries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS record_payout_ledger ON public.provider_payouts;
                DROP TRIGGER IF EXISTS record_refund_ledger ON public.refunds;
                DROP TRIGGER IF EXISTS record_payment_ledger ON public.payments;
                DROP TRIGGER IF EXISTS guard_refund_amount ON public.refunds;
                DROP FUNCTION IF EXISTS public.record_payout_ledger();
                DROP FUNCTION IF EXISTS public.record_refund_ledger();
                DROP FUNCTION IF EXISTS public.record_payment_ledger();
                DROP FUNCTION IF EXISTS public.guard_refund_amount();
                """);

            migrationBuilder.DropTable(
                name: "ledger_entries");

            migrationBuilder.DropTable(
                name: "provider_payouts");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "payments");
        }
    }
}
