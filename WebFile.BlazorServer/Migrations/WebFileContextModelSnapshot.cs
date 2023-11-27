﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebFile.BlazorServer.Data;

#nullable disable

namespace WebFile.BlazorServer.Migrations
{
    [DbContext(typeof(WebFileContext))]
    partial class WebFileContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.14");

            modelBuilder.Entity("WebFile.BlazorServer.Data.FileModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(256)");

                    b.Property<string>("OwnerUserName")
                        .IsRequired()
                        .HasColumnType("varchar(256)");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("OwnerUserName");

                    b.ToTable("FileModel");
                });

            modelBuilder.Entity("WebFile.BlazorServer.Data.UserModel", b =>
                {
                    b.Property<string>("UserName")
                        .HasColumnType("varchar(256)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserName");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("WebFile.BlazorServer.Data.FileModel", b =>
                {
                    b.HasOne("WebFile.BlazorServer.Data.UserModel", "Owner")
                        .WithMany("Files")
                        .HasForeignKey("OwnerUserName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("WebFile.BlazorServer.Data.UserModel", b =>
                {
                    b.Navigation("Files");
                });
#pragma warning restore 612, 618
        }
    }
}
