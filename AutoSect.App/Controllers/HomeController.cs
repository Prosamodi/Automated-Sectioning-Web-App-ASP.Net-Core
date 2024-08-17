using AutoSect.App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Infrastructure;

namespace AutoSect.App.Controllers
{

    public class HomeController : Controller
    {
        private IStudentRepository repository;
        string? activePage;
        
        private readonly string autoSectFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", "AutoSectExampleSheet.xlsx");

        public HomeController(IStudentRepository repo)
        {
            repository = repo;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "AppAutoSect");
            }

            activePage = "home";
            ViewBag.ActivePage = activePage;
            TempData["ariaActive"] = "active";
            return View();
        }

        [HttpGet]
        public IActionResult DownloadAutoSectExcel()
        {
            if (System.IO.File.Exists(autoSectFilePath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(autoSectFilePath);
                string fileName = Path.GetFileName(autoSectFilePath);

                return File(fileBytes, "application/octet-stream", fileName);
            }
            else
            {
                return NotFound();
            }
        }
        
        public IActionResult ContactUs()
        {
            activePage = "contactus";
            ViewBag.ActivePage = activePage;
            TempData["ariaActive"] = "active";
            return View();
        }
    }
}
