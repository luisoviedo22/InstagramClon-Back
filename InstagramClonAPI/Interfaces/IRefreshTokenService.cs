using InstagramClonAPI.Models;
using System.Threading.Tasks;

namespace InstagramClonAPI.Interfaces
{
    public interface IRefreshTokenService
    {
        RefreshToken GenerateRefreshToken(int userId);
        bool ValidateRefreshToken(int userId, string token);
        Task RevokeRefreshTokenAsync(int userId, string token);
    }
}

