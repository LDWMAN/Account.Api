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


namespace AccountApi.Controllers.v1.USER;

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
        if (ModelState.IsValid)
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
                                "파일 에러가 발생하였습니다."
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
                            "회원가입에 성공하였습니다."
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
               "이미 존재하는 이메일 입니다."
            }
            });
        }
        return BadRequest(new ResultDtos()
        {
            Success = false,
            ResultMsg = new List<string>(){
               "The data model needs verification.",
               "데이터 모델이 잘못 되었습니다."
            }
        });
    }
    #endregion


    #region User Login
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        if (ModelState.IsValid)
        {
            var existUser = await _unitOfWork.User.GetByEmail(loginDto.Email);

            if (existUser == null)
            {
                return BadRequest(new ResultDtos()
                {
                    Success = false,
                    ResultMsg = new List<string>(){
                    "There is no registered email address.",
                    "등록된 이메일 주소가 없습니다."
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
                    "패스워드가 일치하지 않습니다."
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
                    "로그인에 성공하였습니다."
                }
            });
        }
        return BadRequest(new ResultDtos()
        {
            Success = false,
            ResultMsg = new List<string>(){
            "The data model needs verification.",
            "데이터 모델이 잘못 되었습니다."
        }
        });
    }
    #endregion


    #region Create RefreshToken
    [HttpPost]
    [Route("RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto RefreshTokenDto)
    {
        if (ModelState.IsValid)
        {
            var result = await VerifyToken(RefreshTokenDto);

            if (result == null)
            {
                return BadRequest(new ResultDtos()
                {
                    Success = false,
                    ResultMsg = new List<string>(){
                    "Token verification failed.",
                    "토큰 검증에 실패하였습니다."
                    }
                });
            }

            return Ok(result);
        }
        else
        {
            return BadRequest(new ResultDtos()
            {
                Success = false,
                ResultMsg = new List<string>(){
                    "The data model needs verification.",
                    "데이터 모델이 잘못 되었습니다."
                }
            });
        }
    }
    #endregion
}
