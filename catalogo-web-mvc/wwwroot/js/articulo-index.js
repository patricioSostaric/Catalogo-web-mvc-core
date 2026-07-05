document.addEventListener('DOMContentLoaded', function () {
    const check = document.getElementById('filtroAvanzado');
    const campos = document.getElementById('camposAvanzados');
    const searchInput = document.getElementById('searchString');
    const campoSelect = document.querySelector('select[name="campo"]');
    const criterioSelect = document.querySelector('select[name="criterio"]');

    function toggleFiltros() {
        if (check.checked) {
            campos.style.display = 'flex';
            searchInput.readOnly = true; // ahora es readonly, no disabled
            searchInput.value = '';
            searchInput.placeholder = 'Deshabilitado por Filtro Avanzado';
        } else {
            campos.style.display = 'none';
            searchInput.readOnly = false; // se habilita de nuevo
            searchInput.placeholder = 'Ej: apple, samsung...';
        }
    }

    function actualizarCriterios() {
        criterioSelect.innerHTML = '';
        if (campoSelect.value === 'Precio' || campoSelect.value === 'Stock') {
            criterioSelect.innerHTML += '<option value="Igual a">Igual a</option>';
            criterioSelect.innerHTML += '<option value="Mayor a">Mayor a</option>';
            criterioSelect.innerHTML += '<option value="Menor a">Menor a</option>';
        } else if (campoSelect.value === 'Activo') {
            criterioSelect.innerHTML += '<option value="Igual a">Igual a</option>';
        } else {
            criterioSelect.innerHTML += '<option value="Contiene">Contiene</option>';
            criterioSelect.innerHTML += '<option value="Comienza con">Comienza con</option>';
            criterioSelect.innerHTML += '<option value="Termina con">Termina con</option>';
        }
    }

    check.addEventListener('change', toggleFiltros);
    campoSelect.addEventListener('change', function () {
        document.querySelector('input[name="filtro"]').value = '';
        actualizarCriterios();
    });

    toggleFiltros();
    actualizarCriterios();
});
