using SGC.AntonioAnte.Domain.Catastro.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Predios
{
    public class CreatePredioDto
    {
        [Required(ErrorMessage = "La clave catastral es totalmente obligatoria.")]
        public string ClaveCatastral { get; set; } = string.Empty;

        public string? ClaveAnterior { get; set; }

        [Required(ErrorMessage = "El tipo de predio es obligatorio (1 = Urbano, 2 = Rural).")]
        public TipoPredio TipoPredio { get; set; }

        [Range(0.01, 99999999.99, ErrorMessage = "El área de terreno según escritura debe ser mayor a 0.")]
        public decimal AreaTerrenoEscritura { get; set; }

        [Range(0.00, 99999999.99, ErrorMessage = "El área gráfica del terreno debe ser un valor válido.")]
        public decimal AreaTerrenoGrafica { get; set; }

        [Required(ErrorMessage = "La dirección física o referencia del predio es obligatoria.")]
        [StringLength(250, ErrorMessage = "La dirección no puede exceder los 250 caracteres.")]
        public string Direccion { get; set; } = string.Empty;

        /// <summary>
        /// Cadena de texto en formato WKT (Well-Known Text), ej: "POLYGON((-78.22 0.33, -78.21 0.33, -78.21 0.32, -78.22 0.32, -78.22 0.33))"
        /// </summary>
        public string? PoligonoWkt { get; set; }
    }
}
