using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreTaskManager.Migrations
{
    public partial class ModifyTaskModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCreated",
                table: "Task");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCreated",
                table: "Task",
                nullable: false,
                defaultValue: false);
        }
    }
}
