using Microsoft.AspNetCore.Identity;
using SimpleAuthApi.Data;
using SimpleAuthApi.Models;
using System.Security.Claims;

namespace SimpleAuthApi.Services
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
        {
            //Récuperation des services DI
            //conteneur qui stocke la liste de tous les services (classes) que vous avez configurés dans Program.cs
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            // création du role Admin
            string adminRoleName = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            }
            // Création du Rôle Employé (pour la clarté)
            string employeeRoleName = "Employee";
            if (!await roleManager.RoleExistsAsync(employeeRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(employeeRoleName));
            }
            //Création du Rôle SuoerAdmin(RBAC)
            string superadminRoleName = "SuperAdmin";
            if (!await roleManager.RoleExistsAsync(superadminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(superadminRoleName));
            }
            /*
             * string[] roles = { "SuperAdmin", "Admin", "Employee" };
               foreach (var role in roles)
               {
                 if (!await roleManager.RoleExistsAsync(role))
                 await roleManager.CreateAsync(new IdentityRole(role));
               }
             */
            //.creation de user admin
            string adminEmail = "admin@app.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true

                };
                var result = await userManager.CreateAsync(adminUser, "P@ssword123");
                if (result.Succeeded)
                {
                    // 5. Attribution du rôle "Admin"
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);

                    // 6. Ajout d'un Claim (pour l'exemple)
                    await userManager.AddClaimAsync(adminUser, new Claim("IsAdmin", "True"));
                }
            }
            // Création de l'utilisateur Employé pour le test de réception
            string employeeEmail = "louatiyossr1@gmail.com";
            var employeeUser = await userManager.FindByEmailAsync(employeeEmail);

            if (employeeUser == null)
            {
                employeeUser = new IdentityUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(employeeUser, "User@123"); // Mot de passe test
                if (result.Succeeded)
                {
                    // Attribution du rôle "Employee"
                    await userManager.AddToRoleAsync(employeeUser, employeeRoleName);

                    // Optionnel : Ajout d'un claim spécifique
                    await userManager.AddClaimAsync(employeeUser, new Claim("Department", "IT"));
                }
            }

            // Création de SuperAdmin par défaut
            string superadminEmail = "superadmin@taskpro.com";
            var superadminUser = await userManager.FindByEmailAsync(superadminEmail);
            if (superadminUser == null)
            {
                superadminUser = new IdentityUser
                {
                    UserName = superadminEmail,
                    Email = superadminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(superadminUser, "SuperAdmin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superadminUser, "SuperAAdmin");
                    // await userManager.AddToRolesAsync(user, new List<string> { "SuperAdmin", "Admin" });
                }
            }
            // 3. Initialiser une ligne par défaut dans GlobalSettings si elle est vide
            if (!context.GlobalSettings.Any())
            {
                context.GlobalSettings.Add(new GlobalSettings
                {
                    SmtpServer = "smtp.gmail.com",
                    SenderEmail = "placeholder@gmail.com",
                    SmtpPassword = "app-password-here"
                });
                await context.SaveChangesAsync();





            }

        }

    }
}
