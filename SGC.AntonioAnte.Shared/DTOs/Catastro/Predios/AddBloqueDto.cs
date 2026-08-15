using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Predios
{
    public class AddBloqueDto
    {
        [Range(1, 99, ErrorMessage = "El número de bloque debe ser un entero positivo.")]
        public int NumeroBloque { get; set; }

        [Required(ErrorMessage = "El ID del tipo de estructura es obligatorio.")]
        public Guid TipoEstructuraId { get; set; }

        [Required(ErrorMessage = "El ID del estado de conservación es obligatorio.")]
        public Guid EstadoConservacionId { get; set; }

        [Range(1, 100, ErrorMessage = "El número de pisos debe ser mayor o igual a 1.")]
        public int NumeroPisos { get; set; }

        [Range(0.01, 99999999.99, ErrorMessage = "El área de construcción debe ser mayor a 0.")]
        public decimal AreaConstruccion { get; set; }

        [Range(1800, 2030, ErrorMessage = "Ingrese un año de construcción válido.")]
        public int AnioConstruccion { get; set; }
    }
}
