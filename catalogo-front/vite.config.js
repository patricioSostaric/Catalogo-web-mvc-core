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
      // Los dos apuntan al gateway y no al MVC: desde que la API se mudo a su
      // propio proyecto, el MVC dejo de atender /api. El gateway es el unico
      // que sabe que ruta va a cual aplicacion, y este proxy solo tiene que
      // llevarlo hasta ahi.
      //
      // Requiere el stack levantado con docker compose up.
      '/api': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
      '/imagen': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
    },
  },
})