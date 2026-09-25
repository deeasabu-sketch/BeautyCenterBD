using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace CustomAuthentication.Helpers
{
    public class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] salt=new byte[16];
            using(var rng=RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            using(var pbkdf2=new Rfc2898DeriveBytes(password,salt,10000,HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                byte[] result = new byte[48];
                Buffer.BlockCopy(salt, 0, result, 0, 16);
                Buffer.BlockCopy(hash, 0, result, 16, 32);
                return Convert.ToBase64String(result);
            }

        }
        public static bool VerifyPassword(string password,string storeHash)
        {
            byte[] stored=Convert.FromBase64String(storeHash);
            byte[] salt = new byte[16];
            Buffer.BlockCopy(stored, 0, salt, 0, 16);
            using(var pbkdf2=new Rfc2898DeriveBytes(password,salt,10000,HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                for(int i=0;i<32;i++)
                {
                    if (stored[i + 16] != hash[i])
                    {
                        return false;
                    }
                }
                return true;
            }

        }
    }
}