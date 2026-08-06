using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.Constants
{
    public static class Permissions
    {
        public const string ClaimType = "Permiso";

        // --- MÓDULO: CATASTRO ---
        public static class Catastro
        {
            public const string Lectura = "Catastro.Lectura";
            public const string Creacion = "Catastro.Creacion";
            public const string Edicion = "Catastro.Edicion";
            public const string Eliminacion = "Catastro.Eliminacion";
        }

        // --- MÓDULO: VISOR GIS ---
        public static class VisorGIS
        {
            public const string Lectura = "VisorGIS.Lectura";
            public const string Creacion = "VisorGIS.Creacion";
            public const string Edicion = "VisorGIS.Edicion";
            public const string Eliminacion = "VisorGIS.Eliminacion";
        }

        // --- MÓDULO: USUARIOS / NÓMINA ---
        public static class Usuarios
        {
            public const string Lectura = "Usuarios.Lectura";
            public const string Creacion = "Usuarios.Creacion";
            public const string Edicion = "Usuarios.Edicion";
            public const string Eliminacion = "Usuarios.Eliminacion";
        }

        // --- MÓDULO: ROLES Y PERMISOS ---
        public static class Roles
        {
            public const string Lectura = "Roles.Lectura";
            public const string Creacion = "Roles.Creacion";
            public const string Edicion = "Roles.Edicion";
            public const string Eliminacion = "Roles.Eliminacion";
        }

        // --- MÓDULO: DEPARTAMENTOS ---
        public static class Departamentos
        {
            public const string Lectura = "Departamentos.Lectura";
            public const string Creacion = "Departamentos.Creacion";
            public const string Edicion = "Departamentos.Edicion";
            public const string Eliminacion = "Departamentos.Eliminacion";
        }

        // --- MÓDULO: AUDITORÍA ---
        public static class Auditoria
        {
            public const string Lectura = "Auditoria.Lectura";
            public const string Creacion = "Auditoria.Creacion";
            public const string Edicion = "Auditoria.Edicion";
            public const string Eliminacion = "Auditoria.Eliminacion";
        }

        /// <summary>
        /// Genera el catálogo maestro completo para construir las matrices en las modales de la UI.
        /// </summary>
        public static List<PermissionDto> ObtenerCatalogoMaestro()
        {
            return new List<PermissionDto>
            {
                // Catastro
                CrearDto("Catastro", Catastro.Lectura, "Lectura", "Permite visualizar fichas catastrales e informes."),
                CrearDto("Catastro", Catastro.Creacion, "Creación", "Permite registrar nuevas predios e inmuebles."),
                CrearDto("Catastro", Catastro.Edicion, "Edición", "Permite actualizar linderos, avalúos y datos del predio."),
                CrearDto("Catastro", Catastro.Eliminacion, "Eliminación", "Permite dar de baja o anular fichas catastrales."),

                // Visor GIS
                CrearDto("Visor GIS", VisorGIS.Lectura, "Lectura", "Permite navegar por el mapa geoespacial del cantón."),
                CrearDto("Visor GIS", VisorGIS.Creacion, "Creación", "Permite trazar nuevas capas cartográficas y polígonos."),
                CrearDto("Visor GIS", VisorGIS.Edicion, "Edición", "Permite editar coordenadas y geometría de predios."),
                CrearDto("Visor GIS", VisorGIS.Eliminacion, "Eliminación", "Permite remover capas del servidor ArcGIS."),

                // Usuarios
                CrearDto("Usuarios", Usuarios.Lectura, "Lectura", "Permite consultar la nómina de funcionarios del GAD."),
                CrearDto("Usuarios", Usuarios.Creacion, "Creación", "Permite registrar nuevos funcionarios en el sistema."),
                CrearDto("Usuarios", Usuarios.Edicion, "Edición", "Permite actualizar datos institucionales del funcionario."),
                CrearDto("Usuarios", Usuarios.Eliminacion, "Eliminación", "Permite desactivar (Soft Delete) el acceso de un funcionario."),

                // Roles
                CrearDto("Roles", Roles.Lectura, "Lectura", "Permite ver el catálogo de roles institucionales."),
                CrearDto("Roles", Roles.Creacion, "Creación", "Permite definir nuevos roles en el sistema."),
                CrearDto("Roles", Roles.Edicion, "Edición", "Permite modificar la matriz de permisos de un rol."),
                CrearDto("Roles", Roles.Eliminacion, "Eliminación", "Permite eliminar roles personalizados no utilizados."),

                // Departamentos
                CrearDto("Departamentos", Departamentos.Lectura, "Lectura", "Permite consultar el organigrama de departamentos."),
                CrearDto("Departamentos", Departamentos.Creacion, "Creación", "Permite registrar nuevas dependencias municipales."),
                CrearDto("Departamentos", Departamentos.Edicion, "Edición", "Permite modificar nombres y jefaturas de departamento."),
                CrearDto("Departamentos", Departamentos.Eliminacion, "Eliminación", "Permite desactivar dependencias municipales."),

                // Auditoría
                CrearDto("Auditoría", Auditoria.Lectura, "Lectura", "Permite consultar la bitácora de eventos y deltas."),
                CrearDto("Auditoría", Auditoria.Creacion, "Creación", "Permite registrar entradas manuales de auditoría."),
                CrearDto("Auditoría", Auditoria.Edicion, "Edición", "Permite categorizar observaciones de auditoría."),
                CrearDto("Auditoría", Auditoria.Eliminacion, "Eliminación", "Permite depurar registros históricos de bitácora.")
            };
        }

        private static PermissionDto CrearDto(string modulo, string valorClaim, string accion, string descripcion)
        {
            return new PermissionDto
            {
                Modulo = modulo,
                TipoClaim = ClaimType,
                ValorClaim = valorClaim,
                Accion = accion,
                Descripcion = descripcion
            };
        }
    }
}
