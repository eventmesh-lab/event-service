using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using event_service.Application.Commands.CrearEvento;
using event_service.Application.Commands.EditarEvento;
using event_service.Application.Commands.FinalizarEvento;
using event_service.Application.Commands.IniciarEvento;
using event_service.Application.Commands.PagarPublicacion;
using event_service.Application.Commands.PublicarEvento;
using event_service.Api.DTOs;
using event_service.Domain.Ports;

namespace event_service.Api.Endpoints
{
    public static class EventosEndpointsExtensions
    {
        public static void MapEventosEndpoints(this WebApplication app)
        {
            var eventos = app.MapGroup("/api/eventos").WithTags("Eventos");

            eventos.MapPost("/", async (CrearEventoCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Created($"/api/eventos/{result.Id}", result);
            }).WithName("CrearEvento").WithSummary("Crea un nuevo evento en estado borrador").Produces<CrearEventoCommandResponse>(StatusCodes.Status201Created);

            eventos.MapPost("/{id:guid}/publicar", async (Guid id, PublicarEventoCommand command, IMediator mediator) =>
            {
                var enriched = command with { EventoId = id };
                await mediator.Send(enriched);
                return Results.Ok();
            }).WithName("PublicarEvento").WithSummary("Publica un evento existente").Produces(StatusCodes.Status200OK);

            eventos.MapPut("/{id:guid}", async (Guid id, EditarEventoCommand command, IMediator mediator) =>
            {
                var enriched = command with { EventoId = id };
                await mediator.Send(enriched);
                return Results.NoContent();
            }).WithName("EditarEvento").WithSummary("Edita un evento en estado borrador").Produces(StatusCodes.Status204NoContent);

            eventos.MapPost("/{id:guid}/pagar-publicacion", async (Guid id, PagarPublicacionCommand command, IMediator mediator) =>
            {
                var enriched = command with { EventoId = id };
                await mediator.Send(enriched);
                return Results.Accepted($"/api/eventos/{id}");
            }).WithName("PagarPublicacionEvento").WithSummary("Inicia el pago de publicación del evento").Produces(StatusCodes.Status202Accepted);

            eventos.MapPost("/{id:guid}/iniciar", async (Guid id, IMediator mediator) =>
            {
                var command = new IniciarEventoCommand { EventoId = id };
                await mediator.Send(command);
                return Results.Ok();
            }).WithName("IniciarEvento").WithSummary("Marca un evento publicado como en curso").Produces(StatusCodes.Status200OK);

            eventos.MapPost("/{id:guid}/finalizar", async (Guid id, IMediator mediator) =>
            {
                var command = new FinalizarEventoCommand { EventoId = id };
                await mediator.Send(command);
                return Results.Ok();
            }).WithName("FinalizarEvento").WithSummary("Finaliza un evento en curso").Produces(StatusCodes.Status200OK);

            eventos.MapGet("/{id:guid}", async (Guid id, IEventoRepository repository) =>
            {
                var evento = await repository.GetByIdAsync(id);
                if (evento == null) return Results.NotFound();
                return Results.Ok(EventoResponseDto.FromDomain(evento));
            }).WithName("ObtenerEvento").WithSummary("Obtiene los detalles de un evento").Produces<EventoResponseDto>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound);
        }
    }
}
