using System.ComponentModel.DataAnnotations;

namespace AccountApi.Model.Dtos.USER;

public class UserRegisterDto
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public string ProfileName { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
    
    [Required]
    public string Phone { get; set; }
    
    public string Tel { get; set; }
    
    [Required]
    public string Address { get; set; }
    
    public string Company { get; set; }
    
    [Required]
    public string Country { get; set; }
    
    public IFormFile ProfileImage { get; set; }
}
