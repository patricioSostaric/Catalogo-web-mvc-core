import { render, screen } from "@testing-library/react";
import PiePagina from "./PiePagina";

describe("PiePagina", () => {
  it("muestra el copyright", () => {
    // Renderizar el componente
    render(<PiePagina />);

    // Buscar el texto esperado
    const copyright =
      screen.getByText("© 2026 Store Sostaric");

    // Afirmar que esta en el documento
    expect(copyright).toBeInTheDocument();
  });
});

