using SGC.AntonioAnte.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Avaluos.Entities
{
    public class FactorAfectacion : AuditableEntity
    {
        public int AnioFiscal { get; private set; }
        public string TipoAfectacion { get; private set; } = string.Empty;
        public decimal Coeficiente { get; private set; }

        private FactorAfectacion() { }

        public static FactorAfectacion Crear(int anioFiscal, string tipoAfectacion, decimal coeficiente)
        {
            return new FactorAfectacion
            {
                Id = Guid.NewGuid(),
                AnioFiscal = anioFiscal,
                TipoAfectacion = tipoAfectacion,
                Coeficiente = coeficiente,
                EstadoActivo = true
            };
        }
    }
}
