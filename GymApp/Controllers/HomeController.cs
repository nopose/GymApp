﻿using System;
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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{

    public class HomeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly GymAppContext _context;
        private IMemoryCache _cache;

        public HomeController(
                    UserManager<AppUser> userManager,
                    SignInManager<AppUser> signInManager,
                    GymAppContext context,
                    IMemoryCache cache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _cache = cache;
        }

        public IActionResult Index()
        {
            Dictionary<int, string> exerciseNames = new Dictionary<int, string>();

            //TrainingProgram program = await _context.Workouts.FirstOrDefaultAsync(W => W.id == id && W.uid == _userManager.GetUserId(User));

            List<TrainingProgram> workouts = getWorkoutsForUser();

            var exer = _context.PExercises.FromSql("SELECT * FROM PExercises").ToList(); // Need to do this to access the data ???

            var sets = _context.ESets.FromSql("SELECT * FROM ESets").ToList(); // Need to do this to access the data ???

            List<Exercise> exercisesFromAPI = getExercisesFromAPI();
            
            foreach (var pro in workouts)
            {
                int day = calculateDayNumber(pro.StartDate, pro.EndDate);
                foreach (var ex in pro.Exercices)
                {
                    if(ex.day == day)
                    {
                        foreach (var real_ex in exercisesFromAPI)
                        {
                            if (ex.ExerciseID == real_ex.id && (exerciseNames.GetValueOrDefault(ex.ExerciseID) == null))
                            {
                                exerciseNames.Add(ex.ExerciseID, real_ex.name);
                                pro.ActualExercisesCount++;
                            }
                        }
                    }
                }
            }

            DashboardViewModel model = new DashboardViewModel(workouts, exerciseNames);

            Random rnd = new Random();
            int r = rnd.Next(exercisesFromAPI.Count);
            int r2;
            do
                r2 = rnd.Next(exercisesFromAPI.Count);
            while (r == r2);

            model.schedule.Suggested = new List<Exercise>() { exercisesFromAPI[r], exercisesFromAPI[r2] };

            return View(model);
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

            ViewBag.Workouts = getWorkoutsForUser();

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
            ViewBag.Workouts = getWorkoutsForUser();

            return View("Exercises", model);
        }

        public IActionResult ExerciseDetail(int id)
        {
            ExerciseDetailViewModel model = new ExerciseDetailViewModel(id);
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExerciseToWorkout(ExercisesViewModel model, string returnUrl = null)
        {
            AddToWorkoutData dataModel = model.AddData;

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                string userID = _userManager.GetUserId(User);
                int programID = dataModel.ProgramID;
                int exerciseID = dataModel.ExerciseID;
                int day = dataModel.Day;
                int amount = dataModel.Amount;
                int aunit = dataModel.Aunit;
                int? weight = dataModel.Weight;
                int? wunit = dataModel.Wunit;

                if (weight != null && wunit < 1)
                {
                    TempData["ErrorMessage"] = "If you enter a weight, you need to enter the weight unit as well.";
                    ViewBag.Workouts = getWorkoutsForUser();
                    return View("Exercises", model);
                }

                TrainingProgram tp = await _context.Workouts.FirstOrDefaultAsync(X => X.id == programID);
                ExerciseSets e_set = new ExerciseSets
                {
                    amount = amount,
                    aunit = aunit,
                    weight = (weight == null ? 0 : (int)weight),
                    wunit = (wunit == null ? 0: (int)wunit)
                };

                ProgramExercises p_ex = new ProgramExercises
                {
                    ExerciseID = exerciseID,
                    day = day
                };

                p_ex.SetInfo.Add(e_set);

                tp.Exercices.Add(p_ex);

                _context.Workouts.Update(tp);
                await _context.SaveChangesAsync();

                ViewBag.Workouts = getWorkoutsForUser();
                TempData["SuccessMessage"] = "Congrats! Your exercise has been added successfully.";
                if(model.Search == null)
                    return RedirectToAction("Exercises");
                else
                    return RedirectToAction("Exercises");
            }
            ViewBag.Workouts = getWorkoutsForUser();
            TempData["ErrorMessage"] = "Oops... Something hapenned...";
            return View("Exercises", model);
        }



        public IActionResult HomePage()
        {
            return View();
        }

        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}


        private List<TrainingProgram> getWorkoutsForUser()
        {
            List<TrainingProgram> workouts = new List<TrainingProgram>();

            var userID = _userManager.GetUserId(User);

            if (userID != null)
            {
                workouts = _context.Workouts
                    .Where(W => W.uid.Equals(userID))
                    .OrderBy(W => W.StartDate)
                    .ToList();
            }

            return workouts;
        }

        private List<Exercise> getExercisesFromAPI()
        {
            Result<Exercise> cacheExercises;

            if (!(_cache.TryGetValue("Exercises", out cacheExercises)))
            {
                var json = new WebClient().DownloadString("https://wger.de/api/v2/exercise/?limit=2000&language=2&status=2");
                cacheExercises = JsonConvert.DeserializeObject<Result<Exercise>>(json);

                // Save data in cache.
                _cache.Set("Exercises", cacheExercises.ShallowCopy());
            }

            return cacheExercises.results;
        }

        //calculate the day number to match with the workout
        //return 0 if invalid for some reason
        private static int calculateDayNumber(DateTime workoutStart, DateTime workoutEnd)
        {
            DateTime today = DateTime.Now;

            //make sure the workout is started and not ended
            if(workoutStart.Date <= today.Date && workoutEnd.Date >= today.Date)
            {
                return (((today - workoutStart).Days % 7) + 1);
            }
            return 0;
        }
    }
}
