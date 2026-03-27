using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkSplit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "settlements",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaidBy = table.Column<Guid>(type: "uuid", nullable: false),
                    PaidTo = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RazorpayPaymentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RazorpayOrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SettledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_settlements_AspNetUsers_PaidBy",
                        column: x => x.PaidBy,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_settlements_AspNetUsers_PaidTo",
                        column: x => x.PaidTo,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_settlements_groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "public",
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    RazorpayOrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RazorpayPaymentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RazorpaySignature = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "INR"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Created"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalSchema: "public",
                        principalTable: "settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payments_SettlementId",
                schema: "public",
                table: "payments",
                column: "SettlementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settlements_GroupId",
                schema: "public",
                table: "settlements",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_settlements_PaidBy",
                schema: "public",
                table: "settlements",
                column: "PaidBy");

            migrationBuilder.CreateIndex(
                name: "IX_settlements_PaidTo",
                schema: "public",
                table: "settlements",
                column: "PaidTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "settlements",
                schema: "public");
        }
    }
}
