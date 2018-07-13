using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GymApp.Models;
using System.Net;
using Newtonsoft.Json;
using GymApp.APIModels;
using GymApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace GymApp.Controllers
{

    public class HomeController : Controller
    {
        private IMemoryCache _cache;
        private readonly GymAppContext _context;

        public HomeController(GymAppContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = memoryCache;
        }

        public IActionResult Index()
        {
            return View(new DashboardViewModel());
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Exercises(int startIndex = 0, int pageSize = 20)
        {
            Result<Exercise> cacheExercises;
            Result<ExerciseImage> cacheImages;
            ExercisesViewModel model;

            if (!(_cache.TryGetValue("Exercises", out cacheExercises) && _cache.TryGetValue("Images", out cacheImages)))
            {
                model = new ExercisesViewModel(false);

                // Save data in cache.
                _cache.Set("Exercises", model.Exercises.ShallowCopy());

                // Save data in cache.
                _cache.Set("Images", model.ExerciseImages.ShallowCopy());
            }
            else
            {
                model = new ExercisesViewModel(true) {
                    Exercises = cacheExercises,
                    ExerciseImages = cacheImages
                };
            }

            return View(model);
        }

        public IActionResult SearchExercises(IFormCollection form)
        {
            Result<Exercise> cacheExercises;
            Result<ExerciseImage> cacheImages;
            ExercisesViewModel model;

            if (!(_cache.TryGetValue("Exercises", out cacheExercises) && _cache.TryGetValue("Images", out cacheImages)))
            {
                model = new ExercisesViewModel(false, Convert.ToString(form["search"]));

                Result<Exercise> copy = model.Exercises.ShallowCopy();

                // Save data in cache.
                _cache.Set("Exercises", copy);

                // Save data in cache.
                _cache.Set("Images", model.ExerciseImages.ShallowCopy());
            }
            else
            {
                Result<Exercise> copy = cacheExercises.ShallowCopy();

                model = new ExercisesViewModel(true, Convert.ToString(form["search"]))
                {
                    Exercises = copy,
                    ExerciseImages = cacheImages
                };
            }
            model.doSearch();

            return View("Exercises", model);
        }

        //public string NavExercises()
        //{
        //    string result = "";

        //    var user = _context.Users.Find("5cd48358-8991-4359-9234-6f0d33354901");

        //    return result;
        //}

        public IActionResult ExerciseDetail(int id)
        {
            ExerciseDetailViewModel model = new ExerciseDetailViewModel(id);
            return View(model);
        }

        public IActionResult HomePage()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
