using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KursDb.Migrations
{
    /// <inheritdoc />
    public partial class AddCarRatingsNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "AverageRating",
                table: "Cars",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CarRatings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CarRatings_UserId",
                table: "CarRatings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarRatings_AspNetUsers_UserId",
                table: "CarRatings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarRatings_AspNetUsers_UserId",
                table: "CarRatings");

            migrationBuilder.DropIndex(
                name: "IX_CarRatings_UserId",
                table: "CarRatings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CarRatings");

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageRating",
                table: "Cars",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
