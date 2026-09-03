/// <reference types="vitest/config" />
import path from "node:path";
import tailwindcss from "@tailwindcss/vite";
import { devtools } from "@tanstack/devtools-vite";
import { tanstackRouter } from "@tanstack/router-plugin/vite";
import viteReact from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
	resolve: {
		alias: {
			"@": path.resolve(import.meta.dirname, "./src"),
			"#": path.resolve(import.meta.dirname, "./src"),
		},
	},
	plugins: [
		devtools(),
		tailwindcss(),
		tanstackRouter({ target: "react", autoCodeSplitting: true }),
		viteReact(),
	],
	build: {
		modulePreload: false,
	},
	test: {
		globals: true,
		environment: "jsdom",
		setupFiles: "./src/test/setup.ts",
		exclude: ["**/node_modules/**", "**/dist/**", "**/e2e/**"],
	},
	server: {
		port: 3000,
		host: "0.0.0.0",
		allowedHosts: true,
		watch: {
			ignored: [
				"**/playwright-report/**",
				"**/test-results/**",
				"**/.devtool/**",
			],
		},
		proxy: {
			"/api": {
				target: "http://localhost:5000",
				changeOrigin: true,
				secure: false,
			},
			"/openapi": {
				target: "http://localhost:5000",
				changeOrigin: true,
				secure: false,
			},
			"/scalar": {
				target: "http://localhost:5000",
				changeOrigin: true,
				secure: false,
			},
		},
	},
	preview: {
		port: 3000,
		host: "0.0.0.0",
		allowedHosts: true,
		proxy: {
			"/api": {
				target: "http://localhost:5000",
				changeOrigin: true,
				secure: false,
			},
			"/openapi": {
				target: "http://localhost:5000",
				changeOrigin: true,
				secure: false,
			},
			"/scalar": {
				target: "http://localhost:5000",
				changeOrigin: true,
				secure: false,
			},
		},
	},
});
