using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Catastro.Entities
{
    public class Propietario : AuditableEntity
    {
        public TipoPropietario TipoPropietario { get; private set; }
        public string Identificacion { get; private set; } = string.Empty;
        public string? Nombres { get; private set; }
        public string? Apellidos { get; private set; }
        public string? RazonSocial { get; private set; }
        public EstadoCivil EstadoCivil { get; private set; }
        public string? Email { get; private set; }
        public string? Telefono { get; private set; }

        public virtual ICollection<Dominio> Dominios { get; private set; } = new List<Dominio>();

        protected Propietario() { }

        public static Propietario CrearPersonaNatural(string identificacion, string nombres, string apellidos, EstadoCivil estadoCivil, string? email, string? telefono)
        {
            return new Propietario
            {
                Id = Guid.NewGuid(),
                TipoPropietario = TipoPropietario.Natural,
                Identificacion = identificacion,
                Nombres = nombres,
                Apellidos = apellidos,
                EstadoCivil = estadoCivil,
                Email = email,
                Telefono = telefono
            };
        }

        public static Propietario CrearPersonaJuridica(string ruc, string razonSocial, string? email, string? telefono)
        {
            return new Propietario
            {
                Id = Guid.NewGuid(),
                TipoPropietario = TipoPropietario.Juridico,
                Identificacion = ruc,
                RazonSocial = razonSocial,
                EstadoCivil = EstadoCivil.NoAplica,
                Email = email,
                Telefono = telefono
            };
        }

        // Método utilitario para devolver el nombre completo según el tipo, usado en [AuditDisplayName]
        public string ObtenerNombreCompleto() => TipoPropietario == TipoPropietario.Natural ? $"{Nombres} {Apellidos}" : RazonSocial!;
    }
}
