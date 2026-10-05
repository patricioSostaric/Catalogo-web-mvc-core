import { useState, useEffect } from 'react';

function AbmSimple({ titulo, singular, recurso, campoId }) {
  const [items, setItems] = useState([]);
  const [descripcion, setDescripcion] = useState('');
  const [error, setError] = useState('');
  const [editandoId, setEditandoId] = useState(null);
  const [textoEdicion, setTextoEdicion] = useState('');

  // 'cargando' | 'listo' | 'fallo'. Hace falta para la tabla: una lista vacia
  // puede ser que todavia no llego, que llego sin filas o que no se pudo pedir, y
  // en cada caso corresponde mostrar algo distinto.
  const [carga, setCarga] = useState('cargando');

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
        if (data === null) {
          setCarga('fallo');
          return;
        }
        setItems(data);
        setCarga('listo');
      })
      .catch(() => {
        // Sin red o con el servidor caido el fetch rechaza la promesa: sin este
        // catch la pagina quedaba en blanco y sin explicacion.
        setError(`No se pudieron cargar las ${titulo.toLowerCase()}.`);
        setCarga('fallo');
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
      {/* Mismo encabezado que las grillas del MVC: titulo, linea descriptiva y
          una separacion antes del contenido. */}
      <div className="mb-4 pb-3 border-bottom">
        <h1 className="fw-bold text-dark h2 mb-1">{titulo}</h1>
        <p className="text-muted small m-0">
          Panel de administración de {titulo.toLowerCase()}.
        </p>
      </div>

      {/* La grilla limita el ancho: un input-group se estira hasta llenar lo que
          lo contiene. En el celular ocupa todo; en pantallas mas grandes, menos
          de la mitad. */}
      <div className="row">
        <div className="col-md-6 col-lg-5">
          <form onSubmit={agregar} className="input-group mb-3">
            <input
              className="form-control"
              value={descripcion}
              onChange={(e) => setDescripcion(e.target.value)}
              placeholder={`Nueva ${singular}`}
              aria-label={`Nombre de la nueva ${singular}`}
            />
            <button type="submit" className="btn btn-success">
              Agregar
            </button>
          </form>
        </div>
      </div>

      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}

      {/* La tabla sigue a _TablaEntidad.cshtml del MVC: tarjeta sin borde,
          cabecera oscura y botonera compacta en la ultima columna. */}
      <div className="card border-0 shadow-sm rounded-4 bg-white overflow-hidden">
        <div className="table-responsive">
          <table className="table table-hover align-middle mb-0">
            <thead className="table-dark">
              <tr>
                <th className="ps-4">Descripción</th>
                <th className="text-center pe-4" style={{ width: '180px' }}>
                  Acción
                </th>
              </tr>
            </thead>
            <tbody>
              {carga === 'cargando' && (
                <tr>
                  <td colSpan={2} className="p-4 text-center text-muted">
                    Cargando {titulo.toLowerCase()}...
                  </td>
                </tr>
              )}

              {carga === 'listo' && items.length === 0 && (
                <tr>
                  <td colSpan={2} className="p-4 text-center text-muted">
                    Todavía no hay {titulo.toLowerCase()} cargadas.
                  </td>
                </tr>
              )}

              {items.map((item) => {
                const editando = editandoId === item[campoId];

                return (
                  <tr key={item[campoId]}>
                    <td className="ps-4 fw-bold text-dark">
                      {editando ? (
                        <input
                          className="form-control form-control-sm"
                          value={textoEdicion}
                          onChange={(e) => setTextoEdicion(e.target.value)}
                          aria-label={`Nombre de la ${singular}`}
                        />
                      ) : (
                        item.descripcion
                      )}
                    </td>

                    <td className="text-center pe-4">
                      <div className="btn-group btn-group-sm" role="group">
                        {editando ? (
                          <>
                            <button
                              type="button"
                              className="btn btn-success fw-medium"
                              onClick={() => guardarEdicion(item[campoId])}
                            >
                              Guardar
                            </button>
                            <button
                              type="button"
                              className="btn btn-outline-secondary fw-medium"
                              onClick={() => setEditandoId(null)}
                            >
                              Cancelar
                            </button>
                          </>
                        ) : (
                          <>
                            <button
                              type="button"
                              className="btn btn-outline-primary fw-medium"
                              onClick={() => empezarEdicion(item)}
                            >
                              Editar
                            </button>
                            <button
                              type="button"
                              className="btn btn-outline-danger fw-medium"
                              onClick={() => eliminar(item[campoId])}
                            >
                              Eliminar
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
export default AbmSimple;
