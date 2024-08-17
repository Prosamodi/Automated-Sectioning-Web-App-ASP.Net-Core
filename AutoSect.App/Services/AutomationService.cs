using AutoSect.App.Models;
using Microsoft.Identity.Client;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AutoSect.App.Services
{
    public class AutomationService
    {
        private IHttpContextAccessor httpContextAccessor;
        private AutoSectDbContext _context;
        private StudentsService studentService;
        private ProjectService projectService;

        public AutomationService(IHttpContextAccessor httpCtxAccessor, StudentsService studRepo, ProjectService projRepo, AutoSectDbContext ctx)
        {
            httpContextAccessor = httpCtxAccessor;
            studentService = studRepo;
            projectService = projRepo;
            _context = ctx;
        }

        public async Task AutomateRandomSectioning()
        {
            Random random = new Random();

            var session = httpContextAccessor.HttpContext.Session;

            var projectIdFilter = session.GetInt32("ProjectId");
            var projectCurrentUserIdFilter = session.GetString("AppUserID");

            //Resets All Sections to Value 0 / per Project
            var totalStudent = _context.Students
                .Where(s => s.AppUserID == projectCurrentUserIdFilter && s.ProjectId == projectIdFilter).Count();

            var resetAllSectionIds = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Select(s => s.StudentId).ToList();
            
            var resetSectionValue = 0;
                
            foreach (var studIds in resetAllSectionIds)
            {
                await studentService.UpdateStudentsSectionAsync((int)studIds, resetSectionValue);
            }

            ////Randomized Automated Sectioning Variables
            var totalSection = _context.ProjectSettings
                .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
                .Select(p => p.TotalSection).FirstOrDefault();

            var totalMaleStudent = _context.ProjectSettings
                .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
                .Select(p => p.MalesPerSection).FirstOrDefault();

            var totalFemaleStudent = _context.ProjectSettings
               .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
               .Select(p => p.FemalesPerSection).FirstOrDefault();


            //Male Random Sectioning
            for (int i = 0; i < totalSection; i++)
            {
                var sectionNum = i + 1;

                var allMaleStudentsIds = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Male")
                .Select(s => s.StudentId).ToList();

                var randomizedMaleIds = allMaleStudentsIds.OrderBy(x => random.Next()).Take((int)totalMaleStudent).ToList();

                
                foreach (var maleIds in randomizedMaleIds)
                {
                    await studentService.UpdateStudentsSectionAsync((int)maleIds, sectionNum);
                }
            }

            //Female Random Sectioning
            for (int i = 0; i < totalSection; i++)
            {
                var sectionNum = i + 1;

                var allFemaleStudentsIds = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Female")
                .Select(s => s.StudentId).ToList();

                var randomizedFemaleIds = allFemaleStudentsIds.OrderBy(x => random.Next()).Take((int)totalFemaleStudent).ToList();

                
                foreach (var femaleIds in randomizedFemaleIds)
                {
                    await studentService.UpdateStudentsSectionAsync((int)femaleIds, sectionNum);
                }
            }


            //Remainder ReSectioning Female from Last Section and Below
            var totalSectionFContext = _context.ProjectSettings
                .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
                .Select(p => p.TotalSection).FirstOrDefault();

            var allFemaleWithNoSection = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Female").Count();

            if (allFemaleWithNoSection > 0)
            {
                var AllFemaleStudentsIds = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Female")
                .Select(s => s.StudentId).ToList();

                foreach (var studFIds in AllFemaleStudentsIds)
                {
                    var sectionNumFemale = totalSectionFContext--;
                    await studentService.UpdateStudentsSectionAsync((int)studFIds, (int)sectionNumFemale);
                }

            }

            ////Remainder ReSectioning Male from Last Section and Below
            var totalSectionMContext = _context.ProjectSettings
                .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
                .Select(p => p.TotalSection).FirstOrDefault();

            var allMaleWithNoSection = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Male").Count();

            if (allMaleWithNoSection > 0)
            {
                var AllMaleStudentsIds = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Male")
                .Select(s => s.StudentId).ToList();

                foreach (var studMIds in AllMaleStudentsIds)
                {
                    var sectionNumMale = totalSectionMContext--;
                    await studentService.UpdateStudentsSectionAsync((int)studMIds, (int)sectionNumMale);
                }
            }

            await projectService.UpdateProjectSettingStatusAndMethodAsync((long)projectIdFilter, "Complete", "Randomized");

        }//end of RandomSectioning






        public async Task AutomateSectioningByGrades()
        {
            var session = httpContextAccessor.HttpContext.Session;

            var projectIdFilter = session.GetInt32("ProjectId");
            var projectCurrentUserIdFilter = session.GetString("AppUserID");

            //Resets All Sections to Value 0 / per Project
            var totalStudent = _context.Students
                .Where(s => s.AppUserID == projectCurrentUserIdFilter && s.ProjectId == projectIdFilter).Count();

            var resetAllSectionIds = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Select(s => s.StudentId).ToList();

            var resetSectionValue = 0;

            foreach (var studIds in resetAllSectionIds)
            {
                await studentService.UpdateStudentsSectionAsync((int)studIds, resetSectionValue);
            }

            // Automated Sectioning By Grades Variables
            var totalSection = _context.ProjectSettings
                .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
                .Select(p => p.TotalSection).FirstOrDefault();

            var totalMaleStudent = _context.ProjectSettings
                .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
                .Select(p => p.MalesPerSection).FirstOrDefault();

            var totalFemaleStudent = _context.ProjectSettings
               .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
               .Select(p => p.FemalesPerSection).FirstOrDefault();

            //Male Sectioning by Grades
            for (int i = 0; i < totalSection; i++)
            {
                var sectionNum = i + 1;

                var allMaleStudents = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Male");

                var maleStudentIds = allMaleStudents.OrderByDescending(x => x.GWA).Take((int)totalMaleStudent).Select(x => x.StudentId).ToList();

                foreach (var maleIds in maleStudentIds)
                {
                    await studentService.UpdateStudentsSectionAsync((int)maleIds, sectionNum);
                }
            }

            //Female Sectioning by Grades
            for (int i = 0; i < totalSection; i++)
            {
                var sectionNum = i + 1;

                var allFemaleStudents = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Female");

                var femaleStudentIds = allFemaleStudents.OrderByDescending(x => x.GWA).Take((int)totalFemaleStudent).Select(x => x.StudentId).ToList();

                foreach (var femaleIds in femaleStudentIds)
                {
                    await studentService.UpdateStudentsSectionAsync((int)femaleIds, sectionNum);
                }
            }

            ////Remainder ReSectioning Male from Last Section and Below
            var totalSectionMContext = _context.ProjectSettings
                .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
                .Select(p => p.TotalSection).FirstOrDefault();

            var allMaleWithNoSection = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Male").Count();


            if (allMaleWithNoSection > 0)
            {
                var AllMaleStudentsIds = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Male")
                .Select(s => s.StudentId).ToList();

                foreach (var studMIds in AllMaleStudentsIds)
                {
                    var sectionNumMale = totalSectionMContext--;
                    await studentService.UpdateStudentsSectionAsync((int)studMIds, (int)sectionNumMale);
                }
            }


            //Remainder ReSectioning Female from Last Section and Below
            var totalSectionFContext = _context.ProjectSettings
                .Where(p => p.PAppUserId == projectCurrentUserIdFilter && p.ProjectId == projectIdFilter)
                .Select(p => p.TotalSection).FirstOrDefault();

            var allFemaleWithNoSection = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Female").Count();

            if (allFemaleWithNoSection > 0)
            {
                var AllFemaleStudentsIds = _context.Students
                .Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter)
                .Where(s => s.Section == 0 && s.Gender == "Female")
                .Select(s => s.StudentId).ToList();

                foreach (var studFIds in AllFemaleStudentsIds)
                {
                    var sectionNumFemale = totalSectionFContext--;
                    await studentService.UpdateStudentsSectionAsync((int)studFIds, (int)sectionNumFemale);
                }

            }

            await projectService.UpdateProjectSettingStatusAndMethodAsync((long)projectIdFilter, "Complete", "By Grades (GWA)");

        }//End of ByGradesSectioning 



    }
}
