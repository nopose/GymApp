using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    public class WorkoutController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly GymAppContext _context;

        public WorkoutController(
                    UserManager<AppUser> userManager,
                    SignInManager<AppUser> signInManager,
                    GymAppContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public  IActionResult Workouts(string returnUrl = null)
        {
            List<TrainingProgram> workouts = getWorkoutsForUser();

            ViewBag.Workouts = workouts;
            return View("Program");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddWorkout(WorkoutViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                string userID = _userManager.GetUserId(User);
                DateTime StartDate = model.StartDate;
                DateTime EndDate = model.EndDate;

                TrainingProgram newProgram = new TrainingProgram();

                if (!(StartDate >= new DateTime()))
                {
                    TempData["ErrorMessage"] = "The StartDate must be greater or equal than today's date.";
                    ViewBag.Workouts = getWorkoutsForUser();
                    return View("Program");
                }
                if (!(EndDate > StartDate))
                {
                    TempData["ErrorMessage"] = "The StartDate must be greater than the EndDate";
                    ViewBag.Workouts = getWorkoutsForUser();
                    return View("Program");
                }

                newProgram.uid = userID;
                newProgram.name = model.WorkoutName;
                newProgram.description = model.WorkoutDescription;
                newProgram.StartDate = StartDate;
                newProgram.EndDate = EndDate;

                _context.Workouts.Add(newProgram);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Your new workout has been added successfully!";
                ViewBag.Workouts = getWorkoutsForUser();
                return View("Program");
            }
            TempData["ErrorMessage"] = "Oops... Something went wrong..";
            ViewBag.Workouts = getWorkoutsForUser();
            return View("Program");
        }

        private List<TrainingProgram> getWorkoutsForUser()
        {
            List<TrainingProgram> workouts = new List<TrainingProgram>();

            var userID = _userManager.GetUserId(User);

            if (userID != null)
            {
                workouts = _context.Workouts
                    .Where(W => W.uid.Equals(userID))
                    .ToList();
            }

            return workouts;
        }
    }
}