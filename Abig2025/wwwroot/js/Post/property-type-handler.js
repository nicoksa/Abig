// post/simple-property-handler.js
$(document).ready(function () {

    // Verificar al cargar la página
    updateFieldsForPropertyType();

    // Escuchar cambios en el tipo de propiedad
    $('#Data_PropertyType').change(function () {
        updateFieldsForPropertyType();
    });

    function updateFieldsForPropertyType() {
        const propertyType = $('#Data_PropertyType').val();

        // Resetear todo primero
        resetAllFields();

        // Aplicar reglas según tipo
        switch (propertyType) {
            case 'Campo':
                setupForCampo();
                break;
            case 'Terreno':
                setupForTerreno();
                break;
            case 'Cochera':
                setupForCochera();
                break;
            default:
                // Para otros tipos, todo habilitado (ya está reset)
                break;
        }
    }

    function resetAllFields() {
        // Habilitar todos los campos
        $('#Data_MainRooms, #Data_Bedrooms, #Data_Bathrooms, #Data_ParkingSpaces, ' +
            '#Data_CoveredArea, #Data_TotalArea, #Data_Age')
            .prop('disabled', false);

        $('#Data_IsUnderConstruction, #Data_IsNew')
            .prop('disabled', false);

        // Restaurar etiqueta original de Superficie Total
        // Buscar el label que está justo antes del input
        $('#Data_TotalArea').closest('.col-md-6')
            .find('label.form-label.fw-bold')
            .text('Superficie total (m²) ');


    }

    function setupForCampo() {
        // Deshabilitar TODO menos Superficie Total
        $('#Data_MainRooms, #Data_Bedrooms, #Data_Bathrooms, #Data_ParkingSpaces, ' +
            '#Data_CoveredArea, #Data_Age')
            .prop('disabled', true)
            .val('');

        $('#Data_IsUnderConstruction, #Data_IsNew')
            .prop('disabled', true)
            .prop('checked', false);

        // Cambiar label de Superficie Total a Hectáreas
        // Encontrar el label correcto
        $('#Data_TotalArea').closest('.col-md-6')
            .find('label.form-label.fw-bold')
            .text('Superficie total (hectáreas)');

    }

    function setupForTerreno() {
        // Deshabilitar TODO menos Superficie Total
        $('#Data_MainRooms, #Data_Bedrooms, #Data_Bathrooms, #Data_ParkingSpaces, ' +
            '#Data_CoveredArea, #Data_Age')
            .prop('disabled', true)
            .val('');

        $('#Data_IsUnderConstruction, #Data_IsNew')
            .prop('disabled', true)
            .prop('checked', false);

    }

    function setupForCochera() {
        // Deshabilitar solo: Ambientes, Dormitorios, Baños, Cocheras
        $('#Data_MainRooms, #Data_Bedrooms, #Data_Bathrooms, #Data_ParkingSpaces')
            .prop('disabled', true)
            .val('');

    }

    function addHint(message) {
        // Primero remover cualquier hint existente
        $('#property-type-hint').remove();

        const hintHtml = `
            <div id="property-type-hint" class="alert alert-info mt-2 mb-0 p-2 small">
                <i class="bi bi-info-circle me-1"></i>${message}
            </div>
        `;

        // Insertar después del título de la sección
        $('.card-header:has(h5:contains("Características principales"))')
            .after(hintHtml);
    }
});