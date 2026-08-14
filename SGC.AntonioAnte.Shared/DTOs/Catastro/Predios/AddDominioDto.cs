using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Predios
{
    public class AddDominioDto
    {
        [Required(ErrorMessage = "El ID del propietario es obligatorio.")]
        public Guid PropietarioId { get; set; }

        [Required(ErrorMessage = "El ID del tipo de tenencia es obligatorio.")]
        public Guid TipoTenenciaId { get; set; }

        [Range(0.01, 100.00, ErrorMessage = "El porcentaje de propiedad debe estar entre 0.01% y 100.00%.")]
        public decimal PorcentajePropiedad { get; set; }

        public DateTime? FechaInscripcion { get; set; }

        [StringLength(100, ErrorMessage = "El nombre o número de notaría no puede exceder los 100 caracteres.")]
        public string? Notaria { get; set; }
    }
}
