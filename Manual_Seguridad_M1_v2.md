# Sistema de Gestión Catastral (SGC) - GAD Municipal de Antonio Ante
## Documentación Técnica, Especificaciones UI/UX y Manual de Pruebas E2E
### Módulo 1: Seguridad, Administración y Auditoría (Versión 2.0 - Release Fullstack Blazor WASM & API)

---

## 1. Introducción y Arquitectura Fullstack

El Módulo de Seguridad, Administración y Auditoría del Sistema de Gestión Catastral (SGC) para el Gobierno Autónomo Descentralizado (GAD) Municipal de Antonio Ante ha sido evolucionado a una solución **Fullstack** unificada. Su objetivo es garantizar el control de acceso, la parametrización de la nómina municipal y la trazabilidad inmutable de todas las operaciones de la entidad, cumpliendo con los estándares de la norma **ISO 27001** y los lineamientos del **OWASP Top 10**.

La arquitectura técnica se estructura en dos capas principales:
* **Backend API (.NET 8 Web API):** Implementa **Clean Architecture** y el patrón **CQRS** (Command Query Responsibility Segregation) con **MediatR** como bus de mensajes en memoria. La persistencia se realiza mediante **Entity Framework Core 8** sobre Azure SQL, extendiendo **ASP.NET Core Identity** con identificadores únicos globales (**Guid**) y un esquema aislado denominado `Seguridad`.
* **Frontend Web (Blazor WebAssembly .NET 8):** Aplicación de página única (SPA) modular y reactiva. Consume la API mediante clientes HTTP fuertemente tipados e integra un sistema visual moderno basado en **Bootstrap 5**, **Bootstrap Icons** y estilos CSS personalizados optimizados para entornos corporativos.

```
+-----------------------------------------------------------------------------------+
|                        ARQUITECTURA FULLSTACK (SGC)                               |
+-----------------------------------------------------------------------------------+
|  [Cliente Blazor WASM] ---> UI/UX Responsive + State Management + Custom Toasts   |
|            |                                                                      |
|      (HTTP / REST)                                                                |
|            v                                                                      |
|  [API REST .NET 8]    ---> Clean Architecture + CQRS (MediatR) + EF Core 8          |
|            |                                                                      |
|      (Persistencia)                                                               |
|            v                                                                      |
|  [BD Azure SQL]       ---> Esquema "Seguridad" (Identity + Auditoría Inmutable)   |
+-----------------------------------------------------------------------------------+
```

---

## 2. Matriz de las 4 A's de la Seguridad e Integración Client/Server

El módulo cubre de extremo a extremo el ciclo de vida de control de acceso mediante los cuatro pilares fundamentales de la seguridad informática:

1. **Autenticación (Authentication):** Mecanismo criptográfico que valida unívocamente la identidad del funcionario municipal mediante su Cédula de Ciudadanía y contraseña hasheada (PBKDF2 en Identity). Emite un par de llaves criptográficas: un *Access Token* JWT (15 minutos) y un *Refresh Token* de larga duración (7 días) gestionado con rotación automática.
2. **Autorización (Authorization):** Enfoque híbrido que combina **RBAC** (Role-Based Access Control) para perfiles macro del sistema (`SuperAdmin`, `AdminSistemas`, `JefeCatastro`, `DigitadorCatastral`) y **Claims Granulares Directos** para excepciones asignables a funcionarios específicos.
3. **Acceso (Access):** Abstracción de la identidad mediante la interfaz `ICurrentUserService` en el backend para la extracción de claims de HTTP Context (IP, Usuario, Claims), complementada en el cliente con componentes `<AuthorizeView>` y servicios de navegación contextual.
4. **Auditoría (Accounting/Auditing):** Registro obligatorio, inmutable e histórico en la tabla `Seguridad.Auditorias`. Captura automáticamente el usuario ejecutor, la acción, el módulo o entidad afectada, la IP del cliente (IPv4/IPv6), el agente de navegador y el delta de datos (*payload*) en formato JSON.

---

## 3. Blueprint del Esquema de Base de Datos y Modelos DTO

