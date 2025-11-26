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
using Microsoft.OpenApi.Models;

namespace event_service.Api.Endpoints
{
    public static class EventosEndpointsExtensions
    {
        public static void MapEventosEndpoints(this WebApplication app)
        {
            var eventos = app.MapGroup("/api/eventos").WithTags("Eventos");

            // POST /api/eventos
            eventos.MapPost("/", async (CrearEventoCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Created($"/api/eventos/{result.Id}", result);
            })
            .WithName("CrearEvento")
            .WithSummary("Crea un nuevo evento en estado borrador")
            .WithDescription("Crea un nuevo evento en el sistema en estado 'Borrador'. " +
                            "El evento debe incluir al menos una sección con capacidad y precio. " +
                            "La fecha del evento debe ser en el futuro. " +
                            "Una vez creado, el evento puede ser editado antes de proceder con el pago de publicación.")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Crea un nuevo evento en estado borrador",
                Description = "Crea un nuevo evento en el sistema en estado 'Borrador'. " +
                             "El evento debe incluir al menos una sección con capacidad y precio. " +
                             "La fecha del evento debe ser en el futuro. " +
                             "Una vez creado, el evento puede ser editado antes de proceder con el pago de publicación.",
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "Datos del evento a crear",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new()
                        {
                            Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""nombre"": ""Concierto Rock 2025"",
  ""descripcion"": ""Gran concierto de rock con artistas internacionales"",
  ""fecha"": ""2025-12-15T20:00:00Z"",
  ""horasDuracion"": 3,
  ""minutosDuracion"": 0,
  ""organizadorId"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""venueId"": ""660e8400-e29b-41d4-a716-446655440001"",
  ""categoria"": ""Música"",
  ""tarifaPublicacion"": 50.00,
  ""secciones"": [
    {
      ""nombre"": ""VIP"",
      ""capacidad"": 100,
      ""precio"": 150.00,
      ""tipoAsiento"": ""Numerado""
    },
    {
      ""nombre"": ""General"",
      ""capacidad"": 500,
      ""precio"": 75.00,
      ""tipoAsiento"": ""General""
    }
  ]
}")
                        }
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["201"] = new OpenApiResponse
                    {
                        Description = "Evento creado exitosamente",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""id"": ""770e8400-e29b-41d4-a716-446655440002""
}")
                            }
                        }
                    },
                    ["400"] = new OpenApiResponse
                    {
                        Description = "Datos inválidos o validación fallida",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
  ""title"": ""Bad Request"",
  ""status"": 400,
  ""detail"": ""Uno o más errores de validación ocurrieron"",
  ""errors"": {
    ""Nombre"": [""El nombre del evento es requerido.""],
    ""Fecha"": [""La fecha del evento debe ser en el futuro.""],
    ""Secciones"": [""El evento debe tener al menos una sección.""]
  }
}")
                            }
                        }
                    }
                }
            })
            .Produces<CrearEventoCommandResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // POST /api/eventos/{id}/publicar
            eventos.MapPost("/{id:guid}/publicar", async (Guid id, PublicarEventoCommand command, IMediator mediator) =>
            {
                var enriched = command with { EventoId = id };
                await mediator.Send(enriched);
                return Results.Ok();
            })
            .WithName("PublicarEvento")
            .WithSummary("Publica un evento existente")
            .WithDescription("Publica un evento que se encuentra en estado 'PendientePago' después de confirmar el pago. " +
                             "El evento debe tener un pago confirmado asociado. " +
                             "Una vez publicado, el evento cambia a estado 'Publicado' y queda visible en el catálogo.")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Publica un evento existente",
                Description = "Publica un evento que se encuentra en estado 'PendientePago' después de confirmar el pago. " +
                             "El evento debe tener un pago confirmado asociado. " +
                             "Una vez publicado, el evento cambia a estado 'Publicado' y queda visible en el catálogo.",
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = "Identificador único del evento a publicar (GUID)",
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "uuid"
                        },
                        Example = new Microsoft.OpenApi.Any.OpenApiString("770e8400-e29b-41d4-a716-446655440002")
                    }
                },
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "Identificador del pago confirmado",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new()
                        {
                            Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""pagoConfirmadoId"": ""aa0e8400-e29b-41d4-a716-446655440005""
}")
                        }
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Evento publicado exitosamente"
                    },
                    ["400"] = new OpenApiResponse
                    {
                        Description = "Evento no puede ser publicado (estado inválido o pago no confirmado)",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
  ""title"": ""Bad Request"",
  ""status"": 400,
  ""detail"": ""El evento no puede ser publicado porque no está en estado PendientePago o el pago no ha sido confirmado.""
}")
                            }
                        }
                    },
                    ["404"] = new OpenApiResponse
                    {
                        Description = "Evento no encontrado",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.4"",
  ""title"": ""Not Found"",
  ""status"": 404,
  ""detail"": ""El evento con el ID especificado no fue encontrado.""
}")
                            }
                        }
                    }
                }
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            // PUT /api/eventos/{id}
            eventos.MapPut("/{id:guid}", async (Guid id, EditarEventoCommand command, IMediator mediator) =>
            {
                var enriched = command with { EventoId = id };
                await mediator.Send(enriched);
                return Results.NoContent();
            })
            .WithName("EditarEvento")
            .WithSummary("Edita un evento en estado borrador")
            .WithDescription("Actualiza los datos de un evento que se encuentra en estado 'Borrador'. " +
                             "Solo los eventos en estado borrador pueden ser editados. " +
                             "Para editar secciones existentes, incluye el ID de la sección. " +
                             "Para agregar nuevas secciones, omite el ID.")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Edita un evento en estado borrador",
                Description = "Actualiza los datos de un evento que se encuentra en estado 'Borrador'. " +
                             "Solo los eventos en estado borrador pueden ser editados. " +
                             "Para editar secciones existentes, incluye el ID de la sección. " +
                             "Para agregar nuevas secciones, omite el ID.",
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = "Identificador único del evento a editar (GUID)",
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "uuid"
                        },
                        Example = new Microsoft.OpenApi.Any.OpenApiString("770e8400-e29b-41d4-a716-446655440002")
                    }
                },
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "Datos actualizados del evento",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new()
                        {
                            Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""nombre"": ""Concierto Rock 2025 - Actualizado"",
  ""descripcion"": ""Gran concierto de rock con artistas internacionales - Edición especial"",
  ""fecha"": ""2025-12-20T20:00:00Z"",
  ""horasDuracion"": 4,
  ""minutosDuracion"": 30,
  ""categoria"": ""Música y Entretenimiento"",
  ""secciones"": [
    {
      ""id"": ""880e8400-e29b-41d4-a716-446655440003"",
      ""nombre"": ""VIP Premium"",
      ""capacidad"": 150,
      ""precio"": 200.00
    },
    {
      ""nombre"": ""General"",
      ""capacidad"": 600,
      ""precio"": 80.00
    }
  ]
}")
                        }
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["204"] = new OpenApiResponse
                    {
                        Description = "Evento actualizado exitosamente"
                    },
                    ["400"] = new OpenApiResponse
                    {
                        Description = "Datos inválidos o evento no está en estado borrador",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
  ""title"": ""Bad Request"",
  ""status"": 400,
  ""detail"": ""El evento no puede ser editado porque no está en estado Borrador.""
}")
                            }
                        }
                    },
                    ["404"] = new OpenApiResponse
                    {
                        Description = "Evento no encontrado",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.4"",
  ""title"": ""Not Found"",
  ""status"": 404,
  ""detail"": ""El evento con el ID especificado no fue encontrado.""
}")
                            }
                        }
                    }
                }
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            // POST /api/eventos/{id}/pagar-publicacion
            eventos.MapPost("/{id:guid}/pagar-publicacion", async (Guid id, PagarPublicacionCommand command, IMediator mediator) =>
            {
                var enriched = command with { EventoId = id };
                await mediator.Send(enriched);
                return Results.Accepted($"/api/eventos/{id}");
            })
            .WithName("PagarPublicacionEvento")
            .WithSummary("Inicia el pago de publicación del evento")
            .WithDescription("Inicia el proceso de pago de publicación de un evento. " +
                             "El evento debe estar en estado 'Borrador'. " +
                             "El monto debe coincidir con la tarifa de publicación configurada en el evento. " +
                             "Después de este paso, el evento pasa a estado 'PendientePago' hasta que se confirme el pago.")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Inicia el pago de publicación del evento",
                Description = "Inicia el proceso de pago de publicación de un evento. " +
                             "El evento debe estar en estado 'Borrador'. " +
                             "El monto debe coincidir con la tarifa de publicación configurada en el evento. " +
                             "Después de este paso, el evento pasa a estado 'PendientePago' hasta que se confirme el pago.",
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = "Identificador único del evento (GUID)",
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "uuid"
                        },
                        Example = new Microsoft.OpenApi.Any.OpenApiString("770e8400-e29b-41d4-a716-446655440002")
                    }
                },
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "Información de la transacción de pago",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new()
                        {
                            Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""transaccionPagoId"": ""990e8400-e29b-41d4-a716-446655440004"",
  ""monto"": 50.00
}")
                        }
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["202"] = new OpenApiResponse
                    {
                        Description = "Pago iniciado, proceso en curso",
                        Headers = new Dictionary<string, OpenApiHeader>
                        {
                            ["Location"] = new OpenApiHeader
                            {
                                Description = "URL del evento",
                                Schema = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "uri"
                                },
                                Example = new Microsoft.OpenApi.Any.OpenApiString("/api/eventos/770e8400-e29b-41d4-a716-446655440002")
                            }
                        }
                    },
                    ["400"] = new OpenApiResponse
                    {
                        Description = "Datos inválidos, monto no coincide con tarifa o evento no está en estado borrador",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
  ""title"": ""Bad Request"",
  ""status"": 400,
  ""detail"": ""El monto abonado no coincide con la tarifa de publicación configurada.""
}")
                            }
                        }
                    },
                    ["404"] = new OpenApiResponse
                    {
                        Description = "Evento no encontrado",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.4"",
  ""title"": ""Not Found"",
  ""status"": 404,
  ""detail"": ""El evento con el ID especificado no fue encontrado.""
}")
                            }
                        }
                    }
                }
            })
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            // POST /api/eventos/{id}/iniciar
            eventos.MapPost("/{id:guid}/iniciar", async (Guid id, IMediator mediator) =>
            {
                var command = new IniciarEventoCommand { EventoId = id };
                await mediator.Send(command);
                return Results.Ok();
            })
            .WithName("IniciarEvento")
            .WithSummary("Marca un evento publicado como en curso")
            .WithDescription("Marca un evento publicado como 'EnCurso'. " +
                             "El evento debe estar en estado 'Publicado' para poder ser iniciado. " +
                             "Una vez iniciado, el evento queda marcado como en ejecución.")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Marca un evento publicado como en curso",
                Description = "Marca un evento publicado como 'EnCurso'. " +
                             "El evento debe estar en estado 'Publicado' para poder ser iniciado. " +
                             "Una vez iniciado, el evento queda marcado como en ejecución.",
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = "Identificador único del evento a iniciar (GUID)",
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "uuid"
                        },
                        Example = new Microsoft.OpenApi.Any.OpenApiString("770e8400-e29b-41d4-a716-446655440002")
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Evento iniciado exitosamente"
                    },
                    ["400"] = new OpenApiResponse
                    {
                        Description = "Evento no puede ser iniciado (estado inválido)",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
  ""title"": ""Bad Request"",
  ""status"": 400,
  ""detail"": ""El evento no puede ser iniciado porque no está en estado Publicado.""
}")
                            }
                        }
                    },
                    ["404"] = new OpenApiResponse
                    {
                        Description = "Evento no encontrado",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.4"",
  ""title"": ""Not Found"",
  ""status"": 404,
  ""detail"": ""El evento con el ID especificado no fue encontrado.""
}")
                            }
                        }
                    }
                }
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            // POST /api/eventos/{id}/finalizar
            eventos.MapPost("/{id:guid}/finalizar", async (Guid id, IMediator mediator) =>
            {
                var command = new FinalizarEventoCommand { EventoId = id };
                await mediator.Send(command);
                return Results.Ok();
            })
            .WithName("FinalizarEvento")
            .WithSummary("Finaliza un evento en curso")
            .WithDescription("Finaliza un evento que se encuentra en estado 'EnCurso'. " +
                             "El evento debe estar en estado 'EnCurso' para poder ser finalizado. " +
                             "Una vez finalizado, el evento cambia a estado 'Finalizado' y no puede ser modificado.")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Finaliza un evento en curso",
                Description = "Finaliza un evento que se encuentra en estado 'EnCurso'. " +
                             "El evento debe estar en estado 'EnCurso' para poder ser finalizado. " +
                             "Una vez finalizado, el evento cambia a estado 'Finalizado' y no puede ser modificado.",
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = "Identificador único del evento a finalizar (GUID)",
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "uuid"
                        },
                        Example = new Microsoft.OpenApi.Any.OpenApiString("770e8400-e29b-41d4-a716-446655440002")
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Evento finalizado exitosamente"
                    },
                    ["400"] = new OpenApiResponse
                    {
                        Description = "Evento no puede ser finalizado (estado inválido)",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
  ""title"": ""Bad Request"",
  ""status"": 400,
  ""detail"": ""El evento no puede ser finalizado porque no está en estado EnCurso.""
}")
                            }
                        }
                    },
                    ["404"] = new OpenApiResponse
                    {
                        Description = "Evento no encontrado",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.4"",
  ""title"": ""Not Found"",
  ""status"": 404,
  ""detail"": ""El evento con el ID especificado no fue encontrado.""
}")
                            }
                        }
                    }
                }
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            // GET /api/eventos/{id}
            eventos.MapGet("/{id:guid}", async (Guid id, IEventoRepository repository) =>
            {
                var evento = await repository.GetByIdAsync(id);
                if (evento == null) return Results.NotFound();
                return Results.Ok(EventoResponseDto.FromDomain(evento));
            })
            .WithName("ObtenerEvento")
            .WithSummary("Obtiene los detalles de un evento")
            .WithDescription("Recupera la información completa de un evento por su identificador único. " +
                             "Incluye todos los datos del evento, sus secciones, estado actual y fechas relevantes.")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Obtiene los detalles de un evento",
                Description = "Recupera la información completa de un evento por su identificador único. " +
                             "Incluye todos los datos del evento, sus secciones, estado actual y fechas relevantes.",
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = "Identificador único del evento (GUID)",
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "uuid"
                        },
                        Example = new Microsoft.OpenApi.Any.OpenApiString("550e8400-e29b-41d4-a716-446655440000")
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Evento encontrado exitosamente",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""id"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""nombre"": ""Concierto Rock 2025"",
  ""descripcion"": ""Gran concierto de rock con artistas internacionales"",
  ""fecha"": ""2025-12-15T20:00:00Z"",
  ""horasDuracion"": 3,
  ""minutosDuracion"": 0,
  ""estado"": ""Publicado"",
  ""organizadorId"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""venueId"": ""660e8400-e29b-41d4-a716-446655440001"",
  ""categoria"": ""Música"",
  ""tarifaPublicacion"": 50.00,
  ""transaccionPagoId"": ""990e8400-e29b-41d4-a716-446655440004"",
  ""fechaCreacion"": ""2025-01-15T10:00:00Z"",
  ""fechaPublicacion"": ""2025-01-16T14:30:00Z"",
  ""version"": 1,
  ""secciones"": [
    {
      ""id"": ""880e8400-e29b-41d4-a716-446655440003"",
      ""nombre"": ""VIP"",
      ""capacidad"": 100,
      ""precio"": 150.00
    },
    {
      ""id"": ""880e8400-e29b-41d4-a716-446655440004"",
      ""nombre"": ""General"",
      ""capacidad"": 500,
      ""precio"": 75.00
    }
  ]
}")
                            }
                        }
                    },
                    ["404"] = new OpenApiResponse
                    {
                        Description = "Evento no encontrado",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{
  ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.4"",
  ""title"": ""Not Found"",
  ""status"": 404,
  ""detail"": ""El evento con el ID especificado no fue encontrado.""
}")
                            }
                        }
                    }
                }
            })
            .Produces<EventoResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
