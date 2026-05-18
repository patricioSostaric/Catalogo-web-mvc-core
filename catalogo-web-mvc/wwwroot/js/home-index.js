document.addEventListener('DOMContentLoaded', function () {
    var input = document.getElementById('searchString');
    var timer;
    input.addEventListener('input', function () {
        clearTimeout(timer);
        timer = setTimeout(function () { input.form.submit(); }, 500);
    });
});
