# Sistema de Gestión Catastral (SGC) - GAD Municipal de Antonio Ante
## Documentación Técnica y Manual de Pruebas E2E
### Módulo 1: Seguridad y Auditoría (Versión 1.0 - Release Backend)

---

## 1. Introducción y Fundamentos del Módulo

El Módulo de Seguridad del Sistema de Gestión Catastral (SGC) para el Gobierno Autónomo Descentralizado (GAD) Municipal de Antonio Ante ha sido diseñado bajo los estándares más estrictos de ingeniería de software corporativa. Su objetivo principal es salvaguardar la confidencialidad, integridad y disponibilidad de la información catastral y multifinalitaria de la entidad, alineándose de forma nativa con los principios de la norma **ISO 27001** y las mitigaciones del **OWASP Top 10**.

La arquitectura técnica implementada se basa en **Clean Architecture** (Arquitectura Limpia) y el patrón **CQRS** (Command Query Responsibility Segregation) utilizando **MediatR** como bus de mensajes en memoria. La persistencia se gestiona a través de **Entity Framework Core 8** sobre una base de datos relacional Azure SQL, extendiendo el motor de **ASP.NET Core Identity** con identificadores únicos globales (**Guid**) y separando físicamente el almacenamiento en un esquema de base de datos exclusivo denominado `Seguridad`.

---

## 2. Matriz de las 4 A's de la Seguridad Informática

El módulo cubre de extremo a extremo el ciclo de vida de control de acceso mediante la materialización de los cuatro pilares fundamentales de la seguridad:

```
+-------------------------------------------------------------------------+
|                        MÓDULO 1: SEGURIDAD (SGC)                        |
+-------------------------------------------------------------------------+
|  [Autenticación] ---> Identity + JWT Bearer (Access & Refresh Tokens)   |
|  [Autorización]  ---> Control de Accesos Basado en Roles (RBAC) + Claims |
|  [Acceso]        ---> Contexto Global Desacoplado via ICurrentUserService|
|  [Auditoría]     ---> Bitácora Transversal e Inmutable en la Base Datos |
+-------------------------------------------------------------------------+
```

1. **Autenticación (Authentication):** Mecanismo criptográfico que valida de forma unívoca la identidad del funcionario municipal mediante su número de identificación (Cédula de Ciudadanía) y contraseñas robustas cifradas con algoritmos adaptativos (PBKDF2 por defecto en Identity). Emite un par de llaves criptográficas: un *Access Token* de corta duración (15 minutos) y un *Refresh Token* de larga duración (7 días).
2. **Autorización (Authorization):** Define los privilegios del funcionario logueado dentro del sistema. Se maneja mediante un enfoque híbrido: **RBAC** (Role-Based Access Control) para perfiles macro del sistema (ej: `SuperAdmin`, `Avaluador`, `Digitador`) y **Claims Granulares** para controlar excepciones específicas del negocio (ej: `PermisoCatastral` = `PuedeAprobarAvaluo`).
3. **Acceso (Access):** Centralización de la identidad a través de la interfaz abstracta `ICurrentUserService` inyectada en la capa de Application. Esto extrae las propiedades de red e identidad del cliente directamente del flujo HTTP sin acoplar las reglas de negocio con librerías web de la infraestructura.
4. **Auditoría (Accounting/Auditing):** Registro obligatorio, inmutable e histórico de todas las operaciones críticas de acceso en la tabla `Seguridad.Auditorias`. Captura automáticamente quién, cuándo, desde qué dirección IP (mapeo real IPv4/IPv6) y mediante qué navegador o dispositivo se alteró el estado transaccional del sistema.

---

## 3. Blueprint del Esquema de Base de Datos (`Seguridad`)

El motor relacional ha sido modificado y personalizado mediante el Fluent API de Entity Framework Core para mapear las tablas nativas de Identity al negocio municipal, agrupándolas bajo el esquema estructurado de base de datos `Seguridad`.

