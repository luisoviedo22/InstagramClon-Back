using InstagramClonAPI.Models;
using InstagramClonAPI.DTOs;
using InstagramClonAPI.Context;
using Microsoft.EntityFrameworkCore;
using InstagramClonAPI.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace InstagramClonAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasherService _passwordHasher;

        public UserService(AppDbContext context, IPasswordHasherService passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<(bool IsSuccess, string ErrorMessage, User User)> CreateUserAsync(User user)
        {
            if (user == null)
                return (false, "El usuario no puede ser nulo.", null);

            var existingEmail = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (existingEmail)
                return (false, "Ya existe un usuario con ese correo electrónico.", null);

            user.Password = _passwordHasher.HashPassword(user.Password);

            var existingUsername = await _context.Users.AnyAsync(u => u.UserName == user.UserName);
            if (existingUsername)
                return (false, "Ya existe un usuario con ese nombre de usuario.", null);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, null, user);
        }

        public async Task<(bool IsSuccess, string ErrorMessage, User User)> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
                return (false, "Usuario no encontrado", null);

            if (!user.IsActive)
                return (false, "Usuario desactivado", null);

            if (!_passwordHasher.VerifyPassword(user.Password, loginDto.Password))
                return (false, "Contraseña incorrecta", null);

            return (true, null, user);
        }

        public async Task<(bool IsSuccess, string ErrorMessage, User User)> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return (false, "Usuario no encontrado", null);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return (true, null, user);
        }

        public async Task<(bool IsSuccess, string ErrorMessage, User User)> DisableUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return (false, "Usuario no encontrado", null);
            user.IsActive = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return (true, null, user);
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> FollowAsync(int userOne, int userTwo)
        {
            var userFrom = await _context.Users.FindAsync(userOne);
            var userTo = await _context.Users.FindAsync(userTwo);

            if (userFrom == null || userTo == null)
            {
                return (false, "Uno o ambos usuarios no existen.");
            }
            if (userFrom.IsActive != true && userTo.IsActive != true)
            {
                return (false, "Uno o ambos usuarios no existen.");
            }
            // Verificar si ya existe la relación de seguimiento
            var existingFollow = await _context.Followers
                .FirstOrDefaultAsync(f => f.FollowerId == userOne && f.FollowedId == userTwo);

            if (existingFollow != null)
            {
                if (existingFollow.IsFollowing)
                {
                    return (false, "Ya sigues a este usuario.");
                }
                else
                {
                    // Reactivar seguimiento (caso "unfollow" previo)
                    existingFollow.IsFollowing = true;
                    existingFollow.FollowedDate = DateTime.UtcNow;
                    existingFollow.UnfollowDate = null;

                    await _context.SaveChangesAsync();
                    return (true, "Has vuelto a seguir al usuario.");
                }
            }

            // Crear nueva relación de seguimiento
            var newFollow = new Followers
            {
                FollowerId = userOne,
                FollowedId = userTwo,
                FollowedDate = DateTime.UtcNow,
                IsFollowing = true
            };

            _context.Followers.Add(newFollow);
            await _context.SaveChangesAsync();

            return (true, "Usuario seguido exitosamente.");
        }
        public async Task<(bool IsSuccess, string ErrorMessage)> UnfollowAsync(int userOne, int userTwo)
        {
            var userFrom = await _context.Users.FindAsync(userOne);
            var userTo = await _context.Users.FindAsync(userTwo);

            if (userFrom == null || userTo == null)
            {
                return (false, "Uno o ambos usuarios no existen.");
            }
            if (userFrom.IsActive != true && userTo.IsActive != true)
            {
                return (false, "Uno o ambos usuarios no existen.");
            }
            return (true, "Ya no sigues a este usuario.");
        }
        public async Task<List<User>> GetFollowersAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new Exception("El usuario no existe o está inactivo.");
            }

            var followers = await _context.Followers
                .Where(f => f.FollowedId == id && f.IsFollowing && f.Follower.IsActive)
                .Include(f => f.Follower)
                .Select(f => f.Follower) // devuelve objetos `User`
                .ToListAsync();

            return followers;
        }
        public async Task<List<User>> GetFollowingAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new Exception("El usuario no existe o está inactivo.");
            }

            var following = await _context.Followers
                .Where(f => f.FollowerId == id && f.IsFollowing && f.Follower.IsActive)
                .Include(f => f.Followed)
                .Select(f => f.Followed) // devuelve objetos `User`
                .ToListAsync();

            return following;
        }
        public async Task<(bool IsSuccess, int Followers)> GetCountFollowersAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new Exception("El usuario no existe o está inactivo.");
            }

            var count = await _context.Followers
                .Where(f => f.FollowedId == id && f.IsFollowing)
                .Join(_context.Users,
                      f => f.FollowerId,
                      u => u.Id,
                      (f, u) => new { Follower = f, User = u })
                .Where(joined => joined.User.IsActive)
                .CountAsync();

            return (true, count);
        }
        public async Task<(bool IsSuccess, int Followers)> GetCountFollowingAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new Exception("El usuario no existe o está inactivo.");
            }

            var count = await _context.Followers
                .Where(f => f.FollowerId == id && f.IsFollowing)
                .Join(_context.Users,
                      f => f.FollowedId,
                      u => u.Id,
                      (f, u) => new { Follower = f, User = u })
                .Where(joined => joined.User.IsActive)
                .CountAsync();

            return (true, count);
        }
    }
}
