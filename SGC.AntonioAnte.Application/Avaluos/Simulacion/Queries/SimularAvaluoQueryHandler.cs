using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Simulacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.Simulacion.Queries
{
    public class SimularAvaluoQueryHandler : IRequestHandler<SimularAvaluoQuery, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public SimularAvaluoQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(SimularAvaluoQuery request, CancellationToken cancellationToken)
        {
            // 1. Extraer los datos físicos del Predio usando la propiedad correcta "Bloques"
            var predio = await _context.Predios
                .Include(p => p.Bloques) // 🔹 Corregido de BloquesConstruccion a Bloques
                    .ThenInclude(b => b.TipoEstructura)
                .Include(p => p.Bloques) // 🔹 Corregido de BloquesConstruccion a Bloques
                    .ThenInclude(b => b.EstadoConservacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PredioId, cancellationToken);

            if (predio == null)
                return OperacionResultadoDto.Fallo("El predio solicitado no existe en el Catastro.");

            // 2. Extraer parámetros económicos del Año Fiscal (Módulo Avalúos)
            var precioSuelo = await _context.PreciosBaseSuelo
                .AsNoTracking()
                .Where(p => p.AnioFiscal == request.AnioFiscal && p.TipoPredio == predio.TipoPredio && p.EstadoActivo)
                .FirstOrDefaultAsync(cancellationToken);

            if (precioSuelo == null)
                return OperacionResultadoDto.Fallo($"No se ha parametrizado el Precio Base de Suelo para el año {request.AnioFiscal}.");

            var costosReposicion = await _context.CostosReposicion
                .AsNoTracking()
                .Where(c => c.AnioFiscal == request.AnioFiscal && c.EstadoActivo)
                .ToListAsync(cancellationToken);

            var factoresDepreciacion = await _context.FactoresDepreciacion
                .AsNoTracking()
                .Where(f => f.AnioFiscal == request.AnioFiscal && f.EstadoActivo)
                .ToListAsync(cancellationToken);

            // 3. 🧮 CÁLCULO MATEMÁTICO

            // A) Avalúo de Terreno
            var valorSueloFinal = predio.AreaTerrenoGrafica * precioSuelo.ValorMetroCuadrado;

            var simulacion = new AvaluoSimuladoDto
            {
                PredioId = predio.Id,
                AnioFiscal = request.AnioFiscal,
                AreaTerrenoTotal = predio.AreaTerrenoGrafica,
                ValorMetroSuelo = precioSuelo.ValorMetroCuadrado,
                ValorTerrenoBase = valorSueloFinal,
                FactorAfectacionAplicado = 1.00m,
                AvaluoSueloFinal = valorSueloFinal * 1.00m
            };

            // B) Avalúo de Edificaciones (Iterando sobre predio.Bloques)
            decimal totalEdificaciones = 0;

            foreach (var bloque in predio.Bloques) // 🔹 Iteramos sobre la colección correcta
            {
                var edad = request.AnioFiscal - bloque.AnioConstruccion;
                if (edad < 0) edad = 0;

                var costoBase = costosReposicion.FirstOrDefault(c => c.TipoEstructuraId == bloque.TipoEstructuraId)?.ValorMetroCuadrado ?? 0;
                var costoReposicionTotal = bloque.AreaConstruccion * costoBase;

                var factor = factoresDepreciacion.FirstOrDefault(f =>
                    f.EstadoConservacionId == bloque.EstadoConservacionId &&
                    f.EdadMinima <= edad && f.EdadMaxima >= edad);

                var coeficiente = factor?.Coeficiente ?? 1.00m;

                var avaluoBloqueFinal = costoReposicionTotal * coeficiente;
                totalEdificaciones += avaluoBloqueFinal;

                simulacion.DesgloseBloques.Add(new BloqueSimuladoDto
                {
                    BloqueId = bloque.Id,
                    NumeroBloque = bloque.NumeroBloque.ToString(),
                    AreaConstruccion = bloque.AreaConstruccion,
                    TipoEstructura = bloque.TipoEstructura?.Nombre ?? "Desconocida",
                    CostoBaseMetroCuadrado = costoBase,
                    CostoReposicionTotal = costoReposicionTotal,
                    EdadConstruccion = edad,
                    EstadoConservacion = bloque.EstadoConservacion?.Nombre ?? "Desconocido",
                    CoeficienteDepreciacion = coeficiente,
                    AvaluoBloqueFinal = Math.Round(avaluoBloqueFinal, 2)
                });
            }

            simulacion.TotalBloquesConstruidos = predio.Bloques.Count; // 🔹 Conteo correcto
            simulacion.AvaluoEdificacionesFinal = Math.Round(totalEdificaciones, 2);

            // C) Gran Total Comercial
            simulacion.BaseImponibleComercial = Math.Round(simulacion.AvaluoSueloFinal + simulacion.AvaluoEdificacionesFinal, 2);

            // 4. Retornar el resultado empaquetado
            return OperacionResultadoDto.Exito("Simulación calculada correctamente.", null, simulacion);
        }
    }
}