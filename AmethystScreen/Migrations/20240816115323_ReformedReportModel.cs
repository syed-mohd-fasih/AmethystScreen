using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmethystScreen.Migrations
{
    /// <inheritdoc />
    public partial class ReformedReportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsProfile",
                table: "ReportedContent",
                newName: "ReportedCommentId");

            migrationBuilder.AddColumn<bool>(
                name: "IsMovie",
                table: "ReportedContent",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReportStatus",
                table: "ReportedContent",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReportedMovieSlug",
                table: "ReportedContent",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMovie",
                table: "ReportedContent");

            migrationBuilder.DropColumn(
                name: "ReportStatus",
                table: "ReportedContent");

            migrationBuilder.DropColumn(
                name: "ReportedMovieSlug",
                table: "ReportedContent");

            migrationBuilder.RenameColumn(
                name: "ReportedCommentId",
                table: "ReportedContent",
                newName: "IsProfile");
        }
    }
}
