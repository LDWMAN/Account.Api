using AccountApi.Model.Dtos.COMMON;

namespace AccountApi.Model.Dtos.JWT
{
    public class AuthResultDto : ResultDtos
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}