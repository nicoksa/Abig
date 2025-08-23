// Objeto global para los filtros aplicados
const filtrosAplicados = {};

document.addEventListener('DOMContentLoaded', () => {
    // Configuración inicial para los filtros de checkbox
    document.querySelectorAll('.grupo-filtro input[type="checkbox"]').forEach(input => {
        input.addEventListener('change', function () {
            const grupo = this.closest('.grupo-filtro');
            const tipo = grupo.dataset.tipo;
            const permiteMultiples = grupo.dataset.multiples === 'true';
            let valor;
            if (tipo === "precio") {
                // Tomamos el texto del <span> dentro del label padre del checkbox
                valor = this.parentElement.querySelector("span").textContent.trim();
            } else {
                const label = document.querySelector(`label[for="${this.id}"] span`);
                valor = label ? label.textContent.trim() : this.value;
            }

            if (this.checked) {
                if (permiteMultiples) {
                    // Para grupos con múltiples selecciones
                    if (!filtrosAplicados[tipo]) {
                        filtrosAplicados[tipo] = [];
                    }
                    filtrosAplicados[tipo].push(valor);
                } else {
                    // Para grupos con selección única
                    filtrosAplicados[tipo] = valor;
                    ocultarOtrasOpciones(tipo);
                }
            } else {
                if (permiteMultiples) {
                    // Eliminar solo este valor del array
                    if (filtrosAplicados[tipo]) {
                        const index = filtrosAplicados[tipo].indexOf(valor);
                        if (index > -1) {
                            filtrosAplicados[tipo].splice(index, 1);
                        }
                        // Eliminar el array si está vacío
                        if (filtrosAplicados[tipo].length === 0) {
                            delete filtrosAplicados[tipo];
                        }
                    }
                } else {
                    // Eliminar el filtro para selección única
                    delete filtrosAplicados[tipo];
                    mostrarOpciones(tipo);
                }
            }

            renderizarEtiquetas();
        });
    });

    // Configuración para el filtro de precio
    document.getElementById('btn-aplicar-precio').addEventListener('click', () => {
        const min = document.getElementById('precio-min').value;
        const max = document.getElementById('precio-max').value;

        if (min || max) {
            const moneda = document.getElementById('selector-moneda').value;
            const simbolo = moneda === 'USD' ? 'USD$' : '$';
            filtrosAplicados['precio'] = `Entre ${simbolo}${min || 0} y ${simbolo}${max || '∞'}`;
            ocultarOtrasOpciones('precio');
            renderizarEtiquetas();
        }
    });

    // Configuración para el selector de moneda
    const selectorMoneda = document.getElementById('selector-moneda');
    selectorMoneda.addEventListener('change', () => {
        const moneda = selectorMoneda.value;
        document.querySelectorAll('.precio-opcion').forEach(label => {
            const texto = moneda === 'USD'
                ? label.getAttribute('data-usd')
                : label.getAttribute('data-ars');
            label.querySelector('span').textContent = texto;
        });

        const min = document.getElementById('precio-min');
        const max = document.getElementById('precio-max');
        min.placeholder = moneda === 'USD' ? 'Mín USD' : 'Mín $';
        max.placeholder = moneda === 'USD' ? 'Máx USD' : 'Máx $';
    });

    // Configuración para el buscador de ubicación
    const inputUbicacion = document.getElementById('input-ubicacion');
    const sugerenciasUbicacion = document.getElementById('sugerencias-ubicacion');
    const grupoUbicacion = document.querySelector('.grupo-filtro[data-tipo="ubicacion"]');

    // Datos de ejemplo (reemplazar con API real si es necesario)
    const ubicacionesDisponibles = [
        "Saladillo", "Roque Perez", "25 de Mayo", "Las Flores", "Pehuajo",
        "Palermo", "Recoleta", "Belgrano", "Caballito", "San Telmo",
        "La Plata", "Mar del Plata", "Córdoba", "Rosario", "Mendoza"
    ];

    // Manejar entrada de búsqueda
    inputUbicacion.addEventListener('input', function () {
        const query = this.value.toLowerCase();
        sugerenciasUbicacion.innerHTML = '';

        if (query.length < 2) {
            sugerenciasUbicacion.style.display = 'none';
            return;
        }

        const resultados = ubicacionesDisponibles.filter(ubicacion =>
            ubicacion.toLowerCase().includes(query)
        );

        if (resultados.length > 0) {
            resultados.forEach(ubicacion => {
                const div = document.createElement('div');
                div.className = 'sugerencia-item';
                div.textContent = ubicacion;
                div.addEventListener('click', function () {
                    seleccionarUbicacion(ubicacion);
                });
                sugerenciasUbicacion.appendChild(div);
            });
            sugerenciasUbicacion.style.display = 'block';
        } else {
            sugerenciasUbicacion.style.display = 'none';
        }
    });

    // Cerrar sugerencias al hacer clic fuera
    document.addEventListener('click', function (e) {
        if (e.target !== inputUbicacion) {
            sugerenciasUbicacion.style.display = 'none';
        }
    });

    // Función para manejar la selección de ubicación
    function seleccionarUbicacion(ubicacion) {
        inputUbicacion.value = '';
        sugerenciasUbicacion.style.display = 'none';

        // Agregar al objeto de filtros aplicados
        filtrosAplicados['ubicacion'] = ubicacion;

        // Ocultar el grupo de ubicación
        ocultarOtrasOpciones('ubicacion');

        // Renderizar las etiquetas
        renderizarEtiquetas();
    }
});

