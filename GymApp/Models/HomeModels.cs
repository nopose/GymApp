using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GymApp.APIModels;
using Microsoft.AspNetCore.Http;

namespace GymApp.Models
{
    #region Dashboard
    public class DashboardViewModel
    {
        public ScheduleData schedule { get; set; }
        public GraphData graphData { get; set; }

        public DashboardViewModel(List<TrainingProgram> workouts, Dictionary<int, string> exerciseNames) {
            schedule = new ScheduleData(workouts, exerciseNames);
            graphData = new GraphData();
        }
    }

    public class GraphData
    {
    }

    public class ScheduleData
    {
        public Dictionary<int, string> ExerciseNames { get; set; }
        public List<TrainingProgram> Workouts { get; set; }

        public ScheduleData(List<TrainingProgram> workouts, Dictionary<int, string> exerciseNames) {
            ExerciseNames = exerciseNames;
            Workouts = workouts;
        }
    }
    #endregion

    #region Exercises
    public class ExercisesViewModel
    {
        public Result<Exercise> Exercises { get; set; }
        //public List<Exercise> SearchedExercises { get; set; }
        public Result<ExerciseImage> ExerciseImages { get; set; }
        public Result<Category> Categories { get; set; }
        public string Search { get; set; }

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
        }

        //public ExercisesViewModel(string query)
        //{
        //    var json = new WebClient().DownloadString(query);
        //    Exercises = JsonConvert.DeserializeObject<Result<Exercise>>(json);

        //    json = new WebClient().DownloadString("https://wger.de/api/v2/exerciseimage/");
        //    ExerciseImages = JsonConvert.DeserializeObject<Result<ExerciseImage>>(json);

        //    foreach (ExerciseImage im in ExerciseImages.results)
        //    {
        //        Exercise ex = Exercises.results.Find(x => x.id == im.id);
        //        if (ex != null)
        //        {
        //            ex.imageURL = im.image;
        //        }
        //    }
        //}

        public ExercisesViewModel(bool hasResult, string search)
        {
            Search = search;

            if (!hasResult)
            {
                Exercises = getAllExercises();
                ExerciseImages = getAllExercisesImages();
            }
            Categories = getAllCategories();
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
    #endregion
}
