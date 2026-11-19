using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Foundation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF to_regnamespace('extensions') IS NULL THEN
                        CREATE SCHEMA extensions;
                    END IF;

                    IF to_regnamespace('auth') IS NULL THEN
                        CREATE SCHEMA auth;
                        CREATE TABLE auth.users (
                            id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                            email text,
                            phone text,
                            raw_user_meta_data jsonb NOT NULL DEFAULT '{}'::jsonb
                        );
                    END IF;

                    IF EXISTS (SELECT FROM pg_roles WHERE rolname = 'anon') THEN
                        ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public REVOKE ALL ON TABLES FROM anon, authenticated;
                        ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public REVOKE ALL ON SEQUENCES FROM anon, authenticated;
                        ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public REVOKE EXECUTE ON FUNCTIONS FROM anon, authenticated;
                        ALTER DEFAULT PRIVILEGES FOR ROLE postgres REVOKE EXECUTE ON FUNCTIONS FROM PUBLIC;
                    END IF;
                END $$;

                CREATE EXTENSION IF NOT EXISTS btree_gist WITH SCHEMA extensions;
                CREATE EXTENSION IF NOT EXISTS postgis WITH SCHEMA extensions;

                CREATE FUNCTION public.set_updated_at() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                BEGIN
                    NEW.updated_at := now();
                    RETURN NEW;
                END $$;

                CREATE FUNCTION public.reject_modification() RETURNS trigger
                LANGUAGE plpgsql SET search_path = '' AS $$
                BEGIN
                    RAISE EXCEPTION '% on % is not allowed', TG_OP, TG_TABLE_NAME USING ERRCODE = 'insufficient_privilege';
                END $$;

                CREATE FUNCTION public.current_actor_id() RETURNS uuid
                LANGUAGE sql STABLE SET search_path = '' AS $$
                    SELECT nullif(current_setting('app.actor_id', true), '')::uuid
                $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS public.current_actor_id();
                DROP FUNCTION IF EXISTS public.reject_modification();
                DROP FUNCTION IF EXISTS public.set_updated_at();
                """);
        }
    }
}
