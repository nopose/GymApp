using Newtonsoft.Json;
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

        public ExercisesViewModel()
        {
            var json = new WebClient().DownloadString("https://wger.de/api/v2/exercise/?limit=20&language=2&status=2");
            Exercises = JsonConvert.DeserializeObject<Result<Exercise>>(json);

            json = new WebClient().DownloadString("https://wger.de/api/v2/exerciseimage/?limit=1000");
            ExerciseImages = JsonConvert.DeserializeObject<Result<ExerciseImage>>(json);

            json = new WebClient().DownloadString("https://wger.de/api/v2/exercisecategory/");
            Categories = JsonConvert.DeserializeObject<Result<Category>>(json);

            foreach (ExerciseImage im in ExerciseImages.results)
            {
                Exercise ex = Exercises.results.Find(x => x.id == im.id);
                if (ex != null) {
                    ex.imageURL = im.image;
                }
            }
        }

        public ExercisesViewModel(string query)
        {
            var json = new WebClient().DownloadString(query);
            Exercises = JsonConvert.DeserializeObject<Result<Exercise>>(json);

            json = new WebClient().DownloadString("https://wger.de/api/v2/exerciseimage/");
            ExerciseImages = JsonConvert.DeserializeObject<Result<ExerciseImage>>(json);

            foreach (ExerciseImage im in ExerciseImages.results)
            {
                Exercise ex = Exercises.results.Find(x => x.id == im.id);
                if (ex != null)
                {
                    ex.imageURL = im.image;
                }
            }
        }


    }
    #endregion
}
