using AccountApi.Controllers.v1.COMMON;
using AccountApi.Model.Dtos.COMMON;
using AccountApi.Model.Dtos.USER;
using AccountApi.Model.Entity;
using AccountApi.UnitOfWork;
using AccountApi.Util;
using Microsoft.AspNetCore.Mvc;
using AccountApi.Model.Dtos.JWT;
using Microsoft.IdentityModel.Tokens;
using AccountApi.Model.Configuration;
using Microsoft.Extensions.Options;
using Account.API.Filter;

namespace AccountApi.Controllers.v1.USER;


[ValidateModel]
public class AccountController : BaseTokenController
{
    public AccountController(IUnitOfWork unitOfWork,
        IWebHostEnvironment webHostEnvironment,
        TokenValidationParameters tokenValidationParameters,
        IOptionsMonitor<JwtConfig> optionMonitor)
        : base(unitOfWork, webHostEnvironment, tokenValidationParameters, optionMonitor)
    {

    }

    #region Register User
    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register([FromForm] UserRegisterDto RegisterDto)
    {
        var existUser = await _unitOfWork.User.GetByEmail(RegisterDto.Email);

        if (existUser == null)
        {
            try
            {
                var newUser = new User()
                {
                    UserName = RegisterDto.UserName,
                    ProfileName = RegisterDto.ProfileName,
                    Email = RegisterDto.Email,
                    Phone = RegisterDto.Phone,
                    Tel = RegisterDto.Tel,
                    Address = RegisterDto.Address,
                    Company = RegisterDto.Company,
                    Country = RegisterDto.Country,
                    IsLogged = true,
                    PasswordHash = EncryptDecypt.SHA256Hash(RegisterDto.Password)
                };

                if (RegisterDto.ProfileImage != null && RegisterDto.ProfileImage.Length > 0)
                {
                    string wwwroot = _webHostEnvironmentl.WebRootPath;
                    string path = wwwroot + FileUtil.PathProfiles;
                    string newFileName = FileUtil.GetNewFileName(RegisterDto.ProfileImage);

                    var fileSucess = await SingleFileUpload(RegisterDto.ProfileImage, path, newFileName);

                    if (!fileSucess)
                    {
                        return BadRequest(new ResultDtos()
                        {
                            Success = false,
                            ResultMsg = new List<string>(){
                                "A file error has occurred.",
                                "?????? ????????? ?????????????????????."
                            }
                        });

                    }
                    newUser.ProfileImage = newFileName;
                }

                var result = await _unitOfWork.User.Add(newUser);

                // Create Token
                var jwtToken = await GenerateJwtToken(newUser);

                return Ok(new AuthResultDto()
                {
                    Success = true,
                    Token = jwtToken.JwtToken,
                    RefreshToken = jwtToken.RefreshToken,
                    ResultMsg = new List<string>(){
                            "You have succeeded in signing up as a member.",
                            "??????????????? ?????????????????????."
                        }
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new ResultDtos()
                {
                    Success = false,
                    ResultMsg = new List<string>(){
                            ex.Message
                        }
                });
            }
        }
        return BadRequest(new ResultDtos()
        {
            Success = false,
            ResultMsg = new List<string>(){
                "This email already exists.",
               "?????? ???????????? ????????? ?????????."
            }
        });
    }
    #endregion


    #region User Login
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        var existUser = await _unitOfWork.User.GetByEmail(loginDto.Email);

        if (existUser == null)
        {
            return BadRequest(new ResultDtos()
            {
                Success = false,
                ResultMsg = new List<string>(){
                    "There is no registered email address.",
                    "????????? ????????? ????????? ????????????."
                }
            });
        }

        var password = EncryptDecypt.SHA256Hash(loginDto.Password);

        if (!existUser.PasswordHash.Equals(password))
        {
            return BadRequest(new ResultDtos()
            {
                Success = false,
                ResultMsg = new List<string>(){
                    "The passwords do not match.",
                    "??????????????? ???????????? ????????????."
                }
            });
        }

        var jwtToken = await GenerateJwtToken(existUser);

        return Ok(new AuthResultDto()
        {
            Success = true,
            Token = jwtToken.JwtToken,
            RefreshToken = jwtToken.RefreshToken,
            ResultMsg = new List<string>(){
                    "You have successfully logged in.",
                    "???????????? ?????????????????????."
                }
        });
    }
    #endregion


    #region Create RefreshToken
    [HttpPost]
    [Route("RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto RefreshTokenDto)
    {
        var result = await VerifyToken(RefreshTokenDto);

        if (result == null)
        {
            return BadRequest(new ResultDtos()
            {
                Success = false,
                ResultMsg = new List<string>(){
                    "Token verification failed.",
                    "?????? ????????? ?????????????????????."
                    }
            });
        }

        return Ok(result);
    }
    #endregion
}
