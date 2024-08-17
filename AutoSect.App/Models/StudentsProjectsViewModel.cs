namespace AutoSect.App.Models
{
    public class StudentsProjectsViewModel
    {
        public IQueryable<Student> Students { get; set; }
        public IQueryable<ProjectSetting> ProjectSettings { get; set; }
        //public IQueryable<AllSections> AllSections { get; set; }
       
    }
}
