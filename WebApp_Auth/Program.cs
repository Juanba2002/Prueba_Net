// Crea el constructor de la aplicaci�n (builder), donde se configuran los servicios y la app.
var builder = WebApplication.CreateBuilder(args);

// Accede a la configuraci�n del archivo appsettings.json
var config = builder.Configuration;

// Define el dominio de Auth0 a partir de la configuraci�n (con https:// incluido)
var domain = $"https://{config["Auth0:Domain"]}/";

// Define el "audience" o identificador �nico de tu API, configurado en Auth0
var audience = config["Auth0:Audience"];


// -------------------------
// CONFIGURACI�N DE SERVICIOS
// -------------------------

// Agrega el servicio de autenticaci�n con esquema "Bearer" y configuraci�n de Auth0
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // URL del servidor de autenticaci�n (Auth0)
        options.Authority = domain;

        // Especifica para qu� API es v�lido el token
        options.Audience = audience;
    });

// Agrega el sistema de autorizaci�n para proteger rutas con [Authorize]
builder.Services.AddAuthorization();

// Agrega los controladores a la app (lo que permite usar la carpeta Controllers)
builder.Services.AddControllers();

// Configura CORS para permitir solicitudes desde el frontend Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:4200") // URL de Angular en local
                        .AllowAnyHeader()  // Permitir cualquier cabecera (como Authorization)
                        .AllowAnyMethod()); // Permitir cualquier m�todo HTTP
});


// -------------------------
// CONFIGURACI�N DE LA APP
// -------------------------

// Construye la aplicaci�n con los servicios configurados
var app = builder.Build();

// Redirige autom�ticamente todas las solicitudes HTTP a HTTPS
app.UseHttpsRedirection();

// Activa la pol�tica CORS que definimos arriba (debe ir antes de autenticaci�n)
app.UseCors("AllowFrontend");

// Activa el middleware de autenticaci�n (revisa el token en las peticiones)
app.UseAuthentication();

// Activa la autorizaci�n (verifica roles, pol�ticas, etc. despu�s de validar token)
app.UseAuthorization();

// Mapea los controladores de la API a las rutas (por ejemplo: /api/secure)
app.MapControllers();

// Inicia la aplicaci�n y la deja escuchando en el puerto configurado
app.Run();
