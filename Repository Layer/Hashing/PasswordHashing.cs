using System;
using System.Security.Cryptography;

namespace Repository_Layer.Hashing;

public class PasswordHashing
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10;

    public static byte[] HashPassword(string password)
    {
        try
        {
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt); // Updated method for generating salt

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            byte[] hashByte = new byte[SaltSize + HashSize];

            Array.Copy(salt, 0, hashByte, 0, SaltSize);
            Array.Copy(hash, 0, hashByte, SaltSize, HashSize);

            return hashByte;
        }
        catch (Exception ex)
        {
            throw new Exception("Error in hashing password", ex);
        }
    }

    public static bool VerifyPassword(string userPass, byte[] storedHashPass)
    {
        try
        {
            byte[] salt = new byte[SaltSize];
            Array.Copy(storedHashPass, 0, salt, 0, SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(userPass, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            for (int i = 0; i < HashSize; i++)
            {
                if (storedHashPass[i + SaltSize] != hash[i])
                    return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Error in verifying password", ex);
        }
    }
}
