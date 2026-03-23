import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Matches OrderInventoryPlatform.Api http profile (launchSettings.json)
      '/api': {
        target: 'http://localhost:5241',
        changeOrigin: true,
      },
    },
  },
})
