// post/location.js
class LocationManager {
    constructor() {
        this.CABA_CITY_ID = 3954;
        this.currentCityId = 0;
        this.initializeEvents();
    }

    initializeEvents() {
        $(document).ready(() => {
            this.loadProvinces();
            this.setupEventHandlers();
        });
    }

    setupEventHandlers() {
        // Provincia
        $('#Data_ProvinceId').change(() => {
            this.handleProvinceChange();
        });

        // Ciudad
        $('#Data_CityId').change(() => {
            this.handleCityChange();
        });
    }

    handleProvinceChange() {
        const provinceSelect = $('#Data_ProvinceId');
        const provinceId = provinceSelect.val();
        const provinceName = provinceSelect.find('option:selected').text();

        $('#ProvinceName').val(provinceName);

        if (provinceId) {
            this.loadCities(provinceId);
            $('#Data_CityId').prop('disabled', false);
            this.disableNeighborhoodField();
        } else {
            $('#Data_CityId').prop('disabled', true).empty()
                .append('<option value="">Primero selecciona provincia</option>');
            this.disableNeighborhoodField();
        }
    }

    handleCityChange() {
        const citySelect = $('#Data_CityId');
        const cityId = citySelect.val();
        const cityName = citySelect.find('option:selected').text();

        this.currentCityId = parseInt(cityId || '0');
        $('#CityName').val(cityName);

        if (this.isCABA(cityId)) {
            this.loadNeighborhoods(cityId);
            $('#Data_NeighborhoodId').prop('disabled', false);
        } else {
            this.disableNeighborhoodField();
        }
    }

    isCABA(cityId) {
        return parseInt(cityId || '0') === this.CABA_CITY_ID;
    }

    disableNeighborhoodField() {
        $('#Data_NeighborhoodId')
            .prop('disabled', true)
            .empty()
            .append('<option value="">Barrio solo disponible para CABA</option>');
        $('#NeighborhoodName').val('');
    }

    loadProvinces() {
        const currentProvinceId = $('#Data_ProvinceId').data('current') || '';

        $.get('/api/location/provinces', (data) => {
            const select = $('#Data_ProvinceId');
            select.empty().append('<option value="">Selecciona provincia</option>');

            $.each(data, (index, province) => {
                const option = $('<option></option>')
                    .val(province.provinceId)
                    .text(province.name);

                if (province.provinceId == currentProvinceId) {
                    option.prop('selected', true);
                    $('#ProvinceName').val(province.name);
                }

                select.append(option);
            });

            if (currentProvinceId) {
                this.loadCities(currentProvinceId);
            }
        });
    }

    loadCities(provinceId) {
        const currentCityId = $('#Data_CityId').data('current') || '';

        $.get(`/api/location/provinces/${provinceId}/cities`, (data) => {
            const select = $('#Data_CityId');
            select.empty().append('<option value="">Selecciona localidad</option>');

            $.each(data, (index, city) => {
                const option = $('<option></option>')
                    .val(city.cityId)
                    .text(city.name);

                if (city.cityId == currentCityId) {
                    option.prop('selected', true);
                    $('#CityName').val(city.name);

                    if (this.isCABA(currentCityId)) {
                        this.loadNeighborhoods(currentCityId);
                    } else {
                        this.disableNeighborhoodField();
                    }
                }

                select.append(option);
            });

            select.prop('disabled', false);
        });
    }

    loadNeighborhoods(cityId) {
        const currentNeighborhoodId = $('#Data_NeighborhoodId').data('current') || '';

        $.get(`/api/location/cities/${cityId}/neighborhoods`, (data) => {
            const select = $('#Data_NeighborhoodId');
            select.empty().append('<option value="">Selecciona barrio</option>');

            $.each(data, (index, neighborhood) => {
                const option = $('<option></option>')
                    .val(neighborhood.neighborhoodId)
                    .text(neighborhood.name);

                if (neighborhood.neighborhoodId == currentNeighborhoodId) {
                    option.prop('selected', true);
                    $('#NeighborhoodName').val(neighborhood.name);
                }

                select.append(option);
            });

            select.prop('disabled', false);
        });
    }
}

// Inicializar cuando el DOM esté listo
$(document).ready(() => {
    window.locationManager = new LocationManager();

    // Verificación inicial si hay ciudad cargada
    const initialCityId = $('#Data_CityId').data('current');
    if (initialCityId && parseInt(initialCityId) !== 3954) {
        window.locationManager.disableNeighborhoodField();
    }
});