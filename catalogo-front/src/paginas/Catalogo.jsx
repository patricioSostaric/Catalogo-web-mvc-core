import { useState, useEffect } from 'react'
import TarjetaArticulo from '../components/TarjetaArticulo'



function Catalogo() {
  // 1. Estado
  const [articulos, setArticulos] = useState([])
  const [pagina, setPagina] = useState(1)
  const [totalPaginas, setTotalPaginas] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState(false)

  // Los ids marcados como favoritos. Un Set y no una lista porque la unica pregunta que
  // se hace es "¿esta este id?", una vez por tarjeta.
  //
  // Empieza en null y no en un Set vacio: null significa "todavia no se sabe" o "no hay
  // sesion", y con eso las tarjetas ocultan el corazon. Un Set vacio significaria "hay
  // sesion y no hay favoritos", que es distinto.
  const [favoritos, setFavoritos] = useState(null)

  useEffect(() => {
    fetch('/api/favoritos')
      .then(respuesta => {
        // 401 es la respuesta normal para quien mira el catalogo sin iniciar sesion:
        // el catalogo es publico, los favoritos no.
        if (!respuesta.ok) return null
        return respuesta.json()
      })
      .then(datos => {
        if (datos === null) return
        setFavoritos(new Set(datos.map(f => f.articuloId)))
      })
      .catch(() => { /* sin favoritos: las tarjetas quedan sin corazon */ })
  }, [])

  async function alternarFavorito(articuloId) {
    const yaEstaba = favoritos.has(articuloId)

    const respuesta = yaEstaba
      ? await fetch(`/api/favoritos/${articuloId}`, { method: 'DELETE' })
      : await fetch('/api/favoritos', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ articuloId })
        })

    if (!respuesta.ok) return

    // Se actualiza el estado local en vez de volver a pedir la lista: el servidor ya
    // confirmo, y el corazon cambia al instante.
    const actualizados = new Set(favoritos)
    yaEstaba ? actualizados.delete(articuloId) : actualizados.add(articuloId)
    setFavoritos(actualizados)
  }

  // 2. Efecto: se vuelve a ejecutar cada vez que cambia `pagina`

  useEffect(() => {
    const temporizador = setTimeout(() => {
      setCargando(true)
      setError(false)
      fetch(`/api/articulos?buscar=${busqueda}&page=${pagina}`)
        .then(respuesta => respuesta.json())
        .then(datos => {
          setArticulos(datos.articulos)
          setTotalPaginas(datos.totalPaginas)
          setCargando(false)
        })
        .catch(() => {
          setError(true)
          setCargando(false)
        })
    }, 300)

    return () => clearTimeout(temporizador)
  }, [busqueda, pagina])



  // 3. JSX
  return (
    <>
      <div className="bg-white py-5 mb-5 border-bottom shadow-sm">
  <div className="container text-center">
    <h1 className="display-5 fw-bold text-dark mb-2">¡Bienvenido/a!</h1>
    <p className="lead text-muted mb-4">Te encontrás en la web de artículos.</p>

    <div className="row justify-content-center">
      <div className="col-md-10 col-lg-8">
        <input
          className="form-control form-control-lg"
          value={busqueda}
          onChange={e => { setBusqueda(e.target.value); setPagina(1) }}
          placeholder="Buscar artículo..."
        />
      </div>
    </div>
  </div>
</div>

<div className="pb-2 mb-4 border-bottom">
  <h2 className="h4 text-secondary m-0">Lista de Artículos</h2>
</div>
      {cargando && <p>Cargando artículos...</p>}
      {error && <p>Error al cargar los artículos.</p>}
      {!cargando && !error && articulos.length === 0 && <p>No se encontraron artículos.</p>}
      

      <div className="row row-cols-1 row-cols-sm-2 row-cols-lg-3 gy-4">
        {articulos.map(a => (
          <TarjetaArticulo
            key={a.id}
            articulo={a}
            esFavorito={favoritos?.has(a.id) ?? false}
            onAlternarFavorito={favoritos ? alternarFavorito : undefined}
          />
        ))}
      </div>
            <nav className="d-flex justify-content-center mt-4">
        <ul className="pagination">
          <li className={`page-item ${pagina === 1 ? 'disabled' : ''}`}>
            <button className="page-link" onClick={() => setPagina(pagina - 1)}>Anterior</button>
          </li>
          <li className="page-item disabled">
            <span className="page-link">Página {pagina} de {totalPaginas}</span>
          </li>
          <li className={`page-item ${pagina >= totalPaginas ? 'disabled' : ''}`}>
            <button className="page-link" onClick={() => setPagina(pagina + 1)}>Siguiente</button>
          </li>
        </ul>
      </nav>
    </>
  )
}

export default Catalogo