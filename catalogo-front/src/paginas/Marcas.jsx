import AbmSimple from '../components/AbmSimple';

function Marcas() {
  return (
    <AbmSimple
      titulo="Marcas"
      singular="marca"
      recurso="/api/marcas"
      campoId="marcaId"
    />
  );
}

export default Marcas;
