using System.Net;

namespace catalogo_web_mvc.Services.Email
{
    public static class EmailTemplates
    {
        public static (string Subject, string Body) BuildWelcomeEmail(string userEmail)
        {
            var safeEmail = WebUtility.HtmlEncode(userEmail);

            var subject = "¡Bienvenido/a a Store Sostaric!";
            var body = $@"
                <h2>¡Bienvenido/a, {safeEmail}!</h2>
                <p>Tu cuenta fue creada correctamente. Ya podés iniciar sesión y explorar el catálogo.</p>
                <p>Si vos no solicitaste esta cuenta, podés ignorar este correo.</p>";

            return (subject, body);
        }

        public static (string Subject, string Body) BuildPasswordResetEmail(string resetLink)
        {
            var safeLink = WebUtility.HtmlEncode(resetLink);

            var subject = "Restablecé tu contraseña";
            var body = $@"
                <h2>Restablecimiento de contraseña</h2>
                <p>Recibimos una solicitud para restablecer tu contraseña. Si fuiste vos, hacé clic en el siguiente enlace:</p>
                <p><a href=""{safeLink}"">Restablecer contraseña</a></p>
                <p>Este enlace vence en un tiempo limitado. Si no solicitaste esto, ignorá este correo: tu contraseña actual seguirá funcionando.</p>";

            return (subject, body);
        }
    }
}
