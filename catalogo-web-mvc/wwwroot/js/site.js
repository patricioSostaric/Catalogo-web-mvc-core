// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Fallback de imagen rota para <img class="img-fallback">, sin usar onerror inline (requerido por la CSP).
document.addEventListener('error', function (e) {
    if (e.target && e.target.matches && e.target.matches('img.img-fallback')) {
        e.target.src = 'https://www.mansor.com.uy/wp-content/uploads/2020/06/imagen-no-disponible2.jpg';
    }
}, true);
