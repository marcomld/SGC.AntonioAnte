using SGC.AntonioAnte.Domain.Catastro.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Predios
{
    public class PredioDto
    {
        public Guid Id { get; set; }
        public string ClaveCatastral { get; set; } = string.Empty;
        public string? ClaveAnterior { get; set; }
        public TipoPredio TipoPredio { get; set; }
        public decimal AreaTerrenoEscritura { get; set; }
        public decimal AreaTerrenoGrafica { get; set; }
        public string? PoligonoWkt { get; set; } // Representación Geoespacial WKT para GIS/ArcGIS
        public string Direccion { get; set; } = string.Empty;
        public bool EstadoActivo { get; set; }

        public List<DominioDto> Dominios { get; set; } = new();
        public List<BloqueConstruccionDto> Bloques { get; set; } = new();
    }
}
