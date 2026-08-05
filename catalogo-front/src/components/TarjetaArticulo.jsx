function TarjetaArticulo({ articulo }) {
  return (
    <div className="tarjeta">
      <img src={articulo.imagenUrl} alt={articulo.nombre} width="150" />
      <h3>{articulo.nombre}</h3>
      <p>{articulo.marca} · {articulo.categoria}</p>
<p>${articulo.precio.toLocaleString('es-AR')}</p>
{articulo.disponible ? <span>Disponible</span> : <span>Sin stock</span>}
    </div>
  )
}

export default TarjetaArticulo