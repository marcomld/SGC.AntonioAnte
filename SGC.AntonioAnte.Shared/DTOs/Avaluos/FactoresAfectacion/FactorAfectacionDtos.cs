using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresAfectacion
{
    public class FactorAfectacionDto
    {
        public Guid Id { get; set; }
        public int AnioFiscal { get; set; }
        public string TipoAfectacion { get; set; } = string.Empty;
        public decimal Coeficiente { get; set; }
        public bool EstadoActivo { get; set; }
    }

    public class CreateFactorAfectacionDto
    {
        [Required(ErrorMessage = "El año fiscal es obligatorio.")]
        public int AnioFiscal { get; set; }

        [Required(ErrorMessage = "El tipo de afectación es obligatorio.")]
        [StringLength(100, ErrorMessage = "El tipo de afectación no puede exceder los 100 caracteres.")]
        public string TipoAfectacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El coeficiente es obligatorio.")]
        [Range(0.0001, 2.0000, ErrorMessage = "El coeficiente debe estar entre 0.0001 y 2.0000.")]
        public decimal Coeficiente { get; set; }
    }

    public class UpdateFactorAfectacionDto : CreateFactorAfectacionDto
    {
        public Guid Id { get; set; }
    }
}