* **`Seguridad.Usuarios`:** Almacena los datos maestros del funcionario (Guid como Primary Key, Identificacion, Nombres, Apellidos, Email, Departamento, EstadoActivo, Contraseña Hasheada, ConcurrencyStamp y contadores de bloqueo).
* **`Seguridad.Roles`:** Almacena los roles disponibles en el GAD Municipal (Id, Name, NormalizedName, Descripcion).
* **`Seguridad.UsuarioRoles`:** Tabla intermedia que rompe la relación de muchos a muchos entre usuarios y roles.
* **`Seguridad.UsuarioTokens`:** Almacena los hilos de persistencia seguros, incluyendo los Refresh Tokens de sesión activa concatenados con marcas de tiempo (`TokenString|FechaExpiracion`) y tokens de recuperación de contraseñas.
* **`Seguridad.UsuarioClaims`:** Almacena los permisos granulares asignados por excepción de forma directa a un usuario específico.
* **`Seguridad.RoleClaims`:** Almacena los permisos fijos que posee un rol o cargo de manera general corporativa.
* **`Seguridad.UsuarioLogins`:** Reservado para integraciones futuras de inicio de sesión único (SSO) con proveedores externos (ej: Microsoft 365 institucional o Google Workspace).
* **`Seguridad.Auditorias`:** Almacena la bitácora transaccional completa del sistema para control interno y requerimientos de Contraloría General del Estado.

---

## 🌐 Plan de Direccionamiento de Endpoints (Estándar REST & OWASP)

Alineados con la **ISO 27001 (Disponibilidad y Trazabilidad)** y para mitigar ataques de nivel de objeto (**OWASP A01:2021 - BOLA**), todos los endpoints se encuentran versionados en minúsculas, utilizando nombres semánticos claros en plural (`kebab-case`) y estructurados de la siguiente forma:

| Acción de Negocio | Método HTTP | URL Semántica | Tipo de Entrada | Seguridad / Acceso |
| :--- | :--- | :--- | :--- | :--- |
| Registrar Funcionario | `POST` | `/api/v1/seguridad/usuarios` | `FromBody` (JSON DTO) | Público / Admin |
| Iniciar Sesión | `POST` | `/api/v1/seguridad/usuarios/login` | `FromBody` (JSON DTO) | Público (OWASP Guard) |
| Renovación de Token | `POST` | `/api/v1/seguridad/usuarios/refresh-token` | `FromBody` (JSON DTO) | Público (Token Expired) |
| Cerrar Sesión Manual | `POST` | `/api/v1/seguridad/usuarios/logout` | Ninguno (Headers) | `[Authorize]` (JWT Activo) |
| Solicitar Token Clave | `POST` | `/api/v1/seguridad/usuarios/solicitar-recuperacion`| `FromBody` (JSON DTO) | Público |
| Ejecutar Cambio Clave | `POST` | `/api/v1/seguridad/usuarios/restablecer-password` | `FromBody` (JSON DTO) | Público (Token Temporal)|
| Asignar Permiso Granular| `POST` | `/api/v1/seguridad/usuarios/{id}/permisos` | `FromRoute` + `FromBody` | `[Authorize]` (SuperAdmin) |

---

## 🧪 Manual de Ejecución de Pruebas de Certificación QA

Este guión detalla la secuencia de ejecución exacta realizada en el entorno de desarrollo (`https://localhost:7245`) para validar la estabilidad y resiliencia del backend antes del despliegue del frontend.

### PASO 1: Registro e Inyección de Dependencias (FluentValidation Guard)
* **Objetivo:** Verificar que el interceptor `ValidationBehavior` de MediatR bloquee datos corruptos o incompletos antes de tocar la persistencia.
* **Acción Realizada:** Se envió una petición a `POST /api/v1/seguridad/usuarios` omitiendo los campos obligatorios `Nombres` y `Apellidos`.
* **Resultado Obtenido:** El sistema cortó el flujo de manera inmediata lanzando una `ValidationException` atrapada globalmente por el `GlobalExceptionHandler`, respondiendo con un **HTTP 400 Bad Request** y el JSON detallando los errores de negocio. Posteriormente se envió la petición con todos los campos correctos y el rol `"SuperAdmin"`.
* **Impacto en BD:** Se insertó la fila en `Seguridad.Usuarios` con ID único Guid y contraseña cifrada. Se generó la fila de relación en `Seguridad.UsuarioRoles`. En `Seguridad.Auditorias` se escribió exitosamente la traza con la acción `REGISTRO_NUEVO_FUNCIONARIO` detallando el nombre y cargo del nuevo empleado del GAD.

