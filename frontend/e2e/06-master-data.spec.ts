import { expect, fillInput, gotoApp, test } from "./fixtures/auth.fixture";

test.describe("Flow 6: Master Data Management", () => {
	test("6.1 should display Organisation (Regions & Areas)", async ({
		adminPage: page,
	}) => {
		await gotoApp(page, "/master/organisation");

		await expect(
			page.getByText(/Struktur Wilayah & Sales Area/i),
		).toBeVisible();
		await expect(
			page.getByRole("button", { name: /Tambah Wilayah/i }),
		).toBeVisible();
	});

	test("6.2 should display Master Data Countries with search", async ({
		adminPage: page,
	}) => {
		await gotoApp(page, "/master/countries");

		await expect(
			page.getByRole("heading", { name: /Daftar Negara/i }),
		).toBeVisible();
		await fillInput(page, 'input[placeholder="Cari data..."]', "Indonesia");
		await expect(page.getByText("Indonesia")).toBeVisible();
		await expect(page.getByText("ID")).toBeVisible();
	});

	test("6.3 should display Master Data Industry Types", async ({
		adminPage: page,
	}) => {
		await gotoApp(page, "/master/industry-types");

		await expect(
			page.getByRole("heading", { name: /Jenis Industri/i }),
		).toBeVisible();
		await expect(page.getByText("Bahan Tekstil")).toBeVisible();
	});

	test("6.4 should display Master Data Fuel Types & Units", async ({
		adminPage: page,
	}) => {
		await gotoApp(page, "/master/fuel-types");
		await expect(
			page.getByRole("heading", { name: /Jenis Bahan Bakar/i }),
		).toBeVisible();

		await gotoApp(page, "/master/units");
		await page.waitForURL("**/master/units**");
		await expect(
			page.getByRole("heading", { name: /Satuan Pengukuran/i }),
		).toBeVisible();
	});

	test("6.5 should display User Directory and Role Assignments", async ({
		adminPage: page,
	}) => {
		await gotoApp(page, "/master/users");

		await expect(
			page.getByRole("heading", { name: /Manajemen Pengguna & Hak Akses/i }),
		).toBeVisible();
		await expect(
			page.getByPlaceholder(/Cari nama, peran, email/i),
		).toBeVisible();
		await expect(
			page.getByRole("button", { name: /Tambah Pengguna/i }),
		).toBeVisible();
	});
});
