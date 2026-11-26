using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Avaluos.Entities
{
    public class PrecioBaseSuelo : AuditableEntity
    {
        public int AnioFiscal { get; private set; }
        public TipoPredio TipoPredio { get; private set; }
        public string Zona { get; private set; } = string.Empty;
        public string Sector { get; private set; } = string.Empty;
        public string ManzanaPoligono { get; private set; } = string.Empty;
        public decimal ValorMetroCuadrado { get; private set; }

        private PrecioBaseSuelo() { } // EF Core

        public static PrecioBaseSuelo Crear(int anioFiscal, TipoPredio tipoPredio, string zona, string sector, string manzanaPoligono, decimal valorMetroCuadrado)
        {
            return new PrecioBaseSuelo
            {
                Id = Guid.NewGuid(),
                AnioFiscal = anioFiscal,
                TipoPredio = tipoPredio,
                Zona = zona,
                Sector = sector,
                ManzanaPoligono = manzanaPoligono,
                ValorMetroCuadrado = valorMetroCuadrado,
                EstadoActivo = true
            };
        }

        public void Actualizar(int anioFiscal, TipoPredio tipoPredio, string zona, string sector, string manzanaPoligono, decimal valorMetroCuadrado)
        {
            AnioFiscal = anioFiscal;
            TipoPredio = tipoPredio;
            Zona = zona;
            Sector = sector;
            ManzanaPoligono = manzanaPoligono;
            ValorMetroCuadrado = valorMetroCuadrado;
        }
    }
}
