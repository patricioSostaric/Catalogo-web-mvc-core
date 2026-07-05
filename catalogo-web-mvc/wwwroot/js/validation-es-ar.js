// jQuery Validate usa por defecto el formato numérico en-US (punto decimal).
// El sitio corre con cultura es-AR (coma decimal, punto de miles), así que
// se redefinen los métodos numéricos para que entiendan ese formato antes
// de compararlo/validarlo.
(function ($) {
    if (!$ || !$.validator) {
        return;
    }

    function parseEsARNumber(value) {
        if (typeof value !== "string") {
            return value;
        }
        return parseFloat(value.replace(/\./g, "").replace(",", "."));
    }

    $.validator.methods.number = function (value, element) {
        return this.optional(element) || /^-?\d{1,3}(\.\d{3})*(,\d+)?$|^-?\d+(,\d+)?$/.test(value);
    };

    $.validator.methods.range = function (value, element, param) {
        var num = parseEsARNumber(value);
        return this.optional(element) || (num >= param[0] && num <= param[1]);
    };

    $.validator.methods.min = function (value, element, param) {
        var num = parseEsARNumber(value);
        return this.optional(element) || num >= param;
    };

    $.validator.methods.max = function (value, element, param) {
        var num = parseEsARNumber(value);
        return this.optional(element) || num <= param;
    };
})(jQuery);
