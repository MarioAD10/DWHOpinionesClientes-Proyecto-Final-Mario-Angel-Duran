using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Agregar controladores
builder.Services.AddControllers();

// Agregar y configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Social Comments API",
        Version = "v1",
        Description = "API simulada que provee comentarios de redes sociales."
    });
});

var app = builder.Build();

// Configurar Swagger en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Social Comments API v1");
        c.RoutePrefix = string.Empty; // para que se abra en la raíz (localhost:5000)
    });
}

// Configuración general
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
