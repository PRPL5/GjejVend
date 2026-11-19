using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Moderation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    reporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reported_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category = table.Column<string>(type: "text", nullable: false, defaultValue: "inaccurate"),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "open"),
                    resolution = table.Column<string>(type: "text", nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reports", x => x.id);
                    table.CheckConstraint("ck_reports_category", "category IN ('inaccurate', 'safety', 'fraud', 'inappropriate', 'other')");
                    table.CheckConstraint("ck_reports_single_target", "num_nonnulls(location_id, reported_user_id, review_id) = 1");
                    table.CheckConstraint("ck_reports_status", "status IN ('open', 'reviewing', 'resolved', 'dismissed')");
                    table.ForeignKey(
                        name: "fk_reports_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reports_profiles_reported_user_id",
                        column: x => x.reported_user_id,
                        principalTable: "profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reports_profiles_reporter_id",
                        column: x => x.reporter_id,
                        principalTable: "profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reports_profiles_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reports_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "reviews",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verification_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location_id = table.Column<Guid>(type: "uuid", nullable: true),
                    submitted_by = table.Column<Guid>(type: "uuid", nullable: false),
                    document_paths = table.Column<string[]>(type: "text[]", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_notes = table.Column<string>(type: "text", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verification_requests", x => x.id);
                    table.CheckConstraint("ck_verification_requests_single_target", "num_nonnulls(provider_id, location_id) = 1");
                    table.CheckConstraint("ck_verification_requests_status", "status IN ('pending', 'approved', 'rejected')");
                    table.ForeignKey(
                        name: "fk_verification_requests_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verification_requests_parking_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "parking_providers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verification_requests_profiles_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verification_requests_profiles_submitted_by",
                        column: x => x.submitted_by,
                        principalTable: "profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_actor_id_created_at",
                table: "audit_logs",
                columns: new[] { "actor_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_type_entity_id_created_at",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reports_location_id",
                table: "reports",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_reported_user_id",
                table: "reports",
                column: "reported_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_reporter_id",
                table: "reports",
                column: "reporter_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_resolved_by",
                table: "reports",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "ix_reports_review_id",
                table: "reports",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_status_created_at",
                table: "reports",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_verification_requests_location_id",
                table: "verification_requests",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_verification_requests_provider_id",
                table: "verification_requests",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_verification_requests_reviewer_id",
                table: "verification_requests",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "ix_verification_requests_status_created_at",
                table: "verification_requests",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_verification_requests_submitted_by",
                table: "verification_requests",
                column: "submitted_by");

            migrationBuilder.Sql("""
                CREATE FUNCTION public.audit_row_change() RETURNS trigger
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

                CREATE TRIGGER audit_row_change AFTER INSERT OR UPDATE ON public.bookings FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER INSERT OR UPDATE ON public.payments FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER INSERT OR UPDATE ON public.refunds FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER INSERT OR UPDATE ON public.provider_payouts FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER INSERT OR UPDATE OR DELETE ON public.user_roles FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER INSERT OR UPDATE OR DELETE ON public.platform_settings FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER UPDATE OF listing_status, verification_status ON public.parking_locations FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER UPDATE OF verification_status ON public.parking_providers FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER UPDATE OF status ON public.reviews FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER UPDATE OF status ON public.reports FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                CREATE TRIGGER audit_row_change AFTER UPDATE OF status ON public.verification_requests FOR EACH ROW EXECUTE FUNCTION public.audit_row_change();
                """);

            migrationBuilder.AddUpdatedAtTriggers("reports", "verification_requests");
            migrationBuilder.RejectModifications("audit_logs", "UPDATE OR DELETE");
            migrationBuilder.ProtectTables("reports", "verification_requests", "audit_logs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS audit_row_change ON public.bookings;
                DROP TRIGGER IF EXISTS audit_row_change ON public.payments;
                DROP TRIGGER IF EXISTS audit_row_change ON public.refunds;
                DROP TRIGGER IF EXISTS audit_row_change ON public.provider_payouts;
                DROP TRIGGER IF EXISTS audit_row_change ON public.user_roles;
                DROP TRIGGER IF EXISTS audit_row_change ON public.platform_settings;
                DROP TRIGGER IF EXISTS audit_row_change ON public.parking_locations;
                DROP TRIGGER IF EXISTS audit_row_change ON public.parking_providers;
                DROP TRIGGER IF EXISTS audit_row_change ON public.reviews;
                """);

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "verification_requests");
        }
    }
}