### 3.1. Tablas del Esquema `Seguridad`
* **`Seguridad.Usuarios`:** Datos maestros del funcionario (Guid, Identificación/Cédula, Nombres, Apellidos, Email, DepartamentoId, EstadoActivo, PasswordHash, ConcurrencyStamp y contadores de bloqueo).
* **`Seguridad.Roles`:** Perfiles de seguridad disponibles en el GAD Municipal (Id, Name, NormalizedName, Descripcion).
* **`Seguridad.UsuarioRoles`:** Relación muchos a muchos entre usuarios y roles.
* **`Seguridad.UsuarioTokens`:** Persistencia de Refresh Tokens de sesión activa concatenados con marcas de tiempo (`TokenString|FechaExpiracion`).
* **`Seguridad.UsuarioClaims`:** Permisos granulares asignados por excepción directamente a un funcionario.
* **`Seguridad.RoleClaims`:** Permisos fijos asignados a un rol institucional.
* **`Seguridad.UsuarioLogins`:** Espacio reservado para federación de identidades (SSO con Microsoft 365 / Google Workspace).
* **`Seguridad.Departamentos`:** Catálogo paramétrico de direcciones, jefaturas y áreas del GAD Municipal (Id, Nombre, Descripcion, EstadoActivo).
* **`Seguridad.Auditorias`:** Bitácora transaccional inmutable para control interno y requerimientos de la Contraloría General del Estado.

### 3.2. DTOs Principales de Transporte
* **`CreateUsuarioDto` / `UpdateUsuarioDto` / `UsuarioResponseDto`:** Transferencia de información de la nómina municipal.
* **`RoleResponseDto` / `CreateRoleDto` / `AssignRoleDto` / `RolePermissionDto`:** Gestión de roles y matriz de claims.
* **`DepartamentoDto` / `CreateDepartamentoDto`:** Gestión del catálogo de áreas municipales.
* **`AuditLogResponseDto` / `ResultadoPaginadoAuditDto`:** DTOs optimizados para respuesta paginada desde el servidor:

```csharp
public class AuditLogResponseDto
{
    public Guid Id { get; set; }
    public Guid? UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string EmailUsuario { get; set; } = string.Empty;
    public string IdentificacionUsuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string? EntidadId { get; set; }
    public string DireccionIp { get; set; } = string.Empty;
    public string Navegador { get; set; } = string.Empty;
    public string DatosAdicionales { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}

public class ResultadoPaginadoAuditDto
{
    public List<AuditLogResponseDto> Items { get; set; } = new();
    public int TotalRegistros { get; set; }
    public int PaginaActual { get; set; }
    public int RegistrosPorPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / (RegistrosPorPagina == 0 ? 1 : RegistrosPorPagina));
}
```

---

## 4. Estándares UI/UX y Guía de Diseño Visual (Fullstack Blazor)

Para garantizar la coherencia de la experiencia de usuario en todo el sistema, se establecieron las siguientes reglas de diseño obligatorio para todas las páginas presentes y futuras:

* **Aprovechamiento Total del Espacio (100% Width):** Uso exclusivo de la clase `container-fluid` en los contenedores raíz, eliminando restricciones arbitrarias de ancho fijo (`max-width: 900px` o `1200px`).
* **Estandarización de Modales:**
  * **Modales de Formulario (`modal-lg` o tamaño estándar):** Eliminada la clase `modal-sm` para evitar campos apretados. Los botones de acción se ubican alineados a la derecha (`d-flex justify-content-end gap-2`), con el botón primario a la derecha y el botón neutro/cancelar a la izquierda.
  * **Modales de Confirmación (Estado/Eliminación):** Tamaño estándar centrado, eliminando el estilo `modal-sm`. Los botones de confirmación se despliegan en una sola línea horizontal centrada (`d-flex justify-content-center gap-3`).
* **Dropdowns Dinámicos:** Prohibido el uso de valores hardcoded en selects. Listas de roles y departamentos se cargan asincrónicamente desde la base de datos durante el `OnInitializedAsync`.
* **Sistema de Notificaciones Automáticas (Toast Flotante):** Toasts de estado posicionados en la esquina superior derecha (`position-fixed top-0 end-0 p-3`, `z-index: 1090`) con auto-cierre gestionado mediante `CancellationTokenSource` y `Task.Delay(4000)`.
* **Detalles Visuales de Tablas:**
  * **Avatares de Usuarios:** Fondo gris neutro claro (`#f1f5f9`), texto oscuro con contraste, borde fino y espaciado proporcional (`gap-3`) con respecto al texto de la celda.
  * **Centrado Vertical de Iconos:** Alineación precisa de iconos dentro de inputs de búsqueda utilizando `position-absolute start-0 top-50 translate-middle-y ms-3 text-muted`.
  * **Columnas Combinadas:** Agrupación de datos correlacionados para optimizar espacio horizontal (ej: Timestamp / IP desplegando la fecha/hora en la línea superior y la IP con icono en la línea inferior).

