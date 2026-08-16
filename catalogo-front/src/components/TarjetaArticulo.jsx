import { Link } from 'react-router-dom'

// El corazon solo aparece con sesion iniciada: sin ella no hay a quien atribuirle el
// favorito, y un boton que siempre manda al login es peor que no mostrarlo.
//
// Igual que en TarjetaFavorito, la tarjeta no hace el fetch: avisa que la apretaron y
// quien maneja la lista decide. Asi el mismo componente sirve aunque el estado se guarde
// de otra forma.
function TarjetaArticulo({ articulo, esFavorito, onAlternarFavorito }) {
  const mostrarCorazon = typeof onAlternarFavorito === 'function'

  return (
    <div className="col">
      <div className="card h-100">
        <div className="position-relative">
          <img
            src={articulo.imagenUrl}
            className="card-img-top p-3"
            alt={articulo.nombre}
            style={{ height: '200px', objectFit: 'contain' }}
          />

          {mostrarCorazon && (
            <button
              type="button"
              className="btn btn-light btn-sm rounded-circle shadow-sm position-absolute top-0 end-0 m-2 lh-1"
              onClick={() => onAlternarFavorito(articulo.id)}
              aria-pressed={esFavorito}
              title={esFavorito ? 'Quitar de favoritos' : 'Agregar a favoritos'}
            >
              {esFavorito ? '❤️' : '🤍'}
            </button>
          )}
        </div>

        <div className="card-body d-flex flex-column">
          <h5 className="card-title">{articulo.nombre}</h5>
          <p className="card-text text-muted small mb-2">
            {articulo.marca} · {articulo.categoria}
          </p>
          <p className="fw-semibold fs-5 mb-2">
            ${articulo.precio.toLocaleString('es-AR')}
          </p>
          <div className="mt-auto">
            {!articulo.disponible && (
              <span className="badge text-bg-secondary mb-2 d-block">Sin stock</span>
            )}
            <Link to={`/articulo/${articulo.id}`} className="btn btn-outline-primary btn-sm">
              Ver detalle
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}

export default TarjetaArticulo
