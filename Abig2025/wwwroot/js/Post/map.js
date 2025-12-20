// post/map.js
class MapManager {
    constructor() {
        this.map = null;
        this.marker = null;
        this.latInput = null;
        this.lngInput = null;
        this.initializeMap();
    }

    initializeMap() {
        this.latInput = document.getElementById("LatitudHidden");
        this.lngInput = document.getElementById("LongitudHidden");

        // Coordenadas iniciales
        let initialLat = -34.6037;
        let initialLng = -58.3816;

        // Usar valores guardados si existen
        if (this.latInput && this.latInput.value.trim() !== '') {
            const latValue = parseFloat(this.latInput.value);
            if (!isNaN(latValue)) initialLat = latValue;
        }

        if (this.lngInput && this.lngInput.value.trim() !== '') {
            const lngValue = parseFloat(this.lngInput.value);
            if (!isNaN(lngValue)) initialLng = lngValue;
        }

        // Crear mapa
        this.map = L.map('map').setView([initialLat, initialLng], 13);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(this.map);

        // Crear marcador
        this.marker = L.marker([initialLat, initialLng], { draggable: true }).addTo(this.map);

        // Configurar eventos
        this.setupMarkerEvents();
        this.setupAddressEvents();
        this.addSearchButton();
    }

    setupMarkerEvents() {
        this.marker.on('dragend', () => {
            const pos = this.marker.getLatLng();
            const lat = pos.lat.toFixed(6);
            const lng = pos.lng.toFixed(6);

            this.latInput.value = lat;
            this.lngInput.value = lng;

            console.log("Coordenadas actualizadas:", lat, lng);
        });
    }

    setupAddressEvents() {
        // Eventos para geocodificación automática
        $('#Data_Street, #Data_Number').on('input', () => {
            this.geocodeOnAddressComplete();
        });

        // Evento cuando cambia ciudad/provincia (usando el LocationManager)
        $('#Data_ProvinceId, #Data_CityId').change(() => {
            setTimeout(() => this.geocodeAddress(), 500);
        });
    }

    async geocodeAddress() {
        const street = $('#Data_Street').val();
        const number = $('#Data_Number').val();
        const citySelect = $('#Data_CityId');
        const cityName = citySelect.find('option:selected').text();
        const provinceSelect = $('#Data_ProvinceId');
        const provinceName = provinceSelect.find('option:selected').text();

        // Construir dirección
        let address = this.buildAddress(street, number, cityName, provinceName);

        if (!address) {
            console.log('Dirección insuficiente para geocodificación');
            return;
        }

        console.log('Geocodificando:', address);

        try {
            this.showLoading();
            const coords = await this.fetchCoordinates(address);

            if (coords) {
                this.updateMapPosition(coords.lat, coords.lng);
            } else {
                // Intentar con solo provincia
                if (provinceName && provinceName !== 'Selecciona provincia') {
                    await this.geocodeProvince(provinceName);
                }
            }
        } catch (error) {
            console.error('Error en geocodificación:', error);
        } finally {
            this.hideLoading();
        }
    }

    buildAddress(street, number, cityName, provinceName) {
        let address = '';

        if (street && street.trim() !== '') {
            address += street;
            if (number && number.trim() !== '') {
                address += ' ' + number;
            }
            address += ', ';
        }

        if (cityName && cityName !== 'Selecciona localidad') {
            address += cityName + ', ';
        }

        if (provinceName && provinceName !== 'Selecciona provincia') {
            address += provinceName + ', Argentina';
        }

        return address.trim();
    }

    async fetchCoordinates(address) {
        const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&countrycodes=ar&limit=1`;

        const response = await fetch(url);
        const data = await response.json();

        if (data && data.length > 0) {
            return {
                lat: parseFloat(data[0].lat),
                lng: parseFloat(data[0].lon)
            };
        }
        return null;
    }

    async geocodeProvince(provinceName) {
        const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(provinceName + ', Argentina')}&limit=1`;

        try {
            const response = await fetch(url);
            const data = await response.json();

            if (data && data.length > 0) {
                const lat = parseFloat(data[0].lat);
                const lng = parseFloat(data[0].lon);

                this.updateMapPosition(lat, lng, 8); // Zoom más amplio para provincia
            }
        } catch (error) {
            console.error('Error geocodificando provincia:', error);
        }
    }

    updateMapPosition(lat, lng, zoom = 13) {
        this.map.setView([lat, lng], zoom);
        this.marker.setLatLng([lat, lng]);

        this.latInput.value = lat.toFixed(6);
        this.lngInput.value = lng.toFixed(6);
    }

    geocodeOnAddressComplete() {
        const street = $('#Data_Street').val();
        const city = $('#Data_CityId').val();

        if ((street && street.trim() !== '') || (city && city !== '')) {
            clearTimeout(this.geocodeTimeout);
            this.geocodeTimeout = setTimeout(() => this.geocodeAddress(), 1000);
        }
    }

    addSearchButton() {
        if ($('#searchOnMapBtn').length === 0) {


            searchBtn.click(() => this.geocodeAddress());
            $('#Data_Street').closest('.col-md-6').append(searchBtn);
        }
    }

    showLoading() {
        if ($('.map-loading').length === 0) {
            $('<div class="map-loading">Buscando ubicación...</div>').appendTo('#map');
        }
    }

    hideLoading() {
        $('.map-loading').remove();
    }
}

// Inicializar cuando el DOM esté listo
$(document).ready(() => {
    if ($('#map').length > 0) {
        window.mapManager = new MapManager();
    }
});