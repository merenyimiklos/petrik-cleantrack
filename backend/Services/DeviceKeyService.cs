using System.Security.Cryptography;
using System.Text;

namespace CleanTrack.Api.Services;

public class DeviceKeyService
{
    public string GenerateKey() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    public string Hash(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string key, string hash)
    {
        var actual = Convert.FromHexString(Hash(key));
        var expected = Convert.FromHexString(hash);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
