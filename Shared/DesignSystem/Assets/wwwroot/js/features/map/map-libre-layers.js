import {
    state,
    ORG_SOURCE_ID,
    USER_SOURCE_ID,
    WAYPOINT_SOURCE_ID,
    ROUTE_SOURCE_ID,
    ZONE_SOURCE_ID,
    OVERLAY_VECTOR_SOURCE_ID,
    TRANSPORT_LAYER_ID,
    TRAFFIC_LAYER_ID,
    SELECTED_PLACE_SOURCE_ID
} from './map-libre-state.js';

import {
    getFallbackStyle,
    ensureMap
} from './map-libre-core.js';

export function closeOrganizationPopup() {
    if (!state.organizationPopup) return;
    state.organizationPopup.remove();
    state.organizationPopup = null;
}

export function showOrganizationPopup(feature, lngLat) {
    if (!state.map || !window.maplibregl?.Popup) return;

    const popupHtml = feature?.properties?.popupHtml;
    if (!popupHtml) return;

    const id = String(feature?.properties?.id || "").trim();

    closeOrganizationPopup();

    state.organizationPopup = new window.maplibregl.Popup({ closeButton: true, closeOnClick: true })
        .setLngLat(lngLat)
        .setHTML(popupHtml)
        .addTo(state.map);

    const popupElement = state.organizationPopup.getElement();
    const container = popupElement?.querySelector(".org-popup");
    if (!container) return;

    container.addEventListener("click", (event) => {
        const action = event?.target?.getAttribute?.("data-action");
        if (!action || !state.activeDotNetRef?.invokeMethodAsync) return;

        if (action === "route") {
            state.activeDotNetRef.invokeMethodAsync("OnMapMarkerRouteById", id).catch(() => { });
            closeOrganizationPopup();
            return;
        }

        if (action === "add-waypoint") {
            state.activeDotNetRef.invokeMethodAsync("OnMapMarkerAddWaypointById", id).catch(() => { });
            closeOrganizationPopup();
        }
    });
}

export function getSource(id) {
    if (!state.map) return null;
    if (!state.mapReady && typeof state.map.isStyleLoaded === "function" && state.map.isStyleLoaded()) {
        state.mapReady = true;
        ensureSourcesAndLayers();
    }
    if (!state.mapReady) return null;
    const source = state.map.getSource(id);
    return source && typeof source.setData === "function" ? source : null;
}

export function syncOrganizations() {
    const source = getSource(ORG_SOURCE_ID);
    if (!source) return;
    source.setData(state.organizationGeoJson || { type: "FeatureCollection", features: [] });
}

export function syncUser() {
    const source = getSource(USER_SOURCE_ID);
    if (!source) return;
    source.setData(state.userLocationGeoJson || { type: "FeatureCollection", features: [] });
}

export function syncWaypoints() {
    const source = getSource(WAYPOINT_SOURCE_ID);
    if (!source) return;
    source.setData(state.waypointGeoJson || { type: "FeatureCollection", features: [] });
}

export function syncRoute() {
    const source = getSource(ROUTE_SOURCE_ID);
    if (!source) return;
    source.setData(state.routeGeoJson || { type: "FeatureCollection", features: [] });
    if (state.map?.getLayer("route-line")) {
        state.map.setPaintProperty("route-line", "line-color", state.routeColor || "#1d4ed8");
    }
}

export function syncZone() {
    const source = getSource(ZONE_SOURCE_ID);
    if (!source) return;
    source.setData(state.zoneGeoJson || { type: "FeatureCollection", features: [] });
}

export function syncSelectedPlace() {
    const source = getSource(SELECTED_PLACE_SOURCE_ID);
    if (!source) return;
    source.setData(state.selectedPlaceGeoJson || { type: "FeatureCollection", features: [] });
}

export function syncAllSources() {
    syncOrganizations();
    syncUser();
    syncWaypoints();
    syncRoute();
    syncZone();
    syncSelectedPlace();
}

export function addLayerIfMissing(layer, beforeLayerId) {
    if (!state.map || state.map.getLayer(layer.id)) return;

    if (beforeLayerId && state.map.getLayer(beforeLayerId)) {
        state.map.addLayer(layer, beforeLayerId);
        return;
    }
    state.map.addLayer(layer);
}

export function setLayerVisibility(layerId, isVisible) {
    if (!state.map || !state.map.getLayer(layerId)) return;
    state.map.setLayoutProperty(layerId, "visibility", isVisible ? "visible" : "none");
}

