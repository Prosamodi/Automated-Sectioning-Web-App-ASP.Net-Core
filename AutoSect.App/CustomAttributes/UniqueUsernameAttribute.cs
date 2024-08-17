using AutoSect.App.Models;
using System.ComponentModel.DataAnnotations;


    public class UniqueUsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value != null)
            {
                var dbContext = (AppIdentityDbContext)validationContext.GetService(typeof(AppIdentityDbContext));
                var username = value.ToString();

                bool userExists = dbContext.Users.Any(u => u.UserName == username);

                if (userExists)
                {
                    return new ValidationResult("Username already exists.");
                }
            }

            return ValidationResult.Success;
        }
    }

