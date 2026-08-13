using SGC.AntonioAnte.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Catastro.Entities
{
    public abstract class CatalogoBase : AuditableEntity
    {
        public string Nombre { get; protected set; } = string.Empty;
        public string Descripcion { get; protected set; } = string.Empty;

        // EF Core Constructor
        protected CatalogoBase() { }

        public void Actualizar(string nombre, string descripcion)
        {
            Nombre = nombre;
            Descripcion = descripcion;
        }
    }
}
