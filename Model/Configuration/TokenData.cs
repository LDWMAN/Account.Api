using System.ComponentModel.DataAnnotations;

namespace AccountApi.Model.Configuration;

public class TokenData
{
    [Required]
    public string JwtToken { get; set; }

    [Required]
    public string RefreshToken { get; set; }
}

