function Privacidad() {
  return (
    <div className="container">
      <h1>Privacidad</h1>

      <p>
        Esta aplicación es una <strong>demostración con fines de portfolio</strong>. No
        tiene uso comercial ni presta un servicio real.
      </p>

      <h2 className="h5 mt-4">Qué datos se guardan</h2>
      <p>
        Si te registrás, se almacenan el correo electrónico y los datos de perfil que
        cargues (nombre, apellido, fecha de nacimiento, localidad, código postal, teléfono
        y avatar). La contraseña nunca se guarda en texto plano: se conserva su hash.
      </p>
      <p>
        También se registran las operaciones de administración en un historial de
        auditoría, junto con la red desde la que se realizaron.
      </p>

      <h2 className="h5 mt-4">Para qué se usan</h2>
      <p>
        Únicamente para el funcionamiento de la demostración: identificarte al iniciar
        sesión, mostrar tus favoritos y enviarte el correo de bienvenida o el de
        recuperación de contraseña. No se comparten con terceros ni se usan con fines
        publicitarios.
      </p>

      <h2 className="h5 mt-4">Advertencias</h2>
      <p>
        Los correos que envía la aplicación son reales. La base de datos puede reiniciarse
        sin aviso, con lo que se perderían las cuentas creadas.{' '}
        <strong>
          No cargues información sensible ni reutilices una contraseña que uses en otros
          sitios.
        </strong>
      </p>

      <h2 className="h5 mt-4">Baja</h2>
      <p>
        Para eliminar una cuenta, escribí a{' '}
        <a href="mailto:patriciosostaric923@gmail.com">patriciosostaric923@gmail.com</a>.
      </p>
    </div>
  )
}

export default Privacidad