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
      {cargando && <p>Cargando artículos...</p>}
      {error && <p>Error al cargar los artículos.</p>}
      {!cargando && !error && articulos.length === 0 && <p>No se encontraron artículos.</p>}
      <input value={busqueda} onChange={e => { setBusqueda(e.target.value); setPagina(1) }} placeholder="Buscar..." />
      <div className="grilla">
        {articulos.map(a => (
          <TarjetaArticulo key={a.id} articulo={a} />
        ))}
      </div>
      <div>
        <button onClick={() => setPagina(pagina - 1)} disabled={pagina === 1}>
          Anterior
        </button>
        <span> Página {pagina} de {totalPaginas} </span>
        <button onClick={() => setPagina(pagina + 1)} disabled={pagina >= totalPaginas}>
          Siguiente
        </button>
      </div>
    </Layout>
  )
}

export default App