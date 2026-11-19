using System;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(GjejVendDbContext))]
    partial class GjejVendDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GjejVend.Domain.Bookings.Booking", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("CancellationReason")
                        .HasColumnType("text")
                        .HasColumnName("cancellation_reason");

                    b.Property<DateTime?>("CancelledAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("cancelled_at");

                    b.Property<Guid?>("CancelledBy")
                        .HasColumnType("uuid")
                        .HasColumnName("cancelled_by");

                    b.Property<DateTime?>("CheckedInAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("checked_in_at");

                    b.Property<DateTime?>("CheckedOutAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("checked_out_at");

                    b.Property<decimal>("CommissionAmount")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("commission_amount");

                    b.Property<decimal>("CommissionRate")
                        .HasPrecision(5, 4)
                        .HasColumnType("numeric(5,4)")
                        .HasColumnName("commission_rate");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("char(3)")
                        .HasDefaultValue("EUR")
                        .HasColumnName("currency");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid")
                        .HasColumnName("customer_id");

                    b.Property<DateTime>("EndsAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("ends_at");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires_at");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<string>("PricingSnapshot")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("pricing_snapshot")
                        .HasDefaultValueSql("'{}'::jsonb");

                    b.Property<decimal>("ProviderAmount")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("provider_amount");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasColumnName("reference")
                        .HasDefaultValueSql("'GV' || upper(substr(md5(gen_random_uuid()::text), 1, 8))");

                    b.Property<Guid?>("SpaceId")
                        .HasColumnType("uuid")
                        .HasColumnName("space_id");

                    b.Property<DateTime>("StartsAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("starts_at");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("pending")
                        .HasColumnName("status");

                    b.Property<string>("TicketToken")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasColumnName("ticket_token")
                        .HasDefaultValueSql("replace(gen_random_uuid()::text, '-', '')");

                    b.Property<decimal>("TotalAmount")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("total_amount");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid?>("VehicleId")
                        .HasColumnType("uuid")
                        .HasColumnName("vehicle_id");

                    b.Property<string>("VehiclePlate")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("vehicle_plate");

                    b.Property<string>("VehicleType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("vehicle_type");

                    b.HasKey("Id")
                        .HasName("pk_bookings");

                    b.HasIndex("CancelledBy")
                        .HasDatabaseName("ix_bookings_cancelled_by");

                    b.HasIndex("ExpiresAt")
                        .HasDatabaseName("ix_bookings_expires_at")
                        .HasFilter("status = 'pending'");

                    b.HasIndex("Reference")
                        .IsUnique()
                        .HasDatabaseName("ix_bookings_reference");

                    b.HasIndex("SpaceId")
                        .HasDatabaseName("ix_bookings_space_id");

                    b.HasIndex("TicketToken")
                        .IsUnique()
                        .HasDatabaseName("ix_bookings_ticket_token");

                    b.HasIndex("VehicleId")
                        .HasDatabaseName("ix_bookings_vehicle_id");

                    b.HasIndex("CustomerId", "StartsAt")
                        .HasDatabaseName("ix_bookings_customer_id_starts_at");

                    b.HasIndex("LocationId", "StartsAt")
                        .HasDatabaseName("ix_bookings_location_id_starts_at");

                    b.ToTable("bookings", null, t =>
                        {
                            t.HasCheckConstraint("ck_bookings_amounts", "total_amount >= 0 AND commission_amount >= 0 AND provider_amount >= 0 AND commission_amount + provider_amount = total_amount");

                            t.HasCheckConstraint("ck_bookings_commission_rate", "commission_rate BETWEEN 0 AND 1");

                            t.HasCheckConstraint("ck_bookings_pending_expiry", "status <> 'pending' OR expires_at IS NOT NULL");

                            t.HasCheckConstraint("ck_bookings_period", "ends_at > starts_at");

                            t.HasCheckConstraint("ck_bookings_status", "status IN ('pending', 'confirmed', 'active', 'completed', 'cancelled', 'expired', 'no_show')");

                            t.HasCheckConstraint("ck_bookings_vehicle_type", "vehicle_type IN ('car', 'motorcycle', 'van', 'suv')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Bookings.BookingStatusChange", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<Guid>("BookingId")
                        .HasColumnType("uuid")
                        .HasColumnName("booking_id");

                    b.Property<DateTime>("ChangedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("changed_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid?>("ChangedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("changed_by");

                    b.Property<string>("FromStatus")
                        .HasColumnType("text")
                        .HasColumnName("from_status");

                    b.Property<string>("Reason")
                        .HasColumnType("text")
                        .HasColumnName("reason");

                    b.Property<string>("ToStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("to_status");

                    b.HasKey("Id")
                        .HasName("pk_booking_status_history");

                    b.HasIndex("ChangedBy")
                        .HasDatabaseName("ix_booking_status_history_changed_by");

                    b.HasIndex("BookingId", "ChangedAt")
                        .HasDatabaseName("ix_booking_status_history_booking_id_changed_at");

                    b.ToTable("booking_status_history", null, t =>
                        {
                            t.HasCheckConstraint("ck_booking_status_history_from_status", "from_status IN ('pending', 'confirmed', 'active', 'completed', 'cancelled', 'expired', 'no_show')");

                            t.HasCheckConstraint("ck_booking_status_history_to_status", "to_status IN ('pending', 'confirmed', 'active', 'completed', 'cancelled', 'expired', 'no_show')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Engagement.FavoriteLocation", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("UserId", "LocationId")
                        .HasName("pk_favorite_locations");

                    b.HasIndex("LocationId")
                        .HasDatabaseName("ix_favorite_locations_location_id");

                    b.ToTable("favorite_locations", (string)null);
                });

            modelBuilder.Entity("GjejVend.Domain.Engagement.Notification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("body");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid?>("EntityId")
                        .HasColumnType("uuid")
                        .HasColumnName("entity_id");

                    b.Property<string>("EntityType")
                        .HasColumnType("text")
                        .HasColumnName("entity_type");

                    b.Property<DateTime?>("ReadAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("read_at");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_notifications");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_notifications_unread")
                        .HasFilter("read_at IS NULL");

                    b.HasIndex("UserId", "CreatedAt")
                        .IsDescending(false, true)
                        .HasDatabaseName("ix_notifications_user_id_created_at");

                    b.ToTable("notifications", (string)null);
                });

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

            modelBuilder.Entity("GjejVend.Domain.Moderation.AuditLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("action");

                    b.Property<Guid?>("ActorId")
                        .HasColumnType("uuid")
                        .HasColumnName("actor_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid?>("EntityId")
                        .HasColumnType("uuid")
                        .HasColumnName("entity_id");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("entity_type");

                    b.Property<string>("Metadata")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("metadata")
                        .HasDefaultValueSql("'{}'::jsonb");

                    b.HasKey("Id")
                        .HasName("pk_audit_logs");

                    b.HasIndex("ActorId", "CreatedAt")
                        .HasDatabaseName("ix_audit_logs_actor_id_created_at");

                    b.HasIndex("EntityType", "EntityId", "CreatedAt")
                        .HasDatabaseName("ix_audit_logs_entity_type_entity_id_created_at");

                    b.ToTable("audit_logs", (string)null);
                });

            modelBuilder.Entity("GjejVend.Domain.Moderation.Report", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("category");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<Guid?>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<Guid?>("ReportedUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("reported_user_id");

                    b.Property<Guid>("ReporterId")
                        .HasColumnType("uuid")
                        .HasColumnName("reporter_id");

                    b.Property<string>("Resolution")
                        .HasColumnType("text")
                        .HasColumnName("resolution");

                    b.Property<DateTime?>("ResolvedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("resolved_at");

                    b.Property<Guid?>("ResolvedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("resolved_by");

                    b.Property<Guid?>("ReviewId")
                        .HasColumnType("uuid")
                        .HasColumnName("review_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("open")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id")
                        .HasName("pk_reports");

                    b.HasIndex("LocationId")
                        .HasDatabaseName("ix_reports_location_id");

                    b.HasIndex("ReportedUserId")
                        .HasDatabaseName("ix_reports_reported_user_id");

                    b.HasIndex("ReporterId")
                        .HasDatabaseName("ix_reports_reporter_id");

                    b.HasIndex("ResolvedBy")
                        .HasDatabaseName("ix_reports_resolved_by");

                    b.HasIndex("ReviewId")
                        .HasDatabaseName("ix_reports_review_id");

                    b.HasIndex("Status", "CreatedAt")
                        .HasDatabaseName("ix_reports_status_created_at");

                    b.ToTable("reports", null, t =>
                        {
                            t.HasCheckConstraint("ck_reports_category", "category IN ('inaccurate', 'safety', 'fraud', 'inappropriate', 'other')");

                            t.HasCheckConstraint("ck_reports_single_target", "num_nonnulls(location_id, reported_user_id, review_id) = 1");

                            t.HasCheckConstraint("ck_reports_status", "status IN ('open', 'reviewing', 'resolved', 'dismissed')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Moderation.VerificationRequest", b =>
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

                    b.Property<string[]>("DocumentPaths")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("document_paths");

                    b.Property<Guid?>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<Guid?>("ProviderId")
                        .HasColumnType("uuid")
                        .HasColumnName("provider_id");

                    b.Property<string>("ReviewNotes")
                        .HasColumnType("text")
                        .HasColumnName("review_notes");

                    b.Property<DateTime?>("ReviewedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("reviewed_at");

                    b.Property<Guid?>("ReviewerId")
                        .HasColumnType("uuid")
                        .HasColumnName("reviewer_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("pending")
                        .HasColumnName("status");

                    b.Property<Guid>("SubmittedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("submitted_by");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id")
                        .HasName("pk_verification_requests");

                    b.HasIndex("LocationId")
                        .HasDatabaseName("ix_verification_requests_location_id");

                    b.HasIndex("ProviderId")
                        .HasDatabaseName("ix_verification_requests_provider_id");

                    b.HasIndex("ReviewerId")
                        .HasDatabaseName("ix_verification_requests_reviewer_id");

                    b.HasIndex("SubmittedBy")
                        .HasDatabaseName("ix_verification_requests_submitted_by");

                    b.HasIndex("Status", "CreatedAt")
                        .HasDatabaseName("ix_verification_requests_status_created_at");

                    b.ToTable("verification_requests", null, t =>
                        {
                            t.HasCheckConstraint("ck_verification_requests_single_target", "num_nonnulls(provider_id, location_id) = 1");

                            t.HasCheckConstraint("ck_verification_requests_status", "status IN ('pending', 'approved', 'rejected')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Payments.LedgerEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<decimal>("Amount")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("amount");

                    b.Property<Guid?>("BookingId")
                        .HasColumnType("uuid")
                        .HasColumnName("booking_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("char(3)")
                        .HasDefaultValue("EUR")
                        .HasColumnName("currency");

                    b.Property<string>("EntryType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("entry_type");

                    b.Property<Guid?>("PaymentId")
                        .HasColumnType("uuid")
                        .HasColumnName("payment_id");

                    b.Property<Guid?>("PayoutId")
                        .HasColumnType("uuid")
                        .HasColumnName("payout_id");

                    b.Property<Guid?>("ProviderId")
                        .HasColumnType("uuid")
                        .HasColumnName("provider_id");

                    b.Property<Guid?>("RefundId")
                        .HasColumnType("uuid")
                        .HasColumnName("refund_id");

                    b.HasKey("Id")
                        .HasName("pk_ledger_entries");

                    b.HasIndex("BookingId")
                        .HasDatabaseName("ix_ledger_entries_booking_id");

                    b.HasIndex("PaymentId")
                        .HasDatabaseName("ix_ledger_entries_payment_id");

                    b.HasIndex("PayoutId")
                        .HasDatabaseName("ix_ledger_entries_payout_id");

                    b.HasIndex("RefundId")
                        .HasDatabaseName("ix_ledger_entries_refund_id");

                    b.HasIndex("ProviderId", "CreatedAt")
                        .HasDatabaseName("ix_ledger_entries_provider_id_created_at");

                    b.ToTable("ledger_entries", null, t =>
                        {
                            t.HasCheckConstraint("ck_ledger_entries_entry_type", "entry_type IN ('charge', 'refund', 'commission', 'provider_earning', 'payout')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Payments.Payment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<decimal>("Amount")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("amount");

                    b.Property<Guid>("BookingId")
                        .HasColumnType("uuid")
                        .HasColumnName("booking_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("char(3)")
                        .HasDefaultValue("EUR")
                        .HasColumnName("currency");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid")
                        .HasColumnName("customer_id");

                    b.Property<string>("Metadata")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("metadata")
                        .HasDefaultValueSql("'{}'::jsonb");

                    b.Property<DateTime?>("PaidAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("paid_at");

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("provider");

                    b.Property<string>("ProviderReference")
                        .HasColumnType("text")
                        .HasColumnName("provider_reference");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("pending")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id")
                        .HasName("pk_payments");

                    b.HasIndex("BookingId")
                        .HasDatabaseName("ix_payments_booking_id");

                    b.HasIndex("CustomerId")
                        .HasDatabaseName("ix_payments_customer_id");

                    b.HasIndex("Provider", "ProviderReference")
                        .IsUnique()
                        .HasDatabaseName("ix_payments_provider_provider_reference")
                        .HasFilter("provider_reference IS NOT NULL");

                    b.ToTable("payments", null, t =>
                        {
                            t.HasCheckConstraint("ck_payments_amount", "amount > 0");

                            t.HasCheckConstraint("ck_payments_status", "status IN ('pending', 'authorized', 'succeeded', 'failed', 'partially_refunded', 'refunded')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Payments.ProviderPayout", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<decimal>("Amount")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("amount");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("char(3)")
                        .HasDefaultValue("EUR")
                        .HasColumnName("currency");

                    b.Property<DateTime?>("PaidAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("paid_at");

                    b.Property<DateOnly>("PeriodEnd")
                        .HasColumnType("date")
                        .HasColumnName("period_end");

                    b.Property<DateOnly>("PeriodStart")
                        .HasColumnType("date")
                        .HasColumnName("period_start");

                    b.Property<Guid>("ProviderId")
                        .HasColumnType("uuid")
                        .HasColumnName("provider_id");

                    b.Property<string>("ProviderReference")
                        .HasColumnType("text")
                        .HasColumnName("provider_reference");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("pending")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id")
                        .HasName("pk_provider_payouts");

                    b.HasIndex("ProviderId", "PeriodStart")
                        .HasDatabaseName("ix_provider_payouts_provider_id_period_start");

                    b.ToTable("provider_payouts", null, t =>
                        {
                            t.HasCheckConstraint("ck_provider_payouts_amount", "amount > 0");

                            t.HasCheckConstraint("ck_provider_payouts_period", "period_end >= period_start");

                            t.HasCheckConstraint("ck_provider_payouts_status", "status IN ('pending', 'processing', 'paid', 'failed')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Payments.Refund", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<decimal>("Amount")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("amount");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid>("PaymentId")
                        .HasColumnType("uuid")
                        .HasColumnName("payment_id");

                    b.Property<string>("ProviderReference")
                        .HasColumnType("text")
                        .HasColumnName("provider_reference");

                    b.Property<string>("Reason")
                        .HasColumnType("text")
                        .HasColumnName("reason");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("pending")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id")
                        .HasName("pk_refunds");

                    b.HasIndex("PaymentId")
                        .HasDatabaseName("ix_refunds_payment_id");

                    b.ToTable("refunds", null, t =>
                        {
                            t.HasCheckConstraint("ck_refunds_amount", "amount > 0");

                            t.HasCheckConstraint("ck_refunds_status", "status IN ('pending', 'succeeded', 'failed')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Pricing.AvailabilityOverride", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<int?>("Capacity")
                        .HasColumnType("integer")
                        .HasColumnName("capacity");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid")
                        .HasColumnName("created_by");

                    b.Property<DateTime>("EndsAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("ends_at");

                    b.Property<string>("Kind")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("kind");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<string>("Reason")
                        .HasColumnType("text")
                        .HasColumnName("reason");

                    b.Property<Guid?>("SpaceId")
                        .HasColumnType("uuid")
                        .HasColumnName("space_id");

                    b.Property<DateTime>("StartsAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("starts_at");

                    b.HasKey("Id")
                        .HasName("pk_availability_overrides");

                    b.HasIndex("CreatedBy")
                        .HasDatabaseName("ix_availability_overrides_created_by");

                    b.HasIndex("SpaceId")
                        .HasDatabaseName("ix_availability_overrides_space_id");

                    b.HasIndex("LocationId", "StartsAt")
                        .HasDatabaseName("ix_availability_overrides_location_id_starts_at");

                    b.ToTable("availability_overrides", null, t =>
                        {
                            t.HasCheckConstraint("ck_availability_overrides_capacity", "(kind = 'reduced_capacity') = (capacity IS NOT NULL) AND coalesce(capacity, 0) >= 0");

                            t.HasCheckConstraint("ck_availability_overrides_kind", "kind IN ('closed', 'reduced_capacity', 'extra_hours')");

                            t.HasCheckConstraint("ck_availability_overrides_period", "ends_at > starts_at");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Pricing.OperatingHours", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<TimeOnly>("ClosesAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("closes_at");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<TimeOnly>("OpensAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("opens_at");

                    b.Property<int>("Weekday")
                        .HasColumnType("integer")
                        .HasColumnName("weekday");

                    b.HasKey("Id")
                        .HasName("pk_parking_operating_hours");

                    b.HasIndex("LocationId", "Weekday")
                        .IsUnique()
                        .HasDatabaseName("ix_parking_operating_hours_location_id_weekday");

                    b.ToTable("parking_operating_hours", null, t =>
                        {
                            t.HasCheckConstraint("ck_parking_operating_hours_weekday", "weekday BETWEEN 1 AND 7");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Pricing.ParkingRate", b =>
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

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("char(3)")
                        .HasDefaultValue("EUR")
                        .HasColumnName("currency");

                    b.Property<decimal?>("DailyRate")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("daily_rate");

                    b.Property<decimal>("HourlyRate")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("hourly_rate");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<int?>("MaxMinutes")
                        .HasColumnType("integer")
                        .HasColumnName("max_minutes");

                    b.Property<int>("MinMinutes")
                        .HasColumnType("integer")
                        .HasDefaultValue(60)
                        .HasColumnName("min_minutes");

                    b.Property<decimal?>("MonthlyRate")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("monthly_rate");

                    b.Property<Guid?>("SpaceId")
                        .HasColumnType("uuid")
                        .HasColumnName("space_id");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("ValidFrom")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("valid_from")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime?>("ValidTo")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("valid_to");

                    b.HasKey("Id")
                        .HasName("pk_parking_rates");

                    b.HasIndex("LocationId")
                        .HasDatabaseName("ix_parking_rates_location_id");

                    b.HasIndex("SpaceId")
                        .HasDatabaseName("ix_parking_rates_space_id");

                    b.ToTable("parking_rates", null, t =>
                        {
                            t.HasCheckConstraint("ck_parking_rates_amounts", "hourly_rate > 0 AND coalesce(daily_rate, 1) > 0 AND coalesce(monthly_rate, 1) > 0");

                            t.HasCheckConstraint("ck_parking_rates_duration", "min_minutes > 0 AND (max_minutes IS NULL OR max_minutes >= min_minutes)");

                            t.HasCheckConstraint("ck_parking_rates_validity", "valid_to IS NULL OR valid_to > valid_from");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Reviews.Review", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<Guid>("BookingId")
                        .HasColumnType("uuid")
                        .HasColumnName("booking_id");

                    b.Property<string>("Comment")
                        .HasColumnType("text")
                        .HasColumnName("comment");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid")
                        .HasColumnName("customer_id");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uuid")
                        .HasColumnName("location_id");

                    b.Property<string>("ProviderResponse")
                        .HasColumnType("text")
                        .HasColumnName("provider_response");

                    b.Property<short>("Rating")
                        .HasColumnType("smallint")
                        .HasColumnName("rating");

                    b.Property<DateTime?>("RespondedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("responded_at");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasDefaultValue("published")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id")
                        .HasName("pk_reviews");

                    b.HasIndex("BookingId")
                        .IsUnique()
                        .HasDatabaseName("ix_reviews_booking_id");

                    b.HasIndex("CustomerId")
                        .HasDatabaseName("ix_reviews_customer_id");

                    b.HasIndex("LocationId", "CreatedAt")
                        .HasDatabaseName("ix_reviews_location_id_created_at");

                    b.ToTable("reviews", null, t =>
                        {
                            t.HasCheckConstraint("ck_reviews_rating", "rating BETWEEN 1 AND 5");

                            t.HasCheckConstraint("ck_reviews_status", "status IN ('published', 'hidden', 'flagged')");
                        });
                });

            modelBuilder.Entity("GjejVend.Domain.Bookings.Booking", b =>
                {
                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("CancelledBy")
                        .HasConstraintName("fk_bookings_profiles_cancelled_by");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .IsRequired()
                        .HasConstraintName("fk_bookings_profiles_customer_id");

                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .IsRequired()
                        .HasConstraintName("fk_bookings_parking_locations_location_id");

                    b.HasOne("GjejVend.Domain.Listings.ParkingSpace", null)
                        .WithMany()
                        .HasForeignKey("SpaceId")
                        .HasConstraintName("fk_bookings_parking_spaces_space_id");

                    b.HasOne("GjejVend.Domain.Identity.Vehicle", null)
                        .WithMany()
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_bookings_vehicles_vehicle_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Bookings.BookingStatusChange", b =>
                {
                    b.HasOne("GjejVend.Domain.Bookings.Booking", null)
                        .WithMany()
                        .HasForeignKey("BookingId")
                        .IsRequired()
                        .HasConstraintName("fk_booking_status_history_bookings_booking_id");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("ChangedBy")
                        .HasConstraintName("fk_booking_status_history_profiles_changed_by");
                });

            modelBuilder.Entity("GjejVend.Domain.Engagement.FavoriteLocation", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_favorite_locations_parking_locations_location_id");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_favorite_locations_profiles_user_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Engagement.Notification", b =>
                {
                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_notifications_profiles_user_id");
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

            modelBuilder.Entity("GjejVend.Domain.Moderation.Report", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .HasConstraintName("fk_reports_parking_locations_location_id");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("ReportedUserId")
                        .HasConstraintName("fk_reports_profiles_reported_user_id");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("ReporterId")
                        .IsRequired()
                        .HasConstraintName("fk_reports_profiles_reporter_id");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("ResolvedBy")
                        .HasConstraintName("fk_reports_profiles_resolved_by");

                    b.HasOne("GjejVend.Domain.Reviews.Review", null)
                        .WithMany()
                        .HasForeignKey("ReviewId")
                        .HasConstraintName("fk_reports_reviews_review_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Moderation.VerificationRequest", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .HasConstraintName("fk_verification_requests_parking_locations_location_id");

                    b.HasOne("GjejVend.Domain.Listings.ParkingProvider", null)
                        .WithMany()
                        .HasForeignKey("ProviderId")
                        .HasConstraintName("fk_verification_requests_parking_providers_provider_id");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("ReviewerId")
                        .HasConstraintName("fk_verification_requests_profiles_reviewer_id");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("SubmittedBy")
                        .IsRequired()
                        .HasConstraintName("fk_verification_requests_profiles_submitted_by");
                });

            modelBuilder.Entity("GjejVend.Domain.Payments.LedgerEntry", b =>
                {
                    b.HasOne("GjejVend.Domain.Bookings.Booking", null)
                        .WithMany()
                        .HasForeignKey("BookingId")
                        .HasConstraintName("fk_ledger_entries_bookings_booking_id");

                    b.HasOne("GjejVend.Domain.Payments.Payment", null)
                        .WithMany()
                        .HasForeignKey("PaymentId")
                        .HasConstraintName("fk_ledger_entries_payments_payment_id");

                    b.HasOne("GjejVend.Domain.Payments.ProviderPayout", null)
                        .WithMany()
                        .HasForeignKey("PayoutId")
                        .HasConstraintName("fk_ledger_entries_provider_payouts_payout_id");

                    b.HasOne("GjejVend.Domain.Listings.ParkingProvider", null)
                        .WithMany()
                        .HasForeignKey("ProviderId")
                        .HasConstraintName("fk_ledger_entries_parking_providers_provider_id");

                    b.HasOne("GjejVend.Domain.Payments.Refund", null)
                        .WithMany()
                        .HasForeignKey("RefundId")
                        .HasConstraintName("fk_ledger_entries_refunds_refund_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Payments.Payment", b =>
                {
                    b.HasOne("GjejVend.Domain.Bookings.Booking", null)
                        .WithMany()
                        .HasForeignKey("BookingId")
                        .IsRequired()
                        .HasConstraintName("fk_payments_bookings_booking_id");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .IsRequired()
                        .HasConstraintName("fk_payments_profiles_customer_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Payments.ProviderPayout", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingProvider", null)
                        .WithMany()
                        .HasForeignKey("ProviderId")
                        .IsRequired()
                        .HasConstraintName("fk_provider_payouts_parking_providers_provider_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Payments.Refund", b =>
                {
                    b.HasOne("GjejVend.Domain.Payments.Payment", null)
                        .WithMany()
                        .HasForeignKey("PaymentId")
                        .IsRequired()
                        .HasConstraintName("fk_refunds_payments_payment_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Pricing.AvailabilityOverride", b =>
                {
                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("CreatedBy")
                        .HasConstraintName("fk_availability_overrides_profiles_created_by");

                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_availability_overrides_parking_locations_location_id");

                    b.HasOne("GjejVend.Domain.Listings.ParkingSpace", null)
                        .WithMany()
                        .HasForeignKey("SpaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("fk_availability_overrides_parking_spaces_space_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Pricing.OperatingHours", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_parking_operating_hours_parking_locations_location_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Pricing.ParkingRate", b =>
                {
                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_parking_rates_parking_locations_location_id");

                    b.HasOne("GjejVend.Domain.Listings.ParkingSpace", null)
                        .WithMany()
                        .HasForeignKey("SpaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("fk_parking_rates_parking_spaces_space_id");
                });

            modelBuilder.Entity("GjejVend.Domain.Reviews.Review", b =>
                {
                    b.HasOne("GjejVend.Domain.Bookings.Booking", null)
                        .WithMany()
                        .HasForeignKey("BookingId")
                        .IsRequired()
                        .HasConstraintName("fk_reviews_bookings_booking_id");

                    b.HasOne("GjejVend.Domain.Identity.Profile", null)
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .IsRequired()
                        .HasConstraintName("fk_reviews_profiles_customer_id");

                    b.HasOne("GjejVend.Domain.Listings.ParkingLocation", null)
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .IsRequired()
                        .HasConstraintName("fk_reviews_parking_locations_location_id");
                });
#pragma warning restore 612, 618
        }
    }
}
