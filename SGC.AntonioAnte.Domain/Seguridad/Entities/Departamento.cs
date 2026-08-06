using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Seguridad.Entities
{
    public class Departamento
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool EstadoActivo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // 🔹 Nuevo: Relación inversa (Un departamento tiene muchos usuarios)
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
