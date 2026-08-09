import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'

function Detalle() {
  // El nombre tiene que coincidir con el comodin declarado en la ruta: /articulo/:id
  const { id } = useParams()

  const [articulo, setArticulo] = useState(null)
  const [cargando, setCargando] = useState(true)
  const [noEncontrado, setNoEncontrado] = useState(false)
  const [error, setError] = useState(false)

  useEffect(() => {
    setCargando(true)
    setNoEncontrado(false)
    setError(false)

    fetch(`/api/articulos/${id}`)
      .then(respuesta => {
        // Un 404 es una respuesta valida del servidor, no un fallo de red:
        // hay que mirarlo antes de intentar convertir el cuerpo a JSON.
        if (respuesta.status === 404) {
          setNoEncontrado(true)
          setCargando(false)
          return null
        }
        if (!respuesta.ok) throw new Error(respuesta.status)
        return respuesta.json()
      })
      .then(datos => {
        if (datos === null) return
        setArticulo(datos)
        setCargando(false)
      })
      .catch(() => {
        setError(true)
        setCargando(false)
      })
  }, [id])

  if (cargando) return <p>Cargando artículo...</p>

  if (noEncontrado) {
    return (
      <>
        <div className="alert alert-warning">
          El artículo que buscás no existe o ya no está disponible.
        </div>
        <Link to="/" className="btn btn-secondary">Volver al catálogo</Link>
      </>
    )
  }

  if (error) {
    return (
      <>
        <div className="alert alert-danger">No se pudo cargar el artículo.</div>
        <Link to="/" className="btn btn-secondary">Volver al catálogo</Link>
      </>
    )
  }

  return (
    <>
      <h1>Detalles del artículo</h1>
      <hr />

      <div className="row align-items-center">
        <div className="col-md-7">
          <dl className="row mb-0">
            <div className="col-12 mb-2 pb-2 border-bottom">
              <dt className="text-muted small">Nombre</dt>
              <dd className="fs-5 mb-0">{articulo.nombre}</dd>
            </div>

            <div className="col-12 mb-2 pb-2 border-bottom">
              <dt className="text-muted small">Descripción</dt>
              <dd className="text-secondary mb-0">{articulo.descripcion}</dd>
            </div>

            <div className="col-6 mb-2 pb-2 border-bottom">
              <dt className="text-muted small">Marca</dt>
              <dd className="mb-0">{articulo.marca}</dd>
            </div>
            <div className="col-6 mb-2 pb-2 border-bottom">
              <dt className="text-muted small">Categoría</dt>
              <dd className="mb-0">{articulo.categoria}</dd>
            </div>

            <div className="col-12 mb-2">
              <dt className="text-muted small">Precio</dt>
              <dd className="fw-bold text-primary fs-5 mb-0">
                ${articulo.precio.toLocaleString('es-AR')}
              </dd>
            </div>
          </dl>
        </div>

        <div className="col-md-5 text-center">
          <img
            src={articulo.imagenUrl}
            alt={articulo.nombre}
            className="img-fluid rounded"
            style={{ maxHeight: '320px', objectFit: 'contain' }}
          />
        </div>
      </div>

      <div className="mt-4">
        <Link to="/" className="btn btn-secondary">Volver al catálogo</Link>
      </div>
    </>
  )
}

export default Detalle
