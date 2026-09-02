import {
	clickElement,
	createTestCompany,
	expect,
	gotoApp,
	test,
} from "./fixtures/auth.fixture";

test.describe("Flow 2: Company Directory & Registration (Stage 1)", () => {
	test("2.1 should render company directory table with search and filters", async ({
		salesAreaPage: page,
	}) => {
		// Ensure at least one test company exists
		const company = await createTestCompany(page);

		await gotoApp(page, "/directory");

		await expect(
			page.getByRole("heading", { name: /Direktori/i }),
		).toBeVisible();
		await expect(
			page.getByPlaceholder(/Cari nama perusahaan atau nomor/i),
		).toBeVisible();
		await expect(page.getByText(company.namaPerusahaan)).toBeVisible();
		await expect(page.getByText(company.nomor)).toBeVisible();
	});

	test("2.2 should open registration page and validate required fields", async ({
		salesAreaPage: page,
	}) => {
		await gotoApp(page, "/directory/new");

		await expect(
			page.getByRole("heading", { name: /Pendaftaran Calon Pelanggan/i }),
		).toBeVisible();
		await expect(page.getByText("Tahap 1: Calon Pelanggan")).toBeVisible();

		// Submit without required fields
		await clickElement(page, 'button[type="submit"]');

		// Verify form validation indicator
		await expect(
			page.getByText(/Pendaftaran Calon Pelanggan Baru/i),
		).toBeVisible();
	});

	test("2.3 should successfully register a company and navigate to company hub", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page, {
			namaPerusahaan: `PT Petrokimia Jaya E2E ${Date.now().toString().slice(-4)}`,
		});

		await gotoApp(page, `/directory/${company.companyId}`);

		await expect(page.getByText(company.namaPerusahaan)).toBeVisible();
		await expect(page.getByText(company.nomor)).toBeVisible();
		await expect(
			page.getByText(/Aksi Alur Kerja|Menu tindakan/i),
		).toBeVisible();
	});
});
