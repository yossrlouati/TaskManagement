using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SimpleAuthApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore; // Nécessaire pour FirstOrDefaultAsync



namespace SimpleAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        //constructor
        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        //POST: /api/Auth/register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            /*vérification si l'utilisateur existe déja"
            //var userExists = await _userManager.FindByNameAsync(model.Username);//_userManager.FindByEmailAsync(model.Email)

            //if (userExists!= null)
               
               //return StatusCode(StatusCodes.Status500InternalServerError, new { Status="Error", Message="Utilisateur exist déja"})*/;
            // 1. Sécurité Blindée : On n'accepte QUE les rôles prévus
            // On ignore la casse avec ToLower() pour plus de souplesse
            var roleNettoyé = model.Role.Trim();
            if (roleNettoyé != "Admin" && roleNettoyé != "Employee")
            {
                return BadRequest(new { Message = "Rôle non autorisé. Seul 'Admin' ou 'Employee' sont permis." });
            }

            //1.Vérifications d'existence (par username et par email)
            if (await _userManager.FindByNameAsync(model.Username) != null || await _userManager.FindByEmailAsync(model.Email) != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "L'utilisateur (Username ou Email) existe déjà !" });
            // 1. Création de l'utilisateur
            IdentityUser user = new()
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                //Email = $"{model.Username}@test.com",
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            // 💡 // On vérifie si le rôle que l'utilisateur a tapé existe vraiment en BD
            /*if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                else
                {
                    // S'il a tapé n'importe quoi, on lui met "Employee" par défaut
                    await _userManager.AddToRoleAsync(user, "Employee");
                }

                return Ok("Inscription réussie");
            }*/
            // -----------------------------
            //Creation de role genere le probleme de sémantique , ça peut etre resoulu en frontend avec liste deroulante 
            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            // 💡 CORRECTION : Vérifier si l'opération a réussi
            // ------------------------------------------------------------------
            if (!result.Succeeded)
            {
                // Si la création échoue (par exemple, validation de mot de passe),
                // on retourne BAD REQUEST avec la liste des erreurs.
                return BadRequest(new
                {
                    Status = "Error",
                    Message = "Échec de la création de l'utilisateur.",
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            if (await _roleManager.RoleExistsAsync(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);
            return Ok(new { Status = "Success", Message = "Utilisateur créé avec succès!" });
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            //var user = await _userManager.FindByNameAsync(model.Username);
            //1.tenter de trouver le user par son nom (Username)
            var user = await _userManager.FindByNameAsync(model.Identifier);
            //2. Si non trouvé, tenter par email
            if(user==null && model.Identifier.Contains("@")){
                user = await _userManager.FindByEmailAsync(model.Identifier);

            }
            //3. i toujours non trouvé , tenter par num de Tel 
            // NOTE: Le Numéro de Téléphone n'est pas indexé par défaut, nous utilisons DbContext via UserManager.Users
            // C'est moins performant qu'une recherche par index, mais fonctionne.
            if (user== null)
            {
                if (!model.Identifier.Contains("@"))

                {
                    //Utilise l'accès direct aux utilisateurs du DbContext (nécessite using Microsoft.EntityFrameworkCore)
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Identifier);
                }
            }

            if(user!=null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim (ClaimTypes.Name, user.UserName),
                    // AJOUT CRITIQUE : L'ID de l'utilisateur (le champ réel dans la BD)
                    new Claim (ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                // 2. Ajout des rôles au Claim (pour l'autorisation)
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                // 3. Création et Signature du Token
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                // 4. Retourne le Token
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized(new { Status = "Error", Message = "Identifiant ou mot de passe invalide." }); // Mauvais identifiants
        }
    
        
    }
}

