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
builder.Services.AddSwaggerGen();

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