// Función para renderizar todas las etiquetas de filtros aplicados
function renderizarEtiquetas() {
    const contenedor = document.getElementById('filtros-aplicados');
    contenedor.innerHTML = '';

    for (const [tipo, valor] of Object.entries(filtrosAplicados)) {
        if (Array.isArray(valor)) {
            // Para filtros con múltiples valores
            valor.forEach(item => {
                const div = document.createElement('div');
                div.className = 'etiqueta-filtro';
                div.innerHTML = `${item} <span class="quitar" data-tipo="${tipo}" data-valor="${item}">✖</span>`;
                contenedor.appendChild(div);
            });
        } else {
            // Para filtros con un solo valor
            const div = document.createElement('div');
            div.className = 'etiqueta-filtro';
            div.innerHTML = `${valor} <span class="quitar" data-tipo="${tipo}">✖</span>`;
            contenedor.appendChild(div);
        }
    }

    // Configurar eventos para los botones de quitar
 document.querySelectorAll('.quitar').forEach(btn => {
        btn.addEventListener('click', function() {
            const tipo = this.dataset.tipo;
            const valorEspecifico = this.dataset.valor;

            if (valorEspecifico) {
                // Para filtros con múltiples valores
                const index = filtrosAplicados[tipo].indexOf(valorEspecifico);
                if (index > -1) {
                    filtrosAplicados[tipo].splice(index, 1);
                }
                if (filtrosAplicados[tipo].length === 0) {
                    delete filtrosAplicados[tipo];
                }
                
                // Desmarcar el checkbox correspondiente
                const grupo = document.querySelector(`.grupo-filtro[data-tipo="${tipo}"]`);
                const checkbox = grupo.querySelector(`input[value="${valorEspecifico}"]`);
                if (checkbox) checkbox.checked = false;
            } else {
                // Para filtros con un solo valor
                delete filtrosAplicados[tipo];
                mostrarOpciones(tipo);
                deseleccionar(tipo);
            }

            renderizarEtiquetas();
        });
    });
}


// Función para ocultar las opciones de un filtro
function ocultarOtrasOpciones(tipo) {
    const grupo = document.querySelector(`.grupo-filtro[data-tipo="${tipo}"]`);
    if (grupo) {
        grupo.style.display = 'none';
    }
}

// Función para mostrar las opciones de un filtro
function mostrarOpciones(tipo) {
    const grupo = document.querySelector(`.grupo-filtro[data-tipo="${tipo}"]`);
    if (grupo) {
        grupo.style.display = 'block';
    }
}

// Función para deseleccionar los inputs de un filtro
function deseleccionar(tipo) {
    const grupo = document.querySelector(`.grupo-filtro[data-tipo="${tipo}"]`);
    if (grupo) {
        const inputs = grupo.querySelectorAll('input');
        inputs.forEach(input => {
            if (input.type === 'checkbox') {
                input.checked = false;
            } else if (input.type === 'number') {
                input.value = '';
            } else if (input.type === 'text') {
                input.value = '';
            }
        });
    }
}










// Configuración para el filtro de superficie total
document.getElementById('btn-aplicar-superficie-total').addEventListener('click', () => {
    const min = document.getElementById('superficie-total-min').value;
    const max = document.getElementById('superficie-total-max').value;

    if (min || max) {
        filtrosAplicados['superficie-total'] = `Superf. total: ${min || 0} - ${max || '∞'} m²`;
        ocultarOtrasOpciones('superficie-total');
        renderizarEtiquetas();
    }
});

// Configuración para el filtro de superficie cubierta
document.getElementById('btn-aplicar-superficie-cubierta').addEventListener('click', () => {
    const min = document.getElementById('superficie-cubierta-min').value;
    const max = document.getElementById('superficie-cubierta-max').value;

    if (min || max) {
        filtrosAplicados['superficie-cubierta'] = `Superf. cubierta: ${min || 0} - ${max || '∞'} m²`;
        ocultarOtrasOpciones('superficie-cubierta');
        renderizarEtiquetas();
    }
});

// Actualiza el evento change para los checkboxes de superficie
document.querySelectorAll('.grupo-filtro[data-tipo^="superficie"] input[type="checkbox"]').forEach(input => {
    input.addEventListener('change', () => {
        const tipo = input.closest('.grupo-filtro').dataset.tipo;
        const texto = input.value;

        if (input.checked) {
            filtrosAplicados[tipo] = texto;
            ocultarOtrasOpciones(tipo);
        } else {
            delete filtrosAplicados[tipo];
            mostrarOpciones(tipo);
        }

        renderizarEtiquetas();
    });
});
