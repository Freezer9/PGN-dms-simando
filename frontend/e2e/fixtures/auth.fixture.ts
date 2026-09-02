import { type Page, test as base } from "@playwright/test";

export interface UserCredentials {
	username: string;
	password: string;
	fullName: string;
	role: string;
}

export const USER_CREDENTIALS: Record<string, UserCredentials> = {
	SalesArea: {
		username: "demo.salesarea",
		password: "Correct-Horse-Battery-Staple-1",
		fullName: "Demo Sales Area",
		role: "SalesArea",
	},
	AreaHead: {
		username: "demo.areahead",
		password: "Correct-Horse-Battery-Staple-1",
		fullName: "Demo Area Head",
		role: "AreaHead",
	},
	RegionalAdmin: {
		username: "demo.regionaladmin",
		password: "Correct-Horse-Battery-Staple-1",
		fullName: "Demo Regional Admin",
		role: "RegionalAdmin",
	},
	Reviewer: {
		username: "demo.reviewer",
		password: "Correct-Horse-Battery-Staple-1",
		fullName: "Demo Reviewer",
		role: "Reviewer",
	},
	DivisionHead: {
		username: "demo.divisionhead",
		password: "Correct-Horse-Battery-Staple-1",
		fullName: "Demo Division Head",
		role: "DivisionHead",
	},
	SystemAdmin: {
		username: "admin",
		password: "Correct-Horse-Battery-Staple-1",
		fullName: "System Admin",
		role: "SystemAdmin",
	},
};

/**
 * Navigate to an app path and ensure the React root element is rendered
 */
export async function gotoApp(page: Page, path: string) {
	await page.goto(path);
	await page.waitForSelector("#app > *", { timeout: 15000 });
	if (
		path !== "/sign-in" &&
		!path.startsWith("/sign-in") &&
		path !== "/access-denied"
	) {
		try {
			await page.waitForSelector("aside nav a", { timeout: 4000 });
		} catch {
			// Best effort wait for navigation menu to render
		}
	}
}

/**
 * Fill input or textarea using React property setters and synthetic events
 */
export async function fillInput(page: Page, selector: string, value: string) {
	await page.evaluate(
		({ sel, val }) => {
			const el = document.querySelector(sel) as
				| HTMLInputElement
				| HTMLTextAreaElement
				| null;
			if (!el) throw new Error(`Element ${sel} not found to fill`);
			const proto =
				el instanceof HTMLTextAreaElement
					? HTMLTextAreaElement.prototype
					: HTMLInputElement.prototype;
			const nativeSetter = Object.getOwnPropertyDescriptor(
				proto,
				"value",
			)?.set;
			if (nativeSetter) {
				nativeSetter.call(el, val);
			} else {
				el.value = val;
			}
			el.dispatchEvent(new Event("input", { bubbles: true }));
			el.dispatchEvent(new Event("change", { bubbles: true }));
		},
		{ sel: selector, val: value },
	);
}

/**
 * Safe DOM click helper for Obscura CDP hit testing (dispatches mousedown, mouseup, click)
 */
export async function clickElement(page: Page, selectorOrText: string) {
	await page.evaluate((sel) => {
		let el: HTMLElement | null = null;
		try {
			el = document.querySelector(sel) as HTMLElement | null;
		} catch {
			// In case selectorOrText is not valid CSS selector
		}
		if (!el) {
			const candidates = Array.from(
				document.querySelectorAll("button, a, [role='button'], [role='tab']"),
			) as HTMLElement[];
			el =
				candidates.find((c) =>
					c.textContent?.toLowerCase().includes(sel.toLowerCase()),
				) || null;
		}
		if (!el) throw new Error(`Element matching "${sel}" not found to click`);
		el.dispatchEvent(
			new MouseEvent("mousedown", {
				bubbles: true,
				cancelable: true,
				button: 0,
			}),
		);
		el.dispatchEvent(
			new MouseEvent("mouseup", { bubbles: true, cancelable: true, button: 0 }),
		);
		el.dispatchEvent(
			new MouseEvent("click", { bubbles: true, cancelable: true, button: 0 }),
		);
		if (typeof el.click === "function") {
			el.click();
		}
	}, selectorOrText);
}

/**
 * Click a tab element by label (handles Radix UI mousedown activation)
 */
export async function clickTab(page: Page, tabName: string) {
	await page.evaluate((name) => {
		const tabs = Array.from(
			document.querySelectorAll('[role="tab"]'),
		) as HTMLElement[];
		const target = tabs.find((t) =>
			t.textContent?.toLowerCase().includes(name.toLowerCase()),
		);
		if (!target) throw new Error(`Tab matching "${name}" not found`);
		target.dispatchEvent(
			new MouseEvent("mousedown", { bubbles: true, cancelable: true, button: 0 }),
		);
		target.dispatchEvent(
			new MouseEvent("mouseup", { bubbles: true, cancelable: true, button: 0 }),
		);
		target.dispatchEvent(
			new MouseEvent("click", { bubbles: true, cancelable: true, button: 0 }),
		);
		if (typeof target.click === "function") {
			target.click();
		}
	}, tabName);
}

/**
 * Authenticate session via backend API and store session cookie
 */
