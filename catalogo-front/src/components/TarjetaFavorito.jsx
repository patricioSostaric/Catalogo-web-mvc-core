import { Link } from 'react-router-dom'

// No reusa TarjetaArticulo a proposito: esta muestra la descripcion, no muestra
// marca ni categoria, y tiene el boton de quitar. Son dos tarjetas parecidas pero
// distintas, igual que en el MVC.
//
// El onQuitar llega desde arriba en vez de hacer el fetch acá: la tarjeta no sabe
// que existe una API, solo avisa que se apreto el boton. Quien maneja la lista es
// quien tiene que actualizarla.
function TarjetaFavorito({ favorito, onQuitar }) {
  return (
    <div className="col">
      <div className="card h-100 border-0 shadow-sm">
        <img
          src={favorito.imagenUrl}
          className="card-img-top p-3"
          alt={favorito.nombre}
          style={{ height: '200px', objectFit: 'contain' }}
        />
        <div className="card-body d-flex flex-column">
          <h5 className="card-title text-truncate">{favorito.nombre}</h5>
          <p className="card-text text-muted small">{favorito.descripcion}</p>

          <div className="mt-auto">
            <div className="d-flex justify-content-between align-items-center mb-3">
              <span className="text-secondary small">Precio</span>
              <span className="fs-5 fw-bold text-dark">
                ${favorito.precio.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
              </span>
            </div>

            <div className="d-flex gap-2">
              <Link
                to={`/articulo/${favorito.articuloId}`}
                className="btn btn-outline-primary btn-sm fw-medium"
              >
                Detalle
              </Link>
              <button type="button" className="btn btn-secondary btn-sm" disabled title="Disponible próximamente">
                Comprar
              </button>
            </div>

            <button
              type="button"
              className="btn btn-link text-danger w-100 text-center small text-decoration-none p-0 fw-medium mt-2"
              onClick={() => onQuitar(favorito.articuloId)}
            >
              💔 Quitar de favoritos
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default TarjetaFavorito
