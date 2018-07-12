using Microsoft.EntityFrameworkCore.Migrations;

namespace GymApp.Migrations
{
    public partial class ModifyModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rep",
                table: "ProgramExercises");

            migrationBuilder.DropColumn(
                name: "sets",
                table: "ProgramExercises");

            migrationBuilder.AlterColumn<string>(
                name: "uid",
                table: "Workouts",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "uid",
                table: "UserEx",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "uid",
                table: "Workouts",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "uid",
                table: "UserEx",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rep",
                table: "ProgramExercises",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sets",
                table: "ProgramExercises",
                nullable: false,
                defaultValue: 0);
        }
    }
}
