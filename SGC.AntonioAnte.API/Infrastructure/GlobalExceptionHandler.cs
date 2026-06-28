using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGC.AntonioAnte.Application.Common.Exceptions;
using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.API.Infrastructure
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // 1. CAPTURAR CUENTA BLOQUEADA (Fuerza Bruta Guard - HTTP 423)
            if (exception is LoginBloqueadoException lockoutException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status423Locked;

                var payloadBloqueo = new LoginBloqueadoDto
                {
                    CuentaBloqueada = true,
                    SegundosRestantes = lockoutException.SegundosRestantes,
                    MensajeError = lockoutException.Message
                };

                await httpContext.Response.WriteAsJsonAsync(payloadBloqueo, cancellationToken);
                return true;
            }

            // 2. CAPTURAR ERRORES DE REGLAS DE NEGOCIO (FluentValidation - HTTP 400)
            if (exception is ValidationException validationException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                var errores = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                var problemDetails = new ValidationProblemDetails(errores)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error en las reglas de negocio.",
                    Detail = "Uno o más errores de validación ocurrieron."
                };

                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
                return true;
            }

            // 3. CAPTURAR EXCEPCIONES GENERALES (HTTP 500)
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = exception.Message
            }, cancellationToken);

            return true;
        }
    }
}