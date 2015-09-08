using System;
using System.Security.Cryptography;
using System.Text;

namespace GitGameServer
{
    public static class HashHelper
    {
        public static string GetMD5(string input, bool addTime = true)
        {
            return GetMD5(Encoding.UTF8.GetBytes(input), addTime);
        }
        public static string GetMD5(byte[] bytes, bool addTime = true)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                if (addTime)
                {
                    byte[] b = new byte[bytes.Length + 8];
                    bytes.CopyTo(b, 0);
                    BitConverter.GetBytes(DateTime.UtcNow.Ticks).CopyTo(b, b.Length - 8);
                    bytes = b;
                }

                bytes = md5Hash.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                    sb.Append(bytes[i].ToString("x2"));

                return sb.ToString();
            }
        }
    }
}