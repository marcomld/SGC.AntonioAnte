using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Avaluos.CostosReposicion
{
    public class CostoReposicionDto
    {
        public Guid Id { get; set; }
        public int AnioFiscal { get; set; }
        public Guid TipoEstructuraId { get; set; }
        public string TipoEstructuraNombre { get; set; } = string.Empty;
        public decimal ValorMetroCuadrado { get; set; }
        public bool EstadoActivo { get; set; }
    }

    public class CreateCostoReposicionDto
    {
        [Required(ErrorMessage = "El año fiscal es obligatorio.")]
        public int AnioFiscal { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de estructura.")]
        public Guid TipoEstructuraId { get; set; }

        [Required(ErrorMessage = "El valor por metro cuadrado es obligatorio.")]
        [Range(0.01, 9999.99, ErrorMessage = "El valor debe ser mayor a cero.")]
        public decimal ValorMetroCuadrado { get; set; }
    }

    public class UpdateCostoReposicionDto : CreateCostoReposicionDto
    {
        public Guid Id { get; set; }
    }
}
