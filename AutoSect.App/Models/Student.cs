using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSect.App.Models
{
    public class Student
    {
        [Key]
        public long? StudentId { get; set; }
        public string FirstName { get; set; } = String.Empty;
        public string MiddleName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string Gender { get; set; } = String.Empty;
        public int Section { get; set; } = 0;
        public long ProjectId { get; set; }
        public ProjectSetting? Project { get; set; } 
        public string AppUserID { get; set; }
        //[Range(typeof(long), "12", "12")]
        public long LRN { get; set; } = 0;
        //[Column(TypeName = "decimal(8,2)")]
        public float GWA { get; set; } = 0;
        public int Grade { get; set; }  = 0;
    }
}
