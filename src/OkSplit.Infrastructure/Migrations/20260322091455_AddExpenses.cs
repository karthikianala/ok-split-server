using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkSplit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "expenses",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaidBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "INR"),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SplitType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReceiptUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expenses_AspNetUsers_PaidBy",
                        column: x => x.PaidBy,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expenses_groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "public",
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expense_splits",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ExpenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwedAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    IsSettled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_splits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expense_splits_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expense_splits_expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalSchema: "public",
                        principalTable: "expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_expense_splits_ExpenseId",
                schema: "public",
                table: "expense_splits",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_splits_ExpenseId_UserId",
                schema: "public",
                table: "expense_splits",
                columns: new[] { "ExpenseId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_expense_splits_UserId",
                schema: "public",
                table: "expense_splits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_GroupId",
                schema: "public",
                table: "expenses",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_PaidBy",
                schema: "public",
                table: "expenses",
                column: "PaidBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expense_splits",
                schema: "public");

            migrationBuilder.DropTable(
                name: "expenses",
                schema: "public");
        }
    }
}
