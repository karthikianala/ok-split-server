using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkSplit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                schema: "public",
                table: "AspNetUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "public",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "public",
                table: "AspNetUsers",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                schema: "public",
                table: "AspNetUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiry",
                schema: "public",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                schema: "public",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "public",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "public",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "public",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                schema: "public",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiry",
                schema: "public",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "AspNetUsers");
        }
    }
}
