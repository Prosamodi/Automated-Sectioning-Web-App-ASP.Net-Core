using AutoSect.App.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AutoSect.App.Services
{
    public class ExportExcelService
    {
        private AutoSectDbContext _context;
        private IHttpContextAccessor httpContextAccessor;

        public ExportExcelService(AutoSectDbContext context, IHttpContextAccessor httpCtxAccessor)
        {
            _context = context;
            httpContextAccessor = httpCtxAccessor;
        }

        public async Task<byte[]> ExportProjectToExcelAsync()
        {
            var session = httpContextAccessor.HttpContext.Session;

            var projectIdFilter = session.GetInt32("ProjectId");
            var projectNameFilter = session.GetString("ProjectName");
            var projectCurrentUserIdFilter = session.GetString("AppUserID");

            var currentProject = await _context.Students.
                Where(s => s.ProjectId == projectIdFilter && s.AppUserID == projectCurrentUserIdFilter).
                OrderBy(s => s.Section).ThenBy(s => s.Gender).ThenBy(s => s.LastName).ToListAsync();

            var currentProjectNumberOfSections = await _context.ProjectSettings.
                Where(p => p.ProjectId == projectIdFilter && p.PAppUserId == projectCurrentUserIdFilter).
                Select(p => p.TotalSection).FirstOrDefaultAsync();

            var currentProjectMethod = await _context.ProjectSettings.
                Where(p => p.ProjectId == projectIdFilter && p.PAppUserId == projectCurrentUserIdFilter).
                Select(p => p.Method).FirstOrDefaultAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add($"{projectNameFilter}({currentProjectMethod})");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "STUDENT NAME";
                worksheet.Cell(currentRow, 2).Value = "GENDER";
                worksheet.Cell(currentRow, 3).Value = "SECTION";
                worksheet.Cell(currentRow, 4).Value = "GWA";

                worksheet.Cell("A1").Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.AshGrey);

                worksheet.Cell("B1").Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.AshGrey);

                worksheet.Cell("C1").Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.AshGrey);

                worksheet.Cell("D1").Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.AshGrey);


                foreach (var students in currentProject)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = $"{students.LastName}, {students.FirstName} {students.MiddleName}";
                    worksheet.Cell(currentRow, 2).Value = students.Gender;
                    worksheet.Cell(currentRow, 3).Value = students.Section;
                    worksheet.Cell(currentRow, 4).Value = students.GWA;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
