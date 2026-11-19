using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Listings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "amenities",
                columns: table => new
                {
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_amenities", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "parking_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false, defaultValue: "individual"),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    legal_name = table.Column<string>(type: "text", nullable: true),
                    tax_number = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    contact_phone = table.Column<string>(type: "text", nullable: true),
                    verification_status = table.Column<string>(type: "text", nullable: false, defaultValue: "unverified"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_providers", x => x.id);
                    table.CheckConstraint("ck_parking_providers_provider_type", "provider_type IN ('individual', 'business')");
                    table.CheckConstraint("ck_parking_providers_verification_status", "verification_status IN ('unverified', 'pending', 'verified', 'rejected')");
                    table.ForeignKey(
                        name: "fk_parking_providers_profiles_owner_id",
                        column: x => x.owner_id,
                        principalTable: "profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "parking_locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    address_line = table.Column<string>(type: "text", nullable: false),
                    neighbourhood = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: false),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    country_code = table.Column<string>(type: "char(2)", nullable: false, defaultValue: "XK"),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    timezone = table.Column<string>(type: "text", nullable: false, defaultValue: "Europe/Belgrade"),
                    parking_type = table.Column<string>(type: "text", nullable: false, defaultValue: "private_space"),
                    inventory_mode = table.Column<string>(type: "text", nullable: false, defaultValue: "capacity"),
                    total_capacity = table.Column<int>(type: "integer", nullable: false),
                    offline_occupied = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    listing_status = table.Column<string>(type: "text", nullable: false, defaultValue: "draft"),
                    verification_status = table.Column<string>(type: "text", nullable: false, defaultValue: "unverified"),
                    arrival_instructions = table.Column<string>(type: "text", nullable: true),
                    access_instructions = table.Column<string>(type: "text", nullable: true),
                    contact_phone = table.Column<string>(type: "text", nullable: true),
                    rating_average = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    rating_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_locations", x => x.id);
                    table.CheckConstraint("ck_parking_locations_inventory_mode", "inventory_mode IN ('capacity', 'spaces')");
                    table.CheckConstraint("ck_parking_locations_latitude", "latitude BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_parking_locations_listing_status", "listing_status IN ('draft', 'pending_review', 'published', 'suspended')");
                    table.CheckConstraint("ck_parking_locations_longitude", "longitude BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_parking_locations_offline_occupied", "offline_occupied BETWEEN 0 AND total_capacity");
                    table.CheckConstraint("ck_parking_locations_parking_type", "parking_type IN ('private_space', 'outdoor_lot', 'indoor', 'garage', 'commercial')");
                    table.CheckConstraint("ck_parking_locations_rating_average", "rating_average BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_parking_locations_total_capacity", "total_capacity > 0");
                    table.CheckConstraint("ck_parking_locations_verification_status", "verification_status IN ('unverified', 'pending', 'verified', 'rejected')");
                    table.ForeignKey(
                        name: "fk_parking_locations_parking_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "parking_providers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "parking_location_amenities",
                columns: table => new
                {
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amenity_code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_location_amenities", x => new { x.location_id, x.amenity_code });
                    table.ForeignKey(
                        name: "fk_parking_location_amenities_amenities_amenity_code",
                        column: x => x.amenity_code,
                        principalTable: "amenities",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "fk_parking_location_amenities_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parking_location_vehicle_types",
                columns: table => new
                {
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_location_vehicle_types", x => new { x.location_id, x.vehicle_type });
                    table.CheckConstraint("ck_parking_location_vehicle_types_vehicle_type", "vehicle_type IN ('car', 'motorcycle', 'van', 'suv')");
                    table.ForeignKey(
                        name: "fk_parking_location_vehicle_types_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parking_photos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_path = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_photos", x => x.id);
                    table.ForeignKey(
                        name: "fk_parking_photos_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parking_spaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    vehicle_type = table.Column<string>(type: "text", nullable: false, defaultValue: "car"),
                    is_covered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_accessible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    has_ev_charger = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    length_cm = table.Column<int>(type: "integer", nullable: true),
                    width_cm = table.Column<int>(type: "integer", nullable: true),
                    height_limit_cm = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_spaces", x => x.id);
                    table.CheckConstraint("ck_parking_spaces_dimensions", "coalesce(length_cm, 1) > 0 AND coalesce(width_cm, 1) > 0 AND coalesce(height_limit_cm, 1) > 0");
                    table.CheckConstraint("ck_parking_spaces_status", "status IN ('active', 'out_of_service')");
                    table.CheckConstraint("ck_parking_spaces_vehicle_type", "vehicle_type IN ('car', 'motorcycle', 'van', 'suv')");
                    table.ForeignKey(
                        name: "fk_parking_spaces_parking_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "parking_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "amenities",
                columns: new[] { "code", "icon", "name" },
                values: new object[,]
                {
                    { "accessible", "accessibility", "Accessible parking" },
                    { "cctv", "cctv", "CCTV" },
                    { "covered", "warehouse", "Covered" },
                    { "ev_charging", "plug-zap", "EV charging" },
                    { "gated", "door-closed", "Gate access" },
                    { "guarded", "shield-check", "Guarded" },
                    { "open_24_7", "clock", "Open 24/7" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_parking_location_amenities_amenity_code",
                table: "parking_location_amenities",
                column: "amenity_code");

            migrationBuilder.CreateIndex(
                name: "ix_parking_locations_city_listing_status",
                table: "parking_locations",
                columns: new[] { "city", "listing_status" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_parking_locations_provider_id",
                table: "parking_locations",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_parking_locations_slug",
                table: "parking_locations",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parking_photos_location_id_sort_order",
                table: "parking_photos",
                columns: new[] { "location_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ux_parking_photos_primary_per_location",
                table: "parking_photos",
                column: "location_id",
                unique: true,
                filter: "is_primary");

            migrationBuilder.CreateIndex(
                name: "ix_parking_providers_owner_id",
                table: "parking_providers",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_parking_spaces_location_id_label",
                table: "parking_spaces",
                columns: new[] { "location_id", "label" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.Sql("""
                ALTER TABLE public.parking_locations
                    ADD COLUMN location geography(Point, 4326)
                    GENERATED ALWAYS AS (ST_SetSRID(ST_MakePoint(longitude::double precision, latitude::double precision), 4326)::geography) STORED;

                CREATE INDEX ix_parking_locations_location ON public.parking_locations USING gist (location) WHERE deleted_at IS NULL;
                """);

            migrationBuilder.AddUpdatedAtTriggers("parking_providers", "parking_locations", "parking_spaces");
            migrationBuilder.ProtectTables(
                "parking_providers",
                "parking_locations",
                "parking_spaces",
                "parking_photos",
                "amenities",
                "parking_location_amenities",
                "parking_location_vehicle_types");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parking_location_amenities");

            migrationBuilder.DropTable(
                name: "parking_location_vehicle_types");

            migrationBuilder.DropTable(
                name: "parking_photos");

            migrationBuilder.DropTable(
                name: "parking_spaces");

            migrationBuilder.DropTable(
                name: "amenities");

            migrationBuilder.DropTable(
                name: "parking_locations");

            migrationBuilder.DropTable(
                name: "parking_providers");
        }
    }
}
