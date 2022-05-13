﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YETwitter.Posts.Web.Data;

#nullable disable

namespace YETwitter.Posts.Web.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220504001434_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("YETwitter.Posts.Web.Data.Entities.Appeal", b =>
                {
                    b.Property<Guid>("PostId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("post_id");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("value");

                    b.HasKey("PostId", "Value");

                    b.ToTable("appeals");
                });

            modelBuilder.Entity("YETwitter.Posts.Web.Data.Entities.HashTag", b =>
                {
                    b.Property<Guid>("PostId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("post_id");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("value");

                    b.HasKey("PostId", "Value");

                    b.ToTable("hashtags");
                });

            modelBuilder.Entity("YETwitter.Posts.Web.Data.Entities.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)")
                        .HasColumnName("username");

                    b.Property<DateTime>("CreateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("create_time")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(140)
                        .HasColumnType("nvarchar(140)")
                        .HasColumnName("text");

                    b.HasKey("Id");

                    b.ToTable("posts");
                });

            modelBuilder.Entity("YETwitter.Posts.Web.Data.Entities.Appeal", b =>
                {
                    b.HasOne("YETwitter.Posts.Web.Data.Entities.Post", "Post")
                        .WithMany("Appeals")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("YETwitter.Posts.Web.Data.Entities.HashTag", b =>
                {
                    b.HasOne("YETwitter.Posts.Web.Data.Entities.Post", "Post")
                        .WithMany("Hashtags")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_hashtags_post_id");

                    b.Navigation("Post");
                });

            modelBuilder.Entity("YETwitter.Posts.Web.Data.Entities.Post", b =>
                {
                    b.Navigation("Appeals");

                    b.Navigation("Hashtags");
                });
#pragma warning restore 612, 618
        }
    }
}
