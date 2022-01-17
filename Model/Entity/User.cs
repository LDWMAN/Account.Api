using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountApi.Model.Entity;

public class User : BaseEntity
{
    [Key]
    public Guid UserId { get; set; } = Guid.NewGuid();
    public int UserRole { get; set; } = 0;
    public string UserName { get; set; }
    public string ProfileName { get; set; }
    public string Email { get; set; }
    public string EmailConfirm { get; set; }
    public string PasswordHash { get; set; }
    public string Phone { get; set; }
    public string Tel { get; set; }
    public string Address { get; set; }
    public string Company { get; set; }
    public string Country { get; set; }
    public string ProfileImage { get; set; }
    public bool IsLogged { get; set; } = false;
}
