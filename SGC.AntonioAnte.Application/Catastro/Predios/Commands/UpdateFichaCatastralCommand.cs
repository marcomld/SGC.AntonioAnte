using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Catastro.ValueObjects;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Predios;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Predios.Commands
{
    public record UpdateFichaCatastralCommand(UpdateFichaCatastralDto Ficha) : IRequest<OperacionResultadoDto>;

    public class UpdateFichaCatastralCommandValidator : AbstractValidator<UpdateFichaCatastralCommand>
    {
        public UpdateFichaCatastralCommandValidator()
        {
            RuleFor(v => v.Ficha.Id).NotEmpty().WithMessage("El ID de la ficha es obligatorio.");
            RuleFor(v => v.Ficha.Predio).NotNull().WithMessage("Los datos del terreno son obligatorios.");
            RuleFor(v => v.Ficha.Dominios)
                .NotEmpty().WithMessage("Debe registrar al menos un dominio/propietario.")
                .Must(dominios => dominios.Sum(d => d.PorcentajePropiedad) == 100.00m)
                .WithMessage("La suma de los porcentajes de propiedad debe ser exactamente 100.00%.");
        }
    }

    public class UpdateFichaCatastralCommandHandler : IRequestHandler<UpdateFichaCatastralCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public UpdateFichaCatastralCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(UpdateFichaCatastralCommand request, CancellationToken cancellationToken)
        {
            // 1. Cargar el Predio con todas sus relaciones (Tracking activado)
            var predio = await _context.Predios
                .Include(p => p.Dominios)
                .Include(p => p.Bloques) // Ojo: Reemplaza "Bloques" por el nombre de tu propiedad de navegación en la entidad Predio.cs (Ej: BloquesConstruccion)
                .FirstOrDefaultAsync(p => p.Id == request.Ficha.Id, cancellationToken);

            if (predio == null)
                return OperacionResultadoDto.Fallo("La ficha catastral solicitada no existe.");

            var dto = request.Ficha.Predio;

            try
            {
                // 2. Validar que la nueva clave catastral no pertenezca a OTRO predio
                var claveVO = ClaveCatastral.Crear(dto.ClaveCatastral);
                var existeClave = await _context.Predios
                    .AnyAsync(p => p.ClaveCatastral == claveVO && p.Id != predio.Id && p.EstadoActivo, cancellationToken);

                if (existeClave)
                    return OperacionResultadoDto.Fallo($"La clave catastral '{claveVO.Valor}' ya está asignada a otro predio activo.");

                // 3. Parsear Geometría (Si cambió)
                Geometry? poligonoGeometry = null;
                if (!string.IsNullOrWhiteSpace(dto.PoligonoWkt))
                {
                    try
                    {
                        var reader = new WKTReader();
                        poligonoGeometry = reader.Read(dto.PoligonoWkt);
                        poligonoGeometry.SRID = 4326;
                    }
                    catch (Exception ex)
                    {
                        return OperacionResultadoDto.Fallo($"El formato espacial WKT ingresado no es válido: {ex.Message}");
                    }
                }

                // 4. Actualizar Datos Base del Terreno
                predio.ActualizarDatosTerreno(claveVO, dto.ClaveAnterior?.Trim(), dto.TipoPredio, dto.AreaTerrenoEscritura, dto.AreaTerrenoGrafica, dto.Direccion.Trim(), poligonoGeometry);

                // --- 5. LÓGICA DELTA PARA DOMINIOS ---
                var dominiosRecibidosIds = request.Ficha.Dominios.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToList();

                // A. Eliminar los que ya no vienen de Blazor
                var dominiosAEliminar = predio.Dominios.Where(d => !dominiosRecibidosIds.Contains(d.Id)).ToList();
                foreach (var dom in dominiosAEliminar)
                {
                    _context.Dominios.Remove(dom);
                }

                // B. Actualizar existentes y Agregar Nuevos
                foreach (var domDto in request.Ficha.Dominios)
                {
                    if (domDto.Id.HasValue) // Actualizar
                    {
                        var domExistente = predio.Dominios.FirstOrDefault(d => d.Id == domDto.Id.Value);
                        if (domExistente != null)
                        {
                            domExistente.Actualizar(domDto.TipoTenenciaId, domDto.PorcentajePropiedad, domDto.FechaInscripcion, domDto.Notaria);
                            // Nota: Si el PropietarioId puede cambiar en tu interfaz, deberás asignarlo directamente aquí si tu dominio lo permite.
                        }
                    }
                    else // Insertar nuevo
                    {
                        var nuevoDominio = Dominio.Crear(predio.Id, domDto.PropietarioId, domDto.TipoTenenciaId, domDto.PorcentajePropiedad, domDto.FechaInscripcion ?? DateTime.UtcNow, domDto.Notaria);
                        _context.Dominios.Add(nuevoDominio);
                    }
                }

                // --- 6. LÓGICA DELTA PARA BLOQUES CONSTRUCTIVOS ---
                var bloquesRecibidosIds = request.Ficha.Bloques.Where(b => b.Id.HasValue).Select(b => b.Id!.Value).ToList();

                var bloquesAEliminar = predio.Bloques.Where(b => !bloquesRecibidosIds.Contains(b.Id)).ToList(); // Cambiar .Bloques por tu propiedad de navegación
                foreach (var bloque in bloquesAEliminar)
                {
                    _context.BloquesConstruccion.Remove(bloque);
                }

                foreach (var bloqDto in request.Ficha.Bloques)
                {
                    if (bloqDto.Id.HasValue) // Actualizar
                    {
                        var bloqExistente = predio.Bloques.FirstOrDefault(b => b.Id == bloqDto.Id.Value); // Cambiar .Bloques
                        if (bloqExistente != null)
                        {
                            bloqExistente.Actualizar(bloqDto.NumeroBloque, bloqDto.TipoEstructuraId, bloqDto.EstadoConservacionId, bloqDto.NumeroPisos, bloqDto.AreaConstruccion, bloqDto.AnioConstruccion);
                        }
                    }
                    else // Insertar nuevo
                    {
                        var nuevoBloque = BloqueConstruccion.Crear(predio.Id, bloqDto.NumeroBloque, bloqDto.TipoEstructuraId, bloqDto.EstadoConservacionId, bloqDto.NumeroPisos, bloqDto.AreaConstruccion, bloqDto.AnioConstruccion);
                        _context.BloquesConstruccion.Add(nuevoBloque);
                    }
                }

                // 7. Guardar Transacción Atómica
                await _context.SaveChangesAsync(cancellationToken);

                return OperacionResultadoDto.Exito("Ficha catastral actualizada correctamente.", predio.Id);
            }
            catch (ArgumentException ex)
            {
                return OperacionResultadoDto.Fallo(ex.Message);
            }
            catch (Exception ex)
            {
                return OperacionResultadoDto.Fallo($"Error transaccional al actualizar la ficha: {ex.Message}");
            }
        }
    }
}