---

## 5. Especificaciones por Componente Blazor WASM

### 5.1. `Pages/Seguridad/Usuarios.razor`
* **Propósito:** Administración integral de la nómina de funcionarios municipales.
* **Características Clave:**
  * Tabla con búsqueda reactiva por cédula, nombres, apellidos o correo.
  * Inclusión del flujo de alta de funcionarios en un Modal dedicado (`modal-lg`), eliminando la vista independiente `RegistrarUsuario.razor` para evitar rupturas de contexto en la SPA.
  * Carga dinámica de roles y departamentos desde BD para los selectores del formulario de creación y edición.
  * Modales de edición, asignación de permisos granulares directos y confirmación de activación/desactivación de acceso (Soft Delete).

### 5.2. `Pages/Seguridad/Roles.razor`
* **Propósito:** Parametrización de perfiles de acceso y matriz de permisos por módulo.
* **Características Clave:**
  * Disposición responsive en grilla de tarjetas (`col-md-6 col-lg-4 col-xl-3`) que se adapta fluidamente al ancho completo de la pantalla.
  * Modal amplio (`modal-xl`) con catálogo de permisos agrupados por módulo (Módulo: Catastro, Módulo: Seguridad, etc.) con controles tipo `form-switch`.
  * Modales para la creación de nuevos roles y asignación/remover funcionarios de un rol.
  * Protección contra eliminación de roles base del sistema (`AdminSistemas`, `SuperAdmin`).

### 5.3. `Pages/Seguridad/Departamentos.razor`
* **Propósito:** Gestión del catálogo de direcciones, jefaturas y áreas administrativas del GAD.
* **Características Clave:**
  * Vista de tabla simplificada con indicadores de estado activo/inactivo (`badge rounded-pill`).
  * Modal estandarizado de creación/edición.
  * Modales de confirmación para el cambio de estado y la eliminación física de departamentos con política de integridad referencial `SetNull` en usuarios.

### 5.4. `Pages/Seguridad/Auditoria.razor`
* **Propósito:** Consulta y rastreo de la bitácora transaccional e inmutable del sistema.
* **Características Clave:**
  * **Paginación en el Servidor (Server-Side Pagination):** Consultas optimizadas en base de datos utilizando `Skip()` y `Take()` mediante los DTOs `AuditLogResponseDto` y `ResultadoPaginadoAuditDto`, garantizando alto rendimiento ante millones de registros.
  * **Panel de Filtros Avanzados:** Búsqueda por rango de fechas (Desde / Hasta), selector de funcionario ejecutor y cuadro de texto para términos generales.
  * **Estructura de Tabla Limpia:** Columna unificada Timestamp / IP, avatar en tono gris neutro, insignias de acción formateadas por color (CREATE, UPDATE, DELETE, LOGIN) y nombre de la entidad limpia sin exponer GUIDs crudos en la vista principal.
  * **Modal de Detalle del Evento:** Vista técnica dividida en dos bloques (Información de Origen y Contexto Operativo) junto a una caja de código (`monospaced`) para la inspección del payload/delta de datos JSON.

---

## 6. Plan de Direccionamiento de Endpoints y Servicios Client

