using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Common.Attributes
{
    /// <summary>
    /// Atributo para indicar a la auditoría automática que reemplace una clave foránea (Guid/Int)
    /// por la propiedad legible de su entidad de navegación asociada.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AuditDisplayNameAttribute : Attribute
    {
        public string NavigationPropertyName { get; }
        public string DisplayPropertyName { get; }

        public AuditDisplayNameAttribute(string navigationPropertyName, string displayPropertyName = "Nombre")
        {
            NavigationPropertyName = navigationPropertyName;
            DisplayPropertyName = displayPropertyName;
        }
    }
}