export function ensureSupplementaryOverlays() {
    if (!state.map || !state.mapReady) return;

    if (!state.map.getSource(OVERLAY_VECTOR_SOURCE_ID)) {
        state.map.addSource(OVERLAY_VECTOR_SOURCE_ID, {
            type: "vector",
            tiles: ["https://tiles.versatiles.org/tiles/osm/{z}/{x}/{y}"],
            attribution: "&copy; OpenStreetMap contributors"
        });
    }

    addLayerIfMissing(
        {
            id: TRANSPORT_LAYER_ID,
            type: "line",
            source: OVERLAY_VECTOR_SOURCE_ID,
            "source-layer": "transportation",
            filter: ["in", ["coalesce", ["get", "class"], ""], ["literal", ["rail", "subway", "tram", "light_rail", "monorail"]]],
            paint: {
                "line-color": "#7c3aed",
                "line-width": ["interpolate", ["linear"], ["zoom"], 10, 0.8, 14, 1.8, 18, 3.2],
                "line-opacity": 0.72
            }
        },
        "route-casing"
    );

    addLayerIfMissing(
        {
            id: TRAFFIC_LAYER_ID,
            type: "line",
            source: OVERLAY_VECTOR_SOURCE_ID,
            "source-layer": "transportation",
            filter: ["in", ["coalesce", ["get", "class"], ""], ["literal", ["motorway", "trunk", "primary", "secondary"]]],
            paint: {
                "line-color": [
                    "match",
                    ["coalesce", ["get", "class"], ""],
                    "motorway",
                    "#ef4444",
                    "trunk",
                    "#f97316",
                    "primary",
                    "#f59e0b",
                    "#fbbf24"
                ],
                "line-width": ["interpolate", ["linear"], ["zoom"], 9, 0.6, 12, 1.6, 15, 2.8, 18, 4.8],
                "line-opacity": 0.6
            }
        },
        "route-casing"
    );
}

export function applyOverlayVisibility() {
    setLayerVisibility("organization-circles", state.showOrganizationsLayer);
    setLayerVisibility("waypoint-circles", state.showOrganizationsLayer);
    setLayerVisibility("user-circle", state.showOrganizationsLayer);
    setLayerVisibility("selected-place-marker", state.showOrganizationsLayer);
    setLayerVisibility("route-casing", state.showRouteLayer);
    setLayerVisibility("route-line", state.showRouteLayer);
    setLayerVisibility("zone-fill", state.showZoneLayer);
    setLayerVisibility("zone-line", state.showZoneLayer);
    setLayerVisibility(TRANSPORT_LAYER_ID, state.showTransportLayer);
    setLayerVisibility(TRAFFIC_LAYER_ID, state.showTrafficLayer);
    setLayerVisibility("3d-buildings", true);
}

