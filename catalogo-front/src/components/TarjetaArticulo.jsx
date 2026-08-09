function TarjetaArticulo({ articulo }) {
  return (
    <div className="col">
      <div className="card h-100">
        <img
          src={articulo.imagenUrl}
          className="card-img-top p-3"
          alt={articulo.nombre}
          style={{ height: '200px', objectFit: 'contain' }}
        />
        <div className="card-body d-flex flex-column">
          <h5 className="card-title">{articulo.nombre}</h5>
          <p className="card-text text-muted small mb-2">
            {articulo.marca} · {articulo.categoria}
          </p>
          <p className="fw-semibold fs-5 mb-2">
            ${articulo.precio.toLocaleString('es-AR')}
          </p>
          <div className="mt-auto">
            {articulo.disponible
              ? <span className="badge text-bg-success">Disponible</span>
              : <span className="badge text-bg-secondary">Sin stock</span>}
          </div>
        </div>
      </div>
    </div>
  )
}

export default TarjetaArticulo