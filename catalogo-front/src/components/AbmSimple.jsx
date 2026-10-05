import { useState, useEffect } from 'react';

function AbmSimple({ titulo, singular, recurso, campoId }) {
  const [items, setItems] = useState([]);
  const [descripcion, setDescripcion] = useState('');
  const [error, setError] = useState('');
  const [editandoId, setEditandoId] = useState(null);
  const [textoEdicion, setTextoEdicion] = useState('');
  useEffect(() => {
    fetch(recurso)
      .then((response) => {
        if (response.status === 401) {
          setError('Iniciá sesión para ver esta página.');
          return null;
        }

        if (response.status === 403) {
          setError(
            `No tenés permisos para administrar ${titulo.toLowerCase()}.`,
          );
          return null;
        }

        if (!response.ok) {
          setError(`No se pudieron cargar las ${titulo.toLowerCase()}.`);
          return null;
        }

        return response.json();
      })
      .then((data) => {
        if (data === null) return;
        setItems(data);
      });
  }, [recurso, titulo]);
  // Alta

  async function agregar(e) {
    e.preventDefault();
    if (descripcion.trim() === '') {
      setError(`Escribí un nombre para la ${singular.toLowerCase()}.`);
      return;
    }

    const respuesta = await fetch(recurso, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ descripcion }),
    });

    if (!respuesta.ok) {
      setError(`No se pudo crear la ${singular.toLowerCase()}.`);
      return;
    }

    setError('');

    const creada = await respuesta.json();
    setItems([...items, creada]);
    setDescripcion('');
  }
  async function eliminar(id) {
    const respuesta = await fetch(`${recurso}/${id}`, { method: 'DELETE' });

    if (!respuesta.ok) {
      setError(await respuesta.text());
      return;
    }
    setError('');
    setItems(items.filter((m) => m[campoId] !== id));
  }

  function empezarEdicion(item) {
    setEditandoId(item[campoId]);
    setTextoEdicion(item.descripcion);
  }

  async function guardarEdicion(id) {
    const respuesta = await fetch(`${recurso}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ descripcion: textoEdicion }),
    });

    if (!respuesta.ok) {
      setError(`No se pudo guardar la ${singular.toLowerCase()}.`);
      return;
    }

    setItems(
      items.map((m) =>
        m[campoId] === id ? { ...m, descripcion: textoEdicion } : m,
      ),
    );
    setEditandoId(null);
  }

  return (
    <div>
      <h2>{titulo}</h2>

      <form onSubmit={agregar}>
        <input
          value={descripcion}
          onChange={(e) => setDescripcion(e.target.value)}
          placeholder={`Nueva ${singular}`}
        />
        <button type="submit">Agregar</button>
      </form>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <ul>
        {items.map((item) => (
          <li key={item[campoId]}>
            {editandoId === item[campoId] ? (
              <>
                <input
                  value={textoEdicion}
                  onChange={(e) => setTextoEdicion(e.target.value)}
                />
                <button onClick={() => guardarEdicion(item[campoId])}>
                  Guardar
                </button>
                <button onClick={() => setEditandoId(null)}>Cancelar</button>
              </>
            ) : (
              <>
                {item.descripcion}
                <button onClick={() => empezarEdicion(item)}>Editar</button>
                <button onClick={() => eliminar(item[campoId])}>Borrar</button>
              </>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}
export default AbmSimple;
