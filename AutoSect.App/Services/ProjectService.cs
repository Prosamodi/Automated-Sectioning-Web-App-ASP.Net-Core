using AutoSect.App.Models;

namespace AutoSect.App.Services
{
    public class ProjectService
    {
        private IProjectSettingRepository projectRepository;

        public ProjectService(IProjectSettingRepository projRepo)
        {
            projectRepository = projRepo;
        }

        public async Task UpdateProjectSettingAsync(long projectId, ProjectSetting proj)
        {
            var project = await projectRepository.GetProjectByIdAsync(projectId);
            if (project != null)
            {
                project.MalesPerSection = proj.MalesPerSection;
                project.FemalesPerSection = proj.FemalesPerSection;
                project.TotalSection = proj.TotalSection;
                project.TotalStudent = proj.TotalStudent;
                project.Status = project.Status;

                await projectRepository.UpdateProjectAsync(project);
                await projectRepository.SaveAsync();
            }
        }

        public async Task UpdateProjectSettingStatusAndMethodAsync(long projectId, string status, string method)
        {
            var project = await projectRepository.GetProjectByIdAsync(projectId);
            if(project != null)
            {
                project.Status = status;
                project.Method = method;

                await projectRepository.UpdateProjectAsync(project);
                await projectRepository.SaveAsync();
            }
        }

        public async Task DeleteProjectById(long projectId)
        {
            var project = await projectRepository.GetProjectByIdAsync(projectId);
            if(project != null)
            {
                await projectRepository.DeleteProjectAsync(projectId); 
                await projectRepository.SaveAsync();
            }
        }


    }
}
