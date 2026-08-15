using SGC.AntonioAnte.Domain.Common;
using SGC.AntonioAnte.Domain.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Catastro.Entities
{
    public class BloqueConstruccion : AuditableEntity
    {
        [AuditDisplayName(nameof(Predio), nameof(Entities.Predio.ClaveCatastral))]
        public Guid PredioId { get; private set; }
        public virtual Predio Predio { get; private set; } = null!;

        public int NumeroBloque { get; private set; }

        [AuditDisplayName(nameof(TipoEstructura), nameof(Entities.TipoEstructura.Nombre))]
        public Guid TipoEstructuraId { get; private set; }
        public virtual TipoEstructura TipoEstructura { get; private set; } = null!;

        [AuditDisplayName(nameof(EstadoConservacion), nameof(Entities.EstadoConservacion.Nombre))]
        public Guid EstadoConservacionId { get; private set; }
        public virtual EstadoConservacion EstadoConservacion { get; private set; } = null!;

        public int NumeroPisos { get; private set; }
        public decimal AreaConstruccion { get; private set; }
        public int AnioConstruccion { get; private set; }

        protected BloqueConstruccion() { }

        public static BloqueConstruccion Crear(Guid predioId, int numeroBloque, Guid tipoEstructuraId, Guid estadoConservacionId, int numeroPisos, decimal areaConstruccion, int anioConstruccion)
        {
            return new BloqueConstruccion
            {
                Id = Guid.NewGuid(),
                PredioId = predioId,
                NumeroBloque = numeroBloque,
                TipoEstructuraId = tipoEstructuraId,
                EstadoConservacionId = estadoConservacionId,
                NumeroPisos = numeroPisos,
                AreaConstruccion = areaConstruccion,
                AnioConstruccion = anioConstruccion
            };
        }
    }
}
