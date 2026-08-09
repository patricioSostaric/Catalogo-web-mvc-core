import { Routes, Route } from 'react-router-dom'
import Layout from './components/Layout'
import Catalogo from './paginas/Catalogo'
import Privacidad from './paginas/Privacidad.jsx'

function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Catalogo />} />
        <Route path="/privacidad" element={<Privacidad />} />
      </Routes>
    </Layout>
  )
}

export default App