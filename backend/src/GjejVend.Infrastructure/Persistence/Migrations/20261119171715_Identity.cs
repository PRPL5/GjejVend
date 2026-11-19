using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Identity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    avatar_path = table.Column<string>(type: "text", nullable: true),
                    preferred_language = table.Column<string>(type: "text", nullable: false, defaultValue: "sq"),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "active"),
                    notify_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notify_push = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notify_booking = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notify_marketing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profiles", x => x.id);
                    table.CheckConstraint("ck_profiles_preferred_language", "preferred_language IN ('sq', 'en', 'sr')");
                    table.CheckConstraint("ck_profiles_status", "status IN ('active', 'suspended', 'deleted')");
                });

            migrationBuilder.CreateTable(
                name: "platform_settings",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "jsonb", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platform_settings", x => x.key);
                    table.ForeignKey(
                        name: "fk_platform_settings_profiles_updated_by",
                        column: x => x.updated_by,
                        principalTable: "profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    granted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role });
                    table.CheckConstraint("ck_user_roles_role", "role IN ('customer', 'provider', 'admin')");
                    table.ForeignKey(
                        name: "fk_user_roles_profiles_granted_by",
                        column: x => x.granted_by,
                        principalTable: "profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_roles_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plate = table.Column<string>(type: "text", nullable: false),
                    vehicle_type = table.Column<string>(type: "text", nullable: false, defaultValue: "car"),
                    label = table.Column<string>(type: "text", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicles", x => x.id);
                    table.CheckConstraint("ck_vehicles_vehicle_type", "vehicle_type IN ('car', 'motorcycle', 'van', 'suv')");
                    table.ForeignKey(
                        name: "fk_vehicles_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "platform_settings",
                columns: new[] { "key", "updated_by", "value" },
                values: new object[,]
                {
                    { "commission_rate", null, "0.10" },
                    { "pending_booking_ttl_minutes", null, "15" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_platform_settings_updated_by",
                table: "platform_settings",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_granted_by",
                table: "user_roles",
                column: "granted_by");

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_user_id_plate",
                table: "vehicles",
                columns: new[] { "user_id", "plate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_vehicles_default_per_user",
                table: "vehicles",
                column: "user_id",
                unique: true,
                filter: "is_default");

            migrationBuilder.Sql("""
                ALTER TABLE public.profiles
                    ADD CONSTRAINT fk_profiles_auth_users_id FOREIGN KEY (id) REFERENCES auth.users (id) ON DELETE CASCADE;

                CREATE FUNCTION public.sync_profile_from_auth_user() RETURNS trigger
                LANGUAGE plpgsql SECURITY DEFINER SET search_path = '' AS $$
                BEGIN
                    INSERT INTO public.profiles (id, email, phone, first_name, last_name)
                    VALUES (
                        NEW.id,
                        NEW.email,
                        NEW.phone,
                        NEW.raw_user_meta_data ->> 'first_name',
                        NEW.raw_user_meta_data ->> 'last_name')
                    ON CONFLICT (id) DO UPDATE SET email = EXCLUDED.email, phone = EXCLUDED.phone;

                    INSERT INTO public.user_roles (user_id, role)
                    VALUES (NEW.id, 'customer')
                    ON CONFLICT DO NOTHING;

                    RETURN NEW;
                END $$;

                REVOKE EXECUTE ON FUNCTION public.sync_profile_from_auth_user() FROM PUBLIC;

                CREATE TRIGGER sync_profile_from_auth_user
                    AFTER INSERT OR UPDATE OF email, phone ON auth.users
                    FOR EACH ROW EXECUTE FUNCTION public.sync_profile_from_auth_user();
                """);

            migrationBuilder.AddUpdatedAtTriggers("profiles", "vehicles");
            migrationBuilder.ProtectTables("profiles", "user_roles", "vehicles", "platform_settings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS sync_profile_from_auth_user ON auth.users;
                DROP FUNCTION IF EXISTS public.sync_profile_from_auth_user();
                """);

            migrationBuilder.DropTable(
                name: "platform_settings");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "profiles");
        }
    }
}
