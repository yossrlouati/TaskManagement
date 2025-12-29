using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using SimpleAuthApi.Data;
using SimpleAuthApi.Models; // Pour ApplicationUser (Assurez-vous que c'est le bon namespace !)
using System.Linq; // Pour SingleOrDefault
// AJOUTER CECI : Il permet au projet de tests de voir la classe ApplicationUser
namespace SimpleAuthApi.SimpleAuthApi.Tests
{
    // Fichier: CustomWebApplicationFactory.cs (ajoutez cette classe à la fin du fichier ou en haut)

    // -----------------------------------------------------------
    // CLASSE UTILITAIRE POUR TEST : PASSE TOUS LES MOTS DE PASSE
    // -----------------------------------------------------------
    public class NoopPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            // Retourne toujours Succès
            return Task.FromResult(IdentityResult.Success);
        }
    }
    // -----------------------------------------------------------
    // CLASSE UTILITAIRE POUR TEST : PASSE TOUS LES NOMS D'UTILISATEUR
    // -----------------------------------------------------------
    public class NoopUserValidator<TUser> : IUserValidator<TUser> where TUser : class
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            // Retourne toujours Succès
            return Task.FromResult(IdentityResult.Success);
        }
    }
    public class CustomWebApplicationFactory<TProgram>: WebApplicationFactory<TProgram> where  TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //Remplace la BD réelle par BD en mémoire 
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                //Ajouter BD en mémoire 
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // ------------------------------------------------------------------
                // CORRECTION CRITIQUE : Remplacer la validation de mot de passe par un validateur qui réussit toujours
                // ------------------------------------------------------------------

                // 1. Suppression du validateur de mot de passe par défaut
                var passwordValidatorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPasswordValidator<IdentityUser>));
                if (passwordValidatorDescriptor != null)
                {
                    services.Remove(passwordValidatorDescriptor);
                }

                // 2. Ajout de notre validateur qui ne fait rien (Noop)
                services.AddScoped<IPasswordValidator<IdentityUser>, NoopPasswordValidator<IdentityUser>>();

                // ------------------------------------------------------------------
                // ... (Votre code de substitution du IPasswordValidator<IdentityUser>)

                // ------------------------------------------------------------------
                // AJOUT CRITIQUE : Remplacer la validation d'utilisateur/email
                // ------------------------------------------------------------------

                // 1. Suppression du validateur d'utilisateur par défaut
                var userValidatorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IUserValidator<IdentityUser>));
                if (userValidatorDescriptor != null)
                {
                    services.Remove(userValidatorDescriptor);
                }

                // 2. Ajout de notre validateur qui ne fait rien (Noop)
                services.AddScoped<IUserValidator<IdentityUser>, NoopUserValidator<IdentityUser>>();

                // ------------------------------------------------------------------





                // ------------------------------------------------------------------
                // S'assurer que les rôles (Admin, Employee) sont créés pour chaque test
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                    var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
                    db.Database.EnsureCreated();
                    //Initialisation des roles
                    string[] roles = { "Admin", "Employee" };
                    foreach (var role in roles)
                    {
                        if (!roleManager.RoleExistsAsync(role).Result)
                        {
                            roleManager.CreateAsync(new IdentityRole(role)).Wait();
                        }
                    }

                }

            });
        }
    }
    
}
