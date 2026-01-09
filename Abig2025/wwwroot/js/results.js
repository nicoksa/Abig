// results2.js - Funcionalidad completa de filtros

document.addEventListener('DOMContentLoaded', function () {
    // Estado de filtros
    const filtrosEstado = {
        tipo: [],
        operacion: [],
        precio: [],
        ubicacion: [],
        ambientes: [],
        dormitorios: [],
        banos: [],
        superficieTotal: [],
        superficieCubierta: [],
        antiguedad: [],
        caracteristicas: []
    };

    // Inicialización
    initAcordeon();
    initFiltros();
    initPrecioMoneda();
    initUbicacionBusqueda();
    initRangos();
    cargarFiltrosDesdeURL();
    actualizarContadorFiltros();

    // ========== FUNCIONALIDAD ACORDEÓN ==========
    function initAcordeon() {
        const acordeones = document.querySelectorAll('.acordeon-titulo');

        acordeones.forEach(titulo => {
            titulo.addEventListener('click', function () {
                this.classList.toggle('active');
                const contenido = this.nextElementSibling;
                const isOpen = contenido.style.display === 'block';

                // Cerrar todos los demás acordeones
                if (!isOpen) {
                    acordeones.forEach(otherTitulo => {
                        if (otherTitulo !== this) {
                            otherTitulo.classList.remove('active');
                            otherTitulo.nextElementSibling.style.display = 'none';
                        }
                    });
                }

                // Toggle current
                contenido.style.display = isOpen ? 'none' : 'block';

                // Animar
                if (!isOpen) {
                    contenido.style.animation = 'slideDown 0.3s ease';
                }
            });

            // Abrir el primer acordeón por defecto
            if (titulo.parentElement.dataset.tipo === 'tipo') {
                titulo.click();
            }
        });
    }

    // ========== FUNCIONALIDAD FILTROS ==========
    function initFiltros() {
        // Event listeners para todos los checkboxes
        document.querySelectorAll('.acordeon-contenido input[type="checkbox"]').forEach(checkbox => {
            checkbox.addEventListener('change', function () {
                const grupo = this.closest('.grupo-filtro');
                const tipo = grupo.dataset.tipo;
                const valor = this.value;
                const multiples = grupo.dataset.multiples === 'true';

                if (this.checked) {
                    if (multiples) {
                        if (!filtrosEstado[tipo].includes(valor)) {
                            filtrosEstado[tipo].push(valor);
                        }
                    } else {
                        // Para filtros single-select (solo uno activo)
                        document.querySelectorAll(`[data-tipo="${tipo}"] input[type="checkbox"]`).forEach(cb => {
                            cb.checked = false;
                        });
                        this.checked = true;
                        filtrosEstado[tipo] = [valor];
                    }
                } else {
                    if (multiples) {
                        filtrosEstado[tipo] = filtrosEstado[tipo].filter(v => v !== valor);
                    } else {
                        filtrosEstado[tipo] = [];
                    }
                }

                actualizarFiltrosAplicados();
                actualizarContadorFiltros();
                aplicarFiltros();
            });
        });
    }

    // ========== FILTROS POR PRECIO Y MONEDA ==========
    function initPrecioMoneda() {
        const selectorMoneda = document.getElementById('selector-moneda');
        const precioOpciones = document.querySelectorAll('.precio-opcion');

        selectorMoneda?.addEventListener('change', function () {
            const moneda = this.value;

            precioOpciones.forEach(opcion => {
                const span = opcion.querySelector('span');
                if (span) {
                    span.textContent = moneda === 'USD'
                        ? opcion.dataset.usd
                        : opcion.dataset.ars;

                    const input = opcion.querySelector('input');
                    if (input) {
                        input.value = span.textContent;
                    }
                }
            });
        });

        // Botón aplicar precio rango
        document.getElementById('btn-aplicar-precio')?.addEventListener('click', function () {
            const min = document.getElementById('precio-min').value;
            const max = document.getElementById('precio-max').value;
            const moneda = document.getElementById('selector-moneda').value;

            if (min || max) {
                const valor = `Rango: ${moneda === 'USD' ? 'USD' : '$'}${min || '0'} - ${moneda === 'USD' ? 'USD' : '$'}${max || '∞'}`;

                // Limpiar otras opciones de precio
                document.querySelectorAll('[data-tipo="precio"] input[type="checkbox"]').forEach(cb => {
                    cb.checked = false;
                });

                filtrosEstado.precio = [valor];
                actualizarFiltrosAplicados();
                aplicarFiltros();
            }
        });
    }

    // ========== BÚSQUEDA DE UBICACIÓN ==========
    function initUbicacionBusqueda() {
        const inputUbicacion = document.getElementById('input-ubicacion');
        const sugerenciasDiv = document.getElementById('sugerencias-ubicacion');

        if (!inputUbicacion || !sugerenciasDiv) return;

        // Datos de ejemplo (deberías cargarlos desde tu backend)
        const ubicaciones = [
            { id: 1, nombre: "Buenos Aires", tipo: "Ciudad" },
            { id: 2, nombre: "Córdoba", tipo: "Ciudad" },
            { id: 3, nombre: "Palermo", tipo: "Barrio" },
            { id: 4, nombre: "Recoleta", tipo: "Barrio" },
            { id: 5, nombre: "Belgrano", tipo: "Barrio" },
            { id: 6, nombre: "Av. Corrientes 1234", tipo: "Dirección" }
        ];

        inputUbicacion.addEventListener('input', function () {
            const query = this.value.toLowerCase().trim();
            sugerenciasDiv.innerHTML = '';

            if (query.length < 2) {
                sugerenciasDiv.style.display = 'none';
                return;
            }

            const resultados = ubicaciones.filter(ubicacion =>
                ubicacion.nombre.toLowerCase().includes(query)
            );

            if (resultados.length > 0) {
                resultados.forEach(ubicacion => {
                    const div = document.createElement('div');
                    div.className = 'sugerencia-item';
                    div.innerHTML = `
                        <strong>${ubicacion.nombre}</strong>
                        <span class="text-muted" style="float: right; font-size: 0.8rem;">${ubicacion.tipo}</span>
                    `;
                    div.addEventListener('click', function () {
                        inputUbicacion.value = ubicacion.nombre;
                        filtrosEstado.ubicacion = [`${ubicacion.tipo}: ${ubicacion.nombre}`];
                        sugerenciasDiv.style.display = 'none';
                        actualizarFiltrosAplicados();
                        aplicarFiltros();
                    });
                    sugerenciasDiv.appendChild(div);
                });
                sugerenciasDiv.style.display = 'block';
            } else {
                sugerenciasDiv.style.display = 'none';
            }
        });

        // Cerrar sugerencias al hacer clic fuera
        document.addEventListener('click', function (e) {
            if (!inputUbicacion.contains(e.target) && !sugerenciasDiv.contains(e.target)) {
                sugerenciasDiv.style.display = 'none';
            }
        });
    }

    // ========== FILTROS POR RANGO ==========
    function initRangos() {
        // Superficie total
        document.getElementById('btn-aplicar-superficie-total')?.addEventListener('click', function () {
            const min = document.getElementById('superficie-total-min').value;
            const max = document.getElementById('superficie-total-max').value;

            if (min || max) {
                const valor = `Superficie Total: ${min || '0'} - ${max || '∞'} m²`;

                // Limpiar otras opciones
                document.querySelectorAll('[data-tipo="superficie-total"] input[type="checkbox"]').forEach(cb => {
                    cb.checked = false;
                });

                filtrosEstado.superficieTotal = [valor];
                actualizarFiltrosAplicados();
                aplicarFiltros();
            }
        });

        // Superficie cubierta
        document.getElementById('btn-aplicar-superficie-cubierta')?.addEventListener('click', function () {
            const min = document.getElementById('superficie-cubierta-min').value;
            const max = document.getElementById('superficie-cubierta-max').value;

            if (min || max) {
                const valor = `Superficie Cubierta: ${min || '0'} - ${max || '∞'} m²`;

                // Limpiar otras opciones
                document.querySelectorAll('[data-tipo="superficie-cubierta"] input[type="checkbox"]').forEach(cb => {
                    cb.checked = false;
                });

                filtrosEstado.superficieCubierta = [valor];
                actualizarFiltrosAplicados();
                aplicarFiltros();
            }
        });
    }

    // ========== FILTROS APLICADOS UI ==========
    function actualizarFiltrosAplicados() {
        const contenedor = document.getElementById('filtros-aplicados');
        if (!contenedor) return;

        contenedor.innerHTML = '';

        let totalFiltros = 0;

        // Agregar cada filtro aplicado
        for (const [tipo, valores] of Object.entries(filtrosEstado)) {
            if (valores.length > 0) {
                valores.forEach(valor => {
                    totalFiltros++;
                    const etiqueta = document.createElement('div');
                    etiqueta.className = 'etiqueta-filtro';
                    etiqueta.innerHTML = `
                        <span class="nombre">${valor}</span>
                        <span class="quitar" data-tipo="${tipo}" data-valor="${valor}">×</span>
                    `;
                    contenedor.appendChild(etiqueta);
                });
            }
        }

        // Botón limpiar todos si hay filtros
        if (totalFiltros > 0) {
            const botonLimpiar = document.createElement('button');
            botonLimpiar.className = 'btn btn-limpiar';
            botonLimpiar.textContent = 'Limpiar todos';
            botonLimpiar.addEventListener('click', limpiarTodosFiltros);
            contenedor.appendChild(botonLimpiar);
        } else {
            contenedor.innerHTML = '<p class="text-muted" style="margin: 0;">No hay filtros aplicados</p>';
        }

        // Agregar eventos a botones quitar
        document.querySelectorAll('.etiqueta-filtro .quitar').forEach(boton => {
            boton.addEventListener('click', function () {
                const tipo = this.dataset.tipo;
                const valor = this.dataset.valor;

                // Remover del estado
                filtrosEstado[tipo] = filtrosEstado[tipo].filter(v => v !== valor);

                // Desmarcar checkbox correspondiente
                document.querySelectorAll(`[data-tipo="${tipo}"] input[type="checkbox"]`).forEach(cb => {
                    if (cb.value === valor) {
                        cb.checked = false;
                    }
                });

                actualizarFiltrosAplicados();
                actualizarContadorFiltros();
                aplicarFiltros();
            });
        });
    }

    // ========== CONTADOR DE FILTROS ==========
    function actualizarContadorFiltros() {
        let total = 0;
        for (const valores of Object.values(filtrosEstado)) {
            total += valores.length;
        }

        // Actualizar título con contador
        const titulo = document.querySelector('.acordeon-titulo[data-tipo="filtros"]');
        if (titulo) {
            let contador = titulo.querySelector('.contador-filtros');
            if (!contador) {
                contador = document.createElement('span');
                contador.className = 'contador-filtros';
                titulo.appendChild(contador);
            }
            contador.textContent = total;
            contador.style.display = total > 0 ? 'inline-flex' : 'none';
        }
    }

    // ========== LIMPIAR FILTROS ==========
    function limpiarTodosFiltros() {
        // Resetear estado
        for (const key in filtrosEstado) {
            filtrosEstado[key] = [];
        }

        // Desmarcar todos los checkboxes
        document.querySelectorAll('.acordeon-contenido input[type="checkbox"]').forEach(cb => {
            cb.checked = false;
        });

        // Limpiar inputs de rango
        document.querySelectorAll('.precio-rango input[type="number"]').forEach(input => {
            input.value = '';
        });

        actualizarFiltrosAplicados();
        actualizarContadorFiltros();
        aplicarFiltros();

        // Redirigir a página sin filtros
        window.location.href = window.location.pathname;
    }

    // ========== CARGAR FILTROS DESDE URL ==========
    function cargarFiltrosDesdeURL() {
        const params = new URLSearchParams(window.location.search);

        if (params.get('Tipo')) {
            const tipo = params.get('Tipo');
            const checkbox = document.querySelector(`[data-tipo="tipo"] input[value="${tipo}"]`);
            if (checkbox) {
                checkbox.checked = true;
                checkbox.dispatchEvent(new Event('change'));
            }
        }

        if (params.get('Operacion')) {
            const operacion = params.get('Operacion');
            const checkbox = document.querySelector(`[data-tipo="operacion"] input[value="${operacion}"]`);
            if (checkbox) {
                checkbox.checked = true;
                checkbox.dispatchEvent(new Event('change'));
            }
        }

        if (params.get('Dormitorios')) {
            const dormitorios = params.get('Dormitorios');
            const texto = dormitorios === '5' ? '5+ dormitorios' : `${dormitorios} dormitorios`;
            const checkbox = document.querySelector(`[data-tipo="dormitorios"] input[value="${texto}"]`);
            if (checkbox) {
                checkbox.checked = true;
                checkbox.dispatchEvent(new Event('change'));
            }
        }
    }

    // ========== APLICAR FILTROS (AJAX) ==========
    function aplicarFiltros() {
        // Construir URL con filtros
        const params = new URLSearchParams();

        // Solo agregamos filtros si tienen valores
        if (filtrosEstado.operacion.length > 0) {
            params.append('Operacion', filtrosEstado.operacion[0]);
        }

        if (filtrosEstado.tipo.length > 0) {
            params.append('Tipo', filtrosEstado.tipo[0]);
        }

        if (filtrosEstado.dormitorios.length > 0) {
            const texto = filtrosEstado.dormitorios[0];
            const numMatch = texto.match(/\d+/);
            if (numMatch) {
                params.append('Dormitorios', numMatch[0]);
            }
        }

        // TODO: Implementar AJAX para Provincia y Ciudad cuando se seleccionen

        const url = `${window.location.pathname}?${params.toString()}`;

        // Usar AJAX para evitar recargar toda la página
        fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => response.text())
            .then(html => {
                // Extraer solo la parte de resultados del HTML
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const nuevosResultados = doc.querySelector('.resultados');

                if (nuevosResultados) {
                    document.querySelector('.resultados').innerHTML = nuevosResultados.innerHTML;

                    // Actualizar URL sin recargar la página
                    window.history.pushState({}, '', url);
                }
            })
            .catch(error => {
                console.error('Error al aplicar filtros:', error);
                // Fallback: recargar la página normalmente
                window.location.href = url;
            });
    }

    // ========== EXPORTAR FUNCIONES PARA DEBUG ==========
    window.filtrosApp = {
        estado: filtrosEstado,
        aplicarFiltros: aplicarFiltros,
        limpiarTodos: limpiarTodosFiltros,
        actualizarUI: actualizarFiltrosAplicados
    };
});