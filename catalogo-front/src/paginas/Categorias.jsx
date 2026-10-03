import { useState, useEffect } from 'react';
function Categorias() {
  // estados
  const [categorias, setCategorias] = useState([]);
  const [descripcion, setDescripcion] = useState('');
  const [error, setError] = useState('');
  const [editandoId, setEditandoId] = useState(null);
  const [textoEdicion, setTextoEdicion] = useState('');

  //useEffect para cargar las categorias
  useEffect(() => {
    fetch('/api/categorias')
      .then((response) => {
        if (response.status === 401) {
          setError('Iniciá sesión para ver esta página.');
          return null;
        }
        if (response.status === 403) {
          setError('No tenés permisos para administrar categorias.');
          return null;
        }

        if (!response.ok) {
          setError('No se pudieron cargar las categorias.');
          return null;
        }

        return response.json();
      })
      .then((data) => {
        if (data === null) return;
        setCategorias(data);
      });
  }, []);

  // Función para agregar una nueva categoria

  async function agregarCategoria(e) {
    e.preventDefault();

    if (descripcion.trim() === '') {
      setError('Escribí un nombre para la categoría.');
      return;
    }

    const respuesta = await fetch('/api/categorias', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ descripcion }),
    });

    if (!respuesta.ok) {
      setError('No se pudo crear la categoría.');
      return;
    }

    setError('');

    const creada = await respuesta.json();
    setCategorias([...categorias, creada]);
    setDescripcion('');
  }
  async function eliminarCategoria(id) {
    const respuesta = await fetch(`/api/categorias/${id}`, {
      method: 'DELETE',
    });

    if (!respuesta.ok) {
      setError(await respuesta.text());
      return;
    }
    setError('');
    setCategorias(categorias.filter((c) => c.categoriaId !== id));
  }

  function empezarEdicion(categoria) {
    setEditandoId(categoria.categoriaId);
    setTextoEdicion(categoria.descripcion);
  }

  async function guardarEdicion(id) {
    const respuesta = await fetch(`/api/categorias/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ descripcion: textoEdicion }),
    });

    if (!respuesta.ok) {
      setError('No se pudo guardar el cambio.');
      return;
    }

    setCategorias(
      categorias.map((c) =>
        c.categoriaId === id ? { ...c, descripcion: textoEdicion } : c,
      ),
    );
    setEditandoId(null);
  }

  return (
    <div>
      <h2>Categorias</h2>

      <form onSubmit={agregarCategoria}>
        <input
          value={descripcion}
          onChange={(e) => setDescripcion(e.target.value)}
          placeholder="Nueva categoria"
        />
        <button type="submit">Agregar</button>
      </form>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <ul>
        {categorias.map((categoria) => (
          <li key={categoria.categoriaId}>
            {editandoId === categoria.categoriaId ? (
              <>
                <input
                  value={textoEdicion}
                  onChange={(e) => setTextoEdicion(e.target.value)}
                />
                <button onClick={() => guardarEdicion(categoria.categoriaId)}>
                  Guardar
                </button>
                <button onClick={() => setEditandoId(null)}>Cancelar</button>
              </>
            ) : (
              <>
                {categoria.descripcion}
                <button onClick={() => empezarEdicion(categoria)}>
                  Editar
                </button>
                <button
                  onClick={() => eliminarCategoria(categoria.categoriaId)}
                >
                  Borrar
                </button>
              </>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}
export default Categorias;
