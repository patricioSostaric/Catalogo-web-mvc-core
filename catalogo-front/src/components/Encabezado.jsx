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
        </ul>
      </div>
    </nav>
  )
}

export default Encabezado