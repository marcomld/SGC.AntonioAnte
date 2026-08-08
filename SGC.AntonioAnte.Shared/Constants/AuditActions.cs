using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.Constants
{
    public static class AuditActions
    {
        // =========================================================================
        // MÓDULO 1: SEGURIDAD, AUTENTICACIÓN Y ROLES
        // =========================================================================

        // Autenticación y Sesiones
        public const string LoginExitoso = "LOGIN_EXITOSO";
        public const string LoginFallido = "LOGIN_FALLIDO";
        public const string Logout = "LOGOUT";
        public const string SolicitarRecuperacionPassword = "SOLICITAR_RECUPERACION_PASSWORD";
        public const string RestablecerPassword = "RESTABLECER_PASSWORD";

        // Gestión de Usuarios
        public const string CrearUsuario = "CREAR_USUARIO";
        public const string ActualizarUsuario = "ACTUALIZAR_USUARIO";
        public const string ActivarUsuario = "ACTIVAR_USUARIO";
        public const string DesactivarUsuario = "DESACTIVAR_USUARIO";

        // Gestión de Roles y Permisos
        public const string CrearRol = "CREAR_ROL";
        public const string EliminarRol = "ELIMINAR_ROL";
        public const string AsignarRolUsuario = "ASIGNAR_ROL_USUARIO";
        public const string DesasignarRolUsuario = "DESASIGNAR_ROL_USUARIO";
        public const string AsignarPermisosRol = "ASIGNAR_PERMISOS_ROL";
        public const string AsignarClaimsUsuario = "ASIGNAR_CLAIMS_USUARIO";

        // Catálogo de Departamentos
        public const string CrearDepartamento = "CREAR_DEPARTAMENTO";
        public const string ActualizarDepartamento = "ACTUALIZAR_DEPARTAMENTO";
        public const string ActivarDepartamento = "ACTIVAR_DEPARTAMENTO";
        public const string DesactivarDepartamento = "DESACTIVAR_DEPARTAMENTO";
        public const string EliminarDepartamento = "ELIMINAR_DEPARTAMENTO";

        // =========================================================================
        // MÓDULO 2: CATASTROS Y FICHA PREDIAL (Preparado para siguientes fases)
        // =========================================================================
        public const string RegistrarPredio = "REGISTRAR_PREDIO";
        public const string ActualizarPredio = "ACTUALIZAR_PREDIO";
        public const string ActualizarFichaCatastral = "ACTUALIZAR_FICHA_CATASTRAL";
        public const string EliminarPredio = "ELIMINAR_PREDIO";
    }
}
