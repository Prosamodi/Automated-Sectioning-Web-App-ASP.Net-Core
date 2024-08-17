using Microsoft.EntityFrameworkCore;

namespace AutoSect.App.Models
{
    public class AutoSectDbContext : DbContext
    {
        public AutoSectDbContext(DbContextOptions<AutoSectDbContext> options) 
            : base(options) { }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<ProjectSetting> ProjectSettings => Set<ProjectSetting>();

    }
}