### PASO 2: Simulación de Ataque por Fuerza Bruta (Lockout Automatizado)
* **Objetivo:** Mitigar ataques de diccionario y accesos no autorizados.
* **Acción Realizada:** Se invocó el endpoint `POST /api/v1/seguridad/usuarios/login` ingresando la identificación del usuario pero con contraseñas erróneas consecutivas.
* **Resultado Obtenido:** Cada intento fallido fue contabilizado en el servidor. Al llegar al intento número 5, el API bloqueó la petición y denegó el acceso con un mensaje de protección perimetral.
* **Impacto en BD:** La columna `AccessFailedCount` incrementó en tiempo real de `1` a `5`. Al quinto fallo, la columna `LockoutEnd` se actualizó automáticamente con la marca de tiempo de penalización (5 minutos a futuro). La tabla `Seguridad.Auditorias` registró 4 filas de `LOGIN_FALLIDO` indicando `"Contraseña incorrecta"` en los datos adicionales, y una quinta fila con la acción `CUENTA_BLOQUEADA`.

### PASO 3: Desbloqueo Transaccional e Inicio de Sesión Corporativo
* **Objetivo:** Validar la emisión del par de tokens seguros criptográficos.
* **Acción Realizada:** Tras limpiar el contador de bloqueos mediante script SQL de administración, se realizó el inicio de sesión con las credenciales correctas.
* **Resultado Obtenido:** HTTP 200 Ok con la respuesta estructurada de tokens encapsulada dentro del nodo de transporte seguro. El Access Token devuelto viaja firmado con el algoritmo HmacSha256.
* **Impacto en BD:** La columna `AccessFailedCount` se reseteó automáticamente a `0` y `LockoutEnd` volvió a `NULL`. En la tabla `Seguridad.UsuarioTokens` se grabó el token de refresco bajo el proveedor `"SGC_System"` concatenando su tiempo de vida de 7 días (`TokenString|2026-07-02...`). La tabla `Seguridad.Auditorias` registró de forma exitosa la fila con `Accion = 'LOGIN_EXITOSO'`.

### PASO 4: Autorización en Swagger y Extracción del Contexto (`ICurrentUserService`)
* **Objetivo:** Inyectar la identidad Bearer en el ecosistema web.
* **Acción Realizada:** Se copió el `accessToken` y se pegó en el botón de seguridad superior de Swagger utilizando el esquema de cabecera estándar `Bearer <token>`.
* **Resultado Obtenido:** Las peticiones subsecuentes adjuntaron exitosamente el header `Authorization`. El sistema resolvió las llamadas protegidas sin fugas de memoria.

### PASO 5: Asignación de Permisos Granulares por Excepción (OWASP BOLA Guard)
* **Objetivo:** Proveer capacidades híbridas de permisos mitigando la manipulación de payloads.
* **Acción Realizada:** Se invocó la URL semántica `POST /api/v1/seguridad/usuarios/40B12DCA-B14E-4C3F-B67D-08DED2625D4B/permisos` pasando el Guid del usuario directamente en la ruta y definiendo el claim en el body.
* **Resultado Obtenido:** HTTP 200 Ok confirmando la asignación del privilegio.
* **Impacto en BD:** Se insertó una fila en `Seguridad.UsuarioClaims` registrando la clave primaria autoincremental, el Guid del usuario afectado, el `ClaimType` (`PermisoCatastral`) y el `ClaimValue` (`PuedeAprobarAvaluo`). En la tabla de auditoría quedó grabada la traza `PERMISO_GRANULAR_ASIGNADO` vinculando la IP de origen local `::1`.

