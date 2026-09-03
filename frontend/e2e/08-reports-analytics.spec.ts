import { expect, gotoApp, test } from "./fixtures/auth.fixture";

test.describe("Flow 8: Reports & Operational Analytics", () => {
	test("8.1 should render Reports Index hub with navigation cards", async ({
		regionalAdminPage: page,
	}) => {
		await gotoApp(page, "/reports");

		await expect(
			page.getByText(/Corong Penjualan \(Funnel\)/i),
		).toBeVisible();
		await expect(
			page.getByText(/Durasi Proses & Ageing/i),
		).toBeVisible();
		await expect(
			page.getByText(/Potensi Kebutuhan Gas/i),
		).toBeVisible();
		await expect(
			page.getByText(/Produktivitas Survei/i),
		).toBeVisible();
		await expect(
			page.getByText(/Hasil NOL \/ RL/i),
		).toBeVisible();
	});

	test("8.2 should render Sales Funnel report", async ({
		regionalAdminPage: page,
	}) => {
		await gotoApp(page, "/reports/funnel");

		await expect(
			page.getByText(/Visualisasi Konversi Antar Tahap/i),
		).toBeVisible();
	});

	test("8.3 should render Pipeline Ageing report", async ({
		regionalAdminPage: page,
	}) => {
		await gotoApp(page, "/reports/ageing");

		await expect(
			page.getByText(/Laporan Durasi Proses & Ageing/i),
		).toBeVisible();
		await expect(
			page.getByPlaceholder(/Cari perusahaan, nomor register/i),
		).toBeVisible();
	});

	test("8.4 should render Gas Demand Forecast report", async ({
		regionalAdminPage: page,
	}) => {
		await gotoApp(page, "/reports/gas-demand");

		await expect(
			page.getByRole("tab", { name: /Per Tahap/i }),
		).toBeVisible();
	});

	test("8.5 should render NOL Outcomes report (NOL vs RL)", async ({
		regionalAdminPage: page,
	}) => {
		await gotoApp(page, "/reports/nol-outcomes");

		await expect(
			page.getByText(/Surat NOL \(Disetujui\)/i),
		).toBeVisible();
		await expect(
			page.getByText(/Surat RL \(Ditolak\)/i),
		).toBeVisible();
	});

	test("8.6 should render Survey Productivity report", async ({
		regionalAdminPage: page,
	}) => {
		await gotoApp(page, "/reports/survey-productivity");

		await expect(
			page.getByText(/Laporan Produktivitas Survei/i),
		).toBeVisible();
		await expect(
			page.getByPlaceholder(/Cari nama Sales Rep atau Area/i),
		).toBeVisible();
	});
});
