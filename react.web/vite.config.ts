/// <reference types="vitest/config" />
import { fileURLToPath } from "node:url";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// Where the ASP.NET Core API runs in development (launch profile "http").
const apiUrl = "http://localhost:5201";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    // "@/lib/format" instead of "../../../lib/format"
    alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) },
  },
  server: {
    port: 5173,
    strictPort: true,
    // The dev server forwards these paths to the API, so the browser only ever talks to
    // http://localhost:5173: no CORS, and the auth cookie is a normal same-origin cookie.
    proxy: {
      "/api": apiUrl,
      "/images": apiUrl,
      "/uploads": apiUrl,
    },
  },
  test: {
    environment: "jsdom",
    setupFiles: ["./src/test/setup.ts"],
  },
});
