using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Common
{
    public class ResultadoPaginadoDto<T>
    {
        public List<T> Items { get; set; } = new();

        // 🔹 Alias de compatibilidad para evitar discrepancias entre "Items" y "Datos"
        public List<T> Datos => Items;

        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int RegistrosPorPagina { get; set; }
        public int TotalPaginas { get; set; }

        // 🔹 Propiedades calculadas para habilitar/deshabilitar botones en UI
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

        // 🔹 Creador de fábrica estático (Calcula TotalPaginas automáticamente)
        public static ResultadoPaginadoDto<T> Crear(List<T> items, int totalRegistros, int paginaActual, int registrosPorPagina)
        {
            var limiteValido = registrosPorPagina > 0 ? registrosPorPagina : 10;
            var paginaValida = paginaActual > 0 ? paginaActual : 1;
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)limiteValido);

            return new ResultadoPaginadoDto<T>
            {
                Items = items ?? new List<T>(),
                TotalRegistros = totalRegistros,
                PaginaActual = paginaValida,
                RegistrosPorPagina = limiteValido,
                TotalPaginas = totalPaginas
            };
        }
    }
}
