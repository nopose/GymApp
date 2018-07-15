using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Models
{
    public class TrainingProgram
    {
        [Key]
        public int id { get; set; }
        public string uid { get; set; } // User ID

        public string name { get; set; }
        public string description { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public List<ProgramExercises> Exercices { get; set; } = new List<ProgramExercises>();

        [NotMapped]
        public int ActualExercisesCount { get; set; } = 0;
        [NotMapped]
        public int ActualDay { get; set; }

    }

    public class ProgramExercises
    {
        [Key]
        public int id { get; set; }

        public int day { get; set; }
        public int ExerciseID { get; set; } // FROM API.

        public List<ExerciseSets> SetInfo { get; set; } = new List<ExerciseSets>();

        [NotMapped]
        public string Name { get; set; }
    }

    public class ExerciseSets
    {
        [Key]
        public int id { get; set; }

        public int amount { get; set; }
        public int aunit { get; set; }
        public int weight { get; set; }
        public int wunit { get; set; }
    }

    // EXERCISES
    public class UserExercise
    {
        [Key]
        public int id { get; set; }

        public string uid { get; set; }

        public string name { get; set; }
        public int category { get; set; }
        public string description { get; set; }
    }
}
