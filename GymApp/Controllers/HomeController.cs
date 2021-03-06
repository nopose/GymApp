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
using Microsoft.AspNetCore.Mvc.Rendering;

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
            //Dictionary<int, string> exerciseNames = new Dictionary<int, string>();

            //TrainingProgram program = await _context.Workouts.FirstOrDefaultAsync(W => W.id == id && W.uid == _userManager.GetUserId(User));

            List<TrainingProgram> workouts = getWorkoutsForUser();

            var exer = _context.PExercises.FromSql("SELECT * FROM PExercises").ToList(); // Need to do this to access the data ???

            var sets = _context.ESets.FromSql("SELECT * FROM ESets").ToList(); // Need to do this to access the data ???

            List<Exercise> exercisesFromAPI = getExercisesFromAPIWithCustom();
            
            foreach (var pro in workouts)
            {
                pro.ActualDay = calculateDayNumber(pro.StartDate, pro.EndDate);
                foreach (var ex in pro.Exercices)
                {
                    if(ex.day == pro.ActualDay)
                    {
                        foreach (var real_ex in exercisesFromAPI)
                        {
                            if (ex.ExerciseID == real_ex.id)
                            {
                                //exerciseNames.Add(ex.ExerciseID, real_ex.name);
                                ex.Name = real_ex.name;
                                pro.ActualExercisesCount++;
                            }
                        }
                    }
                }
            }

            DashboardViewModel model = new DashboardViewModel(workouts);

            Random rnd = new Random();
            int r = rnd.Next(exercisesFromAPI.Count);
            int r2;
            do
                r2 = rnd.Next(exercisesFromAPI.Count);
            while (r == r2);

            model.schedule.Suggested = new List<Exercise>() { exercisesFromAPI[r], exercisesFromAPI[r2] };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetExercisesByID([FromBody]ModelDummy data)
        {
            int workoutID = data.workoutID;
            TrainingProgram program = _context.Workouts.FirstOrDefault(W => W.id == workoutID && W.uid == _userManager.GetUserId(User));

            var exer = _context.PExercises.FromSql("SELECT * FROM PExercises").ToList(); // Need to do this to access the data ???

            var sets = _context.ESets.FromSql("SELECT * FROM ESets").ToList(); // Need to do this to access the data ???

            List<Exercise> exercisesFromAPI = getExercisesFromAPIWithCustom();
            List<ProgramExercises> filtered = new List<ProgramExercises>();

            if (!(program is null))
            {
                foreach (var ex in program.Exercices)
                {
                    if(!filtered.Any(x => x.ExerciseID == ex.ExerciseID))
                    {
                        filtered.Add(ex);
                        foreach (var real_ex in exercisesFromAPI)
                        {
                            if (ex.ExerciseID == real_ex.id)
                            {
                                ex.Name = real_ex.name;
                            }
                        }
                    }
                }
            }

            SelectList list = new SelectList(filtered, "ExerciseID", "Name", 0);
            return Json(JsonConvert.SerializeObject(list));
        }

        public IActionResult About()
        {
            ViewData["Message"] = "This page is where you can find all the information you need to be able to fully understand and use this web application at its full potential.";

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

                model.Exercises.results = model.Exercises.results.ToList();
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

                model.Exercises.results = model.Exercises.results.ToList();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GraphData([FromBody]ModelDummy data)
        {
            TrainingProgram program = _context.Workouts.FirstOrDefault(W => W.id == data.workoutID && W.uid == _userManager.GetUserId(User));

            var exer = _context.PExercises.FromSql("SELECT * FROM PExercises").ToList(); // Need to do this to access the data ???

            var sets = _context.ESets.FromSql("SELECT * FROM ESets").ToList(); // Need to do this to access the data ???

            List<Exercise> exercisesFromAPI = getExercisesFromAPI();
            int exerciseID = data.ExerciseID;
            program.Exercices.Sort((x, y) => x.day.CompareTo(y.day));
            int max = 0;
            int graphMax = 0;
            int choice = 1;
            GraphDataJson result = new GraphDataJson();
            foreach (ProgramExercises ex in program.Exercices)
            {
                if(ex.ExerciseID == exerciseID)
                {
                    switch (choice)
                    {
                        case 0:
                            foreach (ExerciseSets set in ex.SetInfo)
                            {
                                if (set.amount > max)
                                    max = set.amount;
                                if (max > graphMax)
                                    graphMax = max;
                            }
                            result.Label.Add("Day " + ex.day);
                            result.Values.Add(max.ToString());
                            max = 0;
                            break;
                        case 1:
                            foreach (ExerciseSets set in ex.SetInfo)
                            {
                                if (set.weight > max)
                                    max = set.weight;
                                if (max > graphMax)
                                    graphMax = max;
                            }
                            result.Label.Add("Day " + ex.day);
                            result.Values.Add(max.ToString());
                            max = 0;
                            break;
                    }
                    
                }
            }

            //foreach (var ex in workouts[0].Exercices)
            //{
            //    foreach (var real_ex in exercisesFromAPI)
            //    {
            //        if (ex.ExerciseID == real_ex.id && (exerciseNames.GetValueOrDefault(ex.ExerciseID) == null))
            //        {
            //            exerciseNames.Add(ex.ExerciseID, real_ex.name);
            //            pro.ActualExercisesCount++;
            //        }
            //    }
            //}
            result.GraphMax = graphMax * 2;
            result.WorkoutName = program.name;
            result.ExerciseName = getExerciseName(exerciseID);
            return Json(JsonConvert.SerializeObject(result));
        }


        [HttpGet]
        public ActionResult Calendar()
        {
            return View(new CalendarViewModel());
        }

        [HttpPost]
        public JsonResult GetEvents([FromBody]WorkoutViewModel data)
        {
            string[] colors = { "red", "green", "blue", "orange", "grey" };
            List<TrainingProgram> workouts = getWorkoutsForUser();

            var exer = _context.PExercises.FromSql("SELECT * FROM PExercises").ToList(); // Need to do this to access the data ???

            var sets = _context.ESets.FromSql("SELECT * FROM ESets").ToList(); // Need to do this to access the data ???

            List<Exercise> exercisesFromAPI = getExercisesFromAPIWithCustom();
            //var events = new List<CalendarViewModel>();
            var model = new CalendarContent();
            DateTime start;

            for (var i = 0; i < workouts.Count; i++)
            {
                var pro = workouts[i];
                //model.data.Add(new CalendarViewModel()
                //{
                //    id = pro.id,
                //    title = pro.name,
                //    start = pro.StartDate.ToString(),
                //    end = pro.EndDate.ToString(),
                //    allDay = true,
                //    color = "red"
                //});
                var color = colors[i % 5];
                model.items.Add(new CalendarItem()
                {
                    Name = pro.name,
                    Color = color
                });

                foreach (var ex in pro.Exercices)
                {
                    foreach (var real_ex in exercisesFromAPI)
                    {
                        if (ex.ExerciseID == real_ex.id)
                        {
                            ex.Name = real_ex.name;
                        }
                    }
                    start = pro.StartDate.AddDays(ex.day);
                    model.data.Add(new CalendarViewModel()
                    {
                        id = ex.id,
                        title = ex.Name,
                        start = start.ToString(),
                        end = start.ToString(),
                        allDay = true,
                        color = color
                    });
                }
            }

            //for (var i = 0; i < workouts.Count; i++)
            //{
            //events.Add(new CalendarViewModel()
            //{
            //    id = workouts[i].id,
            //    title = workouts[i].name,
            //    start = workouts[i].StartDate.ToString(),
            //    end = workouts[i].EndDate.ToString(),
            //    allDay = true
            //});

            //}

            return Json(JsonConvert.SerializeObject(model));
        }
        

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

                cacheExercises = cacheExercises.ShallowCopy();
                cacheExercises.results = cacheExercises.results.ToList();

                // Save data in cache.
                _cache.Set("Exercises", cacheExercises.ShallowCopy());
            }
            else
            {
                cacheExercises = cacheExercises.ShallowCopy();
                cacheExercises.results = cacheExercises.results.ToList();
            }

            return cacheExercises.results;
        }

        private List<Exercise> getExercisesFromAPIWithCustom()
        {
            Result<Exercise> cacheExercises;
            Result<Exercise> res;

            if (!(_cache.TryGetValue("Exercises", out res)))
            {
                var json = new WebClient().DownloadString("https://wger.de/api/v2/exercise/?limit=2000&language=2&status=2");
                res = JsonConvert.DeserializeObject<Result<Exercise>>(json);

                cacheExercises = res.ShallowCopy();
                cacheExercises.results = res.results.ToList();

                // Save data in cache.
                _cache.Set("Exercises", res.ShallowCopy());
            }
            else
            {
                cacheExercises = res.ShallowCopy();
                cacheExercises.results = res.results.ToList();
            }

            var userID = _userManager.GetUserId(User);
            List<UserExercise> userEx = _context.UserEx
                    .Where(W => W.uid.Equals(userID))
                    .OrderBy(W => W.name)
                    .ToList();

            foreach (UserExercise ex in userEx)
            {
                cacheExercises.results.Insert(0, new Exercise()
                {
                    id = ex.id,
                    name = ex.name,
                    description = ex.description
                });
            }

            return cacheExercises.results;
        }

        private string getExerciseName(int id)
        {
            if(id < 1000)
            {
                var json = new WebClient().DownloadString("https://wger.de/api/v2/exerciseinfo/" + id);
                ExerciseInfo ex = JsonConvert.DeserializeObject<ExerciseInfo>(json);
                return ex.name;
            }
            else
            {
                var ex = _context.UserEx.FirstOrDefault(x => x.id == id);
                return ex.name;
            }
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
