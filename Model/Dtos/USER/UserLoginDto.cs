using System.ComponentModel.DataAnnotations;

namespace AccountApi.Model.Dtos.USER
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Password { get; set; }
    }
}