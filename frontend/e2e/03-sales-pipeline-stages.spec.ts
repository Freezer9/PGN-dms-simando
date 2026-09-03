import {
	addContactForCompany,
	clickTab,
	completeRegistrationForCompany,
	completeSurveyForCompany,
	createTestCompany,
	expect,
	gotoApp,
	setupPlottingForCompany,
	test,
	uploadCompanyAttachment,
} from "./fixtures/auth.fixture";

test.describe("Flow 3: Sales Pipeline Stages 2-6 (Stage Gates & Sales Area Entry)", () => {
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

	test("3.2 Stage Gate: should show locked state on Prospek tab when plotting is unconfigured", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Prospek");

		await expect(
			page.getByText(/Tahap 3: Prospek.*Belum Terbuka/i),
		).toBeVisible();
		await expect(
			page.getByText(/Lengkapi konfigurasi Plotting/i),
		).toBeVisible();
	});

	test("3.3 Stage 3 Prospek: should render contact management when plotting is configured", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await setupPlottingForCompany(page, company.companyId);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Prospek");

		await expect(
			page.getByText("Daftar Kontak PIC Pelanggan"),
		).toBeVisible();
		await expect(
			page.getByRole("button", { name: /Tambah Kontak/i }),
		).toBeVisible();
	});

	test("3.4 Stage Gate: should show locked state on Survei KK0 when no contacts exist", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await setupPlottingForCompany(page, company.companyId);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Survei KK0");

		await expect(
			page.getByText(/Tahap 4: Survei KK0 Belum Terbuka/i),
		).toBeVisible();
		await expect(
			page.getByText(/Tambahkan minimal 1 kontak PIC/i),
		).toBeVisible();
	});

	test("3.5 Stage 4 Survei KK0: should render survey tab when contact is registered", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await setupPlottingForCompany(page, company.companyId);
		await addContactForCompany(page, company.companyId);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Survei KK0");

		await expect(
			page.getByText(/Data Pelaksanaan Survei/i).first(),
		).toBeVisible();
	});

	test("3.6 Stage Gate: should show locked state on Registrasi A1 when KK0 document is missing", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await setupPlottingForCompany(page, company.companyId);
		await addContactForCompany(page, company.companyId);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Registrasi A1");

		await expect(
			page.getByText(/Tahap 5: Registrasi A1 Belum Terbuka/i),
		).toBeVisible();
		await expect(
			page.getByText(/Dokumen KK0 \(Lampiran 10\) bertanda tangan belum diunggah/i),
		).toBeVisible();
	});

	test("3.7 Stage 5 Registrasi A1: should render A1 tab when KK0 document is uploaded", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await setupPlottingForCompany(page, company.companyId);
		await addContactForCompany(page, company.companyId);
		await completeSurveyForCompany(page, company.companyId);
		await uploadCompanyAttachment(page, company.companyId, "Kk0");
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Registrasi A1");

		await expect(
			page.getByText(/Data Penanggung Jawab & Pendaftaran/i).first(),
		).toBeVisible();
	});

	test("3.8 Stage Gate: should show locked state on Permohonan NOL when A1 is missing", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await setupPlottingForCompany(page, company.companyId);
		await addContactForCompany(page, company.companyId);
		await completeSurveyForCompany(page, company.companyId);
		await uploadCompanyAttachment(page, company.companyId, "Kk0");
		await completeRegistrationForCompany(page, company.companyId);
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Permohonan");

		await expect(
			page.getByText(/Tahap 6: Permohonan NOL Belum Terbuka/i),
		).toBeVisible();
		await expect(
			page.getByText(/Dokumen A1 Registrasi \(Lampiran 11\) bertanda tangan belum diunggah/i),
		).toBeVisible();
	});

	test("3.9 Stage 6 Permohonan NOL: should render NOL request tab when A1 and BuktiKelayakan are uploaded", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page);
		await setupPlottingForCompany(page, company.companyId);
		await addContactForCompany(page, company.companyId);
		await completeSurveyForCompany(page, company.companyId);
		await uploadCompanyAttachment(page, company.companyId, "Kk0");
		await completeRegistrationForCompany(page, company.companyId);
		await uploadCompanyAttachment(page, company.companyId, "A1");
		await uploadCompanyAttachment(page, company.companyId, "BuktiKelayakan");
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Permohonan");

		await expect(
			page.getByText(/Data Nota Dinas & Status Registrasi/i).first(),
		).toBeVisible();
	});
});