export function ensureSourcesAndLayers() {
    if (!state.map || !state.mapReady || state.isEnsuringLayers) return;

    state.isEnsuringLayers = true;
    try {
        if (!state.map.getSource(ORG_SOURCE_ID)) {
            state.map.addSource(ORG_SOURCE_ID, { type: "geojson", data: { type: "FeatureCollection", features: [] } });
        }
        if (!state.map.getSource(USER_SOURCE_ID)) {
            state.map.addSource(USER_SOURCE_ID, { type: "geojson", data: { type: "FeatureCollection", features: [] } });
        }
        if (!state.map.getSource(WAYPOINT_SOURCE_ID)) {
            state.map.addSource(WAYPOINT_SOURCE_ID, { type: "geojson", data: { type: "FeatureCollection", features: [] } });
        }
        if (!state.map.getSource(ROUTE_SOURCE_ID)) {
            state.map.addSource(ROUTE_SOURCE_ID, { type: "geojson", data: { type: "FeatureCollection", features: [] } });
        }
        if (!state.map.getSource(ZONE_SOURCE_ID)) {
            state.map.addSource(ZONE_SOURCE_ID, { type: "geojson", data: { type: "FeatureCollection", features: [] } });
        }
        if (!state.map.getSource(SELECTED_PLACE_SOURCE_ID)) {
            state.map.addSource(SELECTED_PLACE_SOURCE_ID, { type: "geojson", data: { type: "FeatureCollection", features: [] } });
        }

        // ── Colorful Globe / Boundaries (User request) ────────
        if (!state.map.getSource("world-countries")) {
            state.map.addSource("world-countries", {
                type: "geojson",
                data: "https://d2ad6b4ur7yvpq.cloudfront.net/naturalearth-3.3.0/ne_110m_admin_0_countries.geojson"
            });
        }

        if (!state.map.getSource("world-states")) {
            state.map.addSource("world-states", {
                type: "geojson",
                data: "https://d2ad6b4ur7yvpq.cloudfront.net/naturalearth-3.3.0/ne_50m_admin_1_states_provinces.geojson"
            });
        }

        // Add country fill layer
        addLayerIfMissing({
            id: "country-fills",
            type: "fill",
            source: "world-countries",
            paint: {
                "fill-color": [
                    "match",
                    ["get", "mapcolor7"],
                    1, "#ff9999", // soft red
                    2, "#ffcc99", // soft orange
                    3, "#ffff99", // soft yellow
                    4, "#b3ffb3", // soft green
                    5, "#b3ffff", // soft light blue
                    6, "#c2c2f0", // soft blue-purple
                    7, "#ffb3ff", // soft pink
                    "#e2e8f0"
                ],
                "fill-opacity": [
                    "interpolate",
                    ["linear"],
                    ["zoom"],
                    0, 0.45,
                    3, 0.45,
                    5, 0.0
                ]
            }
        }, "route-casing"); // insert under route lines

        // Add states / regions fill layer
        addLayerIfMissing({
            id: "state-fills",
            type: "fill",
            source: "world-states",
            paint: {
                "fill-color": [
                    "match",
                    ["%", ["abs", ["round", ["coalesce", ["get", "latitude"], 0]]], 8],
                    0, "#ffb3b3",
                    1, "#ffe0b3",
                    2, "#ffffcc",
                    3, "#d9ffd9",
                    4, "#d9ffff",
                    5, "#e6e6fa",
                    6, "#ffd9ec",
                    7, "#e0f2f1",
                    "#f1f5f9"
                ],
                "fill-opacity": [
                    "interpolate",
                    ["linear"],
                    ["zoom"],
                    3, 0.0,
                    5, 0.38,
                    8, 0.38,
                    10, 0.0
                ]
            }
        }, "route-casing");

        // Check active vector tile source and inject colorful city / district layers
        if (state.map.getSource("openmaptiles")) {
            // Dynamic colorful landuse polygons for dark mode
            addLayerIfMissing({
                id: "colorful-landuse-openmaptiles",
                type: "fill",
                source: "openmaptiles",
                "source-layer": "landuse",
                paint: {
                    "fill-color": [
                        "match", ["get", "class"],
                        "residential", "#fdf2f8", // soft pink
                        "commercial", "#eff6ff",  // soft blue
                        "industrial", "#f8fafc",  // soft slate
                        "retail", "#fffbeb",      // soft yellow
                        "park", "#d1fae5",        // soft emerald green
                        "cemetery", "#f0fdf4",
                        "hospital", "#fff1f2",
                        "school", "#fff7ed",
                        "#f8fafc"
                    ],
                    "fill-opacity": 0.45
                }
            }, "route-casing");

            // District boundaries lines (admin_level 6, 8, 10) for openmaptiles
            addLayerIfMissing({
                id: "district-boundaries-openmaptiles",
                type: "line",
                source: "openmaptiles",
                "source-layer": "boundary",
                filter: ["in", "admin_level", 6, 8, 10],
                paint: {
                    "line-color": [
                        "match", ["get", "admin_level"],
                        6, "#f97316", // orange
                        8, "#10b981", // green (district)
                        10, "#3b82f6", // blue (neighborhood)
                        "#94a3b8"
                    ],
                    "line-width": [
                        "match", ["get", "admin_level"],
                        6, 2,
                        8, 1.5,
                        10, 1,
                        1
                    ]
                }
            }, "route-casing");

            // Dynamic colorful background circles behind labels for dark mode
            addLayerIfMissing({
                id: "colorful-places-bg-openmaptiles",
                type: "circle",
                source: "openmaptiles",
                "source-layer": "place",
                filter: ["in", "class", "city", "town", "suburb", "neighbourhood"],
                paint: {
                    "circle-radius": [
                        "interpolate", ["linear"], ["zoom"],
                        4, ["match", ["get", "class"], "city", 3.5, "town", 2, 0],
                        8, ["match", ["get", "class"], "city", 6.5, "town", 5, "suburb", 3.5, 0],
                        12, ["match", ["get", "class"], "city", 11, "town", 9, "suburb", 7.5, "neighbourhood", 5, 0],
                        16, ["match", ["get", "class"], "city", 15, "town", 13, "suburb", 11.5, "neighbourhood", 9.5, 0]
                    ],
                    "circle-color": [
                        "match", ["get", "class"],
                        "city", "#ec4899",       // pink
                        "town", "#f97316",       // orange
                        "suburb", "#10b981",     // green
                        "neighbourhood", "#3b82f6", // blue
                        "#94a3b8"
                    ],
                    "circle-opacity": [
                        "interpolate", ["linear"], ["zoom"],
                        4, 0.15,
                        8, 0.35,
                        12, 0.45,
                        16, 0.55
                    ],
                    "circle-stroke-color": "#ffffff",
                    "circle-stroke-width": [
                        "interpolate", ["linear"], ["zoom"],
                        4, 0.5,
                        12, 1.5
                    ]
                }
            }, "route-casing");
        }

        if (state.map.getSource("versatiles-shortbread")) {
            // Dynamic colorful landuse polygons for light mode
            addLayerIfMissing({
                id: "colorful-landuse-shortbread",
                type: "fill",
                source: "versatiles-shortbread",
                "source-layer": "land",
                paint: {
                    "fill-color": [
                        "match", ["get", "kind"],
                        "residential", "#fdf2f8", // soft pink
                        "commercial", "#eff6ff",  // soft blue
                        "industrial", "#f8fafc",  // soft slate
                        "retail", "#fffbeb",      // soft yellow
                        "park", "#d1fae5",        // soft emerald green
                        "cemetery", "#f0fdf4",
                        "hospital", "#fff1f2",
                        "school", "#fff7ed",
                        "#f8fafc"
                    ],
                    "fill-opacity": 0.45
                }
            }, "route-casing");

            // District boundaries lines (admin_level 6, 8, 10) for shortbread
            addLayerIfMissing({
                id: "district-boundaries-shortbread",
                type: "line",
                source: "versatiles-shortbread",
                "source-layer": "boundaries",
                filter: ["in", "admin_level", 6, 8, 10],
                paint: {
                    "line-color": [
                        "match", ["get", "admin_level"],
                        6, "#f97316", // orange
                        8, "#10b981", // green (district)
                        10, "#3b82f6", // blue (neighborhood)
                        "#94a3b8"
                    ],
                    "line-width": [
                        "match", ["get", "admin_level"],
                        6, 2,
                        8, 1.5,
                        10, 1,
                        1
                    ]
                }
            }, "route-casing");

            // Dynamic colorful background circles behind labels for light mode
            addLayerIfMissing({
                id: "colorful-places-bg-shortbread",
                type: "circle",
                source: "versatiles-shortbread",
                "source-layer": "place_labels",
                filter: ["in", "kind", "city", "town", "village", "suburb", "quarter", "neighbourhood"],
                paint: {
                    "circle-radius": [
                        "interpolate", ["linear"], ["zoom"],
                        4, ["match", ["get", "kind"], "city", 3.5, "town", 2, 0],
                        8, ["match", ["get", "kind"], "city", 6.5, "town", 5, "suburb", 3.5, 0],
                        12, ["match", ["get", "kind"], "city", 11, "town", 9, "suburb", 7.5, "quarter", 6, "neighbourhood", 5, 0],
                        16, ["match", ["get", "kind"], "city", 15, "town", 13, "suburb", 11.5, "quarter", 10.5, "neighbourhood", 9.5, 0]
                    ],
                    "circle-color": [
                        "match", ["get", "kind"],
                        "city", "#ec4899",       // pink
                        "town", "#f97316",       // orange
                        "suburb", "#10b981",     // green (district)
                        "quarter", "#14b8a6",    // teal
                        "neighbourhood", "#3b82f6", // blue (neighborhood)
                        "#94a3b8"
                    ],
                    "circle-opacity": [
                        "interpolate", ["linear"], ["zoom"],
                        4, 0.15,
                        8, 0.35,
                        12, 0.45,
                        16, 0.55
                    ],
                    "circle-stroke-color": "#ffffff",
                    "circle-stroke-width": [
                        "interpolate", ["linear"], ["zoom"],
                        4, 0.5,
                        12, 1.5
                    ]
                }
            }, "route-casing");
        }

        ensureSupplementaryOverlays();

        const buildingHeightExpr = [
            "coalesce",
            ["get", "render_height"],
            ["get", "height"],
            ["*", ["coalesce", ["get", "building:levels"], ["get", "levels"], 1], 3.5]
        ];
        const buildingMinHeightExpr = [
            "coalesce",
            ["get", "render_min_height"],
            ["get", "min_height"],
            0
        ];

        const existingBuildingLayer = state.map.getStyle()?.layers?.find(
            (l) => l.type === "fill" && (l["source-layer"] === "buildings" || l["source-layer"] === "building")
        );
        const buildingsSourceId = existingBuildingLayer?.source || "3d-buildings-vector";
        const buildingsSourceLayer = existingBuildingLayer?.["source-layer"] || "buildings";

        if (!existingBuildingLayer && !state.map.getSource("3d-buildings-vector")) {
            state.map.addSource("3d-buildings-vector", {
                type: "vector",
                tiles: ["https://tiles.versatiles.org/tiles/osm/{z}/{x}/{y}"],
                minzoom: 0,
                maxzoom: 14,
                attribution: "&copy; OpenStreetMap contributors"
            });
        }

        addLayerIfMissing({
            id: "3d-buildings",
            type: "fill-extrusion",
            source: buildingsSourceId,
            "source-layer": buildingsSourceLayer,
            minzoom: 14,
            paint: {
                "fill-extrusion-color": [
                    "interpolate",
                    ["linear"],
                    buildingHeightExpr,
                    0, "#00ffd0",     // 1
                    8, "#00ff66",     // 2
                    16, "#aaff00",    // 3
                    25, "#00ffb3",    // 4
                    36, "#ffd500",    // 5
                    50, "#ff8c00",    // 6
                    70, "#ff3c00",    // 7
                    95, "#ff0066",    // 8
                    125, "#b700ff",   // 9
                    160, "#6f00ff",   // 10
                    200, "#0066ff",   // 11
                    250, "#00d0ff"    // 12
                ],
                "fill-extrusion-height": buildingHeightExpr,
                "fill-extrusion-base": buildingMinHeightExpr,
                "fill-extrusion-opacity": 0.78
            }
        });

        const originalBuildingColor = [
            "interpolate",
            ["linear"],
            buildingHeightExpr,
            0, "#00ffd0",
            8, "#00ff66",
            16, "#aaff00",
            25, "#00ffb3",
            36, "#ffd500",
            50, "#ff8c00",
            70, "#ff3c00",
            95, "#ff0066",
            125, "#b700ff",
            160, "#6f00ff",
            200, "#0066ff",
            250, "#00d0ff"
        ];

        state.map.on("mouseenter", "3d-buildings", () => {
            state.map.getCanvas().style.cursor = "pointer";
            state.map.setPaintProperty("3d-buildings", "fill-extrusion-opacity", 0.92);
        });

        state.map.on("mouseleave", "3d-buildings", () => {
            state.map.getCanvas().style.cursor = "";
            state.map.setPaintProperty("3d-buildings", "fill-extrusion-opacity", 0.78);
            state.map.setPaintProperty("3d-buildings", "fill-extrusion-color", originalBuildingColor);
        });

        addLayerIfMissing({
            id: "zone-fill",
            type: "fill",
            source: ZONE_SOURCE_ID,
            paint: { "fill-color": "#f97316", "fill-opacity": 0.34 }
        });

        addLayerIfMissing({
            id: "zone-line",
            type: "line",
            source: ZONE_SOURCE_ID,
            paint: { "line-color": "#ea580c", "line-width": 4 }
        });

        addLayerIfMissing({
            id: "route-casing",
            type: "line",
            source: ROUTE_SOURCE_ID,
            layout: { "line-join": "round", "line-cap": "round" },
            paint: { "line-color": "#ffffff", "line-width": 10 }
        });

        addLayerIfMissing({
            id: "route-line",
            type: "line",
            source: ROUTE_SOURCE_ID,
            layout: { "line-join": "round", "line-cap": "round" },
            paint: { "line-color": state.routeColor, "line-width": 6 }
        });

        addLayerIfMissing({
            id: "organization-circles",
            type: "circle",
            source: ORG_SOURCE_ID,
            paint: {
                "circle-radius": 9,
                "circle-color": "#dc2626",
                "circle-stroke-color": "#ffffff",
                "circle-stroke-width": 2.5
            }
        });

        addLayerIfMissing({
            id: "waypoint-circles",
            type: "circle",
            source: WAYPOINT_SOURCE_ID,
            paint: {
                "circle-radius": 6,
                "circle-color": "#111827",
                "circle-stroke-color": "#ffffff",
                "circle-stroke-width": 2
            }
        });

        addLayerIfMissing({
            id: "user-circle",
            type: "circle",
            source: USER_SOURCE_ID,
            paint: {
                "circle-radius": 8,
                "circle-color": "#2563eb",
                "circle-stroke-color": "#ffffff",
                "circle-stroke-width": 2.5
            }
        });

        addLayerIfMissing({
            id: "selected-place-marker",
            type: "circle",
            source: SELECTED_PLACE_SOURCE_ID,
            paint: {
                "circle-radius": 10,
                "circle-color": "#10b981",
                "circle-stroke-color": "#ffffff",
                "circle-stroke-width": 3
            }
        });

        if (!state.map.__orgClickHandlerAttached) {
            state.map.on("click", "organization-circles", (event) => {
                const feature = event?.features?.[0];
                if (!feature) return;

                const markerId = String(feature?.properties?.id || "").trim();
                if (markerId && state.activeDotNetRef?.invokeMethodAsync) {
                    state.activeDotNetRef.invokeMethodAsync("OnMapMarkerClickedById", markerId).catch(() => { });
                }
            });

            state.map.on("mouseenter", "organization-circles", () => {
                state.map.getCanvas().style.cursor = "pointer";
            });

            state.map.on("mouseleave", "organization-circles", () => {
                state.map.getCanvas().style.cursor = "";
            });

            state.map.__orgClickHandlerAttached = true;
        }

        if (!state.map.__mapClickHandlerAttached) {
            state.map.on("click", (event) => {
                // Skip if drawing a polygon
                if (state.isPolygonDrawActive) return;

                // Skip if click was on an organization marker
                const orgFeatures = state.map.queryRenderedFeatures(event.point, {
                    layers: ["organization-circles"]
                });
                if (orgFeatures && orgFeatures.length > 0) return;

                // Skip if click was on a waypoint
                const waypointFeatures = state.map.queryRenderedFeatures(event.point, {
                    layers: ["waypoint-circles"]
                });
                if (waypointFeatures && waypointFeatures.length > 0) return;

                const { lat, lng } = event.lngLat;
                updateClickedPointMarker(lng, lat);

                if (state.activeDotNetRef?.invokeMethodAsync) {
                    state.activeDotNetRef
                        .invokeMethodAsync("OnMapClickedAtPoint", lat, lng)
                        .catch(() => { });
                }
            });
            state.map.__mapClickHandlerAttached = true;
        }

        if (!state.map.__waypointDragHandlerAttached) {
            let isDragging = false;
            let dragFeatureIndex = null;

            const onMouseMove = (e) => {
                if (!isDragging || dragFeatureIndex === null) return;
                const coords = e.lngLat;

                if (state.waypointGeoJson && state.waypointGeoJson.features) {
                    const feature = state.waypointGeoJson.features[dragFeatureIndex];
                    if (feature && feature.geometry) {
                        feature.geometry.coordinates = [coords.lng, coords.lat];
                        syncWaypoints();
                    }
                }
            };

            const onMouseUp = (e) => {
                if (!isDragging) return;
                isDragging = false;

                state.map.off("mousemove", onMouseMove);
                state.map.getCanvas().style.cursor = "";

                const coords = e.lngLat;
                if (dragFeatureIndex !== null && state.activeDotNetRef?.invokeMethodAsync) {
                    state.activeDotNetRef
                        .invokeMethodAsync("OnWaypointDragEnd", dragFeatureIndex, coords.lat, coords.lng)
                        .catch(() => { });
                }
                dragFeatureIndex = null;
            };

            // Touch Drag Helpers for Mobile
            const onTouchMove = (e) => {
                if (!isDragging || dragFeatureIndex === null) return;
                const coords = e.lngLat;

                if (state.waypointGeoJson && state.waypointGeoJson.features) {
                    const feature = state.waypointGeoJson.features[dragFeatureIndex];
                    if (feature && feature.geometry) {
                        feature.geometry.coordinates = [coords.lng, coords.lat];
                        syncWaypoints();
                    }
                }
            };

            const onTouchEnd = (e) => {
                if (!isDragging) return;
                isDragging = false;

                state.map.off("touchmove", onTouchMove);
                if (state.map.dragPan) state.map.dragPan.enable();

                const coords = e.lngLat;
                if (dragFeatureIndex !== null && state.activeDotNetRef?.invokeMethodAsync) {
                    state.activeDotNetRef
                        .invokeMethodAsync("OnWaypointDragEnd", dragFeatureIndex, coords.lat, coords.lng)
                        .catch(() => { });
                }
                dragFeatureIndex = null;
            };

            state.map.on("mouseenter", "waypoint-circles", () => {
                state.map.getCanvas().style.cursor = "move";
            });

            state.map.on("mouseleave", "waypoint-circles", () => {
                if (!isDragging) {
                    state.map.getCanvas().style.cursor = "";
                }
            });

            state.map.on("mousedown", "waypoint-circles", (e) => {
                e.preventDefault();

                const features = state.map.queryRenderedFeatures(e.point, {
                    layers: ["waypoint-circles"]
                });

                if (features.length > 0) {
                    isDragging = true;
                    const idStr = features[0].properties?.id || "";
                    dragFeatureIndex = parseInt(idStr.replace("waypoint-", ""), 10);

                    state.map.getCanvas().style.cursor = "grabbing";

                    state.map.on("mousemove", onMouseMove);
                    state.map.once("mouseup", onMouseUp);
                }
            });

            state.map.on("touchstart", "waypoint-circles", (e) => {
                if (e.points && e.points.length !== 1) return;

                e.preventDefault();
                if (state.map.dragPan) state.map.dragPan.disable();

                const features = state.map.queryRenderedFeatures(e.point, {
                    layers: ["waypoint-circles"]
                });

                if (features.length > 0) {
                    isDragging = true;
                    const idStr = features[0].properties?.id || "";
                    dragFeatureIndex = parseInt(idStr.replace("waypoint-", ""), 10);

                    state.map.on("touchmove", onTouchMove);
                    state.map.once("touchend", onTouchEnd);
                }
            });

            state.map.__waypointDragHandlerAttached = true;
        }

        if (!state.map.__mapMoveHandlerAttached) {
            state.map.on("dragstart", () => {
                if (state.activeDotNetRef?.invokeMethodAsync) {
                    state.activeDotNetRef
                        .invokeMethodAsync("OnMapUserInteraction")
                        .catch(() => { });
                }
            });
            state.map.__mapMoveHandlerAttached = true;
        }

        syncAllSources();
        applyOverlayVisibility();
    } finally {
        state.isEnsuringLayers = false;
    }
}

