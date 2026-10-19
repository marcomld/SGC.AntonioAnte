using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Avaluos.Entities
{
    public class FactorDepreciacion : AuditableEntity
    {
        public int AnioFiscal { get; private set; }
        public Guid EstadoConservacionId { get; private set; }
        public virtual EstadoConservacion? EstadoConservacion { get; private set; } // 🔹 Propiedad de navegación
        public int EdadMinima { get; private set; }
        public int EdadMaxima { get; private set; }
        public decimal Coeficiente { get; private set; }

        private FactorDepreciacion() { }

        public static FactorDepreciacion Crear(int anioFiscal, Guid estadoConservacionId, int edadMinima, int edadMaxima, decimal coeficiente)
        {
            return new FactorDepreciacion
            {
                Id = Guid.NewGuid(),
                AnioFiscal = anioFiscal,
                EstadoConservacionId = estadoConservacionId,
                EdadMinima = edadMinima,
                EdadMaxima = edadMaxima,
                Coeficiente = coeficiente,
                EstadoActivo = true
            };
        }
    }
}
