using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace HisaCat
{
    public static class HashUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetMD5Hash(string text)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                byte[] hash = md5.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }
    }
}
