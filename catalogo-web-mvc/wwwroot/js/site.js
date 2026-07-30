// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Fallback de imagen rota para <img class="img-fallback">, sin usar onerror inline (requerido por la CSP).
var IMAGEN_FALLBACK_URL = 'https://www.mansor.com.uy/wp-content/uploads/2020/06/imagen-no-disponible2.jpg';

document.addEventListener('error', function (e) {
    if (e.target && e.target.matches && e.target.matches('img.img-fallback')) {
        e.target.src = IMAGEN_FALLBACK_URL;
    }
}, true);

// Fallback del avatar para <img data-avatar-fallback>. Va aparte del anterior porque
// la imagen de reemplazo es el avatar por defecto y no el cartel de "no disponible".
var AVATAR_FALLBACK_URL = '/img/avatar-default.svg';

document.addEventListener('error', function (e) {
    if (e.target && e.target.matches && e.target.matches('img[data-avatar-fallback]')) {
        // Se quita el atributo para que un fallo del propio fallback no cicle.
        e.target.removeAttribute('data-avatar-fallback');
        e.target.src = AVATAR_FALLBACK_URL;
    }
}, true);

// Vista previa del avatar recién elegido, antes de enviar el formulario.
// Se usa FileReader (data: URL) y no createObjectURL porque la CSP permite
// "img-src data:" pero no "blob:".
document.addEventListener('change', function (e) {
    if (!e.target.matches || !e.target.matches('input[type="file"][data-avatar-input]')) return;

    var archivo = e.target.files && e.target.files[0];
    var preview = document.querySelector('img[data-avatar-fallback]');
    if (!archivo || !preview) return;

    var lector = new FileReader();
    lector.onload = function (ev) { preview.src = ev.target.result; };
    lector.readAsDataURL(archivo);
});

// Vista previa de imagen en los formularios de Artículo (Create/Edit), sin script inline (requerido por la CSP).
document.getElementById('inputImagenUrl')?.addEventListener('input', function (e) {
    var img = document.getElementById('imgPreview');
    if (img) img.src = e.target.value || IMAGEN_FALLBACK_URL;
});

// Mostrar/ocultar contraseña (ícono de ojo con SVG inline) en los formularios de cuenta,
// sin script inline (requerido por la CSP) y sin depender de una fuente de íconos externa.
document.addEventListener('click', function (e) {
    var boton = e.target.closest('[data-toggle-password]');
    if (!boton) return;

    var input = document.getElementById(boton.getAttribute('data-toggle-password'));
    if (!input) return;

    var mostrando = input.type === 'text';
    input.type = mostrando ? 'password' : 'text';

    var iconoOjo = boton.querySelector('.icono-ojo');
    var iconoOjoTachado = boton.querySelector('.icono-ojo-tachado');
    if (iconoOjo) iconoOjo.classList.toggle('d-none', !mostrando);
    if (iconoOjoTachado) iconoOjoTachado.classList.toggle('d-none', mostrando);

    boton.setAttribute('aria-label', mostrando ? 'Mostrar contraseña' : 'Ocultar contraseña');
});
