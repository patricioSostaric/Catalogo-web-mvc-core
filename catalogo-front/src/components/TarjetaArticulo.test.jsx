import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import TarjetaArticulo from "./TarjetaArticulo";
import userEvent from "@testing-library/user-event";

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

// TarjetaArticulo usa Link, que lee el router desde un contexto de React. En la
// aplicacion lo provee BrowserRouter; aca el componente esta solo, asi que hay que
// envolverlo o falla al renderizar.
function renderizar(articulo, props = {}) {
  return render(
    <MemoryRouter>
      <TarjetaArticulo articulo={articulo} {...props} />
    </MemoryRouter>
  );
}

describe("TarjetaArticulo", () => {
  it("muestra el nombre y el precio", () => {
    renderizar(crearArticulo());

    expect(screen.getByText("Notebook Gamer")).toBeInTheDocument();
    // El precio sale formateado con toLocaleString('es-AR'): $69.999, no 69999.
    expect(screen.getByText("$69.999")).toBeInTheDocument();
  });

  it("muestra 'Sin stock' cuando el articulo no esta disponible", () => {
    renderizar(crearArticulo({ disponible: false }));

    expect(screen.getByText("Sin stock")).toBeInTheDocument();
  });

  // queryByText y no getByText: getByText lanza excepcion cuando no encuentra, asi
  // que el test fallaria por el error en vez de por la asercion.
  it("no muestra 'Sin stock' cuando hay disponibilidad", () => {
    renderizar(crearArticulo());

    expect(screen.queryByText("Sin stock")).not.toBeInTheDocument();
  });
  it("no muestra el corazon si no recibe onAlternarFavorito", () => {
    renderizar(crearArticulo());

    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });
  it("llama a onAlternarFavorito con el id al tocar el corazon", async () => {
    const alternar = vi.fn();
    const user = userEvent.setup();

    renderizar(crearArticulo(), { onAlternarFavorito: alternar });
    await user.click(screen.getByRole("button"));

    expect(alternar).toHaveBeenCalledWith(1);
  });
});
