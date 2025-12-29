using System.ComponentModel.DataAnnotations;

namespace SimpleAuthApi.Models
{
    public class LoginModel
    {
        //[Required]
        //public string Username { get; set; }
        [Required(ErrorMessage = "L'identifiant est requis (Username, Email ou Téléphone)")]
        public string Identifier { get; set; } // Identifiant flexible

        [Required]
        public string Password { get; set; }
    }
}
