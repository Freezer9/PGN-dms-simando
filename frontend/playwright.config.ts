import { defineConfig, devices } from "@playwright/test";

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || "http://127.0.0.1:3000";
const OBSCURA_PORT = process.env.OBSCURA_PORT || "9222";
const OBSCURA_WS =
	process.env.OBSCURA_WS_ENDPOINT || `ws://127.0.0.1:${OBSCURA_PORT}`;

export default defineConfig({
	testDir: "./e2e",
	outputDir: "./test-results",
	timeout: 60000,
	expect: {
		timeout: 10000,
	},
	fullyParallel: false,
	workers: 1, // Single worker to avoid state collision across multi-step pipeline flows
	forbidOnly: !!process.env.CI,
	retries: process.env.CI ? 1 : 0,

	globalSetup: "./e2e/setup/global-setup.ts",
	globalTeardown: "./e2e/setup/global-teardown.ts",

	reporter: [
		["html", { outputFolder: "playwright-report", open: "never" }],
		["list"],
	],

	use: {
		baseURL: BASE_URL,
		viewport: { width: 1280, height: 800 },
		actionTimeout: 15000,
		navigationTimeout: 30000,
		screenshot: "on",
		trace: "on",
		video: "on",
		connectOptions: {
			wsEndpoint: `${OBSCURA_WS}/devtools/browser`,
		},
	},

	projects: [
		{
			name: "obscura",
			use: {
				...devices["Desktop Chrome"],
			},
		},
	],

	webServer: {
		command: "bun run dev --host 127.0.0.1",
		url: "http://127.0.0.1:3000",
		reuseExistingServer: !process.env.CI,
		timeout: 60000,
	},
});
