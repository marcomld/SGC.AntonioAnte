using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Avaluos.Simulacion
{
    // DTO Maestro que contiene el resumen financiero total
    public class AvaluoSimuladoDto
    {
        public Guid PredioId { get; set; }
        public int AnioFiscal { get; set; }

        // Desglose de Terreno
        public decimal AreaTerrenoTotal { get; set; }
        public decimal ValorMetroSuelo { get; set; }
        public decimal ValorTerrenoBase { get; set; } // Area * ValorMetro
        public decimal FactorAfectacionAplicado { get; set; } // Si hay castigos
        public decimal AvaluoSueloFinal { get; set; }

        // Desglose de Edificaciones
        public int TotalBloquesConstruidos { get; set; }
        public decimal AvaluoEdificacionesFinal { get; set; } // Suma de bloques

        // Gran Total
        public decimal BaseImponibleComercial { get; set; } // Suelo + Edificaciones

        // Lista detallada de cómo se calculó cada bloque
        public List<BloqueSimuladoDto> DesgloseBloques { get; set; } = new();
    }

    // DTO Detalle para la amortización de cada bloque constructivo
    public class BloqueSimuladoDto
    {
        public Guid BloqueId { get; set; }
        public string NumeroBloque { get; set; } = string.Empty;
        public decimal AreaConstruccion { get; set; }
        public string TipoEstructura { get; set; } = string.Empty;
        public decimal CostoBaseMetroCuadrado { get; set; }
        public decimal CostoReposicionTotal { get; set; } // Area * CostoBase

        // Depreciación
        public int EdadConstruccion { get; set; }
        public string EstadoConservacion { get; set; } = string.Empty;
        public decimal CoeficienteDepreciacion { get; set; }

        public decimal AvaluoBloqueFinal { get; set; } // CostoReposicion * Coeficiente
    }
}
