namespace InstagramClonAPI.Models
{
    public class Followers
    {
        public int Id { get; set; }

        // Clave foránea al usuario que sigue
        public int FollowerId { get; set; }
        public User Follower { get; set; }

        // Clave foránea al usuario que es seguido
        public int FollowedId { get; set; }
        public User Followed { get; set; }

        public DateTime FollowedDate { get; set; }
        public DateTime? UnfollowDate { get; set; }
        public bool IsFollowing { get; set; } = true;
    }
}