### PASO 6: Renovación Silenciosa de Sesión (Refresh Token Lifecycle)
* **Objetivo:** Mantener la sesión del funcionario activa sin interrumpir la operación técnica catastral.
* **Acción Realizada:** Se envió el Access Token caducado junto con el string del Refresh Token al endpoint `/refresh-token`.
* **Resultado Obtenido:** El Handler interceptó los metadatos, omitió la expiración del Access Token para extraer de forma segura el Claim de identidad `"sub"`, validó contra la base de datos la vigencia del refresh token, y emitió un nuevo par de accesos con vigencia renovada.
* **Impacto en BD:** La fila en `Seguridad.UsuarioTokens` fue actualizada en su columna `Value` con el nuevo token aleatorio seguro y una extensión de fecha por 7 días más. Se generó la traza de auditoría `REFRESH_EXITOSO`.

### PASO 7: Generación de Tokens Temporales y Restablecimiento Cifrado de Clave
* **Objetivo:** Habilitar la recuperación de accesos mediante tokens criptográficos de un solo uso.
* **Acción Realizada:** Se ejecutó `POST /solicitar-recuperacion` pasando el correo electrónico corporativo. El token criptográfico generado se envió posteriormente a `/restablecer-password` definiendo la nueva contraseña robusta.
* **Resultado Obtenido:** Restablecimiento exitoso de la cuenta.
* **Impacto en BD:** Se registró la solicitud en auditoría (`SOLICITUD_RECUPERACION_PASSWORD`). El Handler de ejecución invocó el cambio, lo que modificó la cadena hash de la columna `PasswordHash` en `Seguridad.Usuarios` y reseteó de forma preventiva los contadores de fallas. Quedó grabada la traza final `RESTABLECER_PASSWORD_EXITOSO`.

### PASO 8: Destrucción de Sesión (Logout Seguro)
* **Objetivo:** Invalidar los hilos de acceso del cliente de forma permanente.
* **Acción Realizada:** Se invocó el endpoint protegido `POST /api/v1/seguridad/usuarios/logout`.
* **Resultado Obtenido:** HTTP 200 Ok indicando el cierre de sesión exitoso.
* **Impacto en BD:** **La fila correspondiente al Refresh Token dentro de `Seguridad.UsuarioTokens` fue eliminada físicamente de la base de datos.** Esto rompe cualquier intento de reutilización del token por agentes externos. La tabla `Seguridad.Auditorias` cerró el ciclo registrando la acción `LOGOUT`.

---

## 📌 Backlog Próxima Versión (Fases Técnicas de Expansión - Versión 2.0)

Para la siguiente iteración de arquitectura y escalabilidad de la infraestructura municipal, se ha planificado la incorporación de las siguientes características de nivel Enterprise:

1. **Autenticación Federada con Microsoft 365 (OAuth2 / OIDC):** Integrar la API con Azure Active Directory (Microsoft Entra ID) para permitir el inicio de sesión único (SSO). Los funcionarios usarán su correo institucional (`@antonioante.gob.ec`). Al autenticarse externamente, la tabla `Seguridad.UsuarioLogins` se poblará vinculando el identificador de Microsoft con el Guid local del SGC.
2. **Doble Factor de Autenticación (2FA - Autenticación Multifactor):** Implementar la verificación en dos pasos obligatoria para jefaturas y directores de avalúos. Se usará el estándar TOTP (Time-Based One-Time Password) mediante Google Authenticator o Microsoft Authenticator, utilizando la columna nativa `TwoFactorEnabled` de la tabla de usuarios.
3. **Endpoint de Actualización de Datos y Cambio de Contraseña en Sesión:** Desarrollar el comando `ChangePasswordCommand` y `UpdateUsuarioProfileCommand` bajo una ruta protegida con `[Authorize]`. Esto permitirá a los funcionarios actualizar sus datos básicos (como el teléfono) y cambiar su clave de forma manual desde su panel de perfil dentro de Blazor, sin necesidad de recurrir al flujo de pérdida de contraseña.
4. **Cifrado de Datos en Reposo para Campos Sensibles (PII):** Aplicar mecanismos de enmascaramiento o cifrado a nivel de columna en SQL Server para el almacenamiento de datos personales de los funcionarios, robusteciendo el cumplimiento de las auditorías gubernamentales.
