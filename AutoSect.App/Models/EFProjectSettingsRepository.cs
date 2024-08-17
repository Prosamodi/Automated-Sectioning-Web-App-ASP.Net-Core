

namespace AutoSect.App.Models
{
    public class EFProjectSettingsRepository : IProjectSettingRepository
    {
        private AutoSectDbContext context;

        public EFProjectSettingsRepository(AutoSectDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<ProjectSetting> ProjectSettings => context.ProjectSettings;

        public async Task AddProjectAsync(ProjectSetting p)
        {
            await context.ProjectSettings.AddAsync(p);
        }

        public async Task DeleteProjectAsync(long id)
        {
            var project = await context.ProjectSettings.FindAsync(id);
            if(project != null)
            {
                context.ProjectSettings.Remove(project);
            }
        }

        public async Task<ProjectSetting> GetProjectByIdAsync(long id)
        {
            return await context.ProjectSettings.FindAsync(id);
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(ProjectSetting p)
        {
            context.ProjectSettings.Update(p);
        }
    }
}
