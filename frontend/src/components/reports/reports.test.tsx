import { fireEvent, render, screen } from "@testing-library/react";
import type * as React from "react";
import { describe, expect, it, vi } from "vitest";
import { ReportLayout } from "./report-layout";

vi.mock("@tanstack/react-router", () => ({
	Link: ({
		children,
		to,
		...props
	}: {
		children: React.ReactNode;
		to?: string;
		[key: string]: unknown;
	}) => (
		<a href={typeof to === "string" ? to : "#"} {...props}>
			{children}
		</a>
	),
}));

describe("Reports Components", () => {
	describe("ReportLayout", () => {
		it("renders header, title, description, and export button", () => {
			render(
				<ReportLayout
					title="Laporan Uji Coba"
					description="Deskripsi laporan pengujian"
					exportEndpoint="/api/reports/export/test"
					exportFileName="Test_Export.xlsx"
				>
					<div data-testid="report-content">Konten Laporan</div>
				</ReportLayout>,
			);

			expect(screen.getAllByText("Laporan Uji Coba").length).toBeGreaterThan(0);
			expect(screen.getByText("Deskripsi laporan pengujian")).toBeDefined();
			expect(screen.getByTestId("report-content")).toBeDefined();
			expect(screen.getByText("Unduh Excel (.xlsx)")).toBeDefined();
		});

		it("handles reset filter button callback when provided", () => {
			const handleReset = vi.fn();
			render(
				<ReportLayout
					title="Laporan Filter"
					description="Deskripsi filter"
					exportEndpoint="/api/reports/export/filter"
					exportFileName="Filter.xlsx"
					filterContent={<div>Filter Elements</div>}
					onResetFilters={handleReset}
				>
					<div>Content</div>
				</ReportLayout>,
			);

			expect(screen.getByText("Filter Elements")).toBeDefined();
			const resetButton = screen.getByText("Reset Filter");
			fireEvent.click(resetButton);
			expect(handleReset).toHaveBeenCalledOnce();
		});

		it("triggers fetch on Excel download button click", async () => {
			const fetchMock = vi.fn().mockResolvedValue({
				ok: true,
				blob: vi.fn().mockResolvedValue(new Blob(["test-data"])),
			});
			global.fetch = fetchMock;
			global.URL.createObjectURL = vi
				.fn()
				.mockReturnValue("blob:http://localhost/dummy");
			global.URL.revokeObjectURL = vi.fn();

			render(
				<ReportLayout
					title="Laporan Excel"
					description="Download test"
					exportEndpoint="/api/reports/export/funnel"
					exportFileName="Funnel.xlsx"
				>
					<div>Table Content</div>
				</ReportLayout>,
			);

			const downloadButton = screen.getByText("Unduh Excel (.xlsx)");
			fireEvent.click(downloadButton);

			expect(fetchMock).toHaveBeenCalledWith("/api/reports/export/funnel", {
				credentials: "include",
			});
		});
	});
});
