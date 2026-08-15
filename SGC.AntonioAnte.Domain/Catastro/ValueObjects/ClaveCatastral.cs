using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Catastro.ValueObjects
{
    public class ClaveCatastral
    {
        public string Valor { get; private set; } = string.Empty;

        private ClaveCatastral(string valor)
        {
            Valor = valor;
        }

        public static ClaveCatastral Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("La Clave Catastral no puede estar vacía.");

            var claveLimpia = valor.Replace("-", "").Replace(" ", "");

            if (!Regex.IsMatch(claveLimpia, @"^\d{19,28}$"))
                throw new ArgumentException("Formato de Clave Catastral inválido según norma MIDUVI. Debe contener entre 19 y 28 dígitos numéricos.");

            if (!claveLimpia.StartsWith("1002"))
                throw new ArgumentException("La clave debe pertenecer a la jurisdicción de Antonio Ante (Provincia 10, Cantón 02).");

            return new ClaveCatastral(claveLimpia);
        }

        protected ClaveCatastral() { }

        protected IEnumerable<object> GetEqualityComponents()
        {
            yield return Valor;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var other = (ClaveCatastral)obj;
            return Valor == other.Valor;
        }

        public override int GetHashCode()
        {
            return Valor.GetHashCode();
        }

        public override string ToString() => Valor;
    }
}
