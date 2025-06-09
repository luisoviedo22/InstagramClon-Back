using InstagramClonAPI.Models;

namespace InstagramClonAPI.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
