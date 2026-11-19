using System;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(GjejVendDbContext))]
    [Migration("20261119171747_Listings")]
    partial class Listings
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GjejVend.Domain.Identity.PlatformSetting", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("text")
                        .HasColumnName("key");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("updated_by");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("value");

                    b.HasKey("Key")
                        .HasName("pk_platform_settings");

                    b.HasIndex("UpdatedBy")
                        .HasDatabaseName("ix_platform_settings_updated_by");

                    b.ToTable("platform_settings", (string)null);

                    b.HasData(
                        new
                        {
                            Key = "commission_rate",
                            UpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Value = "0.10"
                        },
                        new
                        {
                            Key = "pending_booking_ttl_minutes",
                            UpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Value = "15"
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Identity.Profile", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("AvatarPath")
                        .HasColumnType("text")
                        .HasColumnName("avatar_path");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<string>("Email")
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .HasColumnType("text")
                        .HasColumnName("first_name");

                    b.Property<string>("LastName")
                        .HasColumnType("text")
                        .HasColumnName("last_name");

                    b.Property<bool>("NotifyBooking")
                        .HasColumnType("boolean")
                        .HasDefaultValue(true)
                        .HasColumnName("notify_booking");

                    b.Property<bool>("NotifyEmail")
                        .HasColumnType("boolean")
                        .HasDefaultValue(true)
                        .HasColumnName("notify_email");

                    b.Property<bool>("NotifyMarketing")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("notify_marketing");

                    b.Property<bool>("NotifyPush")
                        .HasColumnType("boolean")
                        .HasDefaultValue(true)
                        .HasColumnName("notify_push");

                    b.Property<string>("Phone")
                        .HasColumnType("text")
                        .HasColumnName("phone");

                    b.Property<string>("PreferredLanguage")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("sq")
                        .HasColumnName("preferred_language");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("active")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id")
                        .HasName("pk_profiles");

                    b.ToTable("profiles", null, t =>
                        {
                            t.HasCheckConstraint("ck_profiles_preferred_language", "preferred_language IN ('sq', 'en', 'sr')");

                            t.HasCheckConstraint("ck_profiles_status", "status IN ('active', 'suspended', 'deleted')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Identity.UserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<string>("Role")
                        .HasColumnType("text")
                        .HasColumnName("role");

                    b.Property<DateTime>("GrantedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("granted_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid?>("GrantedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("granted_by");

                    b.HasKey("UserId", "Role")
                        .HasName("pk_user_roles");

                    b.HasIndex("GrantedBy")
                        .HasDatabaseName("ix_user_roles_granted_by");

                    b.ToTable("user_roles", null, t =>
                        {
                            t.HasCheckConstraint("ck_user_roles_role", "role IN ('customer', 'provider', 'admin')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Identity.Vehicle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("is_default");

                    b.Property<string>("Label")
                        .HasColumnType("text")
                        .HasColumnName("label");

                    b.Property<string>("Plate")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("plate");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<string>("VehicleType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("car")
                        .HasColumnName("vehicle_type");

                    b.HasKey("Id")
                        .HasName("pk_vehicles");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasDatabaseName("ux_vehicles_default_per_user")
                        .HasFilter("is_default");

                    b.HasIndex("UserId", "Plate")
                        .IsUnique()
                        .HasDatabaseName("ix_vehicles_user_id_plate");

                    b.ToTable("vehicles", null, t =>
                        {
                            t.HasCheckConstraint("ck_vehicles_vehicle_type", "vehicle_type IN ('car', 'motorcycle', 'van', 'suv')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.Amenity", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text")
                        .HasColumnName("code");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("icon");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Code")
                        .HasName("pk_amenities");

                    b.ToTable("amenities", (string)null);

                    b.HasData(
                        new
                        {
                            Code = "cctv",
                            Icon = "cctv",
                            Name = "CCTV"
                        },
                        new
                        {
                            Code = "guarded",
                            Icon = "shield-check",
                            Name = "Guarded"
                        },
                        new
                        {
                            Code = "covered",
                            Icon = "warehouse",
                            Name = "Covered"
                        },
                        new
                        {
                            Code = "ev_charging",
                            Icon = "plug-zap",
                            Name = "EV charging"
                        },
                        new
                        {
                            Code = "open_24_7",
                            Icon = "clock",
                            Name = "Open 24/7"
                        },
                        new
                        {
                            Code = "accessible",
                            Icon = "accessibility",
                            Name = "Accessible parking"
                        },
                        new
                        {
                            Code = "gated",
                            Icon = "door-closed",
                            Name = "Gate access"
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingLocation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("AccessInstructions")
                        .HasColumnType("text")
                        .HasColumnName("access_instructions");

                    b.Property<string>("AddressLine")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("address_line");

                    b.Property<string>("ArrivalInstructions")
                        .HasColumnType("text")
                        .HasColumnName("arrival_instructions");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("city");

                    b.Property<string>("ContactPhone")
                        .HasColumnType("text")
                        .HasColumnName("contact_phone");

                    b.Property<string>("CountryCode")
                        .IsRequired()
                        .HasColumnType("char(2)")
                        .HasDefaultValue("XK")
                        .HasColumnName("country_code");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("InventoryMode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("capacity")
                        .HasColumnName("inventory_mode");

                    b.Property<decimal>("Latitude")
                        .HasPrecision(9, 6)
                        .HasColumnType("numeric(9,6)")
                        .HasColumnName("latitude");

                    b.Property<string>("ListingStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("draft")
                        .HasColumnName("listing_status");

                    b.Property<decimal>("Longitude")
                        .HasPrecision(9, 6)
                        .HasColumnType("numeric(9,6)")
                        .HasColumnName("longitude");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Neighbourhood")
                        .HasColumnType("text")
                        .HasColumnName("neighbourhood");

                    b.Property<int>("OfflineOccupied")
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("offline_occupied");

                    b.Property<string>("ParkingType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("private_space")
                        .HasColumnName("parking_type");

                    b.Property<string>("PostalCode")
                        .HasColumnType("text")
                        .HasColumnName("postal_code");

                    b.Property<Guid>("ProviderId")
                        .HasColumnType("uuid")
                        .HasColumnName("provider_id");

                    b.Property<decimal?>("RatingAverage")
                        .HasPrecision(3, 2)
                        .HasColumnType("numeric(3,2)")
                        .HasColumnName("rating_average");

                    b.Property<int>("RatingCount")
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("rating_count");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("slug");

                    b.Property<string>("Timezone")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("Europe/Belgrade")
                        .HasColumnName("timezone");

                    b.Property<int>("TotalCapacity")
                        .HasColumnType("integer")
                        .HasColumnName("total_capacity");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("VerificationStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("unverified")
                        .HasColumnName("verification_status");

                    b.HasKey("Id")
                        .HasName("pk_parking_locations");

                    b.HasIndex("ProviderId")
                        .HasDatabaseName("ix_parking_locations_provider_id");

                    b.HasIndex("Slug")
                        .IsUnique()
                        .HasDatabaseName("ix_parking_locations_slug");

                    b.HasIndex("City", "ListingStatus")
                        .HasDatabaseName("ix_parking_locations_city_listing_status")
                        .HasFilter("deleted_at IS NULL");

                    b.ToTable("parking_locations", null, t =>
                        {
                            t.HasCheckConstraint("ck_parking_locations_inventory_mode", "inventory_mode IN ('capacity', 'spaces')");

                            t.HasCheckConstraint("ck_parking_locations_latitude", "latitude BETWEEN -90 AND 90");

                            t.HasCheckConstraint("ck_parking_locations_listing_status", "listing_status IN ('draft', 'pending_review', 'published', 'suspended')");

                            t.HasCheckConstraint("ck_parking_locations_longitude", "longitude BETWEEN -180 AND 180");

                            t.HasCheckConstraint("ck_parking_locations_offline_occupied", "offline_occupied BETWEEN 0 AND total_capacity");

                            t.HasCheckConstraint("ck_parking_locations_parking_type", "parking_type IN ('private_space', 'outdoor_lot', 'indoor', 'garage', 'commercial')");

                            t.HasCheckConstraint("ck_parking_locations_rating_average", "rating_average BETWEEN 1 AND 5");

                            t.HasCheckConstraint("ck_parking_locations_total_capacity", "total_capacity > 0");

                            t.HasCheckConstraint("ck_parking_locations_verification_status", "verification_status IN ('unverified', 'pending', 'verified', 'rejected')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingLocationAmenity", b =>
                {
                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<string>("AmenityCode")
                        .HasColumnType("text")
                        .HasColumnName("amenity_code");

                    b.HasKey("LocationId", "AmenityCode")
                        .HasName("pk_parking_location_amenities");

                    b.HasIndex("AmenityCode")
                        .HasDatabaseName("ix_parking_location_amenities_amenity_code");

                    b.ToTable("parking_location_amenities", (string)null);
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingLocationVehicleType", b =>
                {
                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<string>("VehicleType")
                        .HasColumnType("text")
                        .HasColumnName("vehicle_type");

                    b.HasKey("LocationId", "VehicleType")
                        .HasName("pk_parking_location_vehicle_types");

                    b.ToTable("parking_location_vehicle_types", null, t =>
                        {
                            t.HasCheckConstraint("ck_parking_location_vehicle_types_vehicle_type", "vehicle_type IN ('car', 'motorcycle', 'van', 'suv')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingPhoto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<bool>("IsPrimary")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("is_primary");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<int>("SortOrder")
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("sort_order");

                    b.Property<string>("StoragePath")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("storage_path");

                    b.HasKey("Id")
                        .HasName("pk_parking_photos");

                    b.HasIndex("LocationId")
                        .IsUnique()
                        .HasDatabaseName("ux_parking_photos_primary_per_location")
                        .HasFilter("is_primary");

                    b.HasIndex("LocationId", "SortOrder")
                        .HasDatabaseName("ix_parking_photos_location_id_sort_order");

                    b.ToTable("parking_photos", (string)null);
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingProvider", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("ContactEmail")
                        .HasColumnType("text")
                        .HasColumnName("contact_email");

                    b.Property<string>("ContactPhone")
                        .HasColumnType("text")
                        .HasColumnName("contact_phone");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("display_name");

                    b.Property<string>("LegalName")
                        .HasColumnType("text")
                        .HasColumnName("legal_name");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid")
                        .HasColumnName("owner_id");

                    b.Property<string>("ProviderType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("individual")
                        .HasColumnName("provider_type");

                    b.Property<string>("TaxNumber")
                        .HasColumnType("text")
                        .HasColumnName("tax_number");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("VerificationStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("unverified")
                        .HasColumnName("verification_status");

                    b.HasKey("Id")
                        .HasName("pk_parking_providers");

                    b.HasIndex("OwnerId")
                        .HasDatabaseName("ix_parking_providers_owner_id");

                    b.ToTable("parking_providers", null, t =>
                        {
                            t.HasCheckConstraint("ck_parking_providers_provider_type", "provider_type IN ('individual', 'business')");

                            t.HasCheckConstraint("ck_parking_providers_verification_status", "verification_status IN ('unverified', 'pending', 'verified', 'rejected')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingSpace", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<bool>("HasEvCharger")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("has_ev_charger");

                    b.Property<int?>("HeightLimitCm")
                        .HasColumnType("integer")
                        .HasColumnName("height_limit_cm");

                    b.Property<bool>("IsAccessible")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("is_accessible");

                    b.Property<bool>("IsCovered")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("is_covered");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("label");

                    b.Property<int?>("LengthCm")
                        .HasColumnType("integer")
                        .HasColumnName("length_cm");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("active")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("VehicleType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("car")
                        .HasColumnName("vehicle_type");

                    b.Property<int?>("WidthCm")
                        .HasColumnType("integer")
                        .HasColumnName("width_cm");

                    b.HasKey("Id")
                        .HasName("pk_parking_spaces");

                    b.HasIndex("LocationId", "Label")
                        .IsUnique()
                        .HasDatabaseName("ix_parking_spaces_location_id_label")
                        .HasFilter("deleted_at IS NULL");

                    b.ToTable("parking_spaces", null, t =>
                        {
                            t.HasCheckConstraint("ck_parking_spaces_dimensions", "coalesce(length_cm, 1) > 0 AND coalesce(width_cm, 1) > 0 AND coalesce(height_limit_cm, 1) > 0");

                            t.HasCheckConstraint("ck_parking_spaces_status", "status IN ('active', 'out_of_service')");

                            t.HasCheckConstraint("ck_parking_spaces_vehicle_type", "vehicle_type IN ('car', 'motorcycle', 'van', 'suv')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Identity.PlatformSetting", b =>
                {
                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("UpdatedBy")
                        .HasConstraintName("fk_platform_settings_profiles_updated_by");
                });

            modelBuilder.Entity("GjejVend.Domain.Identity.UserRole", b =>
                {
                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("GrantedBy")
                        .HasConstraintName("fk_user_roles_profiles_granted_by");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_roles_profiles_user_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Identity.Vehicle", b =>
                {
                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_vehicles_profiles_user_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingLocation", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingProvider", null)
                        .WithMany()
                        .HasForeignKey("ProviderId")
                        .IsRequired()
                        .HasConstraintName("fk_parking_locations_parking_providers_provider_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingLocationAmenity", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.Amenity", null)
                        .WithMany()
                        .HasForeignKey("AmenityCode")
                        .IsRequired()
                        .HasConstraintName("fk_parking_location_amenities_amenities_amenity_code");

                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_parking_location_amenities_parking_locations_location_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingLocationVehicleType", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_parking_location_vehicle_types_parking_locations_location_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingPhoto", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_parking_photos_parking_locations_location_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingProvider", b =>
                {
                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .IsRequired()
                        .HasConstraintName("fk_parking_providers_profiles_owner_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Listings.ParkingSpace", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_parking_spaces_parking_locations_location_id");
                });
#pragma warning restore 612, 618
        }
    }
}