export function setOrganizations(geoJson) {
    state.setOrganizationsCallCount += 1;

    if (state.map && !state.mapReady && typeof state.map.isStyleLoaded === "function" && state.map.isStyleLoaded()) {
        state.mapReady = true;
        ensureSourcesAndLayers();
    }

    state.organizationGeoJson = geoJson;
    state.lastSetOrganizationsCount = geoJson?.features?.length || 0;
    syncOrganizations();
}

export function setUserLocation(geoJson) {
    state.userLocationGeoJson = geoJson;
    syncUser();
}

export function setSelectedPlace(geoJson) {
    state.selectedPlaceGeoJson = geoJson;
    syncSelectedPlace();
}


export function setWaypoints(geoJson) {
    state.waypointGeoJson = geoJson;
    syncWaypoints();
}

export function drawRoute(geoJson, color = "#1d4ed8") {
    state.routeGeoJson = geoJson;
    state.routeColor = String(color || "#1d4ed8");
    syncRoute();
}


export function showDeliveryZone(geoJson) {
    state.zoneSetCallCount += 1;
    state.lastZoneInputCount = geoJson?.features?.[0]?.geometry?.coordinates?.[0]?.length || 0;
    state.zoneGeoJson = geoJson;
    syncZone();
}


export async function centerOnLocation(longitude, latitude, zoom = 16) {
    await ensureMap(state.activeStyleUrl);
    if (!state.map) return;
    state.map.easeTo({ center: [Number(longitude), Number(latitude)], zoom: Number(zoom), duration: 250 });
}

