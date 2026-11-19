using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Pricing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "availability_overrides",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    space_id = table.Column<Guid>(type: "uuid", nullable: true),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false, defaultValue: "closed"),
                    capacity = table.Column<int>(type: "integer", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_availability_overrides", x => x.id);
                    table.CheckConstraint("ck_availability_overrides_capacity", "(kind = 'reduced_capacity') = (capacity IS NOT NULL) AND coalesce(capacity, 0) >= 0");
                    table.CheckConstraint("ck_availability_overrides_kind", "kind IN ('closed', 'reduced_capacity', 'extra_hours')");
                    table.CheckConstraint("ck_availability_overrides_period", "ends_at > starts_at");
                    table.ForeignKey(
                        name: "fk_availability_overrides_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_availability_overrides_parking_spaces_space_id",
                        column: x => x.space_id,
                        principalTable: "parking_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_availability_overrides_profiles_created_by",
                        column: x => x.created_by,
                        principalTable: "profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "parking_operating_hours",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    weekday = table.Column<int>(type: "integer", nullable: false),
                    opens_at = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    closes_at = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_operating_hours", x => x.id);
                    table.CheckConstraint("ck_parking_operating_hours_weekday", "weekday BETWEEN 1 AND 7");
                    table.ForeignKey(
                        name: "fk_parking_operating_hours_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parking_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    space_id = table.Column<Guid>(type: "uuid", nullable: true),
                    hourly_rate = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    daily_rate = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    monthly_rate = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "EUR"),
                    min_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    max_minutes = table.Column<int>(type: "integer", nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_rates", x => x.id);
                    table.CheckConstraint("ck_parking_rates_amounts", "hourly_rate > 0 AND coalesce(daily_rate, 1) > 0 AND coalesce(monthly_rate, 1) > 0");
                    table.CheckConstraint("ck_parking_rates_duration", "min_minutes > 0 AND (max_minutes IS NULL OR max_minutes >= min_minutes)");
                    table.CheckConstraint("ck_parking_rates_validity", "valid_to IS NULL OR valid_to > valid_from");
                    table.ForeignKey(
                        name: "fk_parking_rates_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_parking_rates_parking_spaces_space_id",
                        column: x => x.space_id,
                        principalTable: "parking_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_availability_overrides_created_by",
                table: "availability_overrides",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_availability_overrides_location_id_starts_at",
                table: "availability_overrides",
                columns: new[] { "location_id", "starts_at" });

            migrationBuilder.CreateIndex(
                name: "ix_availability_overrides_space_id",
                table: "availability_overrides",
                column: "space_id");

            migrationBuilder.CreateIndex(
                name: "ix_parking_operating_hours_location_id_weekday",
                table: "parking_operating_hours",
                columns: new[] { "location_id", "weekday" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parking_rates_location_id",
                table: "parking_rates",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_parking_rates_space_id",
                table: "parking_rates",
                column: "space_id");

            migrationBuilder.Sql("""
                ALTER TABLE public.parking_rates
                    ADD CONSTRAINT ex_parking_rates_no_overlap EXCLUDE USING gist (
                        location_id WITH =,
                        (coalesce(space_id, '00000000-0000-0000-0000-000000000000'::uuid)) WITH =,
                        tstzrange(valid_from, valid_to, '[)') WITH &&);
                """);

            migrationBuilder.AddUpdatedAtTriggers("parking_rates");
            migrationBuilder.ProtectTables("parking_rates", "parking_operating_hours", "availability_overrides");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "availability_overrides");

            migrationBuilder.DropTable(
                name: "parking_operating_hours");

            migrationBuilder.DropTable(
                name: "parking_rates");
        }
    }
}
