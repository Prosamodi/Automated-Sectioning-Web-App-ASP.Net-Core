using System.ComponentModel.DataAnnotations;

namespace AutoSect.App.Models
{
    public class ProjectSetting
    {
        [Key]
        public long ProjectId { get; set; }
        
        public string ProjectName { get; set; } = string.Empty;

        public float TotalSection { get; set; } = 0;
        public float TotalStudent { get; set; } = 0;
        public float MalesPerSection { get; set; } = 0;
        public float FemalesPerSection { get; set; } = 0;
        public string Status { get; set; } = "Incomplete";
        public string Method { get; set; } = string.Empty;
        public string PAppUserId { get; set; }

        public IEnumerable<Student>? Students {get; set;} 
    }
}
