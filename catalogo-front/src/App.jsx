import { Routes, Route } from 'react-router-dom'
import Layout from './components/Layout'
import Catalogo from './paginas/Catalogo'
import Privacidad from './paginas/Privacidad.jsx'
import Detalle from './paginas/Detalle'
import Favoritos from './paginas/Favoritos'

function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Catalogo />} />
        <Route path="/privacidad" element={<Privacidad />} />
        <Route path="/articulo/:id" element={<Detalle />} />
        <Route path="/favoritos" element={<Favoritos />} />
      </Routes>
    </Layout>
  )
}

export default App