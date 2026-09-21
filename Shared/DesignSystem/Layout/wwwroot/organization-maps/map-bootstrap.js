const state = {
    map: null
};

export function registerMap(map) {
    state.map = map ?? null;
    if (state.map) {
        console.info("Organization map instance registered");
    }
}

export function getMap() {
    return state.map;
}

export function clearMap() {
    state.map = null;
}

if (typeof window !== "undefined") {
    window.uiSharedOrganizationMapBootstrap = {
        registerMap,
        getMap,
        clearMap
    };
}

