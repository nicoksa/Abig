document.addEventListener('DOMContentLoaded', function () {
    const fileInput = document.getElementById('Imagenes');
    const previewContainer = document.getElementById('preview-container');
    const emptyMessage = document.getElementById('empty-message');
    const form = document.querySelector('form');
    const imageOrderInput = document.getElementById('ImageOrder');

    let allClientFiles = [];
    let draggedElement = null;

    // Inicializar drag and drop para imágenes existentes
    initializeDragAndDrop();

    // Actualizar contador inicial
    updateImageCounter();

    // Guardar orden inicial
    updateImageOrderInput();

    fileInput.addEventListener('change', function (event) {
        if (this.files.length > 0) {
            Array.from(this.files).forEach(file => {
                const exists = allClientFiles.some(f =>
                    f.name === file.name && f.size === file.size
                );

                if (!exists && (getTotalImageCount() < 20)) {
                    allClientFiles.push(file);
                }
            });

            updateFileInputWithAllFiles();
            updatePreview();
        }
    });

    function updateFileInputWithAllFiles() {
        const dataTransfer = new DataTransfer();
        allClientFiles.forEach(file => {
            dataTransfer.items.add(file);
        });
        fileInput.files = dataTransfer.files;
    }

    function updatePreview() {
        if (emptyMessage && (allClientFiles.length > 0 ||
            document.querySelectorAll('.image-item[data-is-server="true"]').length > 0)) {
            emptyMessage.style.display = 'none';
        }

        const existingClientImages = document.querySelectorAll('.image-item[data-is-server="false"]');
        existingClientImages.forEach(el => el.remove());

        allClientFiles.forEach((file, index) => {
            const reader = new FileReader();

            reader.onload = function (e) {
                const serverImagesCount = document.querySelectorAll('.image-item[data-is-server="true"]').length;
                const totalIndex = serverImagesCount + index;

                const col = document.createElement('div');
                col.className = 'col-6 col-md-4 col-lg-3 image-item';
                col.dataset.imageId = 'client_' + index;
                col.dataset.isServer = 'false';
                col.dataset.fileIndex = index;
                col.draggable = true;

                const card = document.createElement('div');
                card.className = 'card h-100 border-0 shadow-sm image-preview-card';

                const imgContainer = document.createElement('div');
                imgContainer.className = 'ratio ratio-1x1 position-relative';

                if (totalIndex === 0) {
                    const mainBadge = document.createElement('span');
                    mainBadge.className = 'main-image-badge';
                    mainBadge.innerHTML = '<i class="bi bi-star-fill"></i> Principal';
                    imgContainer.appendChild(mainBadge);
                }

                const numberBadge = document.createElement('span');
                numberBadge.className = 'image-number-badge';
                numberBadge.textContent = totalIndex + 1;
                imgContainer.appendChild(numberBadge);

                const img = document.createElement('img');
                img.src = e.target.result;
                img.className = 'card-img-top object-fit-cover';
                img.alt = file.name;
                img.style.objectFit = 'cover';

                const cardFooter = document.createElement('div');
                cardFooter.className = 'card-footer bg-white border-0 py-2';
                cardFooter.innerHTML = `
                    <div class="d-flex justify-content-between align-items-center">
                        <small class="text-truncate" style="max-width: 100px" title="${file.name}">
                            ${file.name}
                        </small>
                        <button type="button" class="btn btn-sm btn-outline-danger remove-client-image"
                                data-index="${index}" title="Eliminar">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                `;

                imgContainer.appendChild(img);
                card.appendChild(imgContainer);
                card.appendChild(cardFooter);
                col.appendChild(card);
                previewContainer.appendChild(col);

                // Inicializar drag and drop para la nueva imagen
                setupDragAndDropForElement(col);

                updateImageCounter();
                updateNumberBadges();
                updateImageOrderInput();
            };

            reader.readAsDataURL(file);
        });
    }

    function initializeDragAndDrop() {
        const imageItems = document.querySelectorAll('.image-item');
        imageItems.forEach(item => {
            setupDragAndDropForElement(item);
        });
    }

    function setupDragAndDropForElement(element) {
        element.addEventListener('dragstart', handleDragStart);
        element.addEventListener('dragend', handleDragEnd);
        element.addEventListener('dragover', handleDragOver);
        element.addEventListener('drop', handleDrop);
        element.addEventListener('dragenter', handleDragEnter);
        element.addEventListener('dragleave', handleDragLeave);
    }

    function handleDragStart(e) {
        draggedElement = this;
        this.querySelector('.image-preview-card').classList.add('dragging');
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/html', this.innerHTML);
    }

    function handleDragEnd(e) {
        this.querySelector('.image-preview-card').classList.remove('dragging');

        document.querySelectorAll('.image-item').forEach(item => {
            item.querySelector('.image-preview-card').classList.remove('drag-over');
        });

        updateNumberBadges();
        updateImageOrderInput();
    }

    function handleDragOver(e) {
        if (e.preventDefault) {
            e.preventDefault();
        }
        e.dataTransfer.dropEffect = 'move';
        return false;
    }

    function handleDragEnter(e) {
        if (this !== draggedElement) {
            this.querySelector('.image-preview-card').classList.add('drag-over');
        }
    }

    function handleDragLeave(e) {
        this.querySelector('.image-preview-card').classList.remove('drag-over');
    }

    function handleDrop(e) {
        if (e.stopPropagation) {
            e.stopPropagation();
        }

        if (draggedElement !== this) {
            const allItems = Array.from(previewContainer.querySelectorAll('.image-item'));
            const draggedIndex = allItems.indexOf(draggedElement);
            const targetIndex = allItems.indexOf(this);

            if (draggedIndex < targetIndex) {
                this.parentNode.insertBefore(draggedElement, this.nextSibling);
            } else {
                this.parentNode.insertBefore(draggedElement, this);
            }

            // Si se movieron imágenes del cliente, actualizar el array
            const isClientImage = draggedElement.dataset.isServer === 'false';
            if (isClientImage) {
                reorganizeClientFiles();
            }
        }

        return false;
    }

    function reorganizeClientFiles() {
        const newClientFiles = [];
        const clientItems = document.querySelectorAll('.image-item[data-is-server="false"]');

        clientItems.forEach(item => {
            const index = parseInt(item.dataset.fileIndex);
            if (!isNaN(index) && allClientFiles[index]) {
                newClientFiles.push(allClientFiles[index]);
            }
        });

        // Actualizar índices
        clientItems.forEach((item, newIndex) => {
            item.dataset.fileIndex = newIndex;
            const deleteButton = item.querySelector('.remove-client-image');
            if (deleteButton) {
                deleteButton.dataset.index = newIndex;
            }
        });

        allClientFiles = newClientFiles;
        updateFileInputWithAllFiles();
    }

    function updateNumberBadges() {
        const allItems = previewContainer.querySelectorAll('.image-item');
        allItems.forEach((item, index) => {
            const badge = item.querySelector('.image-number-badge');
            if (badge) {
                badge.textContent = index + 1;
            }

            const mainBadge = item.querySelector('.main-image-badge');
            if (mainBadge) {
                mainBadge.remove();
            }

            if (index === 0) {
                const imgContainer = item.querySelector('.ratio');
                const newMainBadge = document.createElement('span');
                newMainBadge.className = 'main-image-badge';
                newMainBadge.innerHTML = '<i class="bi bi-star-fill"></i> Principal';
                imgContainer.insertBefore(newMainBadge, imgContainer.firstChild);
            }
        });
    }

    function updateImageOrderInput() {
        const allItems = previewContainer.querySelectorAll('.image-item');
        const order = Array.from(allItems).map((item, index) => {
            if (item.dataset.isServer === 'true') {
                // Imagen del servidor: usar su fileName
                return item.dataset.imageId;
            } else {
                // Imagen del cliente: marcar su posición con un marcador especial
                const fileIndex = parseInt(item.dataset.fileIndex);
                if (!isNaN(fileIndex)) {
                    return `CLIENT_NEW_${fileIndex}`;
                }
            }
            return null;
        }).filter(Boolean);

        imageOrderInput.value = order.join(',');
        console.log('Orden completo actualizado:', imageOrderInput.value);
        console.log('Total items:', allItems.length);
    }

    previewContainer.addEventListener('click', function (e) {
        if (e.target.closest('.remove-client-image')) {
            const button = e.target.closest('.remove-client-image');
            const index = parseInt(button.dataset.index);

            allClientFiles.splice(index, 1);

            updatePreview();
            updateFileInputWithAllFiles();
            updateImageOrderInput();

            showToast('Imagen eliminada de la selección', 'info');
        }

        if (e.target.closest('.delete-server-image')) {
            const button = e.target.closest('.delete-server-image');
            const fileName = button.dataset.filename;
            deleteServerImage(fileName);
        }
    });

    function getTotalImageCount() {
        const serverImages = document.querySelectorAll('.image-item[data-is-server="true"]').length;
        const clientImages = allClientFiles.length;
        return serverImages + clientImages;
    }

    function updateImageCounter() {
        const counterElement = document.getElementById('image-counter');
        const totalImages = getTotalImageCount();

        if (counterElement) {
            counterElement.textContent = `${totalImages} imágenes`;
            counterElement.className = `badge ${totalImages >= 5 ? 'bg-success' : 'bg-warning'}`;
        }
    }

    // Drag and drop para el área de carga
    const fileUploadLabel = document.querySelector('.file-upload-label');

    if (fileUploadLabel) {
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            fileUploadLabel.addEventListener(eventName, preventDefaults, false);
        });

        ['dragenter', 'dragover'].forEach(eventName => {
            fileUploadLabel.addEventListener(eventName, highlight, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            fileUploadLabel.addEventListener(eventName, unhighlight, false);
        });

        fileUploadLabel.addEventListener('drop', handleFileDrop, false);
    }

    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    function highlight() {
        fileUploadLabel.style.borderColor = '#0d6efd';
        fileUploadLabel.style.backgroundColor = 'rgba(13, 110, 253, 0.1)';
    }

    function unhighlight() {
        fileUploadLabel.style.borderColor = '#dee2e6';
        fileUploadLabel.style.backgroundColor = '';
    }

    function handleFileDrop(e) {
        const dt = e.dataTransfer;
        const files = dt.files;

        if (files.length > 0) {
            const dataTransfer = new DataTransfer();
            Array.from(files).forEach(file => {
                dataTransfer.items.add(file);
            });
            fileInput.files = dataTransfer.files;

            const event = new Event('change', { bubbles: true });
            fileInput.dispatchEvent(event);
        }
    }

    form.addEventListener('submit', function (e) {
        updateFileInputWithAllFiles();
        updateImageOrderInput();

        const totalImages = getTotalImageCount();
        const submitter = e.submitter;
        const isBackButton = submitter &&
            (submitter.textContent.includes('Anterior') ||
                submitter.classList.contains('btn-outline-secondary'));

        if (!isBackButton) {
            if (totalImages < 1) {
                if (!confirm('¿Estás seguro de continuar sin subir imágenes? Las propiedades con fotos reciben más consultas.')) {
                    e.preventDefault();
                    return false;
                }
            }
        }

        if (totalImages > 20) {
            alert('El límite máximo es 20 imágenes. Por favor, elimina algunas imágenes antes de continuar.');
            e.preventDefault();
            return false;
        }

        console.log('Enviando con orden:', imageOrderInput.value);
        return true;
    });

    // Vista previa del video
    const videoUrlInput = document.getElementById('VideoUrl');
    const videoPreview = document.getElementById('video-preview');

    if (videoUrlInput && videoPreview) {
        const iframe = videoPreview.querySelector('iframe');
        updateVideoPreview(videoUrlInput.value);

        videoUrlInput.addEventListener('input', function () {
            updateVideoPreview(this.value);
        });

        function updateVideoPreview(videoUrl) {
            if (videoUrl && isValidVideoUrl(videoUrl)) {
                const embedUrl = getEmbedUrl(videoUrl);
                if (embedUrl && iframe) {
                    iframe.src = embedUrl;
                    videoPreview.classList.remove('d-none');
                }
            } else {
                videoPreview.classList.add('d-none');
            }
        }

        function isValidVideoUrl(url) {
            return url.includes('youtube.com') ||
                url.includes('youtu.be') ||
                url.includes('vimeo.com');
        }

        function getEmbedUrl(url) {
            if (url.includes('youtube.com') || url.includes('youtu.be')) {
                let videoId = '';

                if (url.includes('youtube.com/watch?v=')) {
                    videoId = url.split('v=')[1];
                } else if (url.includes('youtu.be/')) {
                    videoId = url.split('youtu.be/')[1];
                }

                if (videoId) {
                    const ampersandPosition = videoId.indexOf('&');
                    if (ampersandPosition !== -1) {
                        videoId = videoId.substring(0, ampersandPosition);
                    }
                    return `https://www.youtube.com/embed/${videoId}`;
                }
            } else if (url.includes('vimeo.com')) {
                const videoId = url.split('vimeo.com/')[1];
                if (videoId) {
                    const questionMarkPosition = videoId.indexOf('?');
                    const cleanVideoId = questionMarkPosition !== -1
                        ? videoId.substring(0, questionMarkPosition)
                        : videoId;
                    return `https://player.vimeo.com/video/${cleanVideoId}`;
                }
            }
            return null;
        }
    }

    window.allClientFiles = allClientFiles;
    window.updateImageCounter = updateImageCounter;
});

