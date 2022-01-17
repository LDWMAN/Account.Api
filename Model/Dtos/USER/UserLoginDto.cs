using System.ComponentModel.DataAnnotations;

namespace AccountApi.Model.Dtos.USER
{
    public class UserLoginDto
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}