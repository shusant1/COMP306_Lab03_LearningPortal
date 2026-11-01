
using System.ComponentModel.DataAnnotations;

namespace COMP306_Lab03_LearningPortal.Models
{
    public class LoginViewModel
    {
        [Required]
        public string UsernameOrEmail { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
