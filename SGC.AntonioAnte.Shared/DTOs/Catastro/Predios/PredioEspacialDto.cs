using SGC.AntonioAnte.Domain.Catastro.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Predios
{
    public class PredioEspacialDto
    {
        public Guid Id { get; set; }
        public string ClaveCatastral { get; set; } = string.Empty;
        public string? ClaveAnterior { get; set; }
        public TipoPredio TipoPredio { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public decimal AreaTerrenoEscritura { get; set; }
        public decimal AreaTerrenoGrafica { get; set; }
        public string PoligonoWkt { get; set; } = string.Empty;
        public string PropietarioPrincipal { get; set; } = "Sin propietario asignado";
        public int TotalBloques { get; set; }
    }
}
