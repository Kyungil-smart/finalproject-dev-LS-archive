using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utils
{

    public static class Encrypter
    {
        // 암호화 키
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // length:32

        // JSON <--> 암호화 문자열
        public static string Encrypt(string text)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(text);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string text)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                byte[] bytes = Convert.FromBase64String(text);

                if (bytes.Length < 16)
                {
                    throw new Exception($"Invalid File, length : {bytes.Length}");
                }

                byte[] iv = new byte[16];
                Array.Copy(bytes, 0, iv, 0, 16);
                aes.IV = iv;

                int payloadLength = bytes.Length - 16;
                byte[] payload = new byte[payloadLength];
                Array.Copy(bytes, 16, payload, 0, payloadLength);

                using (MemoryStream ms = new MemoryStream(payload))
                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }

            }
        }
    }
}