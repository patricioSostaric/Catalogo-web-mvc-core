import { useState, useEffect } from 'react'
import TarjetaArticulo from './components/TarjetaArticulo'

function App() {
  const [articulos, setArticulos] = useState([])

  useEffect(() => {
    fetch('/api/articulos')
      .then(respuesta => respuesta.json())
      .then(datos => setArticulos(datos.articulos))
  }, [])

  return (
    <>
      <h1>Catálogo</h1>
      <div className="grilla">
        {articulos.map(a => (
          <TarjetaArticulo key={a.id} articulo={a} />
        ))}
      </div>
    </>
  )
}

export default App