using Microsoft.EntityFrameworkCore.Migrations;

namespace OrdersApi.Persistence.Migrations
{
    public partial class addStatusStringColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusString",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "[Status]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusString",
                table: "Orders");
        }
    }
}
