using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Reviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<short>(type: "smallint", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "published"),
                    provider_response = table.Column<string>(type: "text", nullable: true),
                    responded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reviews", x => x.id);
                    table.CheckConstraint("ck_reviews_rating", "rating BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_reviews_status", "status IN ('published', 'hidden', 'flagged')");
                    table.ForeignKey(
                        name: "fk_reviews_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reviews_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reviews_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_booking_id",
                table: "reviews",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reviews_customer_id",
                table: "reviews",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_location_id_created_at",
                table: "reviews",
                columns: new[] { "location_id", "created_at" });

            migrationBuilder.Sql("""
                CREATE FUNCTION public.guard_review_eligibility() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM public.bookings
                        WHERE id = NEW.booking_id
                          AND customer_id = NEW.customer_id
                          AND location_id = NEW.location_id
                          AND status = 'completed'
                    ) THEN
                        RAISE EXCEPTION 'review_booking_not_eligible';
                    END IF;

                    RETURN NEW;
                END $$;

                CREATE TRIGGER guard_review_eligibility
                    BEFORE INSERT OR UPDATE OF booking_id, customer_id, location_id ON public.reviews
                    FOR EACH ROW EXECUTE FUNCTION public.guard_review_eligibility();

                CREATE FUNCTION public.refresh_location_rating() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                BEGIN
                    UPDATE public.parking_locations l
                    SET rating_average = s.average, rating_count = s.total
                    FROM (
                        SELECT round(avg(r.rating), 2) AS average, count(*)::integer AS total
                        FROM public.reviews r
                        WHERE r.location_id = coalesce(NEW.location_id, OLD.location_id) AND r.status = 'published'
                    ) AS s
                    WHERE l.id = coalesce(NEW.location_id, OLD.location_id);

                    RETURN NULL;
                END $$;

                CREATE TRIGGER refresh_location_rating
                    AFTER INSERT OR DELETE OR UPDATE OF rating, status ON public.reviews
                    FOR EACH ROW EXECUTE FUNCTION public.refresh_location_rating();
                """);

            migrationBuilder.AddUpdatedAtTriggers("reviews");
            migrationBuilder.ProtectTables("reviews");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS refresh_location_rating ON public.reviews;
                DROP TRIGGER IF EXISTS guard_review_eligibility ON public.reviews;
                DROP FUNCTION IF EXISTS public.refresh_location_rating();
                DROP FUNCTION IF EXISTS public.guard_review_eligibility();
                """);

            migrationBuilder.DropTable(
                name: "reviews");
        }
    }
}
