using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasswordWallet.Migrations
{
    public partial class AddSharedPassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedPasswords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SavedPasswordId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedPasswords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedPasswords_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SharedPasswords_SavedPasswords_SavedPasswordId",
                        column: x => x.SavedPasswordId,
                        principalTable: "SavedPasswords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedPasswords_AccountId",
                table: "SharedPasswords",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedPasswords_SavedPasswordId",
                table: "SharedPasswords",
                column: "SavedPasswordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedPasswords");
        }
    }
}
