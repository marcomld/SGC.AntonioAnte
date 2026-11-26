using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.AntonioAnte.Shared.DTOs.Avaluos.Liquidaciones
{
    public class EmitirLiquidacionDto
    {
        [Required(ErrorMessage = "El ID del predio es obligatorio.")]
        public Guid PredioId { get; set; }

        [Required(ErrorMessage = "El año fiscal es obligatorio.")]
        public int AnioFiscal { get; set; }
    }

    // 🔹 DTO para devolver lo que está vigente
    public class LiquidacionVigenteDto
    {
        public Guid Id { get; set; }
        public Guid PredioId { get; set; }
        public int AnioFiscal { get; set; }
        public decimal ValorTerreno { get; set; }
        public decimal ValorConstruccion { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal ValorEmision { get; set; }
        public string EstadoLiquidacion { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
    }

    // 🔹 DTO de entrada para solicitar reliquidación
    public class ReliquidarLiquidacionDto : EmitirLiquidacionDto
    {
        [Required(ErrorMessage = "Debe justificar el motivo de la reliquidación.")]
        [StringLength(500, ErrorMessage = "El motivo no puede exceder los 500 caracteres.")]
        public string MotivoReliquidacion { get; set; } = string.Empty;
    }
}