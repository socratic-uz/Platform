export const MAP_CONTAINER_ID = "organization-maplibre";
export const ORG_SOURCE_ID = "org-source";
export const USER_SOURCE_ID = "user-source";
export const WAYPOINT_SOURCE_ID = "waypoint-source";
export const ROUTE_SOURCE_ID = "route-source";
export const ZONE_SOURCE_ID = "zone-source";
export const OVERLAY_VECTOR_SOURCE_ID = "versatiles-overlay-source";
export const TRANSPORT_LAYER_ID = "transport-overlay-line";
export const TRAFFIC_LAYER_ID = "traffic-overlay-line";
export const SELECTED_PLACE_SOURCE_ID = "selected-place-source";

export const state = {
    map: null,
    mapReady: false,
    mapReadyPromise: null,
    activeStyleUrl: "",
    activeDotNetRef: null,
    liveLocationWatchId: null,
    organizationGeoJson: null,
    waypointGeoJson: null,
    routeGeoJson: null,
    routeColor: "#1d4ed8",
    zoneGeoJson: null,
    userLocationGeoJson: null,
    selectedPlaceGeoJson: null,
    organizationPopup: null,
    isEnsuringLayers: false,
    setOrganizationsCallCount: 0,
    lastSetOrganizationsCount: 0,
    zoneSetCallCount: 0,
    lastZoneInputCount: 0,
    showOrganizationsLayer: true,
    showRouteLayer: true,
    showZoneLayer: true,
    showTransportLayer: false,
    showTrafficLayer: false,
    mapTiltEnabled: false
};

