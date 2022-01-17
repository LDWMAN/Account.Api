namespace AccountApi.Util;

public static class FileUtil
{
    public static string PathUploads { get; } = @"\Uploads";
    public static string PathProfiles { get; } = @"\Uploads\Profiles\";
    // public static string PathProfiles { get; } = @"\Uploads\Profiles\"; // + DateTime.Now.Month.ToString();
    public static string PathContents { get; } = @"\Uploads\Contents\";
    //public static string PathContents { get; } = @"\Uploads\Contents\"; // + DateTime.Now.Month.ToString();
    public static string GetNewFileName(IFormFile file)
    {
        return DateTime.Now.ToString("MM/dd/yyyy") + CommonUtil.RandomString(6)
         + System.IO.Path.GetExtension(file.FileName).Trim();
    }

}
