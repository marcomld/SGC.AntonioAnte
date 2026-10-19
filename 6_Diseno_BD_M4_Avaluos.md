# 💰 Plan Maestro y Diseño de Base de Datos: Módulo 4 - Motor de Valoración Masiva
## Sistema de Gestión Catastral (SGC) - GAD Municipal de Antonio Ante

---

> [!IMPORTANT]
> **Documento Oficial de Arquitectura Financiera**  
> Este módulo actúa como el motor financiero y matemático del sistema, determinando el valor económico del terreno y las edificaciones[cite: 251]. Toda tabla en este esquema implementa el patrón de **Inmutabilidad Fiscal** mediante el campo `AnioFiscal`, garantizando que los recálculos futuros no alteren los impuestos ya emitidos.

---

## 📑 Tabla de Contenidos
1. [Análisis de Requerimientos y Normativa](#1-análisis-de-requerimientos-y-normativa)
2. [Diseño Relacional: Esquema `Avaluos`](#2-diseño-relacional-esquema-avaluos)
3. [Diagrama del Modelo de Datos (ERD)](#3-diagrama-del-modelo-de-datos-erd)
4. [Flujo de Simulación y Generación de Obligaciones](#4-flujo-de-simulación-y-generación-de-obligaciones)

---

## 🧠 1. Análisis de Requerimientos y Normativa

Para cumplir con la normativa nacional y las ordenanzas del GAD Municipal, el motor de valoración debe procesar las variables bajo tres pilares:

1. **Avalúo de Terreno (Diferenciado):** 
   - **Urbano:** Basado en el valor del metro cuadrado ($m^2$) paramétrico por Zona, Sector y Manzana[cite: 250, 255]. Afectado por infraestructura y servicios[cite: 250, 251].
   - **Rural:** Basado en Sectores Homogéneos, calidad de la tierra y plantaciones/mejoras rurales[cite: 250, 255].
2. **Avalúo de Edificaciones:** Costo de reposición según la tipología constructiva (Estructura, Acabados), aplicando tablas de depreciación por edad (Año de Construcción) y Estado de Conservación[cite: 250, 255].
3. **Liquidación (Hecho Generador):** Consolidación de los avalúos para calcular la Base Imponible, aplicando exoneraciones (Ley del Anciano, Discapacidad) para enviar el cobro de Impuesto Predial y CEM a Rentas[cite: 250, 251, 256]. El sistema debe permitir la **simulación** previa a la emisión.

---

## 🗄️ 2. Diseño Relacional: Esquema `Avaluos`

Todas las tablas heredan de la clase base `AuditableEntity` para registrar el usuario ejecutor y la marca de tiempo (exigencia de Contraloría)[cite: 250, 257].

### 2.1. Tablas de Parametrización Económica (Se actualizan por Ordenanza)

| Tabla | Atributos Principales | Descripción y Propósito |
| :--- | :--- | :--- |
| `Avaluos.PreciosBaseSuelo` | `Id`, `AnioFiscal`, `TipoPredio` (U/R), `Zona`, `Sector`, `ManzanaPoligono`, `ValorMetroCuadrado` | Almacena el precio base del $m^2$ de tierra. Separado por año fiscal para no afectar históricos[cite: 250, 255]. |
| `Avaluos.CostosReposicion` | `Id`, `AnioFiscal`, `TipoEstructuraId` (FK), `ValorMetroCuadrado` | Define cuánto cuesta construir 1 $m^2$ según el material (Hormigón, Madera, etc.) para el año en curso[cite: 250, 255]. |
| `Avaluos.FactoresDepreciacion` | `Id`, `AnioFiscal`, `EstadoConservacionId` (FK), `EdadMinima`, `EdadMaxima`, `Coeficiente` | Matriz de castigo financiero. Un bloque de 20 años en estado "Malo" tendrá un coeficiente (ej. 0.45) que reducirá su valor[cite: 250, 255]. |
| `Avaluos.FactoresAfectacion` | `Id`, `AnioFiscal`, `TipoAfectacion` (Topografía, Vías, Servicios), `Coeficiente` | Modificadores del valor del terreno (ej. un terreno sin agua potable se castiga multiplicando su valor por 0.80)[cite: 250, 255]. |

### 2.2. Tabla Transaccional Core (Hecho Generador)

| Tabla: `Avaluos.LiquidacionesPrediales` | Tipo de Dato | Propósito (Hecho Generador a Rentas) |
| :--- | :--- | :--- |
| `Id` | `Guid` (PK) | Identificador de la liquidación. |
| `PredioId` | `Guid` (FK) | Relación directa al Aggregate Root del Módulo 2[cite: 257]. |
| `AnioFiscal` | `int` | Año al que corresponde el tributo (Ej. 2026). |
| `EsSimulacion` | `bit` (bool) | `true` = Simulación de valoración predial[cite: 250, 256]. `false` = Emisión oficial en firme. |
| `ValorTerreno` | `decimal(18,2)` | Avalúo final del suelo tras aplicar área x precio base x afectaciones[cite: 251, 256]. |
| `ValorConstruccion` | `decimal(18,2)` | Avalúo final sumando todos los bloques depreciados[cite: 251, 256]. |
| `BaseImponible` | `decimal(18,2)` | Valor total comercial del predio (Terreno + Construcción)[cite: 250, 251]. |
| `ExoneracionesPrevias`| `decimal(18,2)` | Descuentos calculados por Ley del Anciano, Discapacidad, etc[cite: 250, 251]. |
| `ValorEmision` | `decimal(18,2)` | Valor real del impuesto a enviar a Tesorería / Rentas[cite: 250, 251]. |
| `CemAplicada` | `decimal(18,2)` | Contribución Especial de Mejoras calculada[cite: 250, 251, 256]. |
| `EstadoLiquidacion` | `varchar(20)` | `Simulado`, `Emitido`, `Pagado`, `Anulado`. |

---

## 📊 3. Diagrama del Modelo de Datos (ERD)

```mermaid
erDiagram
    %% Tablas del Módulo 2 (Core)
    Catastro_Predio {
        Guid Id PK
        varchar ClaveCatastral
        char TipoPredio
        decimal AreaTerrenoGrafica
    }
    Catastro_BloqueConstruccion {
        Guid Id PK
        Guid PredioId FK
        decimal AreaConstruccion
        int AnioConstruccion
    }

    %% Tablas del Módulo 4 (Avaluos)
    Avaluos_PreciosBaseSuelo {
        Guid Id PK
        int AnioFiscal
        char TipoPredio
        varchar ManzanaPoligono
        decimal ValorMetroCuadrado
    }
    Avaluos_CostosReposicion {
        Guid Id PK
        int AnioFiscal
        Guid TipoEstructuraId FK
        decimal ValorMetroCuadrado
    }
    Avaluos_LiquidacionesPrediales {
        Guid Id PK
        Guid PredioId FK
        int AnioFiscal
        bit EsSimulacion
        decimal ValorTerreno
        decimal ValorConstruccion
        decimal BaseImponible
        decimal ValorEmision
        varchar EstadoLiquidacion
    }

    %% Relaciones
    Catastro_Predio ||--o{ Catastro_BloqueConstruccion : "posee"
    Catastro_Predio ||--o{ Avaluos_LiquidacionesPrediales : "genera (por año)"
    Avaluos_LiquidacionesPrediales }|..|{ Avaluos_PreciosBaseSuelo : "calcula con"
    Avaluos_LiquidacionesPrediales }|..|{ Avaluos_CostosReposicion : "calcula bloques con"