| Acción de Negocio | Método | Endpoint API (v1) | Servicio Client (Contracts) |
| :--- | :--- | :--- | :--- |
| Consultar Funcionarios | `GET` | `/api/v1/seguridad/usuarios` | `IUsuarioService.ObtenerTodosLosUsuariosAsync()` |
| Registrar Funcionario | `POST` | `/api/v1/seguridad/usuarios` | `IUsuarioService.RegistrarFuncionarioAsync()` |
| Actualizar Funcionario | `PUT` | `/api/v1/seguridad/usuarios/{id}` | `IUsuarioService.ActualizarFuncionarioAsync()` |
| Activar / Desactivar | `PATCH` | `/api/v1/seguridad/usuarios/{id}/estado` | `IUsuarioService.Activar/DesactivarFuncionarioAsync()` |
| Consultar Roles | `GET` | `/api/v1/seguridad/roles` | `IRoleService.ObtenerTodosLosRolesAsync()` |
| Crear Rol | `POST` | `/api/v1/seguridad/roles` | `IRoleService.CrearRolAsync()` |
| Permisos de Rol | `GET`/`POST` | `/api/v1/seguridad/roles/{id}/permisos` | `IRoleService.Obtener/AsignarPermisosRolAsync()` |
| Consultar Departamentos | `GET` | `/api/v1/seguridad/departamentos` | `IDepartamentoService.ObtenerTodosAsync()` |
| Crear / Editar Depto | `POST`/`PUT` | `/api/v1/seguridad/departamentos` | `IDepartamentoService.Crear/ActualizarAsync()` |
| Bitácora Paginada | `GET` | `/api/v1/seguridad/auditoria` | `IAuditoriaService.ConsultarBitacoraAsync()` |

---

## 7. Manual de Ejecución de Pruebas de Certificación QA (End-to-End)

Secuencia de pruebas efectivas ejecutadas en la solución Fullstack Blazor + API:

* **PASO 1: Alta de Funcionario via Modal y Persistencia Transaccional**
  * En la vista `/seguridad/usuarios`, presionar el botón "Registrar Funcionario".
  * Verificar la apertura del modal `modal-lg`. Comprobar que los selectores de Departamento Municipal y Rol de Seguridad Base contengan los datos actualizados desde la BD.
  * Diligenciar el formulario e ingresar una identificación (cédula) válida. Presionar "Guardar Funcionario".
  * **Resultado:** El modal se cierra, la tabla se refresca automáticamente mostrando al nuevo usuario, se lanza el toast flotante de éxito y en la base de datos se registra la fila correspondiente en `Seguridad.Usuarios` y `Seguridad.UsuarioRoles`.

* **PASO 2: Asignación de Permisos Granulares y Generación de Audit Log**
  * En la tabla de funcionarios, hacer clic en el botón "Permisos" correspondiente al usuario creado.
  * En el modal `modal-xl`, activar un permiso especial directo (ej. `Catastro.AprobarAvaluo`) mediante el switch interactivo. Presionar "Guardar Permisos".
  * **Resultado:** El servidor procesa la inserción en `Seguridad.UsuarioClaims`. La acción queda registrada en la bitácora con el nombre del usuario y la IP de origen.

* **PASO 3: Validación de Bitácora de Auditoría Paginada (Server-Side)**
  * Navegar a la página `/seguridad/auditoria`.
  * Verificar el despliegue del componente con el diseño de ancho completo (`container-fluid`), la columna unificada Timestamp / IP y la ausencia de GUIDs expuestos en la columna Entidad.
  * Interactuar con el paginador inferior: cambiar las filas por página a 10 y presionar el botón de página siguiente.
  * **Resultado:** Se envía una petición `GET` a la API adjuntando `pagina=2&registrosPorPagina=10`. El backend responde utilizando `Skip(10).Take(10)` de Entity Framework Core, actualizando el conteo de registros sin sobrecargar la memoria de la aplicación.

* **PASO 4: Inspección de Payload en Modal de Auditoría**
  * En la tabla de auditoría, hacer clic en el botón "Ver Detalle" de la última transacción registrada.
  * Verificar la apertura del modal estructurado en dos columnas (Información de Origen y Contexto Operativo).
  * **Resultado:** Se visualiza la IP, el User Agent del navegador, el usuario ejecutor con su número de cédula, la entidad afectada, su GUID correspondiente y la caja de código `monospaced` que muestra el payload JSON con los cambios efectuados.

---

## 8. Control de Pendientes e Hoja de Ruta del Módulo de Seguridad (Backlog Versión 3.0)

