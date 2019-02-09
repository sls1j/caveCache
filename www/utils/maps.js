function setupMapLayers(map) {
    let geo = L.tileLayer('https://webmaps.geology.utah.gov/arcgis/rest/services/GeolMap/30x60_Quads/MapServer/tile/{z}/{y}/{x}?blankTile=false');
    let geo_topo = L.tileLayer(
        'https://api.mapbox.com/v4/mapbox.outdoors/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoiYnJpYW4tZGlja2V5IiwiYSI6ImNqcjZ6YWhoZzAxank0YXMzNm44YmJsYzUifQ.jBgCFt06nwAWC6jFmDrqVg',
        { "opacity": 0.6 });

    let geo_group = L.layerGroup([geo, geo_topo]);
    let baseMaps = {
        "Topo": L.tileLayer('https://api.mapbox.com/v4/mapbox.outdoors/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoiYnJpYW4tZGlja2V5IiwiYSI6ImNqcjZ6YWhoZzAxank0YXMzNm44YmJsYzUifQ.jBgCFt06nwAWC6jFmDrqVg', {}).addTo(map),
        "Sat": L.tileLayer('https://api.mapbox.com/v4/mapbox.satellite/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoiYnJpYW4tZGlja2V5IiwiYSI6ImNqcjZ6YWhoZzAxank0YXMzNm44YmJsYzUifQ.jBgCFt06nwAWC6jFmDrqVg', {}),
        "Geo": geo_group
    };

    L.control.layers(baseMaps, {}).addTo(map);
}