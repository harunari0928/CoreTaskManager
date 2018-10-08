using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreTaskManager.Migrations
{
    public partial class AddDecriptionToAchievedTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AchievedTasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AchievedTasks");
        }
    }
}
