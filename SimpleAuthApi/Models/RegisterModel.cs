using System.ComponentModel.DataAnnotations;

namespace SimpleAuthApi.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
        public string Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "L'Email est requis")]
        public string Email { get; set; } // Champ pour l'Email

        public string PhoneNumber { get; set; } // Champ pour le Numéro de Téléphone (Optionnel/À remplir)

        [Required(ErrorMessage = "Le mot de passe est requis")]
        public string Password { get; set; }

        // Le rôle sera utilisé lors de l'inscription pour définir le type d'utilisateur
        [Required]
        public string Role { get; set; }


    }
}
