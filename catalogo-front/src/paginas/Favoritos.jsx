import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import TarjetaFavorito from '../components/TarjetaFavorito'

function Favoritos() {
  const [favoritos, setFavoritos] = useState([])
  const [cargando, setCargando] = useState(true)
  const [sinSesion, setSinSesion] = useState(false)
  const [error, setError] = useState(false)

  useEffect(() => {
    setCargando(true)
    setSinSesion(false)
    setError(false)

    // No lleva token ni cabeceras: la cookie de sesion viaja sola porque es el
    // mismo origen. Eso es lo que hace posible esta pagina.
    fetch('/api/favoritos')
      .then(respuesta => {
        // Un 401 es una respuesta valida, no un fallo de red: el servidor dice
        // "no se quien sos". Hay que mirarlo antes de convertir el cuerpo a JSON.
        if (respuesta.status === 401) {
          setSinSesion(true)
          setCargando(false)
          return null
        }
        if (!respuesta.ok) throw new Error(respuesta.status)
        return respuesta.json()
      })
      .then(datos => {
        if (datos === null) return
        setFavoritos(datos)
        setCargando(false)
      })
      .catch(() => {
        setError(true)
        setCargando(false)
      })
  }, [])

  async function quitar(articuloId) {
    const respuesta = await fetch(`/api/favoritos/${articuloId}`, { method: 'DELETE' })

    if (respuesta.status === 401) {
      // La sesion vencio mientras la pagina estaba abierta.
      setSinSesion(true)
      return
    }
    if (!respuesta.ok) {
      setError(true)
      return
    }

    // Se saca de la lista en memoria en vez de volver a pedirla entera: el
    // servidor ya confirmo que se borro, y el usuario ve el cambio al instante.
    setFavoritos(favoritos.filter(f => f.articuloId !== articuloId))
  }

  if (cargando) return <p>Cargando favoritos...</p>

  if (sinSesion) {
    return (
      <div className="alert alert-info">
        Para ver tus favoritos necesitás iniciar sesión.{' '}
        {/* Ancla y no Link: el login es la aplicacion Razor, no una ruta del router. */}
        <a href="/Account/Login">Iniciar sesión</a>
      </div>
    )
  }

  if (error) {
    return <div className="alert alert-danger">No se pudieron cargar tus favoritos.</div>
  }

  return (
    <div className="container my-5">
      <h2 className="text-center mb-5 fw-bold text-dark">Mis artículos favoritos</h2>

      {favoritos.length === 0 ? (
        <div className="d-flex justify-content-center mt-4">
          <div
            className="card border-0 shadow-sm p-5 text-center d-flex flex-column align-items-center gap-3 bg-white rounded-4"
            style={{ maxWidth: '450px' }}
          >
            <div className="bg-warning bg-opacity-10 p-3 rounded-circle mb-2">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="44" height="44" fill="#ffc107">
                <polygon points="12 2 15 9 22 9 17 14 19 21 12 17 5 21 7 14 2 9 9 9" />
              </svg>
            </div>
            <h4 className="fw-bold text-dark mb-1">Tu lista está vacía</h4>
            <p className="text-muted small px-3 m-0">
              Explorá nuestro catálogo completo y guardá acá los artículos que más te
              interesen para tenerlos a mano.
            </p>
            <Link to="/" className="btn btn-primary px-4 py-2 rounded-3 fw-medium shadow-sm mt-2 text-decoration-none">
              Explorar catálogo
            </Link>
          </div>
        </div>
      ) : (
        <div className="row row-cols-1 row-cols-sm-2 row-cols-lg-3 gy-4">
          {favoritos.map(f => (
            <TarjetaFavorito key={f.articuloId} favorito={f} onQuitar={quitar} />
          ))}
        </div>
      )}
    </div>
  )
}

export default Favoritos
