using Microsoft.EntityFrameworkCore.Migrations;

namespace GjejVend.Infrastructure.Persistence.Migrations;

internal static class MigrationBuilderExtensions
{
    public static void ProtectTables(this MigrationBuilder migrationBuilder, params string[] tables)
    {
        foreach (var table in tables)
        {
            migrationBuilder.Sql($"""
                ALTER TABLE public.{table} ENABLE ROW LEVEL SECURITY;
                DO $$
                BEGIN
                    IF EXISTS (SELECT FROM pg_roles WHERE rolname = 'anon') THEN
                        REVOKE ALL ON public.{table} FROM anon, authenticated;
                    END IF;
                END $$;
                """);
        }
    }

    public static void AddUpdatedAtTriggers(this MigrationBuilder migrationBuilder, params string[] tables)
    {
        foreach (var table in tables)
        {
            migrationBuilder.Sql($"CREATE TRIGGER set_updated_at BEFORE UPDATE ON public.{table} FOR EACH ROW EXECUTE FUNCTION public.set_updated_at();");
        }
    }

    public static void RejectModifications(this MigrationBuilder migrationBuilder, string table, string operations)
    {
        migrationBuilder.Sql($"CREATE TRIGGER reject_modification BEFORE {operations} ON public.{table} FOR EACH ROW EXECUTE FUNCTION public.reject_modification();");
    }
}
