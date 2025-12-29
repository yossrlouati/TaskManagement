
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SimpleAuthApi.Data;
using System.Security.Claims;
using SimpleAuthApi.Services;
using System.Text.Json.Serialization;
using Azure.Core;
using SimpleAuthApi.Hubs;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;



// --- 1. CONFIGURATION DE LA BASE DE DONNÉES SQL SERVER ---
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
// --- 2. CONFIGURATION D'ASP.NET CORE IDENTITY ---
builder.Services.AddIdentity<IdentityUser, IdentityRole>()//options =>
//{
// Configure Identity pour permettre la connexion par Email au lieu du UserName
//options.User.RequireUniqueEmail = true; // S'assurer que les emails sont uniques
//options.SignIn.RequireConfirmedAccount = false; // Désactiver la confirmation par email pour la démo
//options.Password.RequireDigit=true;
//options.Password.RequireLangth=8;
//})
    .AddEntityFrameworkStores<AppDbContext>() // Lie Identity à notre AppDbContext
    .AddDefaultTokenProviders(); // Ajoute des services pour gérer les tokens (reset password, etc.)
// --- 3. CONFIGURATION DE L'AUTHENTIFICATION JWT ---
var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Clé JWT non configuré");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // On vérifie la signature (sécurité)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,    // Éviter l'erreur "invalid issuer" pour le moment
            ValidateAudience = false,  // Éviter l'erreur "invalid audience" pour le moment
            RoleClaimType = ClaimTypes.Role // Important pour que [Authorize(Roles="...")] fonctionne
        };
        //Autorise les WebSockets à utiliser ton Token JWT.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.Request.Path;
                // Si la requête vient de SignalR, on extrait le token
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
//par défaut nal9ahom 
// Add services to the container.

//builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ceci dit à System.Text.Json de gérer les références circulaires
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// lena
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
// AddScoped : Crée une nouvelle instance de EmailService pour chaque requête HTTP.
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// IMPORTANT: Doit être dans cet ordre
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
// Exécution du Seeder
using (var scope = app.Services.CreateScope())
{
    await SimpleAuthApi.Services.DataSeeder.SeedRolesAndAdminUser(scope.ServiceProvider);
}
app.MapHub<ChatHub>("/chatHub");

app.Run();
// Ajout pour l'accessibilité des tests
public partial class Program { }