export async function centerOnLocationWithHeading(longitude, latitude, headingDegrees, zoom = 16) {
    await ensureMap(state.activeStyleUrl);
    if (!state.map) return;

    const hasHeading = Number.isFinite(headingDegrees) && headingDegrees >= 0;
    state.map.easeTo({
        center: [Number(longitude), Number(latitude)],
        zoom: Number(zoom),
        bearing: hasHeading ? -Number(headingDegrees) : state.map.getBearing(),
        duration: 250
    });
}

export async function adjustZoom(delta) {
    await ensureMap(state.activeStyleUrl);
    if (!state.map) return;

    const nextZoom = Math.min(20, Math.max(2, (state.map.getZoom() ?? 12) + Number(delta || 0)));
    state.map.easeTo({ zoom: nextZoom, duration: 220 });
}

export async function setOverlayVisibility(showOrgs, showRt, showZn, showTrans, showTraf) {
    await ensureMap(state.activeStyleUrl);
    if (!state.map) return;

    state.showOrganizationsLayer = showOrgs !== false;
    state.showRouteLayer = showRt !== false;
    state.showZoneLayer = showZn !== false;
    state.showTransportLayer = showTrans === true;
    state.showTrafficLayer = showTraf === true;

    ensureSourcesAndLayers();
    applyOverlayVisibility();
}

export async function fitToBounds(minLng, minLat, maxLng, maxLat, padding = 60, maxZoom = 16) {
    await ensureMap(state.activeStyleUrl);
    if (!state.map) return;

    const basePadding = Number(padding) || 60;
    const panelElement = document.querySelector('.map-controls-panel');
    const panelWidth = panelElement instanceof HTMLElement ? panelElement.offsetWidth : 0;
    const leftPadding = panelWidth > 0 ? panelWidth + 24 : basePadding;

    state.map.fitBounds(
        [
            [Number(minLng), Number(minLat)],
            [Number(maxLng), Number(maxLat)]
        ],
        {
            padding: { top: basePadding, right: basePadding, bottom: basePadding, left: leftPadding },
            duration: 280,
            maxZoom: Number(maxZoom) || 16
        }
    );
}

// ── Clicked point marker & popup ────────────────────────
function updateClickedPointMarker(lng, lat) {
    const geoJson = {
        type: "FeatureCollection",
        features: [{
            type: "Feature",
            geometry: { type: "Point", coordinates: [lng, lat] },
            properties: {}
        }]
    };
    state.selectedPlaceGeoJson = geoJson;
    syncSelectedPlace();
}

function showClickedPointPopup(lng, lat) {
    if (!state.map || !window.maplibregl?.Popup) return;

    if (state._clickedPointPopup) {
        state._clickedPointPopup.remove();
        state._clickedPointPopup = null;
    }

    const coordText = `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
    const html = `<div class="map-click-popup" id="map-click-popup">
        <div class="click-popup-coords">${coordText}</div>
        <div class="click-popup-address" id="click-popup-address">Определение адреса…</div>
        <div class="click-popup-actions">
            <button class="click-popup-btn" data-action="route-here">Маршрут сюда</button>
            <button class="click-popup-btn click-popup-btn-secondary" data-action="add-waypoint">Добавить точку</button>
        </div>
    </div>`;

    state._clickedPointPopup = new window.maplibregl.Popup({
        closeButton: true, closeOnClick: true, maxWidth: "260px"
    })
        .setLngLat([lng, lat])
        .setHTML(html)
        .addTo(state.map);

    const popupEl = state._clickedPointPopup.getElement();
    popupEl?.addEventListener("click", (e) => {
        const action = e.target?.getAttribute?.("data-action");
        if (!action || !state.activeDotNetRef?.invokeMethodAsync) return;
        if (action === "route-here") {
            state.activeDotNetRef.invokeMethodAsync("OnMapClickRouteHere", lat, lng).catch(() => { });
            state._clickedPointPopup?.remove();
        } else if (action === "add-waypoint") {
            state.activeDotNetRef.invokeMethodAsync("OnMapClickAddWaypoint", lat, lng).catch(() => { });
            state._clickedPointPopup?.remove();
        }
    });
}

export function updateClickedPointAddress(address) {
    const el = document.getElementById("click-popup-address");
    if (el) el.textContent = address || "";
}

export function clearClickedPoint() {
    if (state._clickedPointPopup) {
        state._clickedPointPopup.remove();
        state._clickedPointPopup = null;
    }
    state.selectedPlaceGeoJson = { type: "FeatureCollection", features: [] };
    syncSelectedPlace();
}
