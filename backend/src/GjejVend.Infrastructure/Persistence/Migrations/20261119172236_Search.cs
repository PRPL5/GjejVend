using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    public partial class Search : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("""
                CREATE FUNCTION public.search_parking(
                    p_latitude double precision,
                    p_longitude double precision,
                    p_radius_m double precision,
                    p_starts_at timestamptz DEFAULT NULL,
                    p_ends_at timestamptz DEFAULT NULL,
                    p_vehicle_type text DEFAULT NULL,
                    p_max_hourly_rate numeric DEFAULT NULL,
                    p_limit integer DEFAULT 50)
                RETURNS TABLE (
                    location_id uuid,
                    name text,
                    slug text,
                    address_line text,
                    neighbourhood text,
                    latitude numeric,
                    longitude numeric,
                    distance_m double precision,
                    parking_type text,
                    inventory_mode text,
                    hourly_rate numeric,
                    daily_rate numeric,
                    currency char(3),
                    is_open boolean,
                    free_capacity integer,
                    rating_average numeric,
                    rating_count integer)
                LANGUAGE sql STABLE SET search_path = public, extensions AS $$
                    SELECT
                        l.id,
                        l.name,
                        l.slug,
                        l.address_line,
                        l.neighbourhood,
                        l.latitude,
                        l.longitude,
                        ST_Distance(l.location, origin.point) AS distance_m,
                        l.parking_type,
                        l.inventory_mode,
                        rate.hourly_rate,
                        rate.daily_rate,
                        rate.currency,
                        CASE WHEN p_starts_at IS NOT NULL THEN public.location_is_open(l.id, p_starts_at, p_ends_at) END,
                        CASE WHEN p_starts_at IS NOT NULL THEN public.location_free_capacity(l.id, p_starts_at, p_ends_at) END,
                        l.rating_average,
                        l.rating_count
                    FROM (SELECT ST_SetSRID(ST_MakePoint(p_longitude, p_latitude), 4326)::geography AS point) AS origin
                    JOIN public.parking_locations l ON ST_DWithin(l.location, origin.point, p_radius_m)
                    LEFT JOIN LATERAL (
                        SELECT r.hourly_rate, r.daily_rate, r.currency
                        FROM public.parking_rates r
                        WHERE r.location_id = l.id
                          AND tstzrange(r.valid_from, r.valid_to, '[)') @> coalesce(p_starts_at, now())
                        ORDER BY r.space_id NULLS FIRST, r.hourly_rate
                        LIMIT 1
                    ) AS rate ON true
                    WHERE l.deleted_at IS NULL
                      AND l.listing_status = 'published'
                      AND (p_max_hourly_rate IS NULL OR rate.hourly_rate <= p_max_hourly_rate)
                      AND (
                          p_vehicle_type IS NULL
                          OR NOT EXISTS (SELECT 1 FROM public.parking_location_vehicle_types v WHERE v.location_id = l.id)
                          OR EXISTS (SELECT 1 FROM public.parking_location_vehicle_types v WHERE v.location_id = l.id AND v.vehicle_type = p_vehicle_type))
                    ORDER BY distance_m
                    LIMIT p_limit
                $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.search_parking(double precision, double precision, double precision, timestamptz, timestamptz, text, numeric, integer);");

        }
    }
}
