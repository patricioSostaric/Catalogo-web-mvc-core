import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import TarjetaArticulo from "./TarjetaArticulo";
import { it } from "vitest";

function crearArticulo(overrides = {}) {
  return {
    id: 1,
    nombre: "Notebook Gamer",
    marca: "Lenovo",
    categoria: "Computadoras",
    precio: 69999,
    imagenUrl: "/imagen/articulos/notebook.jpg",
    disponible: true,
    ...overrides,
  };
}
function renderizar(articulo, props = {}) {
  return render(
    <MemoryRouter>
      <TarjetaArticulo articulo={articulo} {...props} />
    </MemoryRouter>
  );
}

describe("TarjetaArticulo", () => {
  it("muestra el nombre y el precio", () => {
    const articulo = crearArticulo();
    render(
      <MemoryRouter>
        <TarjetaArticulo articulo={articulo} />
      </MemoryRouter>
    );

    // Nombre
    expect(screen.getByText("Notebook Gamer")).toBeInTheDocument();

    // Precio formateado con toLocaleString('es-AR')
    expect(screen.getByText("$69.999")).toBeInTheDocument();
  });

    it("muestra 'Sin stock' cuando el articulo no esta disponible", () => {
    render(
      <MemoryRouter>
        <TarjetaArticulo articulo={crearArticulo({ disponible: false })} />
      </MemoryRouter>
    );

    expect(screen.getByText("Sin stock")).toBeInTheDocument();
  });
  it("que no muestra “Sin stock” cuando hay disponibilidad", () => {
  renderizar(crearArticulo());
  
  expect(screen.queryByText("Sin stock")).not.toBeInTheDocument();
});
  });