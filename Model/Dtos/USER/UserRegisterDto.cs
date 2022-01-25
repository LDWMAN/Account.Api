using System.ComponentModel.DataAnnotations;

namespace AccountApi.Model.Dtos.USER;

public class UserRegisterDto
{
    [Required(ErrorMessage = "Required")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Required")]
    public string ProfileName { get; set; }

    [Required(ErrorMessage = "Required")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Required")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Required")]
    public string Phone { get; set; }

    public string Tel { get; set; }

    [Required(ErrorMessage = "Required")]
    public string Address { get; set; }

    public string Company { get; set; }

    [Required(ErrorMessage = "Required")]
    public string Country { get; set; }

    public IFormFile ProfileImage { get; set; }
}
