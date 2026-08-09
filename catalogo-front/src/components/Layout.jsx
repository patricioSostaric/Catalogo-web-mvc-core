import Encabezado from './Encabezado'
import PiePagina from './PiePagina'

function Layout({ children }) {
  return (
    <>
      <Encabezado />
      <main className="container py-4">{children}</main>
      <PiePagina />
    </>
  )
}

export default Layout