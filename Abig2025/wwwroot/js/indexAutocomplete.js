// indexAutocomplete.js
document.addEventListener('DOMContentLoaded', function() {
    const inputUbicacion = document.getElementById('input-ubicacion-index');
    const sugerenciasDiv = document.getElementById('sugerencias-ubicacion-index');
    const hiddenProvincia = document.getElementById('hidden-provincia-id');
    const hiddenCiudad = document.getElementById('hidden-ciudad-id');
    const hiddenBarrio = document.getElementById('hidden-barrio-id');
    
    let currentRequest = null;

    if (!inputUbicacion || !sugerenciasDiv) return;

    // Configurar autocompletado
    let timeout;
    inputUbicacion.addEventListener('input', function(e) {
        clearTimeout(timeout);
        
        if (currentRequest) {
            currentRequest.abort();
            currentRequest = null;
        }
        
        timeout = setTimeout(() => {
            buscarUbicaciones(e.target.value);
        }, 300);
    });

    // Ocultar sugerencias al hacer clic fuera
    document.addEventListener('click', function(e) {
        if (!inputUbicacion.contains(e.target) && !sugerenciasDiv.contains(e.target)) {
            sugerenciasDiv.style.display = 'none';
        }
    });

    // Manejar tecla Escape
    inputUbicacion.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            sugerenciasDiv.style.display = 'none';
        }
        
        // Tecla Enter para seleccionar la primera opción
        if (e.key === 'Enter') {
            e.preventDefault();
            const primeraSugerencia = sugerenciasDiv.querySelector('.sugerencia-item');
            if (primeraSugerencia && sugerenciasDiv.style.display !== 'none') {
                primeraSugerencia.click();
            }
        }
    });

    // Función para buscar ubicaciones
    function buscarUbicaciones(query) {
        sugerenciasDiv.innerHTML = '';
        
        if (query.length < 2) {
            sugerenciasDiv.style.display = 'none';
            return;
        }

        // Mostrar indicador de carga
        sugerenciasDiv.innerHTML = `
            <div class="sugerencia-item p-2">
                <div class="text-center">
                    <div class="spinner-border spinner-border-sm text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <small class="ms-2">Buscando ubicaciones...</small>
                </div>
            </div>
        `;
        sugerenciasDiv.style.display = 'block';

        // Crear AbortController
        const controller = new AbortController();
        currentRequest = controller;

        fetch(`/api/location/search?query=${encodeURIComponent(query)}`, {
            signal: controller.signal
        })
            .then(response => {
                if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);
                return response.json();
            })
            .then(locations => {
                currentRequest = null;
                mostrarResultadosUbicacion(locations);
            })
            .catch(error => {
                if (error.name === 'AbortError') return;
                
                console.error('Error al buscar ubicaciones:', error);
                sugerenciasDiv.innerHTML = `
                    <div class="sugerencia-item p-2">
                        <div class="text-center">
                            <small class="text-danger">Error: ${error.message || 'No se pudo conectar'}</small>
                        </div>
                    </div>
                `;
                currentRequest = null;
            });
    }

    // Función para mostrar resultados
    function mostrarResultadosUbicacion(locations) {
        sugerenciasDiv.innerHTML = '';

        if (!locations || locations.length === 0) {
            sugerenciasDiv.innerHTML = `
                <div class="sugerencia-item p-2">
                    <div class="text-center">
                        <small class="text-muted">No se encontraron ubicaciones</small>
                    </div>
                </div>
            `;
            return;
        }

        locations.forEach(location => {
            const div = document.createElement('div');
            div.className = 'sugerencia-item p-2 border-bottom';
            div.style.cursor = 'pointer';
            
            // Determinar color del badge según el tipo
            let badgeClass = 'bg-secondary';
            if (location.tipoId === 'provincia') badgeClass = 'bg-primary';
            if (location.tipoId === 'ciudad') badgeClass = 'bg-success';
            if (location.tipoId === 'barrio') badgeClass = 'bg-info text-dark';

            div.innerHTML = `
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <strong>${location.displayText}</strong>
                        <span class="badge ${badgeClass} ms-2" style="font-size: 0.7rem;">
                            ${location.tipo}
                        </span>
                    </div>
                    <small class="text-muted">
                        <i class="bi bi-geo-alt"></i>
                    </small>
                </div>
                ${location.ciudadNombre ? 
                    `<small class="text-muted d-block mt-1">
                        <i class="bi bi-building"></i> ${location.ciudadNombre}
                        ${location.provinciaNombre ? `, ${location.provinciaNombre}` : ''}
                    </small>` :
                    location.provinciaNombre ? 
                    `<small class="text-muted d-block mt-1">
                        <i class="bi bi-map"></i> ${location.provinciaNombre}
                    </small>` : ''
                }
            `;

            // Evento al hacer clic en una sugerencia
            div.addEventListener('click', function() {
                seleccionarUbicacion(location);
            });

            // Efecto hover
            div.addEventListener('mouseenter', function() {
                this.style.backgroundColor = '#f8f9fa';
            });
            div.addEventListener('mouseleave', function() {
                this.style.backgroundColor = '';
            });

            sugerenciasDiv.appendChild(div);
        });
    }

    // Función para seleccionar una ubicación
    function seleccionarUbicacion(location) {
        // Actualizar el input visible
        inputUbicacion.value = location.displayText;
        
        // Actualizar los datos en el input
        inputUbicacion.dataset.provinciaId = location.provinciaId || '';
        inputUbicacion.dataset.ciudadId = location.ciudadId || '';
        inputUbicacion.dataset.barrioId = location.tipoId === 'barrio' ? location.id : '';
        
        // Actualizar los hidden inputs para el formulario
        if (hiddenProvincia) hiddenProvincia.value = location.provinciaId || '';
        if (hiddenCiudad) hiddenCiudad.value = location.ciudadId || '';
        if (hiddenBarrio) hiddenBarrio.value = location.tipoId === 'barrio' ? location.id : '';
        
        // Ocultar sugerencias
        sugerenciasDiv.style.display = 'none';
        
        // Mostrar confirmación (opcional)
        console.log(`📍 Ubicación seleccionada: ${location.displayText}`);
    }

    // Limpiar ubicación si el usuario borra el texto
    inputUbicacion.addEventListener('input', function(e) {
        if (e.target.value === '') {
            limpiarUbicacion();
        }
    });

    function limpiarUbicacion() {
        inputUbicacion.value = '';
        inputUbicacion.dataset.provinciaId = '';
        inputUbicacion.dataset.ciudadId = '';
        inputUbicacion.dataset.barrioId = '';
        
        if (hiddenProvincia) hiddenProvincia.value = '';
        if (hiddenCiudad) hiddenCiudad.value = '';
        if (hiddenBarrio) hiddenBarrio.value = '';
    }
});