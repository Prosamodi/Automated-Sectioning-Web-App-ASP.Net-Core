namespace AutoSect.App.Models
{
    public interface IProjectSettingRepository
    {
        IQueryable<ProjectSetting> ProjectSettings { get; }

        Task<ProjectSetting> GetProjectByIdAsync(long id);
        Task AddProjectAsync(ProjectSetting p);
        Task DeleteProjectAsync(long id);
        Task UpdateProjectAsync(ProjectSetting p);
        Task SaveAsync();

    }
}
