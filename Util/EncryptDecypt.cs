using System.Security.Cryptography;
using System.Text;

namespace AccountApi.Util;

public static class EncryptDecypt
{

    public static string SHA256Hash(string Data)
    {
        var bytes = Encoding.Unicode.GetBytes(Data);
        using (var hashEngine = SHA256.Create())
        {
            var hashedBytes = hashEngine.ComputeHash(bytes, 0, bytes.Length);

            var sb = new StringBuilder();
            
            foreach (var b in hashedBytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            
            return sb.ToString();
        }
    }

}