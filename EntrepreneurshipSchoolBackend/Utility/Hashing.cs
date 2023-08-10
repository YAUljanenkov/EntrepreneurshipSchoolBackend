using System.Security.Cryptography;

namespace EntrepreneurshipSchoolBackend.Utility;

/// <summary>
/// This class contains methods that has password hashing functions.
/// </summary>
public static class Hashing
{
    /// <summary>
    /// Hashes a password with salt and SHA512 algorithm.
    /// </summary>
    /// <param name="password">A string password to hash.</param>
    /// <returns>A hashed password.</returns>
    /// <exception cref="ArgumentNullException">Throws if a password is null.</exception>
    public static string HashPassword(string password)
    {
        byte[] salt;
        byte[] buffer2;
        if (password == null)
        {
            throw new ArgumentNullException(password);
        }

        using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8, HashAlgorithmName.SHA512))
        {
            salt = bytes.Salt;
            buffer2 = bytes.GetBytes(0x20);
        }

        byte[] dst = new byte[0x31];
        Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
        Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
        return Convert.ToBase64String(dst);
    }

    /// <summary>
    /// Verifies that a password equals to hash.
    /// </summary>
    /// <param name="hashedPassword">A hash of a password.</param>
    /// <param name="password">A password to check.</param>
    /// <returns>True if passwords are equal.</returns>
    /// <exception cref="ArgumentNullException">Throws if a password to check is null.</exception>
    public static bool VerifyHashedPassword(string? hashedPassword, string password)
    {
        byte[] buffer4;
        if (hashedPassword == null)
        {
            return false;
        }

        if (password == null)
        {
            throw new ArgumentNullException("password");
        }

        byte[] src = Convert.FromBase64String(hashedPassword);
        if ((src.Length != 0x31) || (src[0] != 0))
        {
            return false;
        }

        byte[] dst = new byte[0x10];
        Buffer.BlockCopy(src, 1, dst, 0, 0x10);
        byte[] buffer3 = new byte[0x20];
        Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
        using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8, HashAlgorithmName.SHA512))
        {
            buffer4 = bytes.GetBytes(0x20);
        }

        return buffer3.SequenceEqual(buffer4);
    }
}