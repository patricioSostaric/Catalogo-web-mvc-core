import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
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