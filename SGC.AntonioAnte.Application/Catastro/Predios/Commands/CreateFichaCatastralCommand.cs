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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Predios.Commands
{
    // 1. El Comando recibe el DTO completo
    public record CreateFichaCatastralCommand(CreateFichaCatastralDto Ficha) : IRequest<OperacionResultadoDto>;

    // 2. Validación de Negocio Estricta
    public class CreateFichaCatastralCommandValidator : AbstractValidator<CreateFichaCatastralCommand>
    {
        public CreateFichaCatastralCommandValidator()
        {
            RuleFor(v => v.Ficha.Predio)
                .NotNull().WithMessage("Los datos del predio base son obligatorios.");

            RuleFor(v => v.Ficha.Dominios)
                .NotEmpty().WithMessage("Debe registrar al menos un dominio/propietario.")
                .Must(dominios => dominios.Sum(d => d.PorcentajePropiedad) == 100.00m)
                .WithMessage("La suma de los porcentajes de propiedad debe ser exactamente 100.00%.");
        }
    }

    // 3. El Manejador Transaccional
    public class CreateFichaCatastralCommandHandler : IRequestHandler<CreateFichaCatastralCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CreateFichaCatastralCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(CreateFichaCatastralCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Ficha.Predio;

            try
            {
                // A. Validar Clave Catastral Única
                var claveVO = ClaveCatastral.Crear(dto.ClaveCatastral);
                var existe = await _context.Predios
                    .AnyAsync(p => p.ClaveCatastral == claveVO && p.EstadoActivo, cancellationToken);

                if (existe)
                    return OperacionResultadoDto.Fallo($"Ya existe un predio activo con la clave catastral '{claveVO.Valor}'.");

                // B. Parsear Geometría WKT (si existe)
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

                // C. Crear Entidad Predio Base (La entidad ya genera su propio Guid en el método Crear)
                var nuevoPredio = Predio.Crear(
                    claveVO,
                    dto.ClaveAnterior?.Trim(),
                    dto.TipoPredio,
                    dto.AreaTerrenoEscritura,
                    dto.AreaTerrenoGrafica,
                    dto.Direccion.Trim(),
                    poligonoGeometry
                );

                _context.Predios.Add(nuevoPredio);

                // D. Vincular Dominios mediante el Factory Method del Dominio
                foreach (var dom in request.Ficha.Dominios)
                {
                    var nuevoDominio = Dominio.Crear(
                        predioId: nuevoPredio.Id, // Usamos el ID generado en el paso C
                        propietarioId: dom.PropietarioId,
                        tipoTenenciaId: dom.TipoTenenciaId,
                        porcentaje: dom.PorcentajePropiedad,
                        fechaInscripcion: dom.FechaInscripcion ?? DateTime.UtcNow,
                        notaria: dom.Notaria
                    );

                    _context.Dominios.Add(nuevoDominio);
                }

                // E. Vincular Bloques Constructivos mediante el Factory Method del Bloque
                if (request.Ficha.Bloques != null && request.Ficha.Bloques.Any())
                {
                    foreach (var b in request.Ficha.Bloques)
                    {
                        var nuevoBloque = BloqueConstruccion.Crear(
                            predioId: nuevoPredio.Id, // Usamos el ID generado en el paso C
                            numeroBloque: b.NumeroBloque,
                            tipoEstructuraId: b.TipoEstructuraId,
                            estadoConservacionId: b.EstadoConservacionId,
                            numeroPisos: b.NumeroPisos,
                            areaConstruccion: b.AreaConstruccion,
                            anioConstruccion: b.AnioConstruccion
                        );

                        _context.BloquesConstruccion.Add(nuevoBloque);
                    }
                }

                // F. EJECUCIÓN ATÓMICA (Se guarda el predio, dominios y bloques al mismo tiempo)
                await _context.SaveChangesAsync(cancellationToken);

                return OperacionResultadoDto.Exito("Ficha catastral integral registrada exitosamente.", nuevoPredio.Id);
            }
            catch (ArgumentException ex)
            {
                return OperacionResultadoDto.Fallo(ex.Message);
            }
            catch (Exception ex)
            {
                return OperacionResultadoDto.Fallo($"Error transaccional al procesar la ficha: {ex.Message}");
            }
        }
    }
}
