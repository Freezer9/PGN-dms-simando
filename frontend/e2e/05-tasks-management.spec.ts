import {
	clickTab,
	expect,
	fillInput,
	gotoApp,
	test,
} from "./fixtures/auth.fixture";

test.describe("Flow 5: Task Inbox & Management (SLA & Workflows)", () => {
	test("5.1 should render task inbox with scope tabs and filter controls", async ({
		areaHeadPage: page,
	}) => {
		await gotoApp(page, "/tasks");

		await expect(
			page.getByRole("tab", { name: /Tugas Saya/i }),
		).toBeVisible();
		await expect(
			page.getByRole("tab", { name: /Semua Tugas Wilayah/i }),
		).toBeVisible();
		await expect(
			page.getByPlaceholder(/Cari perusahaan, nomor, pengaju/i),
		).toBeVisible();
	});

	test("5.2 should toggle task scope tabs between My Tasks and Regional", async ({
		areaHeadPage: page,
	}) => {
		await gotoApp(page, "/tasks");

		await clickTab(page, "Semua Tugas Wilayah");
		await expect(
			page.getByRole("tab", { name: /Semua Tugas Wilayah/i }),
		).toHaveAttribute("data-state", "active");

		await clickTab(page, "Tugas Saya");
		await expect(
			page.getByRole("tab", { name: /Tugas Saya/i }),
		).toHaveAttribute("data-state", "active");
	});

	test("5.3 should filter task items by search input", async ({
		regionalAdminPage: page,
	}) => {
		await gotoApp(page, "/tasks");

		const searchInput = page.getByPlaceholder(
			/Cari perusahaan, nomor, pengaju/i,
		);
		await expect(searchInput).toBeVisible();
		await fillInput(page, 'input[placeholder*="Cari"]', "PT Test");

		await expect(page.getByText(/Menampilkan/i)).toBeVisible();
	});

	test("5.4 should view blocked tasks list", async ({
		regionalAdminPage: page,
	}) => {
		await gotoApp(page, "/tasks/blocked");

		await expect(
			page.getByText(/Pemantauan Berkas Tertahan/i),
		).toBeVisible();
		await expect(
			page.getByPlaceholder(/Cari berkas tertahan/i),
		).toBeVisible();
	});

	test("5.5 should view task history audit trail", async ({
		regionalAdminPage: page,
	}) => {
		await gotoApp(page, "/tasks/history");

		await expect(
			page.getByText(/Riwayat Keputusan Workflow/i),
		).toBeVisible();
	});
});
