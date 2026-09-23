import { useState, useEffect } from 'react'

function Marcas() {
  const [marcas, setMarcas] = useState([])

  useEffect(() => {
    fetch('/api/marcas')
      .then(response => response.json())
      .then(data => setMarcas(data))
  }, [])

  return (
    <div>
      <h2>Marcas</h2>
      <ul>
        {marcas.map(marca => (
          <li key={marca.marcaId}>{marca.descripcion}</li>
        ))}
      </ul>
    </div>
  )
}

export default Marcas