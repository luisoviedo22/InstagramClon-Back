using InstagramClonAPI.Context;
using InstagramClonAPI.DTOs;
using InstagramClonAPI.Interfaces;
using InstagramClonAPI.Models;
using InstagramClonAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InstagramClonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ITokenService _tokenService;

        public UserController(AppDbContext context, IUserService userService, IRefreshTokenService refreshTokenService, ITokenService tokenService)
        {
            _context = context;
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _tokenService = tokenService;
        }

        [HttpGet("getUsers")]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(errors);
            }

            var (IsSuccess, ErrorMessage, CreatedUser) = await _userService.CreateUserAsync(user);
            if (!IsSuccess)
                return BadRequest(ErrorMessage);

            return CreatedAtAction(nameof(GetUserById), new { id = CreatedUser.Id }, CreatedUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var (IsSuccess, ErrorMessage, User) = await _userService.LoginAsync(loginDto);

            if (!IsSuccess)
                return Unauthorized(new { message = ErrorMessage });

            var token = _tokenService.GenerateToken(User);
            var refreshToken = _refreshTokenService.GenerateRefreshToken(User.Id);

            return Ok(new
            {
                message = "Login exitoso",
                token,
                refreshToken = refreshToken.Token,
                user = new
                {
                    User.Id,
                    User.Email,
                    User.FullName,
                    User.UserName
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var (IsSuccess, ErrorMessage, User) = await _userService.DeleteUserAsync(id);
            if (!IsSuccess)
                return NotFound(ErrorMessage);
            return NoContent();
        }

        [HttpPut("{id}/disable")]
        public async Task<IActionResult> DisableUser(int id)
        {
            var (IsSuccess, ErrorMessage, User) = await _userService.DisableUserAsync(id);
            if (!IsSuccess)
                return NotFound(ErrorMessage);
            return NoContent();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!_refreshTokenService.ValidateRefreshToken(request.UserId, request.RefreshToken))
                return Unauthorized(new { message = "Refresh token inválido o expirado" });

            var user = await _userService.GetUserByIdAsync(request.UserId);
            if (user == null) return NotFound();

            var newJwt = _tokenService.GenerateToken(user);
            var newRefreshToken = _refreshTokenService.GenerateRefreshToken(user.Id);

            return Ok(new
            {
                token = newJwt,
                refreshToken = newRefreshToken.Token
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            await _refreshTokenService.RevokeRefreshTokenAsync(request.UserId, request.RefreshToken);
            return Ok(new { message = "Logout exitoso" });
        }

        [HttpPost("follow")]
        public async Task<IActionResult> Follow([FromBody] FollowRequest request)
        {
            if (request.FollowerId == request.FollowedId)
            {
                return BadRequest("No puedes seguirte a ti mismo.");
            }

            var result = await _userService.FollowAsync(request.FollowerId, request.FollowedId);

            if (result.IsSuccess)
            {
                return Ok(new { message = result.ErrorMessage });
            }

            return BadRequest(new { error = result.ErrorMessage });
        }

        [HttpGet("suggestions/{currentUserId}")]
        public async Task<IActionResult> GetSuggestedUsers(int currentUserId)
        {
            // Usuarios seguidos
            var followedIds = await _context.Followers
                .Where(f => f.FollowerId == currentUserId && f.IsFollowing)
                .Select(f => f.FollowedId)
                .ToListAsync();

            // Todos los usuarios excepto el actual y los que ya sigue
            var suggestedUsers = await _context.Users
                .Where(u => u.Id != currentUserId && !followedIds.Contains(u.Id) && u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.UserName
                })
                .ToListAsync();

            return Ok(suggestedUsers);
        }

        [HttpGet("followers/{userId}")]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("Usuario no encontrado.");
            var followers = await _userService.GetFollowersAsync(userId);
            return Ok(followers);
        }

        [HttpGet("following/{userId}")]
        public async Task<IActionResult> GetFollowing(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("Usuario no encontrado.");
            var following = await _userService.GetFollowingAsync(userId);
            return Ok(following);
        }

        [HttpGet("CountFollowers/{userId}")]
        public async Task<IActionResult> GetCountFollowers(int userId)
        {
            var (IsSuccess, FollowersCount) = await _userService.GetCountFollowersAsync(userId);
            if (!IsSuccess)
                return NotFound("Usuario no encontrado o inactivo.");
            return Ok(FollowersCount);
        }

        [HttpGet("CountFollowing/{userId}")]
        public async Task<IActionResult> GetCountFollowing(int userId)
        {
            var (IsSuccess, FollowersCount) = await _userService.GetCountFollowingAsync(userId);
            if (!IsSuccess)
                return NotFound("Usuario no encontrado o inactivo.");
            return Ok(FollowersCount);
        }

        [HttpPost("unfollow")]
        public async Task<IActionResult> Unfollow([FromBody] FollowRequest request)
        {
            if (request.FollowerId == request.FollowedId)
            {
                return BadRequest("No puedes dejar de seguirte a ti mismo.");
            }
            var result = await _userService.UnfollowAsync(request.FollowerId, request.FollowedId);
            if (result.IsSuccess)
            {
                return Ok(new { message = result.ErrorMessage });
            }
            return BadRequest(new { error = result.ErrorMessage });
        }
    }
}
