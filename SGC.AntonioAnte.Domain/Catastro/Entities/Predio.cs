using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Domain.Catastro.ValueObjects;
using SGC.AntonioAnte.Domain.Common;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Catastro.Entities
{
    public class Predio : AuditableEntity
    {
        public ClaveCatastral ClaveCatastral { get; private set; } = null!;
        public string? ClaveAnterior { get; private set; }
        public TipoPredio TipoPredio { get; private set; }
        public decimal AreaTerrenoEscritura { get; private set; }
        public decimal AreaTerrenoGrafica { get; private set; }
        public Geometry? PoligonoEspacial { get; private set; }
        public string Direccion { get; private set; } = string.Empty;

        public virtual ICollection<Dominio> Dominios { get; private set; } = new List<Dominio>();
        public virtual ICollection<BloqueConstruccion> Bloques { get; private set; } = new List<BloqueConstruccion>();

        protected Predio() { }

        public static Predio Crear(ClaveCatastral clave, string? claveAnterior, TipoPredio tipoPredio, decimal areaEscritura, decimal areaGrafica, string direccion, Geometry? poligono)
        {
            return new Predio
            {
                Id = Guid.NewGuid(),
                ClaveCatastral = clave,
                ClaveAnterior = claveAnterior,
                TipoPredio = tipoPredio,
                AreaTerrenoEscritura = areaEscritura,
                AreaTerrenoGrafica = areaGrafica,
                Direccion = direccion,
                PoligonoEspacial = poligono
            };
        }

        public void AgregarBloque(BloqueConstruccion bloque)
        {
            Bloques.Add(bloque);
        }

        public void AgregarDominio(Dominio dominio)
        {
            Dominios.Add(dominio);
        }
    }
}
