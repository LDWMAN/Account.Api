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
                        "JWT ????????? ?????? ???????????? ???????????????."
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
                        "???????????? ?????? ???????????? ?????? ?????????."
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
                        "???????????? ????????? ?????? ???????????????."
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
                        "???????????? ????????? ???????????? ??? ????????????."
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
                        "????????? ?????? ?????????."
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
                        "???????????? ????????? ????????? JWT ID??? ????????????. (JTI Unmatched)"
                    }
                    };
                }

                //?????? ??????
                var dbUser = await _unitOfWork.User.GetById(Guid.Parse(refreshTokenExist.UserId));

                if (dbUser == null)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ResultMsg = new List<string>()
                    {
                        "The user ID using the token is not searched.",
                        "?????? ????????? ???????????? ??????ID??? ???????????? ????????????."
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
                    "??????????????? ????????? ?????????????????????."
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
                   "????????? ????????? ????????? ???????????????."
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