export async function authenticateSession(
	page: Page,
	role: keyof typeof USER_CREDENTIALS,
) {
	const creds = USER_CREDENTIALS[role];
	await page.goto("/sign-in");
	await page.waitForSelector("#app > *");
	await page.evaluate(
		async ({ u, p }) => {
			const res = await fetch("/api/auth/login", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({ username: u, password: p }),
			});
			if (!res.ok) {
				throw new Error(`Login failed for ${u}: status ${res.status}`);
			}
		},
		{ u: creds.username, p: creds.password },
	);
}

/**
 * Perform UI login via sign-in form
 */
export async function loginViaUi(
	page: Page,
	username: string,
	password: string = "Correct-Horse-Battery-Staple-1",
) {
	await gotoApp(page, "/sign-in");
	await fillInput(page, 'input[name="username"]', username);
	await fillInput(page, 'input[name="password"]', password);
	await clickElement(page, 'button[type="submit"]');
	await page.waitForFunction(
		() => !window.location.pathname.includes("/sign-in"),
		undefined,
		{ timeout: 15000 },
	);
	try {
		await page.waitForSelector("aside nav a", { timeout: 5000 });
		await page.waitForSelector("text=Memuat data", {
			state: "detached",
			timeout: 5000,
		});
	} catch {
		// Best effort wait for dashboard data to settle
	}
}

/**
 * Creates a valid company record via backend API in browser context
 */
export async function createTestCompany(
	page: Page,
	overrides?: Partial<{
		namaPerusahaan: string;
		industryTypeId: string;
		areaId: string;
		villageId: string;
		alamat: string;
	}>,
	reauthRole?: keyof typeof USER_CREDENTIALS,
) {
	const result = await page.evaluate(async (custom) => {
		// Log in as demo.salesarea to ensure company creation permission
		await fetch("/api/auth/login", {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify({
				username: "demo.salesarea",
				password: "Correct-Horse-Battery-Staple-1",
			}),
		});

		const indRes = await fetch("/api/master/industry-types");
		const industries = await indRes.json();
		const areaRes = await fetch("/api/master/areas");
		const areas = await areaRes.json();
		const provRes = await fetch("/api/geography/provinces");
		const provinces = await provRes.json();
		const regRes = await fetch(
			`/api/geography/regencies?provinceId=${provinces[0].id}`,
		);
		const regencies = await regRes.json();
		const distRes = await fetch(
			`/api/geography/districts?regencyId=${regencies[0].id}`,
		);
		const districts = await distRes.json();
		const vilRes = await fetch(
			`/api/geography/villages?districtId=${districts[0].id}`,
		);
		const villages = await vilRes.json();

		const payload = {
			namaPerusahaan:
				custom?.namaPerusahaan ||
				`PT Test E2E ${Date.now().toString().slice(-6)}`,
			industryTypeId: custom?.industryTypeId || industries[0].id,
			areaId: custom?.areaId || areas[0].id,
			villageId: custom?.villageId || villages[0].id,
			alamat: custom?.alamat || "Jl. Industri Raya No. 123, Blok B-4",
			latitude: -6.2088,
			longitude: 106.8456,
			npwp: "01.234.567.8-901.000",
			email: "info@test-company.co.id",
			telp: "021-5551234",
			website: "https://test-company.co.id",
			kodePos: "17530",
		};

		const res = await fetch("/api/companies", {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify(payload),
		});
		if (!res.ok) {
			const err = await res.text();
			throw new Error(`Failed to create test company: ${err}`);
		}
		const data = await res.json();
		return {
			companyId: data.companyId as string,
			nomor: data.nomor as string,
			namaPerusahaan: payload.namaPerusahaan,
		};
	}, overrides);

	if (reauthRole && reauthRole !== "SalesArea") {
		await authenticateSession(page, reauthRole);
	}

	return result;
}

type Fixtures = {
	salesAreaPage: Page;
	areaHeadPage: Page;
	regionalAdminPage: Page;
	reviewerPage: Page;
	divisionHeadPage: Page;
	adminPage: Page;
};

const OBSCURA_PORT = process.env.OBSCURA_PORT || "9222";
const OBSCURA_WS =
	process.env.OBSCURA_WS_ENDPOINT || `ws://127.0.0.1:${OBSCURA_PORT}`;

export const test = base.extend<Fixtures>({
	browser: async ({ playwright }, use) => {
		const browser = await playwright.chromium.connectOverCDP({
			endpointURL: OBSCURA_WS,
		});
		await use(browser);
		await browser.close();
	},
	salesAreaPage: async ({ page }, use) => {
		await authenticateSession(page, "SalesArea");
		await use(page);
	},
	areaHeadPage: async ({ page }, use) => {
		await authenticateSession(page, "AreaHead");
		await use(page);
	},
	regionalAdminPage: async ({ page }, use) => {
		await authenticateSession(page, "RegionalAdmin");
		await use(page);
	},
	reviewerPage: async ({ page }, use) => {
		await authenticateSession(page, "Reviewer");
		await use(page);
	},
	divisionHeadPage: async ({ page }, use) => {
		await authenticateSession(page, "DivisionHead");
		await use(page);
	},
	adminPage: async ({ page }, use) => {
		await authenticateSession(page, "SystemAdmin");
		await use(page);
	},
});

export { expect } from "@playwright/test";
