import { useState, useEffect } from 'react'

function Marcas() {
  const [marcas, setMarcas] = useState([])

  useEffect(() => {
  fetch('/api/marcas')
    .then(response => {
      if (!response.ok) return null
      return response.json()
    })
    .then(data => {
      if (data === null) return
      setMarcas(data)
    })
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