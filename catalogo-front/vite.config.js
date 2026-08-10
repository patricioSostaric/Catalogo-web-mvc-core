import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  // El front se publica bajo /app dentro del MVC, no en la raiz.
  base: '/app/',
  plugins: [react()],
  build: {
    // El resultado compilado va directo a wwwroot, que es lo que sirve el MVC.
    outDir: '../catalogo-web-mvc/wwwroot/app',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      // Todo lo que empiece con /api se reenvia al MVC. Para el navegador
      // sale del mismo origen que el front, asi que no interviene CORS.
      '/api': {
        target: 'https://localhost:7012',
        changeOrigin: true,
        // El certificado de desarrollo de ASP.NET es autofirmado.
        secure: false,
      },
      '/imagen': {
        target: 'https://localhost:7012',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})