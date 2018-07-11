using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GymApp.Migrations
{
    public partial class InitialProgramExercise : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserEx",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    uid = table.Column<int>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    category = table.Column<int>(nullable: false),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEx", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Workouts",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    uid = table.Column<int>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workouts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramExercises",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    day = table.Column<int>(nullable: false),
                    sets = table.Column<int>(nullable: false),
                    rep = table.Column<int>(nullable: false),
                    ExerciseID = table.Column<int>(nullable: false),
                    TrainingProgramid = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramExercises", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProgramExercises_Workouts_TrainingProgramid",
                        column: x => x.TrainingProgramid,
                        principalTable: "Workouts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSets",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    amount = table.Column<int>(nullable: false),
                    aunit = table.Column<int>(nullable: false),
                    weight = table.Column<int>(nullable: false),
                    wunit = table.Column<int>(nullable: false),
                    ProgramExercisesid = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSets", x => x.id);
                    table.ForeignKey(
                        name: "FK_ExerciseSets_ProgramExercises_ProgramExercisesid",
                        column: x => x.ProgramExercisesid,
                        principalTable: "ProgramExercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_ProgramExercisesid",
                table: "ExerciseSets",
                column: "ProgramExercisesid");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramExercises_TrainingProgramid",
                table: "ProgramExercises",
                column: "TrainingProgramid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseSets");

            migrationBuilder.DropTable(
                name: "UserEx");

            migrationBuilder.DropTable(
                name: "ProgramExercises");

            migrationBuilder.DropTable(
                name: "Workouts");
        }
    }
}
