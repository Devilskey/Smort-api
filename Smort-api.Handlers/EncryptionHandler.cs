using Smort_api.Object;
using System.Runtime.Intrinsics.Arm;
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
    }
}
