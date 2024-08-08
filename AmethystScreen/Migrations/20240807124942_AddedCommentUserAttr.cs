using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmethystScreen.Migrations
{
    /// <inheritdoc />
    public partial class AddedCommentUserAttr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CommentBy",
                table: "Comments",
                newName: "CommentByUsername");

            migrationBuilder.AddColumn<string>(
                name: "CommentById",
                table: "Comments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentById",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "CommentByUsername",
                table: "Comments",
                newName: "CommentBy");
        }
    }
}
