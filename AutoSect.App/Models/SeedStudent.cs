using Microsoft.EntityFrameworkCore;

namespace AutoSect.App.Models
{
    public class SeedStudent
    {
        public static void EnsurePopulated(AutoSectDbContext context)
        {
            /*AutoSectDbContext context = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<AutoSectDbContext>();*/

            context.Database.Migrate();

            if (context.Students.Count() == 0 && context.ProjectSettings.Count() == 0)
            {
                ProjectSetting s1 = new ProjectSetting()
                {
                    ProjectName = "Grade 2",
                    PAppUserId = "jflkdsajgl"
                };
                context.Students.AddRange(
                    new Student
                    {
                        FirstName = "Sammy",
                        MiddleName = "Vicente",
                        LastName = "Odi",
                        LRN = 123456789102,
                        Gender = "Male",
                        GWA = 85,
                        Grade = 0,
                        Section = 0,
                        AppUserID = "jflkdsajgl",
                        Project = s1
                    },
                    new Student
                    {
                        FirstName = "Angelica",
                        MiddleName = "Torres",
                        LastName = "Danila",
                        LRN = 123456789103,
                        Gender = "Female",
                        GWA = 88,
                        Grade = 0,
                        Section = 0,
                        AppUserID = "jflkdsajgl",
                        Project = s1
                    },
                    new Student
                    {
                        FirstName = "Brandon",
                        MiddleName = "Odi",
                        LastName = "Palagar",
                        LRN = 123456789104,
                        Gender = "Male",
                        GWA = 85,
                        Grade = 0,
                        Section = 0,
                        AppUserID = "jflkdsajgl",
                        Project = s1
                    },
                    new Student
                    {
                        FirstName = "Maria Leona",
                        MiddleName = "Guttieres",
                        LastName = "Gonzales",
                        LRN = 123456789105,
                        Gender = "Female",
                        GWA = 85,
                        Grade = 0,
                        Section = 0,
                        AppUserID = "jflkdsajgl",
                        Project = s1
                    },
                    new Student
                    {
                        FirstName = "Heavenly Aedan",
                        MiddleName = "Odi",
                        LastName = "Escueta",
                        LRN = 123456789106,
                        Gender = "Male",
                        GWA = 85,
                        Grade = 0,
                        Section = 0,
                        AppUserID = "jflkdsajgl",
                        Project = s1
                    }
                 );
                context.SaveChanges();
            }
        }
    }
}
