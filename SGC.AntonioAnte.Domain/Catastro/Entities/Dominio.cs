using SGC.AntonioAnte.Domain.Common;
using SGC.AntonioAnte.Domain.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Catastro.Entities
{
    public class Dominio : AuditableEntity
    {
        [AuditDisplayName(nameof(Predio), nameof(Entities.Predio.ClaveCatastral))]
        public Guid PredioId { get; private set; }
        public virtual Predio Predio { get; private set; } = null!;

        [AuditDisplayName(nameof(Propietario), nameof(Entities.Propietario.Identificacion))] // Guardamos la Cédula/RUC en la auditoría
        public Guid PropietarioId { get; private set; }
        public virtual Propietario Propietario { get; private set; } = null!;

        [AuditDisplayName(nameof(TipoTenencia), nameof(Entities.TipoTenencia.Nombre))]
        public Guid TipoTenenciaId { get; private set; }
        public virtual TipoTenencia TipoTenencia { get; private set; } = null!;

        public decimal PorcentajePropiedad { get; private set; }
        public DateTime? FechaInscripcion { get; private set; }
        public string? Notaria { get; private set; }

        protected Dominio() { }

        public static Dominio Crear(Guid predioId, Guid propietarioId, Guid tipoTenenciaId, decimal porcentaje, DateTime? fechaInscripcion, string? notaria)
        {
            if (porcentaje <= 0 || porcentaje > 100)
                throw new ArgumentException("El porcentaje de propiedad debe estar entre 0.01 y 100.");

            return new Dominio
            {
                Id = Guid.NewGuid(),
                PredioId = predioId,
                PropietarioId = propietarioId,
                TipoTenenciaId = tipoTenenciaId,
                PorcentajePropiedad = porcentaje,
                FechaInscripcion = fechaInscripcion,
                Notaria = notaria
            };
        }
    }
}