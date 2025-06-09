using InstagramClonAPI.Context;
using InstagramClonAPI.Interfaces;
using InstagramClonAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InstagramClonAPI.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppDbContext _context;

        public RefreshTokenService(AppDbContext context)
        {
            _context = context;
        }

        public RefreshToken GenerateRefreshToken(int userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = userId,
                Expiration = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshToken);
            _context.SaveChanges();

            return refreshToken;
        }

        public bool ValidateRefreshToken(int userId, string token)
        {
            var storedToken = _context.RefreshTokens
                .FirstOrDefault(rt => rt.UserId == userId && rt.Token == token);

            return storedToken != null && storedToken.Expiration > DateTime.UtcNow;
        }

        public async Task RevokeRefreshTokenAsync(int userId, string token)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == token);

            if (storedToken != null)
            {
                _context.RefreshTokens.Remove(storedToken);
                await _context.SaveChangesAsync();
            }
        }
    }
}
