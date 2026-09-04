import { defineConfig, devices } from "@playwright/test";

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || "http://127.0.0.1:3000";

export default defineConfig({
	testDir: "./e2e",
	outputDir: "./test-results",
	timeout: 90000,
	expect: {
		timeout: 30000,
	},
	fullyParallel: false,
	workers: 1, // Single worker to avoid state collision across multi-step pipeline flows
	forbidOnly: !!process.env.CI,
	retries: 1,

	globalSetup: "./e2e/setup/global-setup.ts",
	globalTeardown: "./e2e/setup/global-teardown.ts",

	reporter: [
		["html", { outputFolder: "playwright-report", open: "never" }],
		["list"],
	],

	use: {
		baseURL: BASE_URL,
		viewport: { width: 1280, height: 800 },
		actionTimeout: 30000,
		navigationTimeout: 45000,
		screenshot: "on",
		trace: "on",
		video: "on",
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
		command: "bun run preview --port 3000 --host 127.0.0.1",
		url: "http://127.0.0.1:3000",
		reuseExistingServer: !process.env.CI,
		timeout: 60000,
	},
});
