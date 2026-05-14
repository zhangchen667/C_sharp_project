import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173
  },
  resolve: {
    alias: [
      // 只精确匹配 dayjs，不影响 dayjs/plugin/*
      { find: /^dayjs$/, replacement: 'dayjs/esm/index.js' }
    ]
  },
  optimizeDeps: {
    noDiscovery: false,
    include: ['element-plus', 'dayjs']
  }
})
