// Fichier: SimpleAuthApi.Tests/AuthenticationTests.cs
using System.Net.Http.Json;
using SimpleAuthApi.Models;
using Xunit; // Assurez-vous d'avoir ce using
using System.Net;
using SimpleAuthApi.SimpleAuthApi.Tests;
using System.Text.Json;

public class RegisterSuccessResponse
{
    public string Status { get; set; }
    public string Message { get; set; }
}
public class LoginResponse
{
    public string Token { get; set; } // Notez la majuscule si votre JSON est en camelCase (token)
    public DateTime Expiration { get; set; }
}

// Hériter de IClassFixture pour utiliser notre Factory. TProgram doit pointer vers votre classe Program
public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    // Le constructeur reçoit la Factory et crée un client HTTP
    public AuthenticationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // Fichier: AuthenticationTests.cs

    // ...

    // --- TEST 1 : INSCRIPTION RÉUSSIE (Happy Path) ---
    [Fact]
    public async Task Register_ValidData_ReturnsSuccess()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N");
        var registration = new RegisterModel
        {
            Username = $"testuser_{uniqueId}",
            Email = $"testa_{uniqueId}@example.com",
            Password = "UltraStrongP@ssword123456789!#$",
            Role = "Employee",
            PhoneNumber = "1234567890" // <-- AJOUTER CECI
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", registration);

        // -------------------------------------------------------------------------
        // ASSUREZ-VOUS D'AVOIR LE CODE CI-DESSOUS (pour afficher les erreurs claires)
        // -------------------------------------------------------------------------
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Fail($"L'inscription a échoué avec le code {response.StatusCode}. Détails de l'erreur : {errorContent}");
        }
        // -------------------------------------------------------------------------

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RegisterSuccessResponse>();
        Assert.Equal("Success", result!.Status);
        // ------------------------------------------------------------------
    
    }

    // --- TEST 2 : CONNEXION RÉUSSIE APRÈS INSCRIPTION ---
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Rendre l'utilisateur de connexion unique aussi
        var uniqueId = Guid.NewGuid().ToString("N");
        var username = $"loginuser_{uniqueId}";
        var password = "UltraStrongP@ssword123456789!#$";

        // 1. Arrange (Pré-inscription d'un utilisateur pour la connexion)
        var registration = new RegisterModel { Username = username, Email = $"loginb_{uniqueId}@test.com", Password = password, Role = "Admin",PhoneNumber = "0987654321"};

        // IMPORTANT : Vérifiez la réussite de l'inscription
        var regResponse = await _client.PostAsJsonAsync("/api/Auth/register", registration);
        regResponse.EnsureSuccessStatusCode();

        // 2. Arrange (Données de connexion)
        var login = new LoginModel { Identifier = username, Password = password };

        // 3. Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", login);

        // 4. Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result!.Token);
        //Assert.NotNull(result.GetProperty("token").GetString());
    }

    // ... Votre troisième test Login_InvalidPassword_ReturnsUnauthorized (il n'a pas besoin de GUID)

    // --- TEST 3 : CONNEXION ÉCHOUE AVEC MAUVAIS MDP (Negative Test) ---
    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // 1. Arrange (Inscription d'un utilisateur pour s'assurer qu'il existe)
        var registration = new RegisterModel { Username = "failuser_c", Email = "failc@test.com", Password = "SecureP@ss123", Role = "Employee", PhoneNumber = "1122334455"};
        await _client.PostAsJsonAsync("/api/Auth/register", registration);

        // 2. Arrange (Tentative de connexion avec un mauvais mot de passe)
        var login = new LoginModel { Identifier = "failuser_c", Password = "WrongPassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", login);

        // Assert
        // Vérifie que la réponse est 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}