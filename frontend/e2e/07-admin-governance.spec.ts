import { clickElement, expect, gotoApp, test } from "./fixtures/auth.fixture";

test.describe("Flow 7: Admin Governance & Emergency Operations", () => {
	test("7.1 Break-Glass: should display audit log and open emergency access dialog", async ({
		adminPage: page,
	}) => {
		await gotoApp(page, "/admin/break-glass");

		await expect(
			page.getByText(/Akses Darurat \(Break-Glass Protocol\)/i),
		).toBeVisible();
		await expect(
			page.getByRole("button", { name: /Buka Akses Darurat/i }),
		).toBeVisible();

		await clickElement(page, "Buka Akses Darurat");

		await expect(
			page.getByText(/Permohonan Akses Darurat \(Break-Glass\)/i),
		).toBeVisible();
	});

	test("7.2 Stuck Steps: should view and inspect stuck steps monitor", async ({
		adminPage: page,
	}) => {
		await gotoApp(page, "/admin/stuck-steps");

		await expect(
			page.getByText(/Langkah Tertahan — Lintas Wilayah/i),
		).toBeVisible();
		await expect(
			page.getByPlaceholder(/Cari perusahaan, wilayah, peran/i),
		).toBeVisible();
	});
});
