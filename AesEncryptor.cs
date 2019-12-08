using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Passtable
{
    class AesEncryptor
    {
        //The special sequence to padding instead of zero-sequence
        private static char[] keyPadding = new char[]
        {'1', 'a', '3', 'b', '5', 'c', '7', 'd', '9', 'e', '0', 'f', '2', 'g', '4' };

        public static string Encryption(string data, string password)
        {
            //Initialization AES
            Aes aes = Aes.Create();
            //Working with the key(password): Select mode, padding the key to the required number of characters
            int idKeyPadding = 0;
            if (password.Length > 32 || password.Length == 0) throw
                    new Exception("The password contains 0 or more than 32 characters");
            if (password.Length <= 16)
            {
                while (password.Length < 16) { password += keyPadding[idKeyPadding]; idKeyPadding++; }
                aes.KeySize = 128;
            }
            else if (password.Length > 16 && password.Length <= 24)
            {
                while (password.Length < 24) { password += keyPadding[idKeyPadding]; idKeyPadding++; }
                aes.KeySize = 192;
            }
            else
            {
                while (password.Length < 32) { password += keyPadding[idKeyPadding]; idKeyPadding++; }
            }
            aes.Key = Encoding.UTF8.GetBytes(password);
            //AES setup
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            //Encryption process
            byte[] dataEncrypted;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(data);
                    }
                }
                dataEncrypted = ms.ToArray();
            }
            //Output the result
            string strResult = Convert.ToBase64String(dataEncrypted.Concat(aes.IV).ToArray());
            aes.Dispose();
            return strResult;
        }

        public static string Decryption(string message, string password)
        {
            //Initialization AES
            Aes aes = Aes.Create();
            //Working with the key(password): Select mode, padding the key to the required number of characters
            int idKeyPadding = 0;
            if (password.Length > 32 || password.Length == 0) throw
                    new Exception("The password contains 0 or more than 32 characters");
            if (password.Length <= 16)
            {
                while (password.Length < 16) { password += keyPadding[idKeyPadding]; idKeyPadding++; }
                aes.KeySize = 128;
            }
            else if (password.Length > 16 && password.Length <= 24)
            {
                while (password.Length < 24) { password += keyPadding[idKeyPadding]; idKeyPadding++; }
                aes.KeySize = 192;
            }
            else
            {
                while (password.Length < 32) { password += keyPadding[idKeyPadding]; idKeyPadding++; }
            }
            aes.Key = Encoding.UTF8.GetBytes(password);
            //AES setup
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            //Working with the encrypted message: separation of IV (initialization vector) from main data
            byte[] msgEncrypted = Convert.FromBase64String(message);
            byte[] dataEncrypted = new byte[msgEncrypted.Length - 16];
            byte[] iv = new byte[16];
            for (int i = 0; i < msgEncrypted.Length - 16; i++) dataEncrypted[i] = msgEncrypted[i];
            for (int i = msgEncrypted.Length - 16, j = 0; i < msgEncrypted.Length; i++, j++) iv[j] = msgEncrypted[i];
            aes.IV = iv;
            //Encryption process
            string strResult = "";
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            try
            {
                using (MemoryStream ms = new MemoryStream(dataEncrypted))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            strResult = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                strResult = "/error";
            }
            //Output the result
            aes.Dispose();
            return strResult;
        }
    }
}
