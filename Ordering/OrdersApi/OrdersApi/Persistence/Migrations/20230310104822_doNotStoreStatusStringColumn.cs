using Microsoft.EntityFrameworkCore.Migrations;

namespace OrdersApi.Persistence.Migrations
{
    public partial class doNotStoreStatusStringColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StatusString",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "[Status]",
                stored: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "[Status]",
                oldStored: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StatusString",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "[Status]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "[Status]");
        }
    }
}
