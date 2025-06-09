using InstagramClonAPI.Models;
using InstagramClonAPI.DTOs;

namespace InstagramClonAPI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<(bool IsSuccess, string ErrorMessage, User User)> CreateUserAsync(User user);
        Task<(bool IsSuccess, string ErrorMessage, User User)> LoginAsync(LoginDto loginDto);
        Task<(bool IsSuccess, string ErrorMessage, User User)> DeleteUserAsync(int id);
        Task<(bool IsSuccess, string ErrorMessage, User User)> DisableUserAsync(int id);
        Task<(bool IsSuccess, string ErrorMessage)> FollowAsync(int userOne, int userTwo);
        Task<(bool IsSuccess, string ErrorMessage)> UnfollowAsync(int userOne, int userTwo);
        Task<(bool IsSuccess, int Followers)> GetCountFollowersAsync(int id);
        Task<(bool IsSuccess, int Followers)> GetCountFollowingAsync(int id);
        Task<List<User>> GetFollowersAsync(int id);
        Task<List<User>> GetFollowingAsync(int id);
    }
}