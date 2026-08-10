# Diseño de Base de Datos - Módulo 2: Ficha Catastral Multifinalitaria
**Esquema SQL:** `Catastro`
**Enfoque:** Domain-Driven Design (DDD) + NetTopologySuite (GIS)

## 1. Reglas Arquitectónicas Globales
- **Llaves Primarias:** `Guid Id` autonumérico (Invisible para el usuario).
- **Auditoría:** Todas las tablas heredan de `AuditableEntity` (`FechaCreacion`, `CreadoPor`, `FechaModificacion`, `ModificadoPor`, `EstadoActivo`).
- **Cero Datos Quemados:** Los selectores de la UI se alimentan de Tablas de Catálogo.

## 2. Entidades de Catálogo (Parametrización)
Entidades base (Id, Nombre, Descripcion) para alimentar dropdowns en Blazor:
- `Catastro.TiposTenencia` (Escritura, Posesión, Arrendamiento, etc.)
- `Catastro.TiposEstructura` (Hormigón, Madera, Metálica, Mixta, etc.)
- `Catastro.EstadosConservacion` (Bueno, Regular, Malo, En Construcción.)

## 3. Entidades Core (Transaccionales)
### 3.1. `Catastro.Predios` (Aggregate Root - Físico)
- `ClaveCatastral`: *Value Object*. Único. Validación DPA MIDUVI (19-28 dígitos).
- `ClaveAnterior`: varchar. Trazabilidad con VB6.
- `TipoPredio`: Enum (1 = Urbano, 2 = Rural).
- `AreaTerrenoEscritura`: decimal(18,2).
- `AreaTerrenoGrafica`: decimal(18,2).
- `PoligonoEspacial`: **Geometry** (NetTopologySuite).
- `Direccion`: varchar.

### 3.2. `Catastro.Propietarios` (Jurídico)
- `TipoPropietario`: Enum (1 = Natural, 2 = Jurídico).
- `Identificacion`: varchar (Único). Cédula o RUC.
- `Nombres`, `Apellidos`, `RazonSocial`: varchars.
- `EstadoCivil`: Enum.

### 3.3. `Catastro.Dominios` (Relación Predio - Propietario)
- `PredioId`, `PropietarioId`, `TipoTenenciaId`: Guids (Foreign Keys).
- `PorcentajePropiedad`: decimal(5,2).
- `FechaInscripcion`: datetime.

### 3.4. `Catastro.BloquesConstruccion` (Edificaciones)
- `PredioId`, `TipoEstructuraId`, `EstadoConservacionId`: Guids (Foreign Keys).
- `NumeroBloque`, `NumeroPisos`, `AnioConstruccion`: int.
- `AreaConstruccion`: decimal(18,2).