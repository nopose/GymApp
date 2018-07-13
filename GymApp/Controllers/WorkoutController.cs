using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GymApp.APIModels;
using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace GymApp.Controllers
{
    public class WorkoutController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly GymAppContext _context;
        private IMemoryCache _cache;
        public List<Exercise> ExercisesList { get; set; }

        public WorkoutController(
                    UserManager<AppUser> userManager,
                    SignInManager<AppUser> signInManager,
                    GymAppContext context,
                    IMemoryCache cache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _cache = cache;
            ExercisesList = getExercisesFromAPI();
        }

        [HttpGet]
        public  IActionResult Workouts(string returnUrl = null)
        {
            List<TrainingProgram> workouts = getWorkoutsForUser();

            ViewBag.Exercises = ExercisesList;
            ViewBag.Workouts = workouts;
            return View("Program");
        }

        [HttpGet]
        public async Task<IActionResult> WorkoutInfo(int id, string returnUrl = null)
        {
            Dictionary<int, string> exerciseNames = new Dictionary<int, string>();

            TrainingProgram program = await _context.Workouts.FirstOrDefaultAsync(W => W.id == id);

            var exer = _context.PExercises.FromSql("SELECT * FROM PExercises").ToList();

            if (!(program is null))
            {
                foreach (var ex in program.Exercices)
                {
                    foreach (var real_ex in getExercisesFromAPI())
                    {
                        if (ex.ExerciseID == real_ex.id)
                        {
                            exerciseNames.Add(ex.ExerciseID, real_ex.name);
                        }
                    }
                }
                ViewBag.Names = exerciseNames;
                return View("ProgramInfo", program);
            }
            TempData["ErrorMessage"] = "The program you've selected does not exist...";
            return View("Program");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddWorkout(WorkoutViewModel model, string returnUrl = null)
        {
            ModelState.Remove("ExerciseID");
            ModelState.Remove("Day");
            ModelState.Remove("Amount");
            ModelState.Remove("Aunit");

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                string userID = _userManager.GetUserId(User);
                DateTime StartDate = model.StartDate;
                DateTime EndDate = model.EndDate;

                TrainingProgram newProgram = new TrainingProgram();

                if (!(StartDate.Date >= DateTime.Now.Date))
                {
                    TempData["ErrorMessage"] = "The Start Date must be greater or equal than today's date.";
                    ViewBag.Exercises = ExercisesList;
                    ViewBag.Workouts = getWorkoutsForUser();
                    return View("Program", model);
                }
                if (!(EndDate > StartDate))
                {
                    TempData["ErrorMessage"] = "The Start Date must be greater than the End Date";
                    ViewBag.Exercises = ExercisesList;
                    ViewBag.Workouts = getWorkoutsForUser();
                    return View("Program", model);
                }

                newProgram.uid = userID;
                newProgram.name = model.WorkoutName;
                newProgram.description = model.WorkoutDescription;
                newProgram.StartDate = StartDate;
                newProgram.EndDate = EndDate;

                _context.Workouts.Add(newProgram);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Your new workout has been added successfully!";
                ViewBag.Exercises = ExercisesList;
                ViewBag.Workouts = getWorkoutsForUser();
                return View("Program");
            }
            TempData["ErrorMessage"] = "Oops... Something went wrong..";
            ViewBag.Exercises = ExercisesList;
            ViewBag.Workouts = getWorkoutsForUser();
            return View("Program");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExerciseToWorkout(WorkoutViewModel model, string returnUrl = null)
        {
            ModelState.Remove("WorkoutName");
            ModelState.Remove("WorkoutDescription");
            ModelState.Remove("StartDate");
            ModelState.Remove("EndDate");
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                string userID = _userManager.GetUserId(User);
                int programID = model.ProgramID;
                int exerciseID = model.ExerciseID;
                int day = model.Day;
                int amount = model.Amount;
                int aunit = model.Aunit;
                int? weight = model.Weight;
                int? wunit = model.Wunit;

                if (weight != null && wunit < 1)
                {
                    TempData["ErrorMessage"] = "If you enter a weight, you need to enter the weight unit as well.";
                    ViewBag.Exercises = ExercisesList;
                    ViewBag.Workouts = getWorkoutsForUser();
                    return View("Program", model);
                }

                TrainingProgram tp = await _context.Workouts.FirstOrDefaultAsync(X => X.id == programID);
                ExerciseSets e_set = new ExerciseSets
                {
                    amount = amount,
                    aunit = aunit,
                    weight = (int) weight,
                    wunit = (int) wunit
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

                ViewBag.Exercises = ExercisesList;
                ViewBag.Workouts = getWorkoutsForUser();
                TempData["SuccessMessage"] = "Congrats! Your exercise has been added successfully.";
                return View("Program", new WorkoutViewModel());
            }
            ViewBag.Exercises = ExercisesList;
            ViewBag.Workouts = getWorkoutsForUser();
            TempData["ErrorMessage"] = "Oops... Something hapenned...";
            return View("Program", model);
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

                // Save data in cache.
                _cache.Set("Exercises", cacheExercises.ShallowCopy());
            }

            return cacheExercises.results;
        }
    }
}