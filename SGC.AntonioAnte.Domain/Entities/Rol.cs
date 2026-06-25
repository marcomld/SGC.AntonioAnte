using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Entities
{
    public class Rol : IdentityRole<Guid>
    {
        // Atributo extra para la gestión de roles en la UI
        public string Descripcion { get; set; } = string.Empty;
    }
}
