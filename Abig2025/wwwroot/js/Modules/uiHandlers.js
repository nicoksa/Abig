// modules/uiHandlers.js - Versión simplificada
import {
    toggleCategoriaVisibilidad,
    desmarcarCheckboxesPorTipo,
    limpiarInputsPorId,
    validarRango
} from './utils.js';

import {
    PRECIO_RANGOS,
    SUPERFICIE_TOTAL_RANGOS,
    SUPERFICIE_CUBIERTA_RANGOS,

} from './constants.js';

class UIHandlers {
    constructor(estadoManager) {
        this.estado = estadoManager;
        this.currentRequest = null;
    }

    init() {
        this._setupCheckboxListeners();
        this._setupPrecioListeners();
        this._setupUbicacionListener();
        this._setupRangoListeners();
    }

    // ========== CHECKBOXES SIMPLIFICADOS ==========
    _setupCheckboxListeners() {
        document.querySelectorAll('.acordeon-contenido input[type="checkbox"]').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => this._manejarCheckbox(e.target));
        });
    }

    _manejarCheckbox(checkbox) {
        const grupo = checkbox.closest('.grupo-filtro');
        const tipo = grupo.dataset.tipo;
        const valor = checkbox.value;
        const multiples = grupo.dataset.multiples === 'true';

        // Manejo especial para superficie
        if (tipo === 'superficie-total') {
            this._manejarSuperficieCheckbox(tipo, valor, checkbox.checked, SUPERFICIE_TOTAL_RANGOS);
            return;
        }

        if (tipo === 'superficie-cubierta') {
            this._manejarSuperficieCheckbox(tipo, valor, checkbox.checked, SUPERFICIE_CUBIERTA_RANGOS);
            return;
        }

        // Manejo normal
        if (checkbox.checked) {
            if (multiples || tipo === 'features' || tipo === 'features-campo') {
                this.estado.toggleCaracteristica(valor);
            } else {
                this._manejarSingleSelect(tipo, valor, checkbox);
            }
        } else {
            // Si se desmarca un checkbox
            if (multiples || tipo === 'features' || tipo === 'features-campo') {
                this.estado.toggleCaracteristica(valor);
            } else {
                this.estado.removerFiltro(tipo, valor);
            }
        }
    }

    _manejarSuperficieCheckbox(tipo, valor, checked, rangos) {
        if (checked) {
            desmarcarCheckboxesPorTipo(tipo);

            const inputIds = tipo === 'superficie-total'
                ? ['superficie-total-min', 'superficie-total-max']
                : ['superficie-cubierta-min', 'superficie-cubierta-max'];
            limpiarInputsPorId(inputIds);

            const rango = rangos[valor];
            if (rango) {
                if (tipo === 'superficie-total') {
                    this.estado.actualizarSuperficieTotal(rango.min, rango.max);
                } else {
                    this.estado.actualizarSuperficieCubierta(rango.min, rango.max);
                }
                toggleCategoriaVisibilidad(tipo, true);
            }
        } else {
            if (tipo === 'superficie-total') {
                this.estado.actualizarSuperficieTotal(null, null);
            } else {
                this.estado.actualizarSuperficieCubierta(null, null);
            }
        }
    }

    _manejarSingleSelect(tipo, valor, checkbox) {
        // Solo desmarcar visualmente, el estado se maneja en toggleFiltroLista
        document.querySelectorAll(`[data-tipo="${tipo}"] input[type="checkbox"]`).forEach(cb => {
            if (cb !== checkbox) cb.checked = false;
        });

        this.estado.toggleFiltroLista(tipo, valor, false);
    }

    // ========== PRECIO SIMPLIFICADO ==========
    _setupPrecioListeners() {
        const selectorMoneda = document.getElementById('selector-moneda');
        const precioOpciones = document.querySelectorAll('.precio-opcion');
        const btnAplicarPrecio = document.getElementById('btn-aplicar-precio');

        if (selectorMoneda) {
            selectorMoneda.addEventListener('change', (e) => {
                this.estado.estado.precio.moneda = e.target.value;
                this._actualizarTextosPrecio(e.target.value);
            });
        }

        precioOpciones.forEach(opcion => {
            const checkbox = opcion.querySelector('input[type="checkbox"]');
            if (checkbox) {
                checkbox.addEventListener('change', () => this._manejarPrecioCheckbox(checkbox));
            }
        });

        if (btnAplicarPrecio) {
            btnAplicarPrecio.addEventListener('click', () => this._aplicarPrecioRango());
        }
    }

    _actualizarTextosPrecio(moneda) {
        document.querySelectorAll('.precio-opcion').forEach(opcion => {
            const span = opcion.querySelector('span');
            const input = opcion.querySelector('input');

            if (span && input) {
                const texto = moneda === 'USD' ? opcion.dataset.usd : opcion.dataset.ars;
                span.textContent = texto;
                input.value = texto;
            }
        });
    }

    _manejarPrecioCheckbox(checkbox) {
        if (checkbox.checked) {
            // Desmarcar otros
            document.querySelectorAll('.precio-opcion input[type="checkbox"]').forEach(cb => {
                if (cb !== checkbox) cb.checked = false;
            });

            limpiarInputsPorId(['precio-min', 'precio-max']);

            const texto = checkbox.value;
            const moneda = document.getElementById('selector-moneda')?.value || 'ARS';
            const rangosMoneda = PRECIO_RANGOS[moneda];
            const rango = rangosMoneda[texto];

            if (rango) {
                this.estado.actualizarPrecio(rango.min, rango.max, moneda);
                toggleCategoriaVisibilidad('precio', true);
            }
        } else {
            this.estado.actualizarPrecio(null, null);
        }
    }

    _aplicarPrecioRango() {
        const minInput = document.getElementById('precio-min');
        const maxInput = document.getElementById('precio-max');
        const selectorMoneda = document.getElementById('selector-moneda');

        const min = minInput.value ? parseFloat(minInput.value) : null;
        const max = maxInput.value ? parseFloat(maxInput.value) : null;
        const moneda = selectorMoneda?.value || 'ARS';

        const rangoValido = validarRango(min, max);

        document.querySelectorAll('.precio-opcion input[type="checkbox"]').forEach(cb => {
            cb.checked = false;
        });

        this.estado.actualizarPrecio(rangoValido.min, rangoValido.max, moneda);

        if (rangoValido.min !== null || rangoValido.max !== null) {
            toggleCategoriaVisibilidad('precio', true);
        }
    }


    // ========== UBICACIÓN CON API REAL ==========
    _setupUbicacionListener() {
        const inputUbicacion = document.getElementById('input-ubicacion');
        const sugerenciasDiv = document.getElementById('sugerencias-ubicacion');

        if (!inputUbicacion || !sugerenciasDiv) return;

        let timeout;

        inputUbicacion.addEventListener('input', (e) => {
            clearTimeout(timeout);

            // Cancelar request anterior si existe
            if (this.currentRequest) {
                console.log('🛑 Cancelando request anterior');
                this.currentRequest.abort();
                this.currentRequest = null;
            }

            timeout = setTimeout(() => {
                this._buscarUbicacionesEnAPI(e.target.value, sugerenciasDiv);
            }, 300);
        });

        // Ocultar al hacer clic fuera
        document.addEventListener('click', (e) => {
            if (!inputUbicacion.contains(e.target) && !sugerenciasDiv.contains(e.target)) {
                sugerenciasDiv.style.display = 'none';
            }
        });

        // Ocultar con Escape
        inputUbicacion.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                sugerenciasDiv.style.display = 'none';
            }
        });

        // Limpiar ubicación al borrar texto
        inputUbicacion.addEventListener('input', (e) => {
            if (e.target.value === '') {
                this._limpiarUbicacion();
            }
        });
    }

    _buscarUbicacionesEnAPI(query, sugerenciasDiv) {
        sugerenciasDiv.innerHTML = '';

        if (query.length < 2) {
            sugerenciasDiv.style.display = 'none';
            return;
        }

        // Mostrar indicador de carga
        sugerenciasDiv.innerHTML = `
            <div class="sugerencia-item">
                <div class="text-center py-2">
                    <div class="spinner-border spinner-border-sm text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <small class="ms-2">Buscando ubicaciones...</small>
                </div>
            </div>
        `;
        sugerenciasDiv.style.display = 'block';

        // Crear AbortController para cancelar requests anteriores
        const controller = new AbortController();
        this.currentRequest = controller; // Usa this.currentRequest

        console.log('🔍 Buscando ubicaciones con query:', query);

        fetch(`/api/location/search?query=${encodeURIComponent(query)}`, {
            signal: controller.signal
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(locations => {
                this.currentRequest = null; // Limpiar referencia
                console.log('📍 Ubicaciones encontradas:', locations);
                this._mostrarResultadosUbicacion(locations, sugerenciasDiv);
            })
            .catch(error => {
                if (error.name === 'AbortError') {
                    console.log('⏹️ Búsqueda cancelada (nueva búsqueda iniciada)');
                    return;
                }

                console.error('❌ Error al buscar ubicaciones:', error);

                // Mostrar mensaje de error
                sugerenciasDiv.innerHTML = `
                <div class="sugerencia-item">
                    <div class="text-center py-2">
                        <small class="text-danger">
                            Error: ${error.message || 'No se pudo conectar al servidor'}
                        </small>
                    </div>
                </div>
            `;

                this.currentRequest = null; // Limpiar referencia incluso en error
            });
    }

    _mostrarResultadosUbicacion(locations, sugerenciasDiv) {
        sugerenciasDiv.innerHTML = '';

        if (!locations || locations.length === 0) {
            sugerenciasDiv.innerHTML = `
            <div class="sugerencia-item">
                <div class="text-center py-2">
                    <small class="text-muted">No se encontraron ubicaciones</small>
                </div>
            </div>
        `;
            return;
        }

        locations.forEach(location => {
            const div = document.createElement('div');
            div.className = 'sugerencia-item';

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

            div.addEventListener('click', () => {
                this._seleccionarUbicacion(location);
                sugerenciasDiv.style.display = 'none';
            });

            sugerenciasDiv.appendChild(div);
        });
    }

    _seleccionarUbicacion(location) {
        const input = document.getElementById('input-ubicacion');
        if (!input) return;

        // Actualizar el input
        input.value = location.displayText;

        // Guardar metadatos en data attributes
        input.dataset.ubicacionId = location.id;
        input.dataset.ubicacionTipo = location.tipoId;
        input.dataset.provinciaId = location.provinciaId || '';
        input.dataset.ciudadId = location.ciudadId || '';
        input.dataset.provinciaNombre = location.provinciaNombre || '';
        input.dataset.ciudadNombre = location.ciudadNombre || '';

        // Actualizar el estado según el tipo de ubicación
        switch (location.tipoId) {
            case 'provincia':
                this.estado.actualizarUbicacion(location.id, null, null, location.displayText);
                break;
            case 'ciudad':
                this.estado.actualizarUbicacion(location.provinciaId, location.id, null, location.displayText);
                break;
            case 'barrio':
                this.estado.actualizarUbicacion(location.provinciaId, location.ciudadId, location.id, location.displayText);
                break;
        }

        // Mostrar la categoría como activa
        toggleCategoriaVisibilidad('ubicacion', true);

        // Mostrar notificación (opcional)
        this._mostrarNotificacionUbicacion(location);
    }

    _mostrarNotificacionUbicacion(location) {
        // Puedes agregar una notificación toast aquí si quieres
        console.log(`📍 Ubicación seleccionada: ${location.displayText} (${location.tipo})`);
    }

    _limpiarUbicacion() {
        const input = document.getElementById('input-ubicacion');
        if (!input) return;

        // Limpiar input y datos
        input.value = '';
        delete input.dataset.ubicacionId;
        delete input.dataset.ubicacionTipo;
        delete input.dataset.provinciaId;
        delete input.dataset.ciudadId;
        delete input.dataset.provinciaNombre;
        delete input.dataset.ciudadNombre;

        // Actualizar estado
        this.estado.actualizarUbicacion(null, null, null);
        toggleCategoriaVisibilidad('ubicacion', false);
    }

    // ========== RANGOS SIMPLIFICADOS ==========
    _setupRangoListeners() {
        const btnSuperficieTotal = document.getElementById('btn-aplicar-superficie-total');
        const btnSuperficieCubierta = document.getElementById('btn-aplicar-superficie-cubierta');

        if (btnSuperficieTotal) {
            btnSuperficieTotal.addEventListener('click', () => this._aplicarSuperficieTotal());
        }

        if (btnSuperficieCubierta) {
            btnSuperficieCubierta.addEventListener('click', () => this._aplicarSuperficieCubierta());
        }
    }

    _aplicarSuperficieTotal() {
        const minInput = document.getElementById('superficie-total-min');
        const maxInput = document.getElementById('superficie-total-max');

        const min = minInput.value ? parseFloat(minInput.value) : null;
        const max = maxInput.value ? parseFloat(maxInput.value) : null;

        desmarcarCheckboxesPorTipo('superficie-total');

        const rangoValido = validarRango(min, max);
        this.estado.actualizarSuperficieTotal(rangoValido.min, rangoValido.max);

        if (rangoValido.min !== null || rangoValido.max !== null) {
            toggleCategoriaVisibilidad('superficie-total', true);
        }
    }

    _aplicarSuperficieCubierta() {
        const minInput = document.getElementById('superficie-cubierta-min');
        const maxInput = document.getElementById('superficie-cubierta-max');

        const min = minInput.value ? parseFloat(minInput.value) : null;
        const max = maxInput.value ? parseFloat(maxInput.value) : null;

        desmarcarCheckboxesPorTipo('superficie-cubierta');

        const rangoValido = validarRango(min, max);
        this.estado.actualizarSuperficieCubierta(rangoValido.min, rangoValido.max);

        if (rangoValido.min !== null || rangoValido.max !== null) {
            toggleCategoriaVisibilidad('superficie-cubierta', true);
        }
    }

    // ========== LIMPIAR UI ==========
    limpiarUI() {
        // Esto se maneja ahora desde CheckboxSync
    }
}

export { UIHandlers };