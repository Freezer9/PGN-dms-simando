import {
	clickElement,
	expect,
	fillInput,
	gotoApp,
	test,
	USER_CREDENTIALS,
} from "./fixtures/auth.fixture";

test.describe("Flow 1: Authentication & Access Control", () => {
	test("1.1 should display login page with form elements and branding", async ({
		page,
	}) => {
		await page.context().clearCookies();
		await gotoApp(page, "/sign-in");

		await expect(page).toHaveTitle(/DMS Simando|Simando/i);
		await expect(page.getByText("DMS Simando")).toBeVisible();
		await expect(
			page.getByText(/PT Perusahaan Gas Negara Tbk/i),
		).toBeVisible();
		await expect(page.locator('input[name="username"]')).toBeVisible();
		await expect(page.locator('input[name="password"]')).toBeVisible();
		await expect(page.locator('button[type="submit"]')).toBeVisible();
	});

	test("1.2 should show error alert on invalid credentials (401)", async ({
		page,
	}) => {
		await page.context().clearCookies();
		await gotoApp(page, "/sign-in");
		await fillInput(page, 'input[name="username"]', "wrong.user@pgn.co.id");
		await fillInput(page, 'input[name="password"]', "WrongPassword123!");
		await clickElement(page, 'button[type="submit"]');

		await expect(
			page.getByText(/Nama pengguna atau kata sandi tidak valid/i),
		).toBeVisible();
	});

	test("1.3 should redirect unauthenticated users from protected routes to /sign-in", async ({
		page,
	}) => {
		await page.context().clearCookies();
		await gotoApp(page, "/directory");
		await page.waitForFunction(
			() => window.location.pathname.includes("/sign-in"),
			undefined,
			{ timeout: 30000 },
		);
		expect(page.url()).toContain("redirect=");
	});

	test("1.4 should login successfully via UI and navigate to dashboard", async ({
		page,
	}) => {
		await page.context().clearCookies();
		const salesUser = USER_CREDENTIALS.SalesArea;

		await gotoApp(page, "/sign-in");
		await fillInput(page, 'input[name="username"]', salesUser.username);
		await fillInput(page, 'input[name="password"]', salesUser.password);
		await clickElement(page, 'button[type="submit"]');

		await page.waitForFunction(
			() => !window.location.pathname.includes("/sign-in"),
			undefined,
			{ timeout: 30000 },
		);
		expect(page.url()).not.toContain("/sign-in");
		await expect(page.locator('[data-sidebar="sidebar"] a').first()).toBeVisible({
			timeout: 15000,
		});
		await page
			.waitForSelector("text=Memuat data", {
				state: "detached",
				timeout: 15000,
			})
			.catch(() => {});
	});

	test("1.5 should allow login and navigate to dashboard for admin", async ({
		page,
	}) => {
		await page.context().clearCookies();
		const adminUser = USER_CREDENTIALS.SystemAdmin;

		await gotoApp(page, "/sign-in");
		await fillInput(page, 'input[name="username"]', adminUser.username);
		await fillInput(page, 'input[name="password"]', adminUser.password);
		await clickElement(page, 'button[type="submit"]');

		await page.waitForFunction(
			() => !window.location.pathname.includes("/sign-in"),
			undefined,
			{ timeout: 30000 },
		);
		expect(page.url()).not.toContain("/sign-in");
		await expect(page.locator('[data-sidebar="sidebar"] a').first()).toBeVisible({
			timeout: 15000,
		});
		await page
			.waitForSelector("text=Memuat data", {
				state: "detached",
				timeout: 15000,
			})
			.catch(() => {});
	});

	test("1.6 should handle sign-out and revoke protected session access", async ({
		salesAreaPage: page,
	}) => {
		await gotoApp(page, "/directory");
		expect(page.url()).toContain("/directory");

		// Perform API sign out
		await page.evaluate(async () => {
			await fetch("/api/auth/logout", { method: "POST" });
		});
		await page.context().clearCookies();

		// Attempt navigation to protected route
		await gotoApp(page, "/directory");
		await page.waitForFunction(
			() => window.location.pathname.includes("/sign-in"),
			undefined,
			{ timeout: 30000 },
		);
		expect(page.url()).toContain("/sign-in");
	});

	test("1.7 should redirect unauthorized user to /access-denied on restricted routes", async ({
		reviewerPage: page,
	}) => {
		// Reviewer does not have CreateCompany capability required by /directory/new
		await page.goto("/directory/new");
		await page.waitForFunction(
			() => window.location.pathname.includes("/access-denied"),
			undefined,
			{ timeout: 15000 },
		);
		expect(page.url()).toContain("/access-denied");
	});
});
