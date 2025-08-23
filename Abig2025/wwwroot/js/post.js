// Validación del formulario
(function () {
    'use strict'

    var forms = document.querySelectorAll('.needs-validation')

    Array.prototype.slice.call(forms)
        .forEach(function (form) {
            form.addEventListener('submit', function (event) {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                }

                form.classList.add('was-validated')
            }, false)
        })
})()

// Dinámica entre tipo y subtipo de propiedad
document.getElementById('TipoPropiedad').addEventListener('change', function () {
    const subtipoSelect = document.getElementById('SubtipoPropiedad');
    subtipoSelect.innerHTML = '<option value="" selected disabled>Seleccioná el subtipo de propiedad</option>';

    if (this.value === 'Casa') {
        addOptions(subtipoSelect, ['Casa ', 'Casa quinta', 'Casa de campo', 'Casa de playa']);
    } else if (this.value === 'Departamento') {
        addOptions(subtipoSelect, ['Departamento tipo dúplex', 'Departamento penthouse', 'Monoambiente']);
    } else if (this.value === 'PH') {
        addOptions(subtipoSelect, ['PH tradicional', 'PH moderno']);
    } else if (this.value === 'Terreno') {
        addOptions(subtipoSelect, ['Chacra', 'Estancia', 'Campo']);
    }
});

function addOptions(selectElement, options) {
    options.forEach(option => {
        const opt = document.createElement('option');
        opt.value = option;
        opt.textContent = option;
        selectElement.appendChild(opt);
    });
}


// Función para actualizar la barra de progreso
function updateProgress(currentStep) {
    const steps = document.querySelectorAll('.step');
    const progressLine = document.getElementById('progressLine');
    const stepWidth = 100 / (steps.length - 1);

    // Reset all steps
    steps.forEach(step => {
        step.classList.remove('active', 'completed');
    });

    // Update steps status
    for (let i = 0; i < steps.length; i++) {
        if (i < currentStep - 1) {
            steps[i].classList.add('completed');
        } else if (i === currentStep - 1) {
            steps[i].classList.add('active');
        }
    }

    // Update progress line
    const progressPercentage = (currentStep - 1) * stepWidth;
    progressLine.style.width = `${progressPercentage}%`;

    // Change color based on progress
    if (currentStep === steps.length) {
        progressLine.style.backgroundColor = '#198754'; // Verde cuando está completo
    } else {
        progressLine.style.backgroundColor = '#0d6efd'; // Azul por defecto
    }
}

// Ejemplo de uso: updateProgress(2) para avanzar al paso 2
