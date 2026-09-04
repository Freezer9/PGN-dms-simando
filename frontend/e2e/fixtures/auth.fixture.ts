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
	await page.goto(path, { waitUntil: "domcontentloaded" });
	await page.waitForSelector("#app", { state: "attached", timeout: 30000 });
	await page.waitForFunction(
		() => {
			const app = document.getElementById("app");
			return app !== null && app.children.length > 0;
		},
		undefined,
		{ timeout: 45000 },
	);
}

/**
 * Fill input or textarea using React property setters and synthetic events
 */
export async function fillInput(page: Page, selector: string, value: string) {
	await page.waitForSelector(selector, { state: "attached", timeout: 15000 });
	await page.evaluate(
		({ sel, val }) => {
			const el = document.querySelector(sel) as
				| (HTMLInputElement & { _valueTracker?: { setValue: (v: string) => void } })
				| (HTMLTextAreaElement & { _valueTracker?: { setValue: (v: string) => void } })
				| null;
			if (!el) throw new Error(`Element ${sel} not found to fill`);
			if (el._valueTracker) {
				el._valueTracker.setValue(val + "_diff");
			}
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
			el.dispatchEvent(new Event("blur", { bubbles: true }));
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
		if (el instanceof HTMLButtonElement && el.type === "submit" && el.form) {
			el.form.requestSubmit();
		}
	}, selectorOrText);
}

/**
 * Click a tab element by label (handles Radix UI mousedown activation)
 */
export async function clickTab(page: Page, tabName: string) {
	await page.waitForSelector('[role="tab"]', { state: "attached", timeout: 30000 });
	await page.evaluate((name) => {
		const tabs = Array.from(
			document.querySelectorAll('[role="tab"]'),
		) as HTMLElement[];
		const target = tabs.find((t) =>
			t.textContent?.toLowerCase().includes(name.toLowerCase()),
		);
		if (!target) throw new Error(`Tab matching "${name}" not found`);
		target.focus();
		target.dispatchEvent(
			new MouseEvent("mousedown", {
				bubbles: true,
				cancelable: true,
				button: 0,
				view: window,
			}),
		);
		target.dispatchEvent(
			new MouseEvent("mouseup", {
				bubbles: true,
				cancelable: true,
				button: 0,
				view: window,
			}),
		);
		target.dispatchEvent(
			new MouseEvent("click", {
				bubbles: true,
				cancelable: true,
				button: 0,
				view: window,
			}),
		);
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
	await page.context().clearCookies();
	const res = await page.request.post("/api/auth/login", {
		data: { username: creds.username, password: creds.password },
	});
	if (!res.ok()) {
		throw new Error(`Login failed for ${creds.username}: status ${res.status()}`);
	}
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
		await page.waitForSelector('[data-sidebar="sidebar"] a', { timeout: 5000 });
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
	// Log in as demo.salesarea to ensure company creation permission
	const loginRes = await page.request.post("/api/auth/login", {
		data: {
			username: "demo.salesarea",
			password: "Correct-Horse-Battery-Staple-1",
		},
	});
	if (!loginRes.ok()) {
		throw new Error(`Login failed for demo.salesarea: ${loginRes.status()}`);
	}

	const indRes = await page.request.get("/api/master/industry-types");
	const industries = await indRes.json();
	const areaRes = await page.request.get("/api/master/areas");
	const areas = await areaRes.json();
	const demoArea = areas.find((a: any) => a.code === "DEMO") || areas[0];
	const provRes = await page.request.get("/api/geography/provinces");
	const provinces = await provRes.json();
	const regRes = await page.request.get(
		`/api/geography/regencies?provinceId=${provinces[0].id}`,
	);
	const regencies = await regRes.json();
	const distRes = await page.request.get(
		`/api/geography/districts?regencyId=${regencies[0].id}`,
	);
	const districts = await distRes.json();
	const vilRes = await page.request.get(
		`/api/geography/villages?districtId=${districts[0].id}`,
	);
	const villages = await vilRes.json();

	const payload = {
		namaPerusahaan:
			overrides?.namaPerusahaan ||
			`PT Test E2E ${Date.now().toString().slice(-6)}`,
		industryTypeId: overrides?.industryTypeId || industries[0].id,
		areaId: overrides?.areaId || demoArea.id,
		villageId: overrides?.villageId || villages[0].id,
		alamat: overrides?.alamat || "Jl. Industri Raya No. 123, Blok B-4",
		latitude: -6.2088,
		longitude: 106.8456,
		npwp: "01.234.567.8-901.000",
		email: "info@test-company.co.id",
		telp: "021-5551234",
		website: "https://test-company.co.id",
		kodePos: "17530",
	};

	const res = await page.request.post("/api/companies", {
		data: payload,
	});
	if (!res.ok()) {
		const err = await res.text();
		throw new Error(`Failed to create test company: ${err}`);
	}
	const data = await res.json();

	if (reauthRole && reauthRole !== "SalesArea") {
		await authenticateSession(page, reauthRole);
	}

	return {
		companyId: data.companyId as string,
		nomor: data.nomor as string,
		namaPerusahaan: payload.namaPerusahaan,
	};
}

/**
 * Setup plotting configuration and promote company to stage 3 (Prospek)
 */
export async function setupPlottingForCompany(page: Page, companyId: string) {
	const salesUsersRes = await page.request.get("/api/master/sales-users");
	const salesUsers = await salesUsersRes.json();
	const salesUser = salesUsers[0] || {
		id: "00000000-0000-0000-0000-000000000001",
	};

	await page.request.put(`/api/companies/${companyId}/plotting`, {
		data: {
			salesUserId: salesUser.id,
			posisiPelanggan: "JalurExisting",
			kawasan: "KawasanIndustri",
		},
	});

	await page.request.post(`/api/companies/${companyId}/promote-to-prospek`);
}

/**
 * Add a primary contact for the company
 */
export async function addContactForCompany(
	page: Page,
	companyId: string,
	contact?: Partial<{
		nama: string;
		jabatan: string;
		email: string;
		noHp: string;
	}>,
) {
	await page.request.post(`/api/companies/${companyId}/contacts`, {
		data: {
			nama: contact?.nama || "Budi Santoso",
			jabatan: contact?.jabatan || "Manager Operasional",
			email: contact?.email || "budi.santoso@company.co.id",
			noHp: contact?.noHp || "081234567890",
			isPrimary: true,
		},
	});
}

/**
 * Save dummy survey for company to advance to Stage 4 (Survey completed)
 */
export async function completeSurveyForCompany(page: Page, companyId: string) {
	const res = await page.request.put(`/api/companies/${companyId}/survey`, {
		data: {
			request: {
				tanggalSurvey: "2026-09-01",
				jumlahKaryawan: 50,
			},
			products: [],
			rawMaterials: [],
			markets: [],
			equipment: [],
		},
	});
	if (!res.ok()) {
		throw new Error(
			`Save survey failed: ${res.status()} ${await res.text()}`,
		);
	}
}

/**
 * Save dummy registration for company to advance to Stage 5 (A1 Registration completed)
 */
export async function completeRegistrationForCompany(
	page: Page,
	companyId: string,
) {
	const res = await page.request.put(
		`/api/companies/${companyId}/registration`,
		{
			data: {
				tanggalRegistrasi: "2026-09-01",
				namaPenanggungJawab: "Budi Santoso",
				skemaHarga: "Reguler",
				usagePeriods: [],
			},
		},
	);
	if (!res.ok()) {
		throw new Error(
			`Save registration failed: ${res.status()} ${await res.text()}`,
		);
	}
}

/**
 * Upload a dummy attachment for the company
 */
export async function uploadCompanyAttachment(
	page: Page,
	companyId: string,
	kind: string,
) {
	const dummyPdfBuffer = Buffer.from(
		"%PDF-1.4\n1 0 obj<</Type/Catalog>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF",
	);
	const res = await page.request.post(
		`/api/companies/${companyId}/attachments`,
		{
			multipart: {
				file: {
					name: `${kind.toLowerCase()}-doc.pdf`,
					mimeType: "application/pdf",
					buffer: dummyPdfBuffer,
				},
				kind: kind,
				signatureMethod: "Digital",
			},
		},
	);
	if (!res.ok()) {
		throw new Error(
			`Upload failed for ${kind}: ${res.status()} ${await res.text()}`,
		);
	}
}

/**
 * Save dummy NOL request for company to advance to Stage 6 (NOL Request completed)
 */
export async function completeNolRequestForCompany(
	page: Page,
	companyId: string,
) {
	const res = await page.request.put(`/api/companies/${companyId}/nol-request`, {
		data: {
			nomorNotaDinas: "ND-001/TEST/2026",
			registrationType: "RegistrasiBaru",
			samaDenganA1: true,
			periods: [],
			dailyBasisRows: [],
			referenceDocumentIds: [],
		},
	});
	if (!res.ok()) {
		throw new Error(
			`Save NOL request failed: ${res.status()} ${await res.text()}`,
		);
	}
}

/**
 * Advance company through stages 1-6 and submit to workflow (Area Head step)
 */
export async function advanceCompanyToAreaHead(page: Page) {
	const company = await createTestCompany(page);
	await setupPlottingForCompany(page, company.companyId);
	await addContactForCompany(page, company.companyId);
	await completeSurveyForCompany(page, company.companyId);
	await uploadCompanyAttachment(page, company.companyId, "Kk0");
	await completeRegistrationForCompany(page, company.companyId);
	await uploadCompanyAttachment(page, company.companyId, "A1");
	await uploadCompanyAttachment(page, company.companyId, "BuktiKelayakan");
	await uploadCompanyAttachment(page, company.companyId, "CapexPreGr3");
	await completeNolRequestForCompany(page, company.companyId);

	const submitRes = await page.request.post(
		`/api/companies/${company.companyId}/workflow/start`,
	);
	if (!submitRes.ok()) {
		throw new Error(
			`Failed to start workflow: ${submitRes.status()} ${await submitRes.text()}`,
		);
	}

	await authenticateSession(page, "AreaHead");
	return company;
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
			endpointURL: `${OBSCURA_WS}/devtools/browser`,
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
