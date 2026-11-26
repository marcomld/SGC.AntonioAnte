using SGC.AntonioAnte.Domain.Catastro.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Avaluos.PreciosBaseSuelo
{
    public class PrecioBaseSueloDto
    {
        public Guid Id { get; set; }
        public int AnioFiscal { get; set; }
        public TipoPredio TipoPredio { get; set; }
        public string Zona { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string ManzanaPoligono { get; set; } = string.Empty;
        public decimal ValorMetroCuadrado { get; set; }
        public bool EstadoActivo { get; set; }
    }

    public class CreatePrecioBaseSueloDto
    {
        [Required(ErrorMessage = "El año fiscal es obligatorio.")]
        public int AnioFiscal { get; set; }

        [Required(ErrorMessage = "El tipo de predio es obligatorio.")]
        public TipoPredio TipoPredio { get; set; }

        [Required(ErrorMessage = "La zona es obligatoria.")]
        [StringLength(10, ErrorMessage = "La zona no puede exceder los 10 caracteres.")]
        public string Zona { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sector es obligatorio.")]
        [StringLength(10, ErrorMessage = "El sector no puede exceder los 10 caracteres.")]
        public string Sector { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "La manzana o polígono no puede exceder los 20 caracteres.")]
        public string ManzanaPoligono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El valor por metro cuadrado es obligatorio.")]
        [Range(0.01, 99999.99, ErrorMessage = "El valor debe ser mayor a cero.")]
        public decimal ValorMetroCuadrado { get; set; }
    }

    public class UpdatePrecioBaseSueloDto : CreatePrecioBaseSueloDto
    {
        public Guid Id { get; set; }
    }
}
