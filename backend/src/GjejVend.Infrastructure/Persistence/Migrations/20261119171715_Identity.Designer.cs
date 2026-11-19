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
    [Migration("20261119171715_Identity")]
    partial class Identity
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
#pragma warning restore 612, 618
        }
    }
}
