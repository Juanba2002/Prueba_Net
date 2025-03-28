namespace WebApp_Auth.Services
{
    using System.Net.Http.Headers;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using WebApp_Auth.Models;

    public class Auth0Service
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        // Rol ID por nombre desde config o constante
        private readonly Dictionary<string, string> _roleMap = new()
        {
            { "admin", "rol_wGJJV7TbNO9Ws97s" },
            { "general", "rol_3cvrYqN0BldQEG0J" },
            { "inspector", "rol_jJtJRxfOq01iKt98" },
            { "cliente", "rol_jXLKWk7YbZd4RHJX" }
        };

        public Auth0Service(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _config = config;
        }

        public async Task<string> GetManagementTokenAsync()
        {
            var body = new
            {
                client_id = _config["Auth0:ClientId"],
                client_secret = _config["Auth0:ClientSecret"],
                audience = _config["Auth0:Audience"],
                grant_type = "client_credentials"
            };

            var response = await _httpClient.PostAsync(
                $"{_config["Auth0:Domain"]}oauth/token",
                new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode(); // 🛡️ Protege de errores silenciosos
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<dynamic>(content).access_token;
        }

        public async Task<string> CreateUserAsync(Auth0UserRegistration model)
        {
            var token = await GetManagementTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userBody = new
            {
                email = model.Email,
                password = model.Password,
                connection = "Username-Password-Authentication",
                name = model.Name
            };

            var response = await _httpClient.PostAsync(
                $"{_config["Auth0:Audience"]}users",
                new StringContent(JsonConvert.SerializeObject(userBody), Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode(); // ✅ Protege contra errores

            var content = await response.Content.ReadAsStringAsync();
            var userId = JsonConvert.DeserializeObject<dynamic>(content).user_id;

            await AssignRoleToUserAsync(userId.ToString(), model.Role);
            return userId;
        }

        private async Task AssignRoleToUserAsync(string userId, string roleName)
        {
            if (!_roleMap.TryGetValue(roleName, out var roleId))
                throw new Exception($"Rol no válido: {roleName}");

            var roleBody = new
            {
                roles = new[] { roleId }
            };

            var response = await _httpClient.PostAsync(
                $"{_config["Auth0:Audience"]}users/{userId}/roles",
                new StringContent(JsonConvert.SerializeObject(roleBody), Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode(); // ✅ Detecta si hubo error al asignar
        }
    }
}
