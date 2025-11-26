using event_service.Infrastructure.DependencyInjection;
using MediatR;
using FluentValidation;
using event_service.Application.Commands.CrearEvento;
using event_service.Application.Commands.PublicarEvento;
using event_service.Application.Commands.EditarEvento;
using event_service.Application.Commands.PagarPublicacion;
using event_service.Application.Commands.IniciarEvento;
using event_service.Application.Commands.FinalizarEvento;
using event_service.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// DI: registramos implementaciones concretas
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Events Service API",
        Version = "v1",
        Description = "API para la gestión del ciclo de vida completo de eventos. " +
                      "Permite crear, editar, publicar y gestionar eventos desde su creación en estado borrador hasta su finalización. " +
                      "Incluye gestión de secciones, precios y estados del evento.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Events Service Team",
            Email = "events-service@eventmesh-lab.com"
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Incluir comentarios XML si existen
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configurar schemaIds únicos para evitar conflictos con tipos anidados
    c.CustomSchemaIds(type =>
    {
        if (type.FullName != null && type.FullName.Contains('+'))
        {
            var parts = type.FullName.Split('+');
            var containingType = parts[0].Split('.').Last();
            var nestedType = parts[1].Split('.').Last();
            return $"{containingType}{nestedType}";
        }
        return type.Name;
    });
});

// Registrar infraestructura (DbContext, repositorios, fallback, mensajería)
builder.Services.AddInfrastructure(builder.Configuration);

// Registrar MediatR y validators desde el assembly de Application
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CrearEventoCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CrearEventoCommandValidator).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapEventosEndpoints();

app.Run();
