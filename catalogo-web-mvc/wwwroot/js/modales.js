// Abre automaticamente los modales marcados con data-modal-auto. Las vistas solo
// declaran el modal y el atributo; no necesitan script propio.
//
// Va en un archivo y no inline porque la Content-Security-Policy del sitio declara
// script-src 'self': un <script> embebido en la pagina seria bloqueado.
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-modal-auto]').forEach(function (elemento) {
        new bootstrap.Modal(elemento).show();
    });
});
