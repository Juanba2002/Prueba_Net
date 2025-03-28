using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using WebApp_Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;

namespace WebApp_Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(IConfiguration config, ILogger<RegisterController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] Auth0UserRegistration request)
        {
            try
            {
                var domain = _config["Auth0:Domain"];
                var clientId = _config["Auth0:ClientId"];
                var clientSecret = _config["Auth0:ClientSecret"];
                var audience1 = $"https://{domain}/api/v2/";


                // Obtiene un token de acceso usando Client Credentials (para Management API)
                using var client = new HttpClient();
                var tokenResponse = await client.PostAsJsonAsync($"https://{domain}/oauth/token", new
                {
                    client_id = clientId,
                    client_secret = clientSecret,
                    audience = audience1,
                    grant_type = "client_credentials"
                });

                var tokenData = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

                var token = tokenData.access_token;

                // Instancia del cliente de Management API
                var managementClient = new ManagementApiClient(token, new Uri(audience1));

                // Crea el usuario
                var userCreateRequest = new UserCreateRequest
                {
                    Connection = "Username-Password-Authentication",
                    Email = request.Email,
                    Password = request.Password,
                    FullName = request.Name,
                    EmailVerified = false
                };

                var user = await managementClient.Users.CreateAsync(userCreateRequest);

                // Obtiene el ID del rol en Auth0 por nombre
                var roles = await managementClient.Roles.GetAllAsync(new GetRolesRequest());
                var role = roles.FirstOrDefault(r => r.Name.ToLower() == request.Role.ToLower());

                if (role == null)
                    return BadRequest(new { error = "Rol no encontrado" });

                // Asigna el rol al usuario creado
                await managementClient.Users.AssignRolesAsync(user.UserId, new AssignRolesRequest
                {
                    Roles = new[] { role.Id }
                });

                return Ok(new { message = "Usuario creado exitosamente y rol asignado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el registro");

                return StatusCode(500, new
                {
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}
