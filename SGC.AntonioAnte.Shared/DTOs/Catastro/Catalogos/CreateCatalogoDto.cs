using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Catalogos
{
    public class CreateCatalogoDto
    {
        [Required(ErrorMessage = "El nombre del catálogo es completamente obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del catálogo no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
