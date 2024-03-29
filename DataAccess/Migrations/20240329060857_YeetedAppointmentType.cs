using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class YeetedAppointmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderAvailability_AppointmentsCategories_AppointmentCategoryId",
                table: "ProviderAvailability");

            migrationBuilder.DropTable(
                name: "AppointmentsCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProviderAvailability_AppointmentCategoryId",
                table: "ProviderAvailability");

            migrationBuilder.DropColumn(
                name: "AppointmentCategoryId",
                table: "ProviderAvailability");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentCategoryId",
                table: "ProviderAvailability",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AppointmentsCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentsCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAvailability_AppointmentCategoryId",
                table: "ProviderAvailability",
                column: "AppointmentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderAvailability_AppointmentsCategories_AppointmentCategoryId",
                table: "ProviderAvailability",
                column: "AppointmentCategoryId",
                principalTable: "AppointmentsCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
