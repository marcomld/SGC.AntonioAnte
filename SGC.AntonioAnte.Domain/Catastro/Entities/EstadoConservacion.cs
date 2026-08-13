using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Catastro.Entities
{
    public class EstadoConservacion : CatalogoBase
    {
        private EstadoConservacion() { }
        public static EstadoConservacion Crear(string nombre, string descripcion) =>
            new() { Id = Guid.NewGuid(), Nombre = nombre, Descripcion = descripcion };
    }
}
