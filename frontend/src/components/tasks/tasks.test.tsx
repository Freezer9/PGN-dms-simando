import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import type * as React from "react";
import { describe, expect, it, vi } from "vitest";
import type { TaskListItem } from "@/api/types";
import { formatWaitingDuration, SlaClockBadge } from "./sla-clock-badge";
import { TaskActionModal } from "./task-action-modal";
import { TaskQuickPreviewDrawer } from "./task-quick-preview-drawer";

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

function createTestQueryClient() {
	return new QueryClient({
		defaultOptions: {
			queries: {
				retry: false,
			},
		},
	});
}

function renderWithClient(ui: React.ReactElement) {
	const queryClient = createTestQueryClient();
	return render(
		<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
	);
}

const mockTask: TaskListItem = {
	companyId: "00000000-0000-0000-0000-000000000001",
	nomor: "1-35-3578",
	namaPerusahaan: "PT Test Gas Nusantara",
	industryTypeName: "Manufaktur",
	stepId: "00000000-0000-0000-0000-000000000002",
	stepKind: "AreaHead",
	areaName: "Area Surabaya",
	regionName: "Region 3 - Jatim Bali Nusa",
	submittedByName: "Budi Sales",
	waitingSince: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(), // 2 days ago
};

describe("Tasks UI Components", () => {
	describe("SlaClockBadge and Duration Formatter", () => {
		it("correctly identifies normal SLA (< 3 days)", () => {
			const twoDaysAgo = new Date(Date.now() - 2 * 24 * 60 * 60 * 1000);
			const duration = formatWaitingDuration(twoDaysAgo);

			expect(duration.status).toBe("normal");
			expect(duration.days).toBe(2);
			expect(duration.label).toContain("2 hari");
		});

		it("correctly identifies warning SLA (3 to 7 days)", () => {
			const fourDaysAgo = new Date(Date.now() - 4 * 24 * 60 * 60 * 1000);
			const duration = formatWaitingDuration(fourDaysAgo);

			expect(duration.status).toBe("warning");
			expect(duration.days).toBe(4);
			expect(duration.label).toContain("4 hari");
		});

		it("correctly identifies urgent/overdue SLA (> 7 days)", () => {
			const tenDaysAgo = new Date(Date.now() - 10 * 24 * 60 * 60 * 1000);
			const duration = formatWaitingDuration(tenDaysAgo);

			expect(duration.status).toBe("urgent");
			expect(duration.days).toBe(10);
			expect(duration.label).toContain("10 hari");
		});

		it("renders SlaClockBadge with normal status", () => {
			render(
				<SlaClockBadge
					waitingSince={new Date(Date.now() - 2 * 24 * 60 * 60 * 1000)}
				/>,
			);
			expect(screen.getByText(/2 hari/i)).toBeInTheDocument();
		});

		it("renders SlaClockBadge with urgent tertahan label for overdue tasks", () => {
			render(
				<SlaClockBadge
					waitingSince={new Date(Date.now() - 8 * 24 * 60 * 60 * 1000)}
				/>,
			);
			expect(screen.getByText(/8 hari/i)).toBeInTheDocument();
			expect(screen.getByText(/\(Tertahan\)/i)).toBeInTheDocument();
		});
	});

	describe("TaskActionModal", () => {
		it("renders approval modal elements", () => {
			renderWithClient(
				<TaskActionModal
					task={mockTask}
					actionType="Setuju"
					isOpen={true}
					onClose={vi.fn()}
				/>,
			);

			expect(screen.getByText("Konfirmasi Persetujuan")).toBeInTheDocument();
			expect(screen.getByText(/PT Test Gas Nusantara/i)).toBeInTheDocument();
			expect(
				screen.getByRole("button", { name: "Setujui Permohonan" }),
			).toBeInTheDocument();
		});

		it("renders revision modal elements with mandatory notes label", () => {
			renderWithClient(
				<TaskActionModal
					task={mockTask}
					actionType="Revisi"
					isOpen={true}
					onClose={vi.fn()}
				/>,
			);

			expect(screen.getByText("Permintaan Revisi Berkas")).toBeInTheDocument();
			expect(
				screen.getByText(/Catatan Revisi \/ Poin Perbaikan/i),
			).toBeInTheDocument();
			expect(
				screen.getByRole("button", { name: "Minta Revisi" }),
			).toBeInTheDocument();
		});

		it("renders rejection modal elements with warning notice", () => {
			renderWithClient(
				<TaskActionModal
					task={mockTask}
					actionType="Tolak"
					isOpen={true}
					onClose={vi.fn()}
				/>,
			);

			expect(screen.getByText("Konfirmasi Penolakan")).toBeInTheDocument();
			expect(screen.getByText(/Alasan Penolakan/i)).toBeInTheDocument();
			expect(
				screen.getByText(
					/Tindakan ini tidak dapat dibatalkan secara otomatis/i,
				),
			).toBeInTheDocument();
			expect(
				screen.getByRole("button", { name: "Tolak Berkas" }),
			).toBeInTheDocument();
		});
	});

	describe("TaskQuickPreviewDrawer", () => {
		it("renders drawer with basic task information", () => {
			renderWithClient(
				<TaskQuickPreviewDrawer
					task={mockTask}
					isOpen={true}
					onClose={vi.fn()}
					onTakeAction={vi.fn()}
				/>,
			);

			expect(screen.getByText("PT Test Gas Nusantara")).toBeInTheDocument();
			expect(screen.getByText("1-35-3578")).toBeInTheDocument();
			expect(screen.getByText("Budi Sales")).toBeInTheDocument();
			expect(
				screen.getByRole("button", { name: /Setuju/i }),
			).toBeInTheDocument();
			expect(
				screen.getByRole("button", { name: /Revisi/i }),
			).toBeInTheDocument();
			expect(
				screen.getByRole("button", { name: /Tolak/i }),
			).toBeInTheDocument();
		});
	});
});
