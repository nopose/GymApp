using Microsoft.EntityFrameworkCore.Migrations;

namespace GymApp.Migrations
{
    public partial class NewContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseSets_ProgramExercises_ProgramExercisesid",
                table: "ExerciseSets");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramExercises_Workouts_TrainingProgramid",
                table: "ProgramExercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgramExercises",
                table: "ProgramExercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseSets",
                table: "ExerciseSets");

            migrationBuilder.RenameTable(
                name: "ProgramExercises",
                newName: "PExercises");

            migrationBuilder.RenameTable(
                name: "ExerciseSets",
                newName: "ESets");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramExercises_TrainingProgramid",
                table: "PExercises",
                newName: "IX_PExercises_TrainingProgramid");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseSets_ProgramExercisesid",
                table: "ESets",
                newName: "IX_ESets_ProgramExercisesid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PExercises",
                table: "PExercises",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ESets",
                table: "ESets",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ESets_PExercises_ProgramExercisesid",
                table: "ESets",
                column: "ProgramExercisesid",
                principalTable: "PExercises",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PExercises_Workouts_TrainingProgramid",
                table: "PExercises",
                column: "TrainingProgramid",
                principalTable: "Workouts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ESets_PExercises_ProgramExercisesid",
                table: "ESets");

            migrationBuilder.DropForeignKey(
                name: "FK_PExercises_Workouts_TrainingProgramid",
                table: "PExercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PExercises",
                table: "PExercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ESets",
                table: "ESets");

            migrationBuilder.RenameTable(
                name: "PExercises",
                newName: "ProgramExercises");

            migrationBuilder.RenameTable(
                name: "ESets",
                newName: "ExerciseSets");

            migrationBuilder.RenameIndex(
                name: "IX_PExercises_TrainingProgramid",
                table: "ProgramExercises",
                newName: "IX_ProgramExercises_TrainingProgramid");

            migrationBuilder.RenameIndex(
                name: "IX_ESets_ProgramExercisesid",
                table: "ExerciseSets",
                newName: "IX_ExerciseSets_ProgramExercisesid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgramExercises",
                table: "ProgramExercises",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExerciseSets",
                table: "ExerciseSets",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseSets_ProgramExercises_ProgramExercisesid",
                table: "ExerciseSets",
                column: "ProgramExercisesid",
                principalTable: "ProgramExercises",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramExercises_Workouts_TrainingProgramid",
                table: "ProgramExercises",
                column: "TrainingProgramid",
                principalTable: "Workouts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
