﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StoreApp.Data.Concrete;

#nullable disable

namespace StoreApp.Web.Migrations
{
    [DbContext(typeof(StoreDbContext))]
    [Migration("20250407225345_InıtıalCreate")]
    partial class InıtıalCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("StoreApp.Data.Concrete.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Category")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("Products");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Category = "Telefon",
                            Description = "Description 1",
                            Name = "Product 1",
                            Price = 1000m
                        },
                        new
                        {
                            Id = 2,
                            Category = "Telefon",
                            Description = "Description 2",
                            Name = "Product 2",
                            Price = 2000m
                        },
                        new
                        {
                            Id = 3,
                            Category = "Telefon",
                            Description = "Description 3",
                            Name = "Product 3",
                            Price = 3000m
                        },
                        new
                        {
                            Id = 4,
                            Category = "Telefon",
                            Description = "Description 4",
                            Name = "Product 4",
                            Price = 4000m
                        },
                        new
                        {
                            Id = 5,
                            Category = "Telefon",
                            Description = "Description 5",
                            Name = "Product 5",
                            Price = 5000m
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
