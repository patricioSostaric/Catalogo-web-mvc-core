import { useState, useEffect } from 'react'

function Marcas() {
  const [marcas, setMarcas] = useState([])
  const [descripcion, setDescripcion] = useState('') 
  const [error, setError] = useState('')
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
// Función para agregar una nueva marca

async function agregarMarca(e) {
    e.preventDefault()

    const respuesta = await fetch('/api/marcas', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ descripcion })
    })

    if (!respuesta.ok) return

    const creada = await respuesta.json()
    setMarcas([...marcas, creada])
    setDescripcion('')
  }
  async function eliminarMarca(id) {
  const respuesta = await fetch(`/api/marcas/${id}`, { method: 'DELETE' })

  if (!respuesta.ok) {
  setError(await respuesta.text())
  return
}

setError('')
setMarcas(marcas.filter(m => m.marcaId !== id))
}


  return (
   <div>
    <h2>Marcas</h2>

    <form onSubmit={agregarMarca}>
  <input
    value={descripcion}
    onChange={e => setDescripcion(e.target.value)}
    placeholder="Nueva marca"
  />
  <button type="submit">Agregar</button>
</form>
{error && <p style={{ color: 'red' }}>{error}</p>}
    <ul>
      {marcas.map(marca => (
        <li key={marca.marcaId}>
          {marca.descripcion}
          <button onClick={() => eliminarMarca(marca.marcaId)}>Borrar</button>
        </li>
      ))}
    </ul>
  </div>
  )
}

export default Marcas