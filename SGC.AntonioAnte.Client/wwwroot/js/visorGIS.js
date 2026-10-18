window.visorGIS = {
    view: null,
    graphicsLayer: null,
    dotNetRef: null,
    miniView: null,

    setDotNetRef: function (ref) {
        this.dotNetRef = ref;
    },

    abrirFicha: function (idPredio) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('AbrirFichaDesdeMapa', idPredio);
        }
    },

    initMap: async function (containerElement) {
        try {
            if (this.view) {
                this.view.destroy();
                this.view = null;
            }

            const [MapModule, MapViewModule, GraphicsLayerModule] = await Promise.all([
                import("https://js.arcgis.com/4.28/@arcgis/core/Map.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/views/MapView.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/layers/GraphicsLayer.js")
            ]);

            const Map = MapModule.default;
            const MapView = MapViewModule.default;
            const GraphicsLayer = GraphicsLayerModule.default;

            const map = new Map({
                basemap: "streets-navigation-vector"
            });

            this.graphicsLayer = new GraphicsLayer();
            map.add(this.graphicsLayer);

            this.view = new MapView({
                container: containerElement,
                map: map,
                center: [-78.2160, 0.3320],
                zoom: 17
            });

            await this.view.when();
        } catch (error) {
            console.error("Error crítico inicializando ArcGIS:", error);
        }
    },

    loadPredios: async function (predios) {
        try {
            if (!this.graphicsLayer) return;
            this.graphicsLayer.removeAll();

            const [GraphicModule, PolygonModule, SimpleFillSymbolModule] = await Promise.all([
                import("https://js.arcgis.com/4.28/@arcgis/core/Graphic.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/geometry/Polygon.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/symbols/SimpleFillSymbol.js")
            ]);

            const Graphic = GraphicModule.default;
            const Polygon = PolygonModule.default;
            const SimpleFillSymbol = SimpleFillSymbolModule.default;

            var wktParser = new Wkt.Wkt();

            predios.forEach(p => {
                if (p.poligonoWkt) {
                    wktParser.read(p.poligonoWkt);
                    var geojson = wktParser.toJson();

                    var polygon = new Polygon({
                        rings: geojson.coordinates,
                        spatialReference: { wkid: 4326 }
                    });

                    // Proteger contra WKT corruptos en la carga global
                    if (polygon.extent.xmax > 180 || polygon.extent.xmin < -180) return;

                    var isUrbano = p.tipoPredio === 1;
                    var fillSymbol = new SimpleFillSymbol({
                        color: isUrbano ? [59, 130, 246, 0.4] : [34, 197, 94, 0.4],
                        outline: {
                            color: isUrbano ? [37, 99, 235, 1] : [22, 163, 74, 1],
                            width: 2
                        }
                    });

                    var popupTemplate = {
                        title: "Ficha: {claveCatastral}",
                        content: function (feature) {
                            var attr = feature.graphic.attributes;
                            var divContenedor = document.createElement("div");
                            var divInfo = document.createElement("div");
                            divInfo.style.fontSize = "13px";
                            divInfo.style.fontFamily = "monospace";
                            divInfo.style.marginBottom = "15px";
                            divInfo.innerHTML = `
                                <b>Área GIS:</b> ${attr.area} m²<br/>
                                <b>Propietario:</b> ${attr.propietario}<br/>
                                <b>Bloques Const.:</b> ${attr.bloques}<br/>
                                <b>Dirección:</b> ${attr.direccion}
                            `;
                            divContenedor.appendChild(divInfo);

                            var btn = document.createElement("button");
                            btn.className = "btn btn-sm btn-primary w-100 fw-bold shadow-sm";
                            btn.innerHTML = `<i class="bi bi-box-arrow-up-right me-2"></i>Ir a Ficha Catastral`;
                            btn.onclick = function () {
                                window.visorGIS.abrirFicha(attr.id);
                            };

                            divContenedor.appendChild(btn);
                            return divContenedor;
                        }
                    };

                    var graphic = new Graphic({
                        geometry: polygon,
                        symbol: fillSymbol,
                        attributes: {
                            id: p.id,
                            claveCatastral: p.claveCatastral,
                            area: p.areaTerrenoGrafica,
                            propietario: p.propietarioPrincipal,
                            bloques: p.totalBloques,
                            direccion: p.direccion
                        },
                        popupTemplate: popupTemplate
                    });

                    this.graphicsLayer.add(graphic);
                }
            });
        } catch (error) {
            console.error("Error renderizando polígonos WKT:", error);
        }
    },

    initMiniMap: async function (containerId, dotNetRefObj, modoEdicion, wktActual) {
        try {
            if (this.miniView) {
                this.miniView.destroy();
                this.miniView = null;
            }

            // 🔥 SE IMPORTA webMercatorUtils PARA TRANSFORMAR LAS COORDENADAS 🔥
            const [MapModule, MapViewModule, GraphicsLayerModule, GraphicModule, PolygonModule, SimpleFillSymbolModule, SketchModule, geometryEngineModule, webMercatorUtilsModule] = await Promise.all([
                import("https://js.arcgis.com/4.28/@arcgis/core/Map.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/views/MapView.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/layers/GraphicsLayer.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/Graphic.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/geometry/Polygon.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/symbols/SimpleFillSymbol.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/widgets/Sketch.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/geometry/geometryEngine.js"),
                import("https://js.arcgis.com/4.28/@arcgis/core/geometry/support/webMercatorUtils.js")
            ]);

            const Map = MapModule.default;
            const MapView = MapViewModule.default;
            const GraphicsLayer = GraphicsLayerModule.default;
            const Graphic = GraphicModule.default;
            const Polygon = PolygonModule.default;
            const SimpleFillSymbol = SimpleFillSymbolModule.default;
            const Sketch = SketchModule.default;
            const geometryEngine = geometryEngineModule.default || geometryEngineModule;
            const webMercatorUtils = webMercatorUtilsModule.default || webMercatorUtilsModule;

            const tempGraphicsLayer = new GraphicsLayer();
            const map = new Map({
                basemap: "hybrid",
                layers: [tempGraphicsLayer]
            });

            this.miniView = new MapView({
                container: containerId,
                map: map,
                center: [-78.2160, 0.3320], // Atuntaqui
                zoom: 18,
                ui: { components: ["zoom"] }
            });

            await this.miniView.when();

            if (wktActual && wktActual.startsWith("POLYGON")) {
                var wktParser = new Wkt.Wkt();
                wktParser.read(wktActual);
                var geojson = wktParser.toJson();

                var polygon = new Polygon({
                    rings: geojson.coordinates,
                    spatialReference: { wkid: 4326 }
                });

                // 🔥 ESCUDO: Si el WKT guardado antes es Web Mercator (millones), no lo dibujamos para no romper el mapa
                if (polygon.extent.xmax > 180 || polygon.extent.xmin < -180) {
                    console.warn("El WKT almacenado es corrupto (Web Mercator). Debe redibujar el polígono.");
                } else {
                    var fillSymbol = new SimpleFillSymbol({
                        color: [234, 179, 8, 0.4],
                        outline: { color: [202, 138, 4, 1], width: 2 }
                    });

                    var graphic = new Graphic({
                        geometry: polygon,
                        symbol: fillSymbol
                    });

                    tempGraphicsLayer.add(graphic);
                    this.miniView.goTo(polygon.extent.expand(2.0));
                }
            }

            if (modoEdicion) {
                const sketch = new Sketch({
                    layer: tempGraphicsLayer,
                    view: this.miniView,
                    creationMode: "single",
                    availableCreateTools: ["polygon", "rectangle"]
                });

                this.miniView.ui.add(sketch, "top-right");

                const procesarGeometria = (geometry) => {
                    const areaGeodesica = geometryEngine.geodesicArea(geometry, "square-meters");

                    // 🔥 CONVERSIÓN OBLIGATORIA A LATITUD/LONGITUD ANTES DE GENERAR WKT 🔥
                    let geomToWkt = geometry;
                    if (geometry.spatialReference.isWebMercator) {
                        geomToWkt = webMercatorUtils.webMercatorToGeographic(geometry);
                    }

                    let anillosWkt = geomToWkt.rings.map(ring => {
                        let puntos = ring.map(pt => `${pt[0].toFixed(6)} ${pt[1].toFixed(6)}`);
                        return `(${puntos.join(", ")})`;
                    }).join(", ");

                    const wktFinal = `POLYGON (${anillosWkt})`;

                    if (dotNetRefObj) {
                        dotNetRefObj.invokeMethodAsync('ActualizarDatosEspaciales', wktFinal, areaGeodesica);
                    }
                };

                sketch.on("create", (event) => {
                    if (event.state === "complete") {
                        if (tempGraphicsLayer.graphics.length > 1) {
                            const newGraphic = event.graphic;
                            tempGraphicsLayer.removeAll();
                            tempGraphicsLayer.add(newGraphic);
                        }
                        procesarGeometria(event.graphic.geometry);
                    }
                });

                sketch.on("update", (event) => {
                    if (event.state === "complete" || event.state === "active") {
                        procesarGeometria(event.graphics[0].geometry);
                    }
                });
            }
        } catch (error) {
            console.error("Error inicializando Mini-Map de Catastro:", error);
        }
    }
};