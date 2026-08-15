using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Predios
{
    public class BloqueConstruccionDto
    {
        public Guid Id { get; set; }
        public Guid PredioId { get; set; }
        public int NumeroBloque { get; set; }
        public Guid TipoEstructuraId { get; set; }
        public string NombreTipoEstructura { get; set; } = string.Empty;
        public Guid EstadoConservacionId { get; set; }
        public string NombreEstadoConservacion { get; set; } = string.Empty;
        public int NumeroPisos { get; set; }
        public decimal AreaConstruccion { get; set; }
        public int AnioConstruccion { get; set; }
        public bool EstadoActivo { get; set; }
    }
}
