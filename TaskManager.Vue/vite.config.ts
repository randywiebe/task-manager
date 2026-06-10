import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueJsx(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  test:{
    environment: 'jsdom',
  },
  server: {
    proxy: {
      '/lists': {
        target: 'https://localhost:7299',
        changeOrigin: true,
        secure: false, // needed because your backend is on https with a local cert
      }
    }
  }
})
