using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AccountApi.Util;

public static class CommonUtil
{
    private static Random random = new Random();

    #region Random String
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    #endregion

    #region Random Number
    public static string RandomNumber(int length)
    {
        const string chars = "0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    #endregion

    #region Verify Time Stamp 
    public static DateTime UnixTimeStampToDateTime(long unixDate)
    {
        //Check Utc, Local (Importnt! : DateTimeKind)
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixDate).ToLocalTime();
        return dateTime;
    }
    #endregion 
}