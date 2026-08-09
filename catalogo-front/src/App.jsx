import { useState, useEffect } from 'react'
import TarjetaArticulo from './components/TarjetaArticulo'
import Layout from './components/Layout'

function App() {
  // 1. Estado
  const [articulos, setArticulos] = useState([])
  const [pagina, setPagina] = useState(1)
  const [totalPaginas, setTotalPaginas] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState(false)
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
    <Layout>
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
          <TarjetaArticulo key={a.id} articulo={a} />
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
    </Layout>
  )
}

export default App