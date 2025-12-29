using System;
using System.Net.Http.Json;//biblio client qui sert à mon code de parler aux autres APi(Gemini) et les methodes (PostAsJsonAsync/ReadFromJsonAsyncà
using System.Text;
using System.Text.Json;//Serialize

namespace SimpleAuthApi.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        public GeminiService(HttpClient httpclient, IConfiguration config)
        { 
            _httpClient = httpclient;//ReadAsStringAsync
            _config = config;

        }

        public async Task<string> GenerateTaskDescrip(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return "Taper le titre";


            }
            // 2. Récupération de la clé depuis appsettings
            var apiKey = _config["Gemini:ApiKey"];

            var requestBody = new
            {
                contents = new[] 
                {

                    new
                    {
                        parts = new[]
                        {
                            new { text = $"Génère une description professionnelle pour la tâche : {title}" }

                        }
                        
                    }
               }
            };
            // PostAsJsonAsync est très simple, il transforme l'objet en JSON tout seul
            //var response = await _httpClient.PostAsJsonAsync(url, requestBody);


            var request = new HttpRequestMessage(HttpMethod.Post,"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent");
            request.Headers.Add("X-goog-api-key", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();//using ....ReadAsStreamAsync
                using var jsonDoc = JsonDocument.Parse(responseContent);
                var text = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                return text ?? "No responbse from Gemini"; 

            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erreur Gemini: {response.StatusCode} - {error}");
            }

  
        
        
        }

      
    }
}
