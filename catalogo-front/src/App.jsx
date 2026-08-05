import { useState, useEffect } from 'react'
import TarjetaArticulo from './components/TarjetaArticulo'

function App() {
  // 1. Estado
  const [articulos, setArticulos] = useState([])
  const [pagina, setPagina] = useState(1)
  const [totalPaginas, setTotalPaginas] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  // 2. Efecto: se vuelve a ejecutar cada vez que cambia `pagina`
    useEffect(() => {
    const temporizador = setTimeout(() => {
      fetch(`/api/articulos?buscar=${busqueda}&page=${pagina}`)
        .then(respuesta => respuesta.json())
        .then(datos => {
          setArticulos(datos.articulos)
          setTotalPaginas(datos.totalPaginas)
        })
    }, 300)

    return () => clearTimeout(temporizador)
  }, [busqueda, pagina])

  // 3. JSX
  return (
    <>
      <h1>Catálogo</h1>
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
        <button onClick={() => setPagina(pagina + 1)} disabled={pagina === totalPaginas}>
          Siguiente
        </button>
      </div>
    </>
  )
}

export default App