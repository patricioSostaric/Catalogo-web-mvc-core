using System.Net;
using System.Net.Sockets;

namespace catalogo_web_mvc.Services.Audit
{
    /// <summary>
    /// Enmascara la parte de la direccion IP que identifica a un equipo concreto y
    /// conserva solo la que identifica a la red de origen.
    ///
    /// El registro de auditoria necesita distinguir si veinte intentos fallidos vienen
    /// de un mismo origen o de veinte personas distintas, pero no necesita saber quien
    /// es cada una. Guardar la red y descartar el resto cubre ese uso sin almacenar un
    /// dato personal: es el principio de minimizacion de datos del GDPR, y la misma
    /// tecnica que aplican las herramientas de analitica web.
    /// </summary>
    public static class IpAnonimizador
    {
        public static string? Anonimizar(IPAddress? direccion)
        {
            if (direccion == null) return null;

            // Detras de un proxy las IPv4 suelen llegar mapeadas como ::ffff:186.13.114.8.
            // Se las devuelve a su forma IPv4 para no enmascararlas como si fueran IPv6.
            if (direccion.IsIPv4MappedToIPv6)
                direccion = direccion.MapToIPv4();

            var bytes = direccion.GetAddressBytes();

            if (direccion.AddressFamily == AddressFamily.InterNetwork)
            {
                // IPv4: se conservan los primeros tres octetos (la red /24).
                bytes[3] = 0;
            }
            else
            {
                // IPv6: se conservan los primeros 48 bits, el bloque que los proveedores
                // asignan a un cliente. El resto identifica al equipo dentro de esa red.
                for (int i = 6; i < bytes.Length; i++)
                    bytes[i] = 0;
            }

            return new IPAddress(bytes).ToString();
        }
    }
}
