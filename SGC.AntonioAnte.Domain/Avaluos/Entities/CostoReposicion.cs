using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Avaluos.Entities
{
    public class CostoReposicion : AuditableEntity
    {
        public int AnioFiscal { get; private set; }
        public Guid TipoEstructuraId { get; private set; }
        public virtual TipoEstructura? TipoEstructura { get; private set; }
        public decimal ValorMetroCuadrado { get; private set; }

        private CostoReposicion() { }

        public static CostoReposicion Crear(int anioFiscal, Guid tipoEstructuraId, decimal valorMetroCuadrado)
        {
            return new CostoReposicion
            {
                Id = Guid.NewGuid(),
                AnioFiscal = anioFiscal,
                TipoEstructuraId = tipoEstructuraId,
                ValorMetroCuadrado = valorMetroCuadrado,
                EstadoActivo = true
            };
        }

        public void Actualizar(int anioFiscal, Guid tipoEstructuraId, decimal valorMetroCuadrado)
        {
            AnioFiscal = anioFiscal;
            TipoEstructuraId = tipoEstructuraId;
            ValorMetroCuadrado = valorMetroCuadrado;
        }
    }
}
