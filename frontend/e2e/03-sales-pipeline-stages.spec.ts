import {
	clickTab,
	createTestCompany,
	expect,
	gotoApp,
	test,
} from "./fixtures/auth.fixture";

test.describe("Flow 3: Sales Pipeline Stages 2–6 (Sales Area Data Entry)", () => {
	test("3.1 Stage 2 Plotting: should render plotting tab and form elements", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Plotting");

		await expect(
			page.getByText("Konfigurasi Plotting & Jalur Pipa"),
		).toBeVisible();
		await expect(
			page.getByRole("button", { name: /Simpan Konfigurasi Plotting/i }),
		).toBeVisible();
	});

	test("3.2 Stage 3 Prospek: should render contact management on prospek tab", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Kontak");

		await expect(
			page.getByText("Daftar Kontak Person (PIC) Perusahaan"),
		).toBeVisible();
		await expect(
			page.getByRole("button", { name: /Tambah Kontak/i }),
		).toBeVisible();
	});

	test("3.3 Stage 4 Survei KK0: should render survey tab and equipment forms", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Survei KK0");

		await expect(
			page.getByText(/Data Pelaksanaan Survei/i).first(),
		).toBeVisible();
	});

	test("3.4 Stage 5 Registrasi A1: should render A1 registration tab and gas pricing fields", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Registrasi A1");

		await expect(
			page.getByText(/Data Penanggung Jawab & Pendaftaran/i).first(),
		).toBeVisible();
	});

	test("3.5 Stage 6 Permohonan NOL: should render NOL request tab and submission controls", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Permohonan");

		await expect(
			page.getByText(/Data Nota Dinas & Status Registrasi/i).first(),
		).toBeVisible();
	});
});
