using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToFlashcards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcard_Users_UserId",
                table: "Flashcard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Flashcard",
                table: "Flashcard");

            migrationBuilder.RenameTable(
                name: "Flashcard",
                newName: "Flashcards");

            migrationBuilder.RenameIndex(
                name: "IX_Flashcard_UserId",
                table: "Flashcards",
                newName: "IX_Flashcards_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Flashcards",
                table: "Flashcards",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Users_UserId",
                table: "Flashcards",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Users_UserId",
                table: "Flashcards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Flashcards",
                table: "Flashcards");

            migrationBuilder.RenameTable(
                name: "Flashcards",
                newName: "Flashcard");

            migrationBuilder.RenameIndex(
                name: "IX_Flashcards_UserId",
                table: "Flashcard",
                newName: "IX_Flashcard_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Flashcard",
                table: "Flashcard",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcard_Users_UserId",
                table: "Flashcard",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
