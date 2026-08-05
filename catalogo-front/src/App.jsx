
import { useState, useEffect } from 'react'

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
      <ul>
        {articulos.map(a => (
          <li key={a.id}>
            <img src={a.imagenUrl} alt={a.nombre} width="80" />
            {a.nombre} — {a.marca}
          </li>
        ))}
      </ul>
    </>
  )
}

export default App