function deleteServerImage(fileName) {
    if (!confirm('¿Eliminar imagen?')) {
        return;
    }

    const draftIdInput = document.querySelector('input[name="DraftId"]');
    const draftId = draftIdInput ? draftIdInput.value : null;

    if (!draftId) {
        alert('Error: No se encontró el draft');
        return;
    }

    const deleteButton = document.querySelector(`[data-filename="${fileName}"]`);
    if (deleteButton) {
        deleteButton.innerHTML = '<i class="bi bi-hourglass-split"></i>';
        deleteButton.disabled = true;
    }

    const formData = new FormData();
    formData.append('fileName', fileName);
    formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);

    fetch(`?handler=DeleteImage&draftId=${draftId}`, {
        method: 'POST',
        body: formData,
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => {
            if (response.ok) {
                const imageElement = document.querySelector(`.image-item[data-image-id="${fileName}"]`);
                if (imageElement) {
                    imageElement.style.transition = 'opacity 0.3s';
                    imageElement.style.opacity = '0';

                    setTimeout(() => {
                        imageElement.remove();

                        const imageOrderInput = document.getElementById('ImageOrder');
                        const allItems = document.querySelectorAll('.image-item');
                        const order = Array.from(allItems).map(item => item.dataset.imageId).filter(Boolean);
                        imageOrderInput.value = order.join(',');

                        updateNumberBadges();
                        updateImageCounter();
                        showToast('Imagen eliminada correctamente', 'success');

                        const serverImages = document.querySelectorAll('.image-item[data-is-server="true"]').length;
                        const clientImages = window.allClientFiles ? window.allClientFiles.length : 0;

                        if (serverImages === 0 && clientImages === 0) {
                            const emptyMessage = document.getElementById('empty-message');
                            if (emptyMessage) {
                                emptyMessage.style.display = 'block';
                            }
                        }
                    }, 300);
                }
            } else {
                throw new Error('Error al eliminar la imagen');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('Error al eliminar la imagen', 'danger');
            if (deleteButton) {
                deleteButton.innerHTML = '<i class="bi bi-trash"></i>';
                deleteButton.disabled = false;
            }
        });
}

function updateNumberBadges() {
    const allItems = document.querySelectorAll('.image-item');
    allItems.forEach((item, index) => {
        const badge = item.querySelector('.image-number-badge');
        if (badge) {
            badge.textContent = index + 1;
        }

        const mainBadge = item.querySelector('.main-image-badge');
        if (mainBadge) {
            mainBadge.remove();
        }

        if (index === 0) {
            const imgContainer = item.querySelector('.ratio');
            const newMainBadge = document.createElement('span');
            newMainBadge.className = 'main-image-badge';
            newMainBadge.innerHTML = '<i class="bi bi-star-fill"></i> Principal';
            imgContainer.insertBefore(newMainBadge, imgContainer.firstChild);
        }
    });
}

function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toast-container');
    let container;

    if (!toastContainer) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999;';
        document.body.appendChild(container);
    } else {
        container = toastContainer;
    }

    const toast = document.createElement('div');
    toast.className = `alert alert-${type} alert-dismissible fade show`;
    toast.style.cssText = 'min-width: 250px; margin-bottom: 10px;';
    toast.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Cerrar"></button>
    `;

    container.appendChild(toast);

    setTimeout(() => {
        if (toast.parentNode) {
            toast.style.opacity = '0';
            toast.style.transition = 'opacity 0.3s';
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.remove();
                }
            }, 300);
        }
    }, 3000);
}