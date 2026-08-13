using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Catastro.Entities
{
    public class TipoEstructura : CatalogoBase
    {
        private TipoEstructura() { }
        public static TipoEstructura Crear(string nombre, string descripcion) =>
            new() { Id = Guid.NewGuid(), Nombre = nombre, Descripcion = descripcion };
    }
}
