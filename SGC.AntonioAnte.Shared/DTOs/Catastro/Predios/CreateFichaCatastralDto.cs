using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Predios
{
    public class CreateFichaCatastralDto
    {
        [Required]
        public CreatePredioDto Predio { get; set; } = new();

        [Required(ErrorMessage = "Debe vincular al menos un propietario.")]
        public List<AddDominioDto> Dominios { get; set; } = new();

        public List<AddBloqueDto> Bloques { get; set; } = new();
    }
}
