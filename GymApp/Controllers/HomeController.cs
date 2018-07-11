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

namespace GymApp.Controllers
{
    public class HomeController : Controller
    {
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
            TempData["ErrorMessage"] = "BAD";
            ExercisesViewModel model = new ExercisesViewModel();
            return View(model);
        }

        //public IActionResult NavExercises(string query)
        //{
        //    ExercisesViewModel model = new ExercisesViewModel(query);
        //    return View("Exercises", model);
        //}

        public IActionResult ExerciseDetail(int id)
        {
            ExerciseDetailViewModel model = new ExerciseDetailViewModel(id);
            return View(model);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
