using AutoSect.App.Models;
using AutoSect.App.Models.ViewModels;
using AutoSect.App.SessionGetter;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using AutoSect.App.Services;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;


namespace AutoSect.App.Controllers
{
    [Authorize(Roles ="Admin,User,Member")]
    public class AppAutoSectController : Controller
    {
        string? activePage;
        
        private UserManager<IdentityUser> userManager;

        private AutoSectDbContext _context;

        private ProjectService projectService;
        private StudentsService studentService;
        private AutomationService automationService;
        private ExportExcelService exportService;

        public AppAutoSectController(ProjectService projService, StudentsService studService, AutomationService autService, ExportExcelService excelExportService, AutoSectDbContext context, UserManager<IdentityUser> userMgr)
        {
            projectService = projService;
            studentService = studService;
            automationService = autService;
            exportService = excelExportService;
            _context = context;
            userManager = userMgr;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //Setting Up Sessions AppIndex
            var userID = userManager.GetUserId(HttpContext.User);
            var userName = userManager.GetUserName(HttpContext.User);

           
            if (userID != null && userName != null)
            {
                HttpContext.Session.SetString("AppUserID", userID);
                HttpContext.Session.SetString("AppUserName", userName);
                HttpContext.Session.SetString("ProjectName", string.Empty);
                HttpContext.Session.SetInt32("ProjectId", 0);
                HttpContext.Session.SetInt32("SelectedSection", 100);
                HttpContext.Session.SetString("SelectedByName", "LastName");
                HttpContext.Session.SetString("SelectedAllSection", string.Empty);
                await HttpContext.Session.CommitAsync();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

           
            //Project Count App Index
            var projectCount = _context.ProjectSettings.Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")).Count();
            TempData["projectcount"] = projectCount.ToString();


            //Navigations App Index
            activePage = "home";
            ViewBag.ActivePage = activePage;
            TempData["ariaActive"] = "active";
            TempData["AppUserName"] = HttpContext.Session.GetString("AppUserName");

            return View(_context.ProjectSettings
                .Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")));
        }
        public async void SettingSessionInfo()
        {
            //Another way to Store Session
            var userID = userManager.GetUserId(HttpContext.User);
            var userName = userManager.GetUserName(HttpContext.User);

            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyUserName)))
            {
                if (userID != null && userName != null)
                {
                    HttpContext.Session.SetString(SessionVariables.SessionKeyUserID, userID);
                    HttpContext.Session.SetString(SessionVariables.SessionKeyUserName, userName);
                    await HttpContext.Session.CommitAsync();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(IFormFile file, ProjectForm form)
        {
            //Create Project
            //Upload Excel and Save it to the Database using Excel Data Reader
            if (!string.IsNullOrEmpty(form.ProjectName))
            {
                if (!CheckProjectName(form.ProjectName))
                {

                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    if (file != null && file.Length > 0)
                    {
                        var uploadsFolder = $"{Directory.GetCurrentDirectory()}\\wwwroot\\Uploads\\";

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        var filePath = Path.Combine(uploadsFolder, file.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        try
                        {
                            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                            {
                                var appUserId = userManager.GetUserId(HttpContext.User);

                                using (var reader = ExcelReaderFactory.CreateReader(stream))
                                {

                                    ProjectSetting p = new ProjectSetting();
                                    p.ProjectName = form.ProjectName;
                                    p.PAppUserId = appUserId;

                                    do
                                    {
                                        bool isHeaderSkipped = false;
                                        while (reader.Read())
                                        {
                                            if (!isHeaderSkipped)
                                            {
                                                isHeaderSkipped = true;
                                                continue;
                                            }

                                            Student s = new Student();

                                            s.FirstName = reader.GetValue(0).ToString();
                                            s.MiddleName = reader.GetValue(1).ToString();
                                            s.LastName = reader.GetValue(2).ToString();
                                            s.Gender = reader.GetValue(3).ToString();
                                            s.GWA = (float)(double)reader.GetValue(4);
                                            s.Project = p;
                                            s.AppUserID = appUserId;
                                            _context.Add(s);

                                            await _context.SaveChangesAsync();
                                        }
                                    } while (reader.NextResult());

                                    TempData["message"] = "success";
                                    TempData["projectname"] = form.ProjectName.ToString();
                                }
                            }
                        }
                        catch (IOException e)
                        {
                            TempData["message"] = e.Message;
                        }
                    }
                    else
                    {
                        TempData["Message"] = "empty";
                        TempData["ProjectName"] = "empty";
                    }

                }//end of Check Project Name Bool 

            }//end of IF FormModel is Valid


            return RedirectToAction(nameof(Index));
        }

        public bool CheckProjectName(string projectname)
        {
            var filterProjectId = HttpContext.Session.GetInt32("ProjectId");
            var filterCurrentUserId = HttpContext.Session.GetString("AppUserID");

            bool isValid = _context.ProjectSettings.
                Where(p => p.ProjectId == filterProjectId && p.PAppUserId == filterCurrentUserId).ToList().
                Exists(p => p.ProjectName.Equals(projectname,
                StringComparison.CurrentCultureIgnoreCase));

            return (isValid);
        }

        [HttpGet]
        public JsonResult CheckUserAvailability(string projectname)
        {
            System.Threading.Thread.Sleep(100);
            var SearchData = _context.ProjectSettings.
                Where(p => p.ProjectName == projectname).
                Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")).SingleOrDefault();

            if (SearchData != null)
            {
                return Json(1);
            }
            else
            {
                return Json(0);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(long projectId, string projectName)
        {
            await projectService.DeleteProjectById(projectId);
            await studentService.DeleteStudentsByProjectIdAndCurrentUser(projectId);

            TempData["DeletedMessage"] = "DeletedSuccess";
            TempData["DeletedProjectName"] = projectName;

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitToDashBoard(ProjectForm form)
        {
            //var projectname = form.ProjectName;
            HttpContext.Session.SetString("ProjectName", form.ProjectName);
            HttpContext.Session.SetInt32("ProjectId", form.ProjectId);
            await HttpContext.Session.CommitAsync();
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFromSelect(int selectedProjectId)
        {
            var selectedProject = _context.ProjectSettings.
                Where(p => p.ProjectId == selectedProjectId).FirstOrDefault();


            //var projectname = form.ProjectName;
            HttpContext.Session.SetString("ProjectName", selectedProject.ProjectName);
            HttpContext.Session.SetInt32("ProjectId", (int)selectedProject.ProjectId);
            await HttpContext.Session.CommitAsync();

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public ActionResult Dashboard()
        {
            var filterProjectName = HttpContext.Session.GetString("ProjectName");
            var filterProjectId = HttpContext.Session.GetInt32("ProjectId");


            //Selected Value in ProjectName Dropdown
            ViewBag.Selected = filterProjectName;

            if (!string.IsNullOrEmpty(filterProjectName))
            {
                //Total Male Student per Project
                var studentmalecounter = _context.Students.
                Include(p => p.Project).
                Where(p => p.AppUserID == HttpContext.Session.GetString("AppUserID") && p.Gender == "Male").
                Where(p => p.Project.ProjectName == filterProjectName).Count();

                TempData["malecountdashboard"] = studentmalecounter;


                //Total Female Student per Project
                var studentfemalecounter = _context.Students.
                Include(p => p.Project).
                Where(p => p.AppUserID == HttpContext.Session.GetString("AppUserID") && p.Gender == "Female").
                Where(p => p.Project.ProjectName == filterProjectName).Count();

                TempData["femalecountdashboard"] = studentfemalecounter;



                //Analyzed Data From ProjectSettings Table
                //Student Per Section / Project
                var studentPerSection = _context.ProjectSettings.
                    Where(p => p.ProjectName == filterProjectName).
                    Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")).
                    Select(p => p.TotalSection).SingleOrDefault();

                TempData["studentpersectiondashboard"] = (float)Math.Round(studentPerSection, 1);

                //Total Student Per Section
                var studentTotalPerSection = _context.ProjectSettings.
                    Where(p => p.ProjectName == filterProjectName).
                    Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")).
                    Select(p => p.TotalStudent).SingleOrDefault();

                TempData["totalstudentpersectiondashboard"] = (float)Math.Round(studentTotalPerSection, 1);

                //Male per Section / Project
                var studentmalePerSection = _context.ProjectSettings.
                    Where(p => p.ProjectName == filterProjectName).
                    Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")).
                    Select(p => p.MalesPerSection).SingleOrDefault();

                TempData["malepersectiondashboard"] = (float)Math.Round(studentmalePerSection, 1);

                //Female per Section / Project
                var studentFemalePerSection = _context.ProjectSettings.
                    Where(p => p.ProjectName == filterProjectName).
                    Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")).
                    Select(p => p.FemalesPerSection).SingleOrDefault();

                TempData["femalepersectiondashboard"] = (float)Math.Round(studentFemalePerSection, 1);

                //Total Section
                var totalSection = _context.ProjectSettings.
                    Where(p => p.ProjectId == filterProjectId).
                    Select(p => p.TotalSection).SingleOrDefault();

                TempData["totalSection"] = totalSection;
            }
            else
            {
                return RedirectToAction("Index", "AppAutoSect", null);
            }

            //Project Count App DashBoard
            var projectCount = (_context.ProjectSettings.Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")).Count());
            TempData["projectcount"] = projectCount.ToString();

            activePage = "dashboard";
            ViewBag.ActivePage = activePage;
            TempData["ariaActive"] = "active";
            TempData["AppUserName"] = HttpContext.Session.GetString("AppUserName");

            return View(_context.ProjectSettings.
            Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyzeProject(ProjectForm form)
        {
            var filterProjectName = HttpContext.Session.GetString("ProjectName");
            var projectId = HttpContext.Session.GetInt32("ProjectId");

            var totalsection = form.TotalSection;

            var totalfemaleStudent = _context.Students.
                Include(p => p.Project).
                Where(p => p.AppUserID == HttpContext.Session.GetString("AppUserID") && p.Gender == "Female").
                Where(p => p.Project.ProjectName == filterProjectName).Count();

            var totalmaleStudent = _context.Students.
               Include(p => p.Project).
               Where(p => p.AppUserID == HttpContext.Session.GetString("AppUserID") && p.Gender == "Male").
               Where(p => p.Project.ProjectName == filterProjectName).Count();

            //Project Status
            var projectStatus = _context.ProjectSettings.
                Where(p => p.ProjectName == filterProjectName).
                Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")).
                Select(p => p.Status).SingleOrDefault();

            var femalepersection = totalfemaleStudent / totalsection;
            var malepersection = totalmaleStudent / totalsection;

            var totalstudentspersection = femalepersection + malepersection;

            ProjectSetting psettings = new ProjectSetting();

            psettings.FemalesPerSection = femalepersection;
            psettings.MalesPerSection = malepersection;
            psettings.TotalSection = totalsection;
            psettings.TotalStudent = totalstudentspersection;
            psettings.Status = projectStatus;

            await projectService.UpdateProjectSettingAsync((long)projectId, psettings);

            var DashboardOrStudent = form.FromDashboardOrStudent;

            //return RedirectToAction(nameof(DashboardOrStudent));
            return RedirectToAction(DashboardOrStudent, "AppAutoSect", null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitToStudents(ProjectForm form)
        {
            //var projectname = form.ProjectName;
            HttpContext.Session.SetString("ProjectName", form.ProjectName);
            HttpContext.Session.SetInt32("ProjectId", form.ProjectId);
            await HttpContext.Session.CommitAsync();
            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFromSelectSection(int SelectedSection)
        {
            if (SelectedSection == 100)
            {
                HttpContext.Session.SetInt32("SelectedSection", SelectedSection);
            }
            else
            {
                var selectedSectionDb = _context.Students.
                Where(s => s.Section == SelectedSection).FirstOrDefault();
                HttpContext.Session.SetInt32("SelectedSection", SelectedSection);
            }

            await HttpContext.Session.CommitAsync();

            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFromSelectName(string SelectedName)
        {

            HttpContext.Session.SetString("SelectedByName", SelectedName);
            await HttpContext.Session.CommitAsync();

            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutomateSectioningRandomly()
        {
            var filterProjectId = HttpContext.Session.GetInt32("ProjectId");
            var filterCurrentUserId = HttpContext.Session.GetString("AppUserID");
            
            var projectTotalSectionIsZeroOrNull = _context.ProjectSettings.
                Where(p => p.ProjectId == filterProjectId && p.PAppUserId == filterCurrentUserId).
                Select(p => p.TotalSection).FirstOrDefault();

            if (projectTotalSectionIsZeroOrNull > 0)
            {
                await automationService.AutomateRandomSectioning();
            } else

            {
                TempData["TargetSectionErrMsg"] = "Target Section must not be equal to zero!";
            }
            
            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutomateSectioningByGrades()
        {
            var filterProjectId = HttpContext.Session.GetInt32("ProjectId");
            var filterCurrentUserId = HttpContext.Session.GetString("AppUserID");

            var projectTotalSectionIsZeroOrNull = _context.ProjectSettings.
                Where(p => p.ProjectId == filterProjectId && p.PAppUserId == filterCurrentUserId).
                Select(p => p.TotalSection).FirstOrDefault();

            if (projectTotalSectionIsZeroOrNull > 0)
            {
                await automationService.AutomateSectioningByGrades();
            } else
            {
                TempData["TargetSectionErrMsg"] = "Target Section must not be equal to zero!";
            }

            return RedirectToAction(nameof(Students));
        }

      
        [HttpGet]
        public async Task<IActionResult> Students()
        {
            var filterProjectName = HttpContext.Session.GetString("ProjectName");
            var filterProjectId = HttpContext.Session.GetInt32("ProjectId");
            var filterCurrentUserId = HttpContext.Session.GetString("AppUserID");
            var filterBySection = HttpContext.Session.GetInt32("SelectedSection");
            var filterByName = HttpContext.Session.GetString("SelectedByName");

            ViewBag.SelectedProject = filterProjectName;
            ViewBag.SelectedSection = filterBySection;

            if (string.IsNullOrEmpty(filterProjectName))
            {               
                return RedirectToAction("Index", "AppAutoSect", null);
            }

            //Total Male Student per Project
            var studentmalecounter = _context.Students.
            Include(p => p.Project).
            Where(p => p.AppUserID == filterCurrentUserId && p.Gender == "Male").
            Where(p => p.Project.ProjectName == filterProjectName).Count();

            TempData["malecount"] = studentmalecounter;


            //Total Female Student per Project
            var studentfemalecounter = _context.Students.
            Include(p => p.Project).
            Where(p => p.AppUserID == filterCurrentUserId && p.Gender == "Female").
            Where(p => p.Project.ProjectName == filterProjectName).Count();

            TempData["femalecount"] = studentfemalecounter;

            //Analyzed Data From ProjectSettings Table
            //Student Per Section / Project
            var studentPerSection = _context.ProjectSettings.
                Where(p => p.ProjectName == filterProjectName).
                Where(p => p.PAppUserId == filterCurrentUserId).
                Select(p => p.TotalSection).SingleOrDefault();

            TempData["studentpersection"] = (float)Math.Round(studentPerSection, 1);


            //Total Student Per Section
            var studentTotalPerSection = _context.ProjectSettings.
                Where(p => p.ProjectName == filterProjectName).
                Where(p => p.PAppUserId == filterCurrentUserId).
                Select(p => p.TotalStudent).SingleOrDefault();

            TempData["totalstudentpersection"] = (float)Math.Round(studentTotalPerSection, 1);


            //Male per Section / Project
            var studentmalePerSection = _context.ProjectSettings.
                Where(p => p.ProjectName == filterProjectName).
                Where(p => p.PAppUserId == filterCurrentUserId).
                Select(p => p.MalesPerSection).SingleOrDefault();

            TempData["malepersection"] = (float)Math.Round(studentmalePerSection, 1);

            //Female per Section / Project
            var studentFemalePerSection = _context.ProjectSettings.
                Where(p => p.ProjectName == filterProjectName).
                Where(p => p.PAppUserId == filterCurrentUserId).
                Select(p => p.FemalesPerSection).SingleOrDefault();

            TempData["femalepersection"] = (float)Math.Round(studentFemalePerSection, 1);


            //Total Female in Selected Section
            var totalFemaleCountInSection = _context.Students.
                Where(s => s.ProjectId == filterProjectId && s.Gender == "Female").
                Where(s => s.AppUserID == filterCurrentUserId).
                Where(s => s.Section == filterBySection).Count();

            TempData["femaleperselectedsection"] = totalFemaleCountInSection;

            //Total Male in Selected Section
            var totalMaleCountInSection = _context.Students.
                Where(s => s.ProjectId == filterProjectId && s.Gender == "Male").
                Where(s => s.AppUserID == filterCurrentUserId).
                Where(s => s.Section == filterBySection).Count();

            TempData["maleperselectedsection"] = totalMaleCountInSection;

            //Total Student in Selected Section
            TempData["totalperselectedsection"] = totalMaleCountInSection + totalFemaleCountInSection;

            //Check Updated Status
            var isCompleted = _context.ProjectSettings.
                Where(s => s.ProjectId == filterProjectId).
                Select(s => s.Status).SingleOrDefault();

            TempData["iscompleted"] = isCompleted;

            var usedMethod = _context.ProjectSettings.
                 Where(s => s.ProjectId == filterProjectId).
                Select(s => s.Method).SingleOrDefault();

            TempData["methodused"] = usedMethod;


            //Sections To List
            var allSections = _context.Students.
                Where(s => s.ProjectId == filterProjectId).
                Where(s => s.AppUserID == filterCurrentUserId).
                Select(s => s.Section).ToList();
            
            List<int> sections = new List<int>();
            sections = allSections.Distinct().Order().ToList(); 

            ViewBag.AllSections = sections;

            //For View Model Data
            IQueryable<Student> studentsContext;
            if(filterBySection == 100)
            {
                studentsContext = _context.Students.
                Where(s => s.ProjectId == filterProjectId).
                Where(s => s.AppUserID == filterCurrentUserId).
                OrderBy(s => s.Section).ThenBy(s => s.Gender).ThenBy(s => s.LastName);
            } 
            else
            {
                studentsContext = _context.Students.
                Where(s => s.ProjectId == filterProjectId).
                Where(s => s.AppUserID == filterCurrentUserId).
                Where(s => s.Section == filterBySection).
                OrderBy(s => s.Gender).ThenBy(s => s.LastName); 
            }
            

            var projectContext = _context.ProjectSettings.
                Where(p => p.PAppUserId == filterCurrentUserId);

            var viewModel = new StudentsProjectsViewModel
            {
                Students = studentsContext,
                ProjectSettings = projectContext,
                //AllSections = (IQueryable<AllSections>)allSections
            };


            //project Count App Students
            var projectCount = (_context.ProjectSettings.Where(p => p.PAppUserId == HttpContext.Session.GetString("AppUserID")).Count());
            TempData["projectcount"] = projectCount.ToString();

            activePage = "students";
            ViewBag.ActivePage = activePage;
            TempData["ariaActive"] = "active";
            TempData["AppUserName"] = HttpContext.Session.GetString("AppUserName");

         
            return View(viewModel);
               
        }// end of student

        [HttpGet]
        public async Task<IActionResult> ExportStudentsProject()
        {
            var content = await exportService.ExportProjectToExcelAsync();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{HttpContext.Session.GetString("ProjectName")}.xlsx");
        }

        public IActionResult DownloadAndClose()
        {
            return View();
        }
    }//end of class controller
}
