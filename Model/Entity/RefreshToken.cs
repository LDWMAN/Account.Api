using System.ComponentModel.DataAnnotations;

namespace AccountApi.Model.Entity;

public class RefreshToken : BaseEntity
{
    [Key]
    public Guid RefreshTokenId { get; set; }
    public string UserId { get; set; }
    public string Token { get; set; }
    public string JwtId { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime ExpriryDate { get; set; }
}
