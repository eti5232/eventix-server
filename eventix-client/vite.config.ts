import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

// In development the client calls /api on its own origin and Vite forwards it to the local Eventix API,
// so the browser never makes a cross-origin request and no CORS setup is needed on the server.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: {
        '/api': { target: env.VITE_API_PROXY_TARGET || 'http://localhost:5139', changeOrigin: true },
      },
    },
  }
})
