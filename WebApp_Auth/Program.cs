var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
var domain = $"https://{config["Auth0:Domain"]}/";
var audience = config["Auth0:Audience"];

// Servicios
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = domain;
        options.Audience = audience;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Configurar CORS ANTES de construir la app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();

app.UseCors("AllowFrontend"); // <- Importante: va antes de autenticación

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
