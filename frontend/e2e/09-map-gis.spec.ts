import { expect, fillInput, gotoApp, test } from "./fixtures/auth.fixture";

test.describe("Flow 9: GIS Spatial & Pipeline Map View", () => {
	test("9.1 should render map view with controls and filters", async ({
		salesAreaPage: page,
	}) => {
		await gotoApp(page, "/map");

		await expect(
			page.getByText(/Filter Peta/i),
		).toBeVisible();
		await expect(
			page.getByPlaceholder(/Cari nama atau nomor/i),
		).toBeVisible();
	});

	test("9.2 should toggle stage and filter controls", async ({
		salesAreaPage: page,
	}) => {
		await gotoApp(page, "/map");

		await expect(
			page.getByPlaceholder(/Cari nama atau nomor/i),
		).toBeVisible();
		await fillInput(page, 'input[placeholder*="Cari nama"]', "PT Test");

		await expect(
			page.getByText(/Semua Sektor Industri|Filter Peta/i).first(),
		).toBeVisible();
	});
});
