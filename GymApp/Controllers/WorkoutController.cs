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
            List<TrainingProgram> workouts = new List<TrainingProgram>();

            var userID = _userManager.GetUserId(User);

            if (userID != null)
            {
                workouts = _context.Workouts.ToList();
            }

            return View("Program", workouts);
        }
    }
}