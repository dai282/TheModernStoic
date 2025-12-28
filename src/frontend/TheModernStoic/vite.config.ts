//import { defineConfig } from 'vite'
import { defineConfig } from 'vitest/config' 
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  test: {
    globals: true, // Allows using describe, it, expect without imports
    environment: 'jsdom', // Simulates browser
    setupFiles: './src/test/setup.ts', // Bootstrapping
    css: true, // Process CSS (useful if you test class names/Tailwind)
  },
})
