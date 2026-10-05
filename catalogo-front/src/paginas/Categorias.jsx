import AbmSimple from '../components/AbmSimple';

function Categorias() {
  return (
    <AbmSimple
      titulo="Categorías"
      singular="categoría"
      recurso="/api/categorias"
      campoId="categoriaId"
    />
  );
}

export default Categorias;
