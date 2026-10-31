using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Common;
using System;

namespace SGC.AntonioAnte.Domain.Avaluos.Entities
{
    public class LiquidacionPredial : AuditableEntity
    {
        public Guid PredioId { get; private set; }
        public virtual Predio? Predio { get; private set; }
        public int AnioFiscal { get; private set; }
        public bool EsSimulacion { get; private set; }
        public decimal ValorTerreno { get; private set; }
        public decimal ValorConstruccion { get; private set; }
        public decimal BaseImponible { get; private set; }
        public decimal ExoneracionesPrevias { get; private set; }
        public decimal ValorEmision { get; private set; }
        public decimal CemAplicada { get; private set; }
        public string EstadoLiquidacion { get; private set; } = string.Empty;

        // 🔹 Nuevo: Rastro de auditoría para reliquidaciones
        public string? MotivoAnulacion { get; private set; }

        private LiquidacionPredial() { }

        public static LiquidacionPredial Crear(Guid predioId, int anioFiscal, bool esSimulacion, decimal valorTerreno, decimal valorConstruccion, decimal exoneracionesPrevias, decimal cemAplicada, string estado)
        {
            var baseImponible = valorTerreno + valorConstruccion;
            var emision = baseImponible - exoneracionesPrevias + cemAplicada;

            return new LiquidacionPredial
            {
                Id = Guid.NewGuid(),
                PredioId = predioId,
                AnioFiscal = anioFiscal,
                EsSimulacion = esSimulacion,
                ValorTerreno = valorTerreno,
                ValorConstruccion = valorConstruccion,
                BaseImponible = baseImponible,
                ExoneracionesPrevias = exoneracionesPrevias,
                CemAplicada = cemAplicada,
                ValorEmision = emision,
                EstadoLiquidacion = estado,
                EstadoActivo = true
            };
        }

        // 🔹 Nuevo: Comportamiento DDD para la baja tributaria
        public void AnularOSustituir(string motivo)
        {
            EstadoLiquidacion = "Sustituido";
            MotivoAnulacion = motivo;
        }
    }
}