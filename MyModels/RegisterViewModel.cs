using System.ComponentModel.DataAnnotations;

namespace MyModels
{
    public class RegisterViewModel
    {
            [Required]
            public string Username { get; set; }

            [Required, EmailAddress]
            public string EmailAddress { get; set; }

           [Required]
           [StringLength(100, MinimumLength = 6, ErrorMessage = "Password should be a minimum of 6 characters.")]
           [DataType(DataType.Password)]
            public string Password { get; set; }


            [Required, Compare("Password", ErrorMessage = "Passwords do not match.")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; }
        }

    
}
