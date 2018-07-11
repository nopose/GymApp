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

namespace GymApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly GymAppContext _context;

        public HomeController(GymAppContext context)
        {
            _context = context;
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
            ExercisesViewModel model = new ExercisesViewModel();
            return View(model);
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

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
