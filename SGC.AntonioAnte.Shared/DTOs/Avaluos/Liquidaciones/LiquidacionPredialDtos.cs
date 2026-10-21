using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Avaluos.Liquidaciones
{
    public class EmitirLiquidacionDto
    {
        [Required(ErrorMessage = "El ID del predio es obligatorio.")]
        public Guid PredioId { get; set; }

        [Required(ErrorMessage = "El año fiscal es obligatorio.")]
        public int AnioFiscal { get; set; }
    }
}
