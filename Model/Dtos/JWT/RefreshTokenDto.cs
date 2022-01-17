using System.ComponentModel.DataAnnotations;

namespace AccountApi.Model.Dtos.JWT
{
    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}