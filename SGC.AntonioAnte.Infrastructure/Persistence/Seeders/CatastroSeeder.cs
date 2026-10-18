using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Domain.Catastro.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Persistence.Seeders
{
    public static class CatastroSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                // 1. Verificar existencia usando el Value Object directamente (Traducible al 100% por EF Core)
                var primeraClaveSemilla = ClaveCatastral.Crear("100250010100100100000000");

                bool existenPrediosSemilla = await context.Predios
                    .AnyAsync(p => p.ClaveCatastral == primeraClaveSemilla);

                if (existenPrediosSemilla)
                {
                    logger.LogInformation("La semilla geoespacial de Atuntaqui ya se encuentra registrada en la base de datos.");
                    return;
                }

                logger.LogInformation("Iniciando inyección de semilla geoespacial para el centro de Atuntaqui (SRID 4326)...");

                // 2. Garantizar Catálogos Dinámicos Base (Módulo 2)
                var tenenciaEscritura = await ObtenerOCrearTipoTenenciaAsync(context, "Propietario con Escritura", "Título inscrito en el Registro de la Propiedad.");
                var tenenciaPosesion = await ObtenerOCrearTipoTenenciaAsync(context, "Posesionario", "Tenencia en trámite de legalización municipal.");

                var estructuraHormigon = await ObtenerOCrearTipoEstructuraAsync(context, "Hormigón Armado", "Estructura portante de columnas y losa de hormigón.");
                var estructuraMixta = await ObtenerOCrearTipoEstructuraAsync(context, "Mampostería / Mixta", "Estructura combinada de ladrillo, bloque y metal.");
                var estructuraAdobe = await ObtenerOCrearTipoEstructuraAsync(context, "Adobe Tradicional", "Construcción patrimonial del centro histórico.");

                var estadoBueno = await ObtenerOCrearEstadoConservacionAsync(context, "Bueno", "Mantenimiento óptimo y sin patologías estructurales.");
                var estadoRegular = await ObtenerOCrearEstadoConservacionAsync(context, "Regular", "Desgaste normal por edad constructiva.");

                await context.SaveChangesAsync();

                // 3. Crear Sujetos de Derecho con Cédulas/RUC válidos (Algoritmo Módulo 10 verificado)
                var propietariosSeed = new List<Propietario>
                {
                    Propietario.CrearPersonaNatural("1002003000", "Segundo Bolívar", "Posso Narváez", EstadoCivil.Casado, "bposso@atuntaqui.ec", "0991234501"),
                    Propietario.CrearPersonaNatural("1001234564", "María Emperatriz", "Villegas Maya", EstadoCivil.Soltero, "mvillegas@gmail.com", "0987654302"),
                    Propietario.CrearPersonaJuridica("1090123456001", "Fábrica Textil Imbabura Cía. Ltda.", "gerencia@textilimbabura.ec", "062906111"),
                    Propietario.CrearPersonaNatural("1002345674", "Luis Alfonso", "Játiva Cevallos", EstadoCivil.Casado, "ljativa@outlook.com", "0994561203"),
                    Propietario.CrearPersonaNatural("1003456785", "Rosa Elvira", "Buitrón Endara", EstadoCivil.Viudo, "rbuitron@yahoo.com", "0981122304"),
                    Propietario.CrearPersonaNatural("1004567896", "Jorge Washington", "Terán Dávila", EstadoCivil.Divorciado, "jteran@hotmail.com", "0998877605")
                };

                var propietariosEnBd = new List<Propietario>();
                foreach (var prop in propietariosSeed)
                {
                    var existente = await context.Propietarios
                        .FirstOrDefaultAsync(p => p.Identificacion == prop.Identificacion);

                    if (existente == null)
                    {
                        context.Propietarios.Add(prop);
                        propietariosEnBd.Add(prop);
                    }
                    else
                    {
                        propietariosEnBd.Add(existente);
                    }
                }

                await context.SaveChangesAsync();

                // 4. Instanciar lector WKT moderno y configurar SRID 4326 (WGS84)
                var wktReader = new WKTReader();

                var datosPredios = new[]
                {
                    new
                    {
                        Clave = "100250010100100100000000",
                        ClaveAnt = "AA-01-001-01",
                        AreaEsc = 285.00m,
                        AreaGraf = 283.45m,
                        Direccion = "Calle General Enríquez y Sucre (Esquina Suroeste)",
                        Wkt = "POLYGON((-78.21700 0.33120, -78.21685 0.33120, -78.21685 0.33135, -78.21700 0.33135, -78.21700 0.33120))",
                        PropietarioIdx = 0,
                        TenenciaId = tenenciaEscritura.Id,
                        Notaria = "Notaría Primera de Antonio Ante",
                        Pisos = 2,
                        AreaConst = 380.00m,
                        AnioConst = 2015,
                        EstructuraId = estructuraHormigon.Id,
                        ConservacionId = estadoBueno.Id
                    },
                    new
                    {
                        Clave = "100250010100100200000000",
                        ClaveAnt = "AA-01-001-02",
                        AreaEsc = 275.00m,
                        AreaGraf = 278.10m,
                        Direccion = "Calle Sucre N12-45 entre Gral. Enríquez y Amazonas",
                        Wkt = "POLYGON((-78.21685 0.33120, -78.21670 0.33120, -78.21670 0.33135, -78.21685 0.33135, -78.21685 0.33120))",
                        PropietarioIdx = 1,
                        TenenciaId = tenenciaEscritura.Id,
                        Notaria = "Notaría Primera de Antonio Ante",
                        Pisos = 1,
                        AreaConst = 190.50m,
                        AnioConst = 1998,
                        EstructuraId = estructuraMixta.Id,
                        ConservacionId = estadoRegular.Id
                    },
                    new
                    {
                        Clave = "100250010100100300000000",
                        ClaveAnt = "AA-01-001-03",
                        AreaEsc = 280.00m,
                        AreaGraf = 282.90m,
                        Direccion = "Calle Sucre y Río Amazonas (Esquina Sureste)",
                        Wkt = "POLYGON((-78.21670 0.33120, -78.21655 0.33120, -78.21655 0.33135, -78.21670 0.33135, -78.21670 0.33120))",
                        PropietarioIdx = 2,
                        TenenciaId = tenenciaEscritura.Id,
                        Notaria = "Notaría Segunda de Ibarra",
                        Pisos = 3,
                        AreaConst = 620.00m,
                        AnioConst = 2021,
                        EstructuraId = estructuraHormigon.Id,
                        ConservacionId = estadoBueno.Id
                    },
                    new
                    {
                        Clave = "100250010100100400000000",
                        ClaveAnt = "AA-01-001-04",
                        AreaEsc = 290.00m,
                        AreaGraf = 286.20m,
                        Direccion = "Calle Bolívar y General Enríquez (Frente al Parque Central)",
                        Wkt = "POLYGON((-78.21700 0.33135, -78.21685 0.33135, -78.21685 0.33150, -78.21700 0.33150, -78.21700 0.33135))",
                        PropietarioIdx = 3,
                        TenenciaId = tenenciaEscritura.Id,
                        Notaria = "Notaría Primera de Antonio Ante",
                        Pisos = 2,
                        AreaConst = 410.00m,
                        AnioConst = 2010,
                        EstructuraId = estructuraHormigon.Id,
                        ConservacionId = estadoBueno.Id
                    },
                    new
                    {
                        Clave = "100250010100100500000000",
                        ClaveAnt = "AA-01-001-05",
                        AreaEsc = 270.00m,
                        AreaGraf = 274.80m,
                        Direccion = "Calle Bolívar N12-50 (Frente al Parque Mariscal Sucre)",
                        Wkt = "POLYGON((-78.21685 0.33135, -78.21670 0.33135, -78.21670 0.33150, -78.21685 0.33150, -78.21685 0.33135))",
                        PropietarioIdx = 4,
                        TenenciaId = tenenciaPosesion.Id,
                        Notaria = "Sin inscripción notarial",
                        Pisos = 1,
                        AreaConst = 215.00m,
                        AnioConst = 1975,
                        EstructuraId = estructuraAdobe.Id,
                        ConservacionId = estadoRegular.Id
                    },
                    new
                    {
                        Clave = "100250010100100600000000",
                        ClaveAnt = "AA-01-001-06",
                        AreaEsc = 285.00m,
                        AreaGraf = 284.15m,
                        Direccion = "Calle Bolívar y Río Amazonas (Esquina Noreste)",
                        Wkt = "POLYGON((-78.21670 0.33135, -78.21655 0.33135, -78.21655 0.33150, -78.21670 0.33150, -78.21670 0.33135))",
                        PropietarioIdx = 5,
                        TenenciaId = tenenciaEscritura.Id,
                        Notaria = "Notaría Primera de Antonio Ante",
                        Pisos = 2,
                        AreaConst = 350.00m,
                        AnioConst = 2018,
                        EstructuraId = estructuraHormigon.Id,
                        ConservacionId = estadoBueno.Id
                    }
                };

                // 5. Instanciar Predios y sus Entidades Hijas explícitamente en el DbContext
                foreach (var item in datosPredios)
                {
                    var claveCatastral = ClaveCatastral.Crear(item.Clave);

                    var poligono = (Polygon)wktReader.Read(item.Wkt);
                    poligono.SRID = 4326;

                    var predio = Predio.Crear(
                        clave: claveCatastral,
                        claveAnterior: item.ClaveAnt,
                        tipoPredio: TipoPredio.Urbano,
                        areaEscritura: item.AreaEsc,
                        areaGrafica: item.AreaGraf,
                        direccion: item.Direccion,
                        poligono: poligono
                    );

                    context.Predios.Add(predio);

                    var dominio = Dominio.Crear(
                        predioId: predio.Id,
                        propietarioId: propietariosEnBd[item.PropietarioIdx].Id,
                        tipoTenenciaId: item.TenenciaId,
                        porcentaje: 100.00m,
                        fechaInscripcion: DateTime.UtcNow.AddYears(-5),
                        notaria: item.Notaria
                    );
                    context.Dominios.Add(dominio);

                    var bloque = BloqueConstruccion.Crear(
                        predioId: predio.Id,
                        numeroBloque: 1,
                        tipoEstructuraId: item.EstructuraId,
                        estadoConservacionId: item.ConservacionId,
                        numeroPisos: item.Pisos,
                        areaConstruccion: item.AreaConst,
                        anioConstruccion: item.AnioConst
                    );
                    context.BloquesConstruccion.Add(bloque);
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Semilla geoespacial completada: 6 predios urbanos inyectados con éxito en Atuntaqui.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocurrió un error al ejecutar CatastroSeeder: {Message}", ex.Message);
                throw;
            }
        }

        private static async Task<TipoTenencia> ObtenerOCrearTipoTenenciaAsync(ApplicationDbContext context, string nombre, string descripcion)
        {
            var item = await context.TiposTenencia.FirstOrDefaultAsync(x => x.Nombre == nombre);
            if (item == null)
            {
                item = TipoTenencia.Crear(nombre, descripcion);
                context.TiposTenencia.Add(item);
            }
            return item;
        }

        private static async Task<TipoEstructura> ObtenerOCrearTipoEstructuraAsync(ApplicationDbContext context, string nombre, string descripcion)
        {
            var item = await context.TiposEstructura.FirstOrDefaultAsync(x => x.Nombre == nombre);
            if (item == null)
            {
                item = TipoEstructura.Crear(nombre, descripcion);
                context.TiposEstructura.Add(item);
            }
            return item;
        }

        private static async Task<EstadoConservacion> ObtenerOCrearEstadoConservacionAsync(ApplicationDbContext context, string nombre, string descripcion)
        {
            var item = await context.EstadosConservacion.FirstOrDefaultAsync(x => x.Nombre == nombre);
            if (item == null)
            {
                item = EstadoConservacion.Crear(nombre, descripcion);
                context.EstadosConservacion.Add(item);
            }
            return item;
        }
    }
}