using AccountApi.UnitOfWork;
using Microsoft.AspNetCore.Mvc;


namespace AccountApi.Controllers.v1.COMMON;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1")]
public class BaseController : ControllerBase
{
    public IWebHostEnvironment _webHostEnvironmentl;
    public IUnitOfWork _unitOfWork;


    public BaseController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironmentl = webHostEnvironment;
    }

    #region SingleFileUpload
    protected async Task<bool> SingleFileUpload(IFormFile file, string path, string newFileName)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (FileStream fs = System.IO.File.Create(Path.Combine(path, newFileName)))
            {
                await file.CopyToAsync(fs);
                fs.Flush();
            }
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    #endregion
}
