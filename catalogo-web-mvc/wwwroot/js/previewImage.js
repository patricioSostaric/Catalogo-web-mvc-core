(function () {
    var input = document.getElementById('ImagenUrl');
    var preview = document.getElementById('previewImage');
    if (!input || !preview) return;

    // estado inicial
    if (input.value && input.value.trim()) {
        preview.src = input.value.trim();
        preview.style.display = 'block';
    }

    input.addEventListener('input', function (e) {
        var url = e.target.value.trim();
        if (url) {
            preview.src = url;
            preview.style.display = 'block';
        } else {
            preview.src = '';
            preview.style.display = 'none';
        }
    });
})();
