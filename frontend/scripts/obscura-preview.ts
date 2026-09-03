import fs from "node:fs";
import path from "node:path";
import { chromium } from "playwright-core";

const SCREENSHOT_DIR = "/tmp/screenshots";
fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });

async function run() {
	console.log("Connecting to Obscura via CDP at ws://127.0.0.1:9222/devtools/browser...");
	const browser = await chromium.connectOverCDP({
		endpointURL: "ws://127.0.0.1:9222/devtools/browser",
	});

	try {
		const context = await browser.newContext({
			viewport: { width: 1440, height: 900 },
			baseURL: "http://127.0.0.1:3000",
		});

		const page = await context.newPage();

		console.log("Authenticating as demo.salesarea via API...");
		const loginRes = await page.request.post("http://127.0.0.1:3000/api/auth/login", {
			data: {
				username: "demo.salesarea",
				password: "Correct-Horse-Battery-Staple-1",
			},
		});
		if (!loginRes.ok()) {
			throw new Error(`Login failed with status ${loginRes.status()}`);
		}

		console.log("Navigating to dashboard...");
		await page.goto("http://127.0.0.1:3000/");
		await page.waitForSelector('[data-sidebar="sidebar"]', { timeout: 15000 });
		await page.waitForTimeout(1500);

		console.log("Capturing 01-dashboard-sales-area.png...");
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "01-dashboard-sales-area.png"),
			fullPage: true,
		});

		console.log("Capturing 02-directory-index.png...");
		await page.goto("http://127.0.0.1:3000/directory");
		await page.waitForTimeout(1500);
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "02-directory-index.png"),
			fullPage: true,
		});

		console.log("Capturing 03-directory-new.png...");
		await page.goto("http://127.0.0.1:3000/directory/new");
		await page.waitForTimeout(1500);
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "03-directory-new.png"),
			fullPage: true,
		});

		console.log("Capturing 04-tasks-inbox.png...");
		await page.goto("http://127.0.0.1:3000/tasks");
		await page.waitForTimeout(1500);
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "04-tasks-inbox.png"),
			fullPage: true,
		});

		console.log("Capturing 05-tasks-blocked.png...");
		await page.goto("http://127.0.0.1:3000/tasks/blocked");
		await page.waitForTimeout(1500);
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "05-tasks-blocked.png"),
			fullPage: true,
		});

		console.log("Capturing 05b-tasks-history.png...");
		await page.goto("http://127.0.0.1:3000/tasks/history");
		await page.waitForTimeout(1500);
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "05b-tasks-history.png"),
			fullPage: true,
		});

		console.log("Capturing 06-map-explorer.png...");
		await page.goto("http://127.0.0.1:3000/map");
		await page.waitForTimeout(2500);
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "06-map-explorer.png"),
			fullPage: true,
		});

		console.log("Capturing 07-reports-index.png...");
		await page.goto("http://127.0.0.1:3000/reports");
		await page.waitForTimeout(1500);
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "07-reports-index.png"),
			fullPage: true,
		});

		console.log("Capturing 08-reports-funnel.png...");
		await page.goto("http://127.0.0.1:3000/reports/funnel");
		await page.waitForTimeout(1500);
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "08-reports-funnel.png"),
			fullPage: true,
		});

		console.log("Capturing 09-reports-ageing.png...");
		await page.goto("http://127.0.0.1:3000/reports/ageing");
		await page.waitForTimeout(1500);
		await page.screenshot({
			path: path.join(SCREENSHOT_DIR, "09-reports-ageing.png"),
			fullPage: true,
		});

		await context.close();

		// Now test Admin views
		console.log("Authenticating as admin via API...");
		const adminContext = await browser.newContext({
			viewport: { width: 1440, height: 900 },
			baseURL: "http://127.0.0.1:3000",
		});
		const adminPage = await adminContext.newPage();

		const adminLoginRes = await adminPage.request.post("http://127.0.0.1:3000/api/auth/login", {
			data: {
				username: "admin",
				password: "Correct-Horse-Battery-Staple-1",
			},
		});
		if (!adminLoginRes.ok()) {
			throw new Error(`Admin login failed with status ${adminLoginRes.status()}`);
		}

		console.log("Capturing 10-master-organisation.png...");
		await adminPage.goto("http://127.0.0.1:3000/master/organisation");
		await adminPage.waitForSelector('[data-sidebar="sidebar"]', { timeout: 15000 });
		await adminPage.waitForTimeout(1500);
		await adminPage.screenshot({
			path: path.join(SCREENSHOT_DIR, "10-master-organisation.png"),
			fullPage: true,
		});

		console.log("Capturing 11-master-users.png...");
		await adminPage.goto("http://127.0.0.1:3000/master/users");
		await adminPage.waitForTimeout(1500);
		await adminPage.screenshot({
			path: path.join(SCREENSHOT_DIR, "11-master-users.png"),
			fullPage: true,
		});

		console.log("Capturing 12-admin-break-glass.png...");
		await adminPage.goto("http://127.0.0.1:3000/admin/break-glass");
		await adminPage.waitForTimeout(1500);
		await adminPage.screenshot({
			path: path.join(SCREENSHOT_DIR, "12-admin-break-glass.png"),
			fullPage: true,
		});

		console.log("Capturing 13-admin-stuck-steps.png...");
		await adminPage.goto("http://127.0.0.1:3000/admin/stuck-steps");
		await adminPage.waitForTimeout(1500);
		await adminPage.screenshot({
			path: path.join(SCREENSHOT_DIR, "13-admin-stuck-steps.png"),
			fullPage: true,
		});

		await adminContext.close();

		console.log("All screenshots captured successfully in", SCREENSHOT_DIR);
	} finally {
		await browser.close();
	}
}

run().catch((err) => {
	console.error("Obscura preview error:", err);
	process.exit(1);
});
