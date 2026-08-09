import Encabezado from './Encabezado'
import PiePagina from './PiePagina'

function Layout({ children }) {
  return (
    <>
      <Encabezado />
      <main>{children}</main>
      <PiePagina />
    </>
  )
}

export default Layout