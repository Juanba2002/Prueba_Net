using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    // Este controlador se encarga de exponer endpoints protegidos por Auth0
    [ApiController]
    [Route("api/[controller]")]
    public class SecureController : ControllerBase
    {
        // ✅ Endpoint accesible para cualquier usuario autenticado
        [HttpGet("general")]
        [Authorize]
        public IActionResult GeneralAccess()
        {
            return Ok(new { message = "Acceso autorizado a Auth0 validando el token en .NET (usuario autenticado)" });
        }

        //Endpoint exclusivo para usuarios con rol "admin"
        [HttpGet("admin")]
        [Authorize(Roles = "admin")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "Acceso autorizado SOLO para usuarios con rol 'admin'" });
        }

        //Endpoint exclusivo para usuarios con rol "inspector"
        [HttpGet("inspector")]
        [Authorize(Roles = "inspector")]
        public IActionResult InspectorOnly()
        {
            return Ok(new { message = "Acceso autorizado SOLO para usuarios con rol 'inspector'" });
        }

        //Endpoint exclusivo para usuarios con rol "inspector"
        [HttpGet("cliente")]
        [Authorize(Roles = "cliente")]
        public IActionResult CustomerOnly()
        {
            return Ok(new { message = "Acceso autorizado SOLO para usuarios con rol 'cliente'" });
        }
        
    }
}
