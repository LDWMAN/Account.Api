using System.ComponentModel.DataAnnotations;

namespace AccountApi.Model.Entity;

public class AdminUser : BaseEntity
{
    [Key]
    public Guid AdminId { get; set; } = Guid.NewGuid();
    public int AdminRole { get; set; }
    public string AdminUserName { get; set; }
    public string AdminName { get; set; }
    public string Email { get; set; }
    public string EmailConfirm { get; set; }
    public string PasswordHash { get; set; }
    public string Phone { get; set; }
    public string Tel { get; set; }
    public string Depart { get; set; }
    public int IsUsed { get; set; } = 1;
    public bool IsLogged { get; set; } = false;
}
