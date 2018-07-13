﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GymApp.APIModels;

namespace GymApp.Models
{
    #region Dashboard
    public class DashboardViewModel
    {
        public ScheduleData schedule { get; set; }
        public GraphData graphData { get; set; }

        public DashboardViewModel() {
            schedule = new ScheduleData();
            graphData = new GraphData();
        }
    }

    public class GraphData
    {
    }

    public class ScheduleData
    {
        public List<string> Exercises { get; set; }

        public ScheduleData() {
            Exercises = new List<string>();
        }
    }
    #endregion

    #region Exercises
    public class ExercisesViewModel
    {
        public Result<Exercise> Exercises { get; set; }
        public Result<ExerciseImage> ExerciseImages { get; set; }
        public Result<Category> Categories { get; set; }
        public string Search { get; set; }

        public ExercisesViewModel()
        {
            Exercises = getAllExercises();

            //Exercises.results = Exercises.results.OrderBy(x => x.category).ToList();

            ExerciseImages = getAllExercisesImages();

            Categories = getAllCategories();

            foreach (ExerciseImage im in ExerciseImages.results)
            {
                Exercise ex = Exercises.results.Find(x => x.id == im.id);
                if (ex != null) {
                    ex.imageURL = im.image;
                }
            }
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

        public ExercisesViewModel(string search)
        {
            Search = search;
            Exercises = getAllExercises();
            ExerciseImages = getAllExercisesImages();
            Categories = getAllCategories();

            //int index = 0;
            //for (int i = 0; i < Exercises.results.Count; i++)
            //{
            //    if (!(Exercises.results[index].name.ToUpper().Contains(search.ToUpper())))
            //        Exercises.results.RemoveAt(index);
            //    else
            //        index++;
            //}
            Exercises.results.RemoveAll(x => !x.name.ToUpper().Contains(search.ToUpper()));
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
