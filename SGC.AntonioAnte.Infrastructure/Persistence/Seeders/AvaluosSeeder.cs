using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Domain.Avaluos.Entities;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Persistence.Seeders
{
    public static class AvaluosSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            int anioActual = 2026;

            // 1. Precios Base de Suelo (Atuntaqui)
            if (!await context.PreciosBaseSuelo.AnyAsync(p => p.AnioFiscal == anioActual))
            {
                context.PreciosBaseSuelo.AddRange(
                    PrecioBaseSuelo.Crear(anioActual, TipoPredio.Urbano, "01", "01", "001", 45.50m), // Lote céntrico
                    PrecioBaseSuelo.Crear(anioActual, TipoPredio.Urbano, "01", "02", "000", 30.00m), // Lote periférico
                    PrecioBaseSuelo.Crear(anioActual, TipoPredio.Rural, "02", "01", "000", 12.00m)   // Finca
                );
            }

            // Extraemos los catálogos físicos para armar las llaves foráneas
            var tiposEstructura = await context.TiposEstructura.ToListAsync();
            var estadosConservacion = await context.EstadosConservacion.ToListAsync();

            if (!tiposEstructura.Any() || !estadosConservacion.Any())
                return; // Si no hay catálogos físicos sembrados previamente, detenemos la siembra financiera.

            // 2. Costos de Reposición (Valor del metro cuadrado por material)
            if (!await context.CostosReposicion.AnyAsync(c => c.AnioFiscal == anioActual))
            {
                // Buscamos los IDs de forma flexible por si el texto varía ligeramente
                var estHormigon = tiposEstructura.FirstOrDefault(e => e.Nombre.Contains("Hormigón") || e.Nombre.Contains("Concreto"))?.Id;
                var estMixta = tiposEstructura.FirstOrDefault(e => e.Nombre.Contains("Mixta"))?.Id;
                var estAdobe = tiposEstructura.FirstOrDefault(e => e.Nombre.Contains("Adobe") || e.Nombre.Contains("Madera"))?.Id;

                if (estHormigon.HasValue) context.CostosReposicion.Add(CostoReposicion.Crear(anioActual, estHormigon.Value, 280.00m));
                if (estMixta.HasValue) context.CostosReposicion.Add(CostoReposicion.Crear(anioActual, estMixta.Value, 150.00m));
                if (estAdobe.HasValue) context.CostosReposicion.Add(CostoReposicion.Crear(anioActual, estAdobe.Value, 85.00m));
            }

            // 3. Matriz de Depreciación (Castigo por edad y estado físico)
            if (!await context.FactoresDepreciacion.AnyAsync(f => f.AnioFiscal == anioActual))
            {
                var estBueno = estadosConservacion.FirstOrDefault(e => e.Nombre.Contains("Bueno"))?.Id;
                var estRegular = estadosConservacion.FirstOrDefault(e => e.Nombre.Contains("Regular"))?.Id;
                var estMalo = estadosConservacion.FirstOrDefault(e => e.Nombre.Contains("Malo"))?.Id;

                if (estBueno.HasValue)
                {
                    context.FactoresDepreciacion.Add(FactorDepreciacion.Crear(anioActual, estBueno.Value, 0, 10, 1.0000m));  // Nuevo: Sin castigo
                    context.FactoresDepreciacion.Add(FactorDepreciacion.Crear(anioActual, estBueno.Value, 11, 25, 0.9000m)); // Pierde 10%
                    context.FactoresDepreciacion.Add(FactorDepreciacion.Crear(anioActual, estBueno.Value, 26, 100, 0.8000m));
                }
                if (estRegular.HasValue)
                {
                    context.FactoresDepreciacion.Add(FactorDepreciacion.Crear(anioActual, estRegular.Value, 0, 10, 0.8500m));
                    context.FactoresDepreciacion.Add(FactorDepreciacion.Crear(anioActual, estRegular.Value, 11, 25, 0.7000m));
                    context.FactoresDepreciacion.Add(FactorDepreciacion.Crear(anioActual, estRegular.Value, 26, 100, 0.5500m));
                }
                if (estMalo.HasValue)
                {
                    context.FactoresDepreciacion.Add(FactorDepreciacion.Crear(anioActual, estMalo.Value, 0, 10, 0.6000m));   // Ya nace perdiendo valor
                    context.FactoresDepreciacion.Add(FactorDepreciacion.Crear(anioActual, estMalo.Value, 11, 25, 0.4000m));
                    context.FactoresDepreciacion.Add(FactorDepreciacion.Crear(anioActual, estMalo.Value, 26, 100, 0.2000m)); // Pierde 80%
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
