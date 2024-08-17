using System.ComponentModel.DataAnnotations;

namespace AutoSect.App.Models.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Username is Required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is Required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        //public string ReturnUrl { get; set; } = "/";
    }
}
