using System.Security.Cryptography;
using Identity.Application.Common.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Identity.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 100_000;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var subkey = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize);

        var outputBytes = new byte[SaltSize + KeySize];
        Buffer.BlockCopy(salt, 0, outputBytes, 0, SaltSize);
        Buffer.BlockCopy(subkey, 0, outputBytes, SaltSize, KeySize);

        return Convert.ToBase64String(outputBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            var decodedHashedPassword = Convert.FromBase64String(passwordHash);
            if (decodedHashedPassword.Length != SaltSize + KeySize)
            {
                return false;
            }

            var salt = new byte[SaltSize];
            Buffer.BlockCopy(decodedHashedPassword, 0, salt, 0, SaltSize);

            var expectedSubkey = new byte[KeySize];
            Buffer.BlockCopy(decodedHashedPassword, SaltSize, expectedSubkey, 0, KeySize);

            var actualSubkey = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }
        catch
        {
            return false;
        }
    }
}
