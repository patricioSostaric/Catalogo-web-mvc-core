// Abre el modal de confirmacion cuando la vista lo incluyo tras un pedido exitoso.
// Va en un archivo propio y no inline porque la Content-Security-Policy del sitio
// declara script-src 'self': un <script> embebido en la pagina seria bloqueado.
document.addEventListener('DOMContentLoaded', function () {
    var elemento = document.getElementById('modalPagoExitoso');
    if (elemento) {
        new bootstrap.Modal(elemento).show();
    }
});
