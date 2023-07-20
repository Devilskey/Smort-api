using Smort_api.Object;
using System.Security.Cryptography;
using System.Text;

namespace Smort_api.Handlers
{
    public static class EncryptionHandler
    {
        public static bool VerifyData(PasswordObject PasswordData, string Passwordreceived)
        {
            using (SHA512 sha = SHA512.Create())
            {
                byte[] dataByteArray = Encoding.Default.GetBytes($"{Passwordreceived}/{PasswordData.Salt}");

                byte[] hashedData = sha.ComputeHash(dataByteArray);

                string DataHashedNew = Convert.ToBase64String(hashedData);

                if (DataHashedNew == PasswordData.Password)
                {
                    return true;
                }
                return false;
            }
        }

        public static string[] HashAndSaltData(string Data)
        {

            string[] SaltAndHash = { "", "" };

            using (SHA512 sha = SHA512.Create())
            {
                RandomNumberGenerator randomNumber = RandomNumberGenerator.Create();

                byte[] salt = new byte[32];

                randomNumber.GetBytes(salt);

                SaltAndHash[0] = Convert.ToBase64String(salt);

                byte[] dataByteArray = Encoding.Default.GetBytes($"{Data}/{SaltAndHash[0]}");

                byte[] hashedData = sha.ComputeHash(dataByteArray);

                SaltAndHash[1] = Convert.ToBase64String(hashedData);

                return SaltAndHash;
            }
        }

        public static AESObject EncryptAES(string data)
        {
            AESObject AESEncrypted = new AESObject();

            using (Aes aes = Aes.Create())
            {
                AESEncrypted.Iv = Convert.ToBase64String(aes.IV);
                AESEncrypted.Key = Convert.ToBase64String(aes.Key);

                ICryptoTransform encryptor = aes.CreateEncryptor();

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(data);
                        }
                        AESEncrypted.CipherText = Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }

            return AESEncrypted;
        }
        public static string DeCrypteAES(AESObject AesObject)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(AesObject.Key);
                aes.IV = Encoding.UTF8.GetBytes(AesObject.Iv);
                byte[] encryptedText = Encoding.UTF8.GetBytes(AesObject.CipherText);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
