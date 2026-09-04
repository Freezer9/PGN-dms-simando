import {
	advanceCompanyToAreaHead,
	clickElement,
	clickTab,
	createTestCompany,
	expect,
	gotoApp,
	test,
} from "./fixtures/auth.fixture";

test.describe("Flow 4: Multi-Actor Approval Workflow (Stages 6-8)", () => {
	test("4.1 Area Head: should access tasks and view workflow action bar on company hub", async ({
		areaHeadPage: page,
	}) => {
		const company = await advanceCompanyToAreaHead(page);
		await gotoApp(page, `/directory/${company.companyId}`);

		await expect(page.getByText(company.namaPerusahaan)).toBeVisible();
		await expect(
			page.getByText(/Alur Persetujuan|Aksi Alur Kerja|Workflow Gate/i),
		).toBeVisible();
	});

	test("4.2 Regional Admin: should view company hub and access evaluation tab", async ({
		regionalAdminPage: page,
	}) => {
		const company = await createTestCompany(page, undefined, "RegionalAdmin");
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Evaluasi");
		await expect(
			page.getByText(/Resume Evaluasi Kelayakan|Evaluasi/i).first(),
		).toBeVisible();
	});

	test("4.3 Reviewer: should access company hub and review dossier tabs", async ({
		reviewerPage: page,
	}) => {
		const company = await createTestCompany(page, undefined, "Reviewer");
		await gotoApp(page, `/directory/${company.companyId}`);

		await expect(page.getByText(company.namaPerusahaan)).toBeVisible();
		await expect(
			page.getByText(/Tahapan Saat Ini|Tahapan CRM/i),
		).toBeVisible();
	});

	test("4.4 Division Head: should access company hub and view issuance tab", async ({
		divisionHeadPage: page,
	}) => {
		const company = await createTestCompany(page, undefined, "DivisionHead");
		await gotoApp(page, `/directory/${company.companyId}`);

		await clickTab(page, "Penerbitan");
		await expect(
			page.getByText(/Penerbitan Surat|Penerbitan/i).first(),
		).toBeVisible();
	});

	test("4.5 Workflow Gate & Discontinue: should render stop process dialog controls", async ({
		salesAreaPage: page,
	}) => {
		const company = await createTestCompany(page, undefined, "SalesArea");
		await gotoApp(page, `/directory/${company.companyId}`);

		await expect(
			page.getByRole("button", { name: /Hentikan Proses/i }),
		).toBeVisible();
		await clickElement(page, "Hentikan Proses");

		await expect(
			page.getByText(/Hentikan Proses Pelanggan|Discontinue/i),
		).toBeVisible();
	});
});
