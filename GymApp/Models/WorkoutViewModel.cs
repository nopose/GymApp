using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Models
{
    public class WorkoutViewModel
    {
        public string UserID { get; set; }
        public int ProgramID { get; set; }
        public int DBExerciseID { get; set; }
        public int SetID { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        [Display(Name = "Name")]
        public string WorkoutName { get; set; }

        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
        [Display(Name = "Description")]
        public string WorkoutDescription { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        // Add new exercise!

        [Display(Name = "Exercise Name")]
        [Range(0, Int32.MaxValue, ErrorMessage = "The exercise name is required to be able to continue.")]
        public int ExerciseID { get; set; }

        [Display(Name = "Day of workouk (1-7)")]
        [Range(1, 7, ErrorMessage = "The day represent a day of the week, so it must be a value between 1 and 7 included.")]
        public int Day { get; set; }

        [Display(Name = "Amount")]
        public int Amount { get; set; }

        [Display(Name = "Unit")]
        [Range(1, 4, ErrorMessage = "You need to select the unit for the amount that you've entered.")]
        public int Aunit { get; set; }

        [Display(Name = "Weight")]
        public Nullable<int> Weight { get; set; }

        [Display(Name = "Unit")]
        public Nullable<int> Wunit { get; set; }
    }
}
