using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GymApp.APIModels;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    #region Dashboard
    public class DashboardViewModel
    {
        public ScheduleData schedule { get; set; }
        public GraphData graphData { get; set; }
        public int ProgramID { get; set; }

        public DashboardViewModel(List<TrainingProgram> workouts) {
            schedule = new ScheduleData(workouts);
            graphData = new GraphData();
        }
    }

    public class GraphData
    {
        //public int WorkoutID { get; set; }
        //public int ExerciseID { get; set; }
        //public int Choice { get; set; }
    }

    public class ScheduleData
    {
        //public Dictionary<int, string> ExerciseNames { get; set; }
        public List<TrainingProgram> Workouts { get; set; }
        public List<Exercise> Suggested { get; set; }
        public int Day { get; set; }

        public ScheduleData(List<TrainingProgram> workouts) {
            //ExerciseNames = exerciseNames;
            Workouts = workouts;
        }
    }

    public class ModelDummy
    {
        public int workoutID { get; set; }
        public int ExerciseID { get; set; }

        public ModelDummy() { }
    }
    #endregion

    #region Exercises
    public class ExercisesViewModel
    {
        public Result<Exercise> Exercises { get; set; }
        public Result<ExerciseImage> ExerciseImages { get; set; }
        public Result<Category> Categories { get; set; }
        public string Search { get; set; }
        public AddToWorkoutData AddData { get; set; }

        public ExercisesViewModel() { }

        public ExercisesViewModel(bool hasResult)
        {
            if (!hasResult)
            {
                Exercises = getAllExercises();

                //Exercises.results = Exercises.results.OrderBy(x => x.category).ToList();

                ExerciseImages = getAllExercisesImages();

                foreach (ExerciseImage im in ExerciseImages.results)
                {
                    Exercise ex = Exercises.results.Find(x => x.id == im.id);
                    if (ex != null)
                    {
                        ex.imageURL = im.image;
                    }
                }
            }
            Categories = getAllCategories();
            AddData = new AddToWorkoutData();
        }

        public ExercisesViewModel(bool hasResult, string search)
        {
            Search = search;

            if (!hasResult)
            {
                Exercises = getAllExercises();
                ExerciseImages = getAllExercisesImages();
            }
            Categories = getAllCategories();
            AddData = new AddToWorkoutData();
        }

        public void doSearch()
        {
            List<Exercise> copy = Exercises.results.ToList();
            copy.RemoveAll(x => !x.name.ToUpper().Contains(Search.ToUpper()));
            Exercises.results = copy;
        }

        public Result<Exercise> getAllExercises()
        {
            var json = new WebClient().DownloadString("https://wger.de/api/v2/exercise/?limit=2000&language=2&status=2");
            return JsonConvert.DeserializeObject<Result<Exercise>>(json);
        }

        public Result<ExerciseImage> getAllExercisesImages()
        {
            var json = new WebClient().DownloadString("https://wger.de/api/v2/exerciseimage/?limit=1000");
            return JsonConvert.DeserializeObject<Result<ExerciseImage>>(json);
        }

        public Result<Category> getAllCategories()
        {
            var json = new WebClient().DownloadString("https://wger.de/api/v2/exercisecategory/");
            return JsonConvert.DeserializeObject<Result<Category>>(json);
        }
    }

    public class ExerciseDetailViewModel
    {
        public ExerciseInfo exerciseInfo { get; set; }
        public ExerciseImage exerciseImage { get; set; }

        public ExerciseDetailViewModel(int id)
        {
            var json = new WebClient().DownloadString("https://wger.de/api/v2/exerciseinfo/" + id);
            exerciseInfo = JsonConvert.DeserializeObject<ExerciseInfo>(json);

            json = new WebClient().DownloadString("https://wger.de/api/v2/exerciseimage/?exercise=" + id);
            Result<ExerciseImage> ExerciseImages = JsonConvert.DeserializeObject<Result<ExerciseImage>>(json);
            exerciseImage = ExerciseImages.results.FirstOrDefault();
        }
    }

    public class AddToWorkoutData
    {
        //public string UserID { get; set; }
        public int ExerciseID { get; set; }

        [Display(Name = "Workout Name")]
        [Range(0, Int32.MaxValue, ErrorMessage = "The workout is required to be able to continue.")]
        public int ProgramID { get; set; }

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
    #endregion

    #region Calendar
    public class CalendarViewModel
    {
        public int id { get; set; }

        public string title { get; set; }

        public string start { get; set; }

        public string end { get; set; }

        public bool allDay { get; set; }
        public string color { get; set; }
    }

    public class CalendarContent
    {
        public List<CalendarViewModel> data { get; set; } = new List<CalendarViewModel>();
        public List<CalendarItem> items { get; set; } = new List<CalendarItem>();
    }

    public class CalendarItem
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }
    #endregion
}
