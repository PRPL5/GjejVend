using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Storage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("""
                DO $storage$
                BEGIN
                    IF to_regnamespace('storage') IS NULL THEN
                        RETURN;
                    END IF;

                    CREATE SCHEMA IF NOT EXISTS private;

                    CREATE FUNCTION private.owns_location(p_location_id text) RETURNS boolean
                    LANGUAGE sql STABLE SECURITY DEFINER SET search_path = '' AS $$
                        SELECT EXISTS (
                            SELECT 1
                            FROM public.parking_locations l
                            JOIN public.parking_providers p ON p.id = l.provider_id
                            WHERE l.id::text = p_location_id
                              AND p.owner_id = auth.uid()
                              AND p.deleted_at IS NULL)
                    $$;

                    CREATE FUNCTION private.is_admin() RETURNS boolean
                    LANGUAGE sql STABLE SECURITY DEFINER SET search_path = '' AS $$
                        SELECT EXISTS (SELECT 1 FROM public.user_roles WHERE user_id = auth.uid() AND role = 'admin')
                    $$;

                    REVOKE EXECUTE ON FUNCTION private.owns_location(text), private.is_admin() FROM PUBLIC;
                    GRANT USAGE ON SCHEMA private TO authenticated;
                    GRANT EXECUTE ON FUNCTION private.owns_location(text), private.is_admin() TO authenticated;

                    INSERT INTO storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
                    VALUES
                        ('parking-photos', 'parking-photos', true, 5242880, ARRAY['image/jpeg', 'image/png', 'image/webp']),
                        ('avatars', 'avatars', true, 2097152, ARRAY['image/jpeg', 'image/png', 'image/webp']),
                        ('verification-documents', 'verification-documents', false, 10485760, ARRAY['application/pdf', 'image/jpeg', 'image/png'])
                    ON CONFLICT (id) DO NOTHING;

                    CREATE POLICY "parking photos: providers manage own locations" ON storage.objects
                        FOR ALL TO authenticated
                        USING (bucket_id = 'parking-photos' AND private.owns_location((storage.foldername(name))[1]))
                        WITH CHECK (bucket_id = 'parking-photos' AND private.owns_location((storage.foldername(name))[1]));

                    CREATE POLICY "avatars: users manage own folder" ON storage.objects
                        FOR ALL TO authenticated
                        USING (bucket_id = 'avatars' AND (storage.foldername(name))[1] = auth.uid()::text)
                        WITH CHECK (bucket_id = 'avatars' AND (storage.foldername(name))[1] = auth.uid()::text);

                    CREATE POLICY "verification documents: owners upload" ON storage.objects
                        FOR INSERT TO authenticated
                        WITH CHECK (bucket_id = 'verification-documents' AND (storage.foldername(name))[1] = auth.uid()::text);

                    CREATE POLICY "verification documents: owners and admins read" ON storage.objects
                        FOR SELECT TO authenticated
                        USING (bucket_id = 'verification-documents' AND ((storage.foldername(name))[1] = auth.uid()::text OR private.is_admin()));
                END $storage$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $storage$
                BEGIN
                    IF to_regnamespace('storage') IS NULL THEN
                        RETURN;
                    END IF;

                    DROP POLICY IF EXISTS "verification documents: owners and admins read" ON storage.objects;
                    DROP POLICY IF EXISTS "verification documents: owners upload" ON storage.objects;
                    DROP POLICY IF EXISTS "avatars: users manage own folder" ON storage.objects;
                    DROP POLICY IF EXISTS "parking photos: providers manage own locations" ON storage.objects;
                    DROP FUNCTION IF EXISTS private.is_admin();
                    DROP FUNCTION IF EXISTS private.owns_location(text);
                END $storage$;
                """);

        }
    }
}
