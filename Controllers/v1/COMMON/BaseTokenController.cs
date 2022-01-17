using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AccountApi.Model.Configuration;
using AccountApi.Model.Dtos.JWT;
using AccountApi.Model.Entity;
using AccountApi.UnitOfWork;
using AccountApi.Util;

namespace AccountApi.Controllers.v1.COMMON
{
    public class BaseTokenController : BaseController
    {
        protected readonly TokenValidationParameters _tokenValidationParameters;
        protected readonly JwtConfig _jwtConfig;

        public BaseTokenController(IUnitOfWork unitOfWork,
        IWebHostEnvironment webHostEnvironment,
        TokenValidationParameters tokenValidationParameters,
        IOptionsMonitor<JwtConfig> optionMonitor) : base(unitOfWork, webHostEnvironment)
        {
            _jwtConfig = optionMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
        }

        #region GenerateJwtToken Method
        protected async Task<TokenData> GenerateJwtToken(User newUser)
        {
            var jwtHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Id", newUser.UserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, newUser.Email),
                new Claim(JwtRegisteredClaimNames.Sub, newUser.UserName),
                new Claim(JwtRegisteredClaimNames.Email, newUser.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
                Expires = DateTime.Now.AddHours(_jwtConfig.ExpriryTimeFrame),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = jwtHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                Token = $"{CommonUtil.RandomString(25)}_{Guid.NewGuid()}",
                UserId = newUser.UserId.ToString(),
                IsRevoked = false,
                IsUsed = false,
                JwtId = token.Id,
                ExpriryDate = DateTime.Now.AddHours(_jwtConfig.RefreshExpriryDate)
            };

            var usedToken = await _unitOfWork.RefreshToken.GetRefreshTokenbyUserId(refreshToken.UserId);

            foreach (var t in usedToken)
            {
                await _unitOfWork.RefreshToken.MakeRefreshTokenAsUsed(t);
            }

            await _unitOfWork.RefreshToken.Add(refreshToken);


            var tokenData = new TokenData()
            {
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token
            };

            return tokenData;

        }
        #endregion

        #region VerifyToken 
        protected async Task<AuthResultDto> VerifyToken(RefreshTokenDto refreshTokenDto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principla = tokenHandler.ValidateToken(refreshTokenDto.Token, _tokenValidationParameters, out var validateToken);

                if (validateToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)
                    {
                        return null;
                    }
                }


                var utcExpiryDate = long.Parse(principla.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expDate = CommonUtil.UnixTimeStampToDateTime(utcExpiryDate);

                if (expDate > DateTime.Now)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ResultMsg = new List<string>()
                    {
                        "The Jwt token has not expired yet.",
                        "JWT 토큰이 아직 만료되지 않았습니다."
                    }
                    };
                }

                var refreshTokenExist = await _unitOfWork.RefreshToken.GetByRefreshToken(refreshTokenDto.RefreshToken);

                if (refreshTokenExist == null)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ResultMsg = new List<string>()
                    {
                        "Unregistered refresh token.",
                        "등록되지 않은 리프레시 토큰 입니다."
                    }
                    };
                }

                if (refreshTokenExist.ExpriryDate < DateTime.Now)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ResultMsg = new List<string>()
                    {
                        "Expired refresh token.",
                        "리프레시 토큰이 만료 되었습니다."
                    }
                    };
                }

                if (refreshTokenExist.IsUsed)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ResultMsg = new List<string>()
                    {
                        "The refresh token cannot be reused.",
                        "리프레시 토큰은 재사용할 수 없습니다."
                    }
                    };
                }

                if (refreshTokenExist.IsRevoked)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ResultMsg = new List<string>()
                    {
                        "It's a canceled token.",
                        "취소된 토큰 입니다."
                    }
                    };
                }

                var jti = principla.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;


                if (refreshTokenExist.JwtId != jti)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ResultMsg = new List<string>()
                    {
                        "The JWT ID included in the refresh token is different.",
                        "리프레시 토큰에 포함된 JWT ID가 다릅니다. (JTI Unmatched)"
                    }
                    };
                }

                //유저 검증
                var dbUser = await _unitOfWork.User.GetById(Guid.Parse(refreshTokenExist.UserId));

                if (dbUser == null)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ResultMsg = new List<string>()
                    {
                        "The user ID using the token is not searched.",
                        "해당 토큰을 사용하는 유저ID가 검색되지 않습니다."
                    }
                    };
                }

                var tokens = await GenerateJwtToken(dbUser);

                //Remove Used Token
                await RemoveUsedToken(dbUser);

                return new AuthResultDto()
                {
                    Token = tokens.JwtToken,
                    RefreshToken = tokens.RefreshToken,
                    Success = true,
                    ResultMsg = new List<string>()
                {
                    "Tokens were issued normally.",
                    "정상적으로 토큰이 발행되었습니다."
                }
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDto()
                {
                    Success = false,
                    ResultMsg = new List<string>()
                {
                   ex.Message,
                   "The data model needs verification.",
                   "데이터 모델이 검증이 필요합니다."
                }
                };
            }
        }
        #endregion

        #region 
        private async Task RemoveUsedToken(User dbUser)
        {
            var userTokens = await _unitOfWork.RefreshToken.GetRefreshTokensAsUsed(dbUser.UserId.ToString());
            if (userTokens.Count() > 0)
            {
                foreach (var t in userTokens)
                {
                    await _unitOfWork.RefreshToken.RemoveRefreshToken(t.RefreshTokenId.ToString());
                }
            }
        }
        #endregion

    }
}