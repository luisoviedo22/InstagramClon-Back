using System.ComponentModel.DataAnnotations;

namespace InstagramClonAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string UserName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Followers> Followers { get; set; } = new(); // Usuarios que siguen a este
        public List<Followers> Following { get; set; } = new(); // Usuarios que este sigue
    }
}
