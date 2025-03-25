// Importa los atributos y tipos para proteger rutas con [Authorize]
using Microsoft.AspNetCore.Authorization;

// Importa los tipos necesarios para construir controladores y manejar respuestas HTTP
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers // Espacio de nombres que agrupa los controladores
{
    // Define que esta clase es un controlador de API
    [ApiController]

    // Define la ruta base del controlador: /api/secure
    [Route("api/[controller]")]
    public class SecureController : ControllerBase
    {
        // Maneja solicitudes GET a /api/secure
        [HttpGet]

        // Protege este endpoint: solo accesible con un token válido (JWT)
        [Authorize]
        public IActionResult Get()
        {
            // Devuelve una respuesta 200 OK con un mensaje JSON
            return Ok(new { message = "Acceso autorizado a Auth0 validando el Token en .Net" });
        }
    }
}
