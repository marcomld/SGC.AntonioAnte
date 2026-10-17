using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Catastro.Predios
{
    public class UpdateFichaCatastralDto
    {
        [Required(ErrorMessage = "El identificador de la ficha es obligatorio.")]
        public Guid Id { get; set; }

        // Reutilizamos el DTO de creación porque los campos del terreno son exactamente los mismos
        [Required]
        public CreatePredioDto Predio { get; set; } = new();

        [Required(ErrorMessage = "Debe mantener al menos un propietario vinculado.")]
        public List<UpdateDominioDto> Dominios { get; set; } = new();

        public List<UpdateBloqueDto> Bloques { get; set; } = new();
    }

    public class UpdateDominioDto
    {
        public Guid? Id { get; set; } // Null = Nuevo registro, Con Valor = Actualizar registro existente

        [Required(ErrorMessage = "El propietario es obligatorio.")]
        public Guid PropietarioId { get; set; }

        [Required(ErrorMessage = "El tipo de tenencia es obligatorio.")]
        public Guid TipoTenenciaId { get; set; }

        [Range(0.01, 100.00, ErrorMessage = "El porcentaje debe estar entre 0.01% y 100.00%")]
        public decimal PorcentajePropiedad { get; set; }

        public string? Notaria { get; set; }
        public DateTime? FechaInscripcion { get; set; }
    }

    public class UpdateBloqueDto
    {
        public Guid? Id { get; set; } // Null = Nuevo registro, Con Valor = Actualizar registro existente

        [Required]
        [Range(1, 99)]
        public int NumeroBloque { get; set; }

        [Required]
        public Guid TipoEstructuraId { get; set; }

        [Required]
        public Guid EstadoConservacionId { get; set; }

        [Required]
        [Range(1, 100)]
        public int NumeroPisos { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal AreaConstruccion { get; set; }

        [Required]
        public int AnioConstruccion { get; set; }
    }
}
