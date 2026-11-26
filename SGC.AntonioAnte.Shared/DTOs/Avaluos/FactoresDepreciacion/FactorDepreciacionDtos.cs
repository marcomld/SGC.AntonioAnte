using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresDepreciacion
{
    public class FactorDepreciacionDto
    {
        public Guid Id { get; set; }
        public int AnioFiscal { get; set; }
        public Guid EstadoConservacionId { get; set; }
        public string EstadoConservacionNombre { get; set; } = string.Empty;
        public int EdadMinima { get; set; }
        public int EdadMaxima { get; set; }
        public decimal Coeficiente { get; set; }
        public bool EstadoActivo { get; set; }
    }

    public class CreateFactorDepreciacionDto
    {
        [Required(ErrorMessage = "El año fiscal es obligatorio.")]
        public int AnioFiscal { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el estado de conservación.")]
        public Guid EstadoConservacionId { get; set; }

        [Required(ErrorMessage = "La edad mínima es obligatoria.")]
        [Range(0, 200, ErrorMessage = "La edad mínima debe estar entre 0 y 200 años.")]
        public int EdadMinima { get; set; }

        [Required(ErrorMessage = "La edad máxima es obligatoria.")]
        [Range(0, 200, ErrorMessage = "La edad máxima debe estar entre 0 y 200 años.")]
        public int EdadMaxima { get; set; }

        [Required(ErrorMessage = "El coeficiente es obligatorio.")]
        [Range(0.0001, 1.0000, ErrorMessage = "El coeficiente debe estar entre 0.0001 y 1.0000.")]
        public decimal Coeficiente { get; set; }
    }

    public class UpdateFactorDepreciacionDto : CreateFactorDepreciacionDto
    {
        public Guid Id { get; set; }
    }
}