### 8.1. Tarea Pendiente Registrada (`PENDIENTE_POLITICAS_AUTORIZACION.md`)
* **Estado:** Pospuesto intencionalmente para la fase final del proyecto.
* **Descripción:** Aplicación granular de atributos `[Authorize(Policy = ...)]` en los controladores de la API y bloques `<AuthorizeView Policy="...">` en el cliente Blazor. Se ejecutará una vez que la totalidad de los módulos de negocio del sistema estén construidos, para realizar las pruebas de acceso sin interferencias en la etapa de desarrollo activa.

### 8.2. Futuras Mejoras del Módulo de Seguridad y Administración (Backlog Versión 3.0)
Para las siguientes fases de madurez y endurecimiento de la infraestructura de seguridad municipal, se ha planificado la incorporación de los siguientes componentes especializados:

#### 8.2.1. Autenticación Federada y SSO (Microsoft Entra ID / Google Workspace)
* **Objetivo:** Permitir el inicio de sesión único (SSO) utilizando las cuentas de correo institucionales `@antonioante.gob.ec`.
* **Implementación:** Integración de OpenID Connect (OIDC) / OAuth2. Al autenticarse con el proveedor externo, la tabla `Seguridad.UsuarioLogins` mapeará el token federado con el Guid interno del usuario en el SGC.

#### 8.2.2. Autenticación Multifactor Obligatoria (2FA / TOTP)
* **Objetivo:** Añadir una capa extra de seguridad para roles con privilegios elevados (`SuperAdmin`, `AdminSistemas`, `JefeCatastro`).
* **Implementación:** Uso del estándar TOTP (Time-Based One-Time Password) compatible con aplicaciones como Google Authenticator o Microsoft Authenticator, aprovechando el soporte nativo de ASP.NET Core Identity (`TwoFactorEnabled`).

#### 8.2.3. Autogestión de Perfil de Usuario y Cambio de Clave en Sesión
* **Objetivo:** Permitir que los funcionarios gestionen su información de contacto y renueven su clave periódicamente sin intervención del administrador.
* **Implementación:** Creación de los comandos `UpdateUserProfileCommand` y `ChangePasswordCommand`, junto con el componente Blazor `PerfilUsuario.razor` accesible desde el encabezado del sistema.

#### 8.2.4. Control, Monitoreo y Revocación Remota de Sesiones Activas
* **Objetivo:** Permitir a los administradores visualizar las sesiones abiertas en tiempo real y cerrar accesos de forma remota en caso de pérdida o compromiso de un dispositivo.
* **Implementación:** Módulo de gestión de tokens en la UI para consultar la tabla `Seguridad.UsuarioTokens` y ejecutar comandos de eliminación física (Token Blacklisting / Kill Session) por usuario o de manera global.

#### 8.2.5. Exportación de Bitácora de Auditoría con Firma o Hash de Integridad
* **Objetivo:** Generar reportes oficiales de auditoría requeridos por la Contraloría General del Estado u órganos de control interno.
* **Implementación:** Generación de reportes en formatos Excel (`.xlsx`) y PDF con resúmenes filtrados, incluyendo un código de verificación hash (SHA-256) en el pie del documento para validar la autenticidad e inmutabilidad de la información exportada.

#### 8.2.6. Cifrado de Datos Personales Sensibles en Reposo (PII / Column Encryption)
* **Objetivo:** Proteger información sensible del funcionario a nivel de base de datos.
* **Implementación:** Aplicación de cifrado o enmascaramiento a nivel de columna (Always Encrypted o cifrado simétrico por software) sobre campos como el número de teléfono personal o la cédula de ciudadanía en SQL Server.

#### 8.2.7. Opción "Recordar Sesión" (Remember Me) y Persistencia Dinámica de Sesión
* **Objetivo:** Permitir al usuario elegir entre mantener su sesión iniciada de manera prolongada en un equipo de confianza o restringirla estrictamente a la jornada laboral/pestaña actual.
* **Implementación:** * **Estrategia Normal (Checkbox desmarcado):** Las llaves se almacenan en `sessionStorage` con un `RefreshToken` de corta duración (8 a 10 horas). Expirará al cerrar la ventana o al día siguiente.
  * **Estrategia Recordarme (Checkbox marcado):** Las llaves se almacenan en `localStorage` con un `RefreshToken` extendido (7 a 30 días), manteniendo la sesión activa entre días y fines de semana hasta que el usuario cierre sesión explícitamente.