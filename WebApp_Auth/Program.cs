// Crea el constructor de la aplicación (builder), donde se configuran los servicios y la app.
var builder = WebApplication.CreateBuilder(args);

// Accede a la configuración del archivo appsettings.json
var config = builder.Configuration;

// Define el dominio de Auth0 a partir de la configuración (con https:// incluido)
var domain = $"https://{config["Auth0:Domain"]}/";

// Define el "audience" o identificador único de tu API, configurado en Auth0
var audience = config["Auth0:Audience"];


// -------------------------
// CONFIGURACIÓN DE SERVICIOS
// -------------------------

// Agrega el servicio de autenticación con esquema "Bearer" y configuración de Auth0
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // URL del servidor de autenticación (Auth0)
        options.Authority = domain;

        // Especifica para qué API es válido el token
        options.Audience = audience;

        options.RequireHttpsMetadata = false;
    });

// Agrega el sistema de autorización para proteger rutas con [Authorize]
builder.Services.AddAuthorization();

// Agrega los controladores a la app (lo que permite usar la carpeta Controllers)
builder.Services.AddControllers();

// Agrega Swagger para documentación y pruebas de la API (solo se activa en desarrollo)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WebApp_Auth", Version = "v1" });

    // Agrega soporte para autenticación Bearer en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese un token JWT con el prefijo 'Bearer'",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configura CORS para permitir solicitudes desde el frontend Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:4200") // URL de Angular en local
                        .AllowAnyHeader()  // Permitir cualquier cabecera (como Authorization)
                        .AllowAnyMethod()); // Permitir cualquier método HTTP
});


// -------------------------
// CONFIGURACIÓN DE LA APP
// -------------------------

// Construye la aplicación con los servicios configurados
var app = builder.Build();

// Habilita Swagger y su UI solo en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirige automáticamente todas las solicitudes HTTP a HTTPS
app.UseHttpsRedirection();

// Activa la política CORS que definimos arriba (debe ir antes de autenticación)
app.UseCors("AllowFrontend");

// Activa el middleware de autenticación (revisa el token en las peticiones)
app.UseAuthentication();

// Activa la autorización (verifica roles, políticas, etc. después de validar token)
app.UseAuthorization();

// Mapea los controladores de la API a las rutas (por ejemplo: /api/secure)
app.MapControllers();

// Inicia la aplicación y la deja escuchando en el puerto configurado
app.Run();
