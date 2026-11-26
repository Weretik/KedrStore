using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ArticleSeoAndStoreAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "content",
                table: "CompanyContacts",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionRu",
                schema: "content",
                table: "CompanyContacts",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionUk",
                schema: "content",
                table: "CompanyContacts",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "SaturdayCloses",
                schema: "content",
                table: "CompanyContacts",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "SaturdayOpens",
                schema: "content",
                table: "CompanyContacts",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreName",
                schema: "content",
                table: "CompanyContacts",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetAddressRu",
                schema: "content",
                table: "CompanyContacts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetAddressUk",
                schema: "content",
                table: "CompanyContacts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "SundayCloses",
                schema: "content",
                table: "CompanyContacts",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "SundayOpens",
                schema: "content",
                table: "CompanyContacts",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WeekdaysCloses",
                schema: "content",
                table: "CompanyContacts",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WeekdaysOpens",
                schema: "content",
                table: "CompanyContacts",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                schema: "content",
                table: "ArticleTranslations",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                schema: "content",
                table: "ArticleTranslations",
                type: "character varying(70)",
                maxLength: 70,
                nullable: true);

            // The shop the owner gave on 2026-11-26 (spec content/012 R-02); an address someone already typed stays.
            migrationBuilder.Sql("""
                UPDATE content."CompanyContacts"
                SET "StoreName" = 'Крамниця Дверна фурнітура CLASS',
                    "StreetAddressUk" = 'вул. Ахієзерів, 46',
                    "StreetAddressRu" = 'ул. Ахиезеров, 46',
                    "RegionUk" = 'Харківська обл.',
                    "RegionRu" = 'Харьковская обл.',
                    "PostalCode" = '61038',
                    "WeekdaysOpens" = TIME '09:00',
                    "WeekdaysCloses" = TIME '16:00'
                WHERE "Id" = 1 AND "StreetAddressUk" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "RegionRu",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "RegionUk",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "SaturdayCloses",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "SaturdayOpens",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "StoreName",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "StreetAddressRu",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "StreetAddressUk",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "SundayCloses",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "SundayOpens",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "WeekdaysCloses",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "WeekdaysOpens",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                schema: "content",
                table: "ArticleTranslations");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                schema: "content",
                table: "ArticleTranslations");
        }
    }
}
