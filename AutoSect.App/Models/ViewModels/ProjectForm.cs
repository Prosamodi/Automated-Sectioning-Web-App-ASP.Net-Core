namespace AutoSect.App.Models.ViewModels
{
    public class ProjectForm
    {
        public  string ProjectName { get; set; } = string.Empty;
        public  float TotalSection { get; set; } = 0;
        public int ProjectId { get; set; } = 0;
        public string FromDashboardOrStudent { get; set;} = string.Empty;
    }
}
