# Historias de Usuario - Módulo 1: Seguridad e Identity (Las 4 A's)

## Épica: Control de Acceso y Gestión de Identidad
Como Administrador del Sistema del GADM, necesito un módulo de seguridad robusto para garantizar que solo el personal autorizado pueda acceder y modificar la información catastral, manteniendo un registro de auditoría estricto.

### HU-SEG-01: Login de Usuario (Autenticación y Acceso)
**Como** funcionario del GAD  
**Quiero** iniciar sesión con mi `UserName` y contraseña  
**Para** obtener un Token de acceso (JWT) e ingresar al sistema.  
**Criterios de Aceptación:**
- Validar credenciales contra la tabla `Seguridad.Usuarios`.
- Generar JWT con Claims correspondientes (Id, Roles).
- Si el `EstadoActivo` es `false`, denegar acceso inmediatamente.

### HU-SEG-02: Creación de Funcionarios (Autorización Base)
**Como** Administrador de Sistemas  
**Quiero** registrar a un nuevo funcionario en el sistema  
**Para** otorgarle credenciales de acceso.  
**Criterios de Aceptación:**
- Capturar: Nombres, Apellidos, Identificacion (Cédula), Departamento, UserName, Email.
- Contraseña encriptada vía Identity.
- Validar unicidad de Identificacion y UserName.

### HU-SEG-03: Gestión de Roles (Autorización Granular)
**Como** Administrador de Sistemas  
**Quiero** asignar o remover roles a un funcionario  
**Para** limitar las acciones y menús a los que tiene acceso en Blazor y Endpoints.  
**Criterios de Aceptación:**
- Listar roles desde `Seguridad.Roles`.
- Vincular usuario a múltiples roles si es necesario.

### HU-SEG-04: Baja Lógica de Usuario (Auditoría - Soft Delete)
**Como** Administrador de Sistemas  
**Quiero** desactivar la cuenta de un funcionario sin borrar el registro  
**Para** revocar su acceso manteniendo intacto su historial de modificaciones en el catastro.  
**Criterios de Aceptación:**
- Cambiar propiedad `EstadoActivo` a `false`.
- El sistema debe rechazar nuevos inicios de sesión.