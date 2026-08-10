import { Link } from 'react-router-dom'

function Encabezado() {
  return (
    <nav className="navbar navbar-expand-sm navbar-light bg-white border-bottom mb-3">
      <div className="container-fluid">
        <Link className="navbar-brand" to="/">Store Sostaric</Link>
        <ul className="navbar-nav me-auto">
          <li className="nav-item">
            <Link className="nav-link" to="/">Inicio</Link>
          </li>
          <li className="nav-item">
            <Link className="nav-link" to="/privacidad">Privacidad</Link>
          </li>
          <li className="nav-item">
            {/* Sale del front en React hacia las vistas Razor: es otra aplicacion
                dentro del mismo dominio, asi que va un ancla y no un Link, que
                intentaria resolver la ruta con el router y no encontraria nada. */}
            <a className="nav-link" href="/" title="La versión servida por Razor">
              Volver al sitio clásico
            </a>
          </li>
        </ul>
      </div>
    </nav>
  )
}

export default Encabezado