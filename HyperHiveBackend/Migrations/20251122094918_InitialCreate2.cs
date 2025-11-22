using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HyperHiveBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIProfileData",
                table: "Mentors",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AIProfileData",
                table: "Managers",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AIProfileData",
                table: "Learners",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIProfileData",
                table: "Mentors");

            migrationBuilder.DropColumn(
                name: "AIProfileData",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "AIProfileData",
                table: "Learners");
        }
    }
}
