using System.ComponentModel.DataAnnotations;

namespace InstagramClonAPI.DTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
