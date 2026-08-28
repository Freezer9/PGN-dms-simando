import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { Activity } from "lucide-react";
import type * as React from "react";
import { describe, expect, it, vi } from "vitest";
import type {
	ApproverDashboardDto,
	RegionalAdminDashboardDto,
	SalesAreaDashboardDto,
	SystemAdminDashboardDto,
} from "@/api/types";
import { ApproverDashboard } from "./approver-dashboard";
import { RegionalAdminDashboard } from "./regional-admin-dashboard";
import { SalesAreaDashboard } from "./sales-area-dashboard";
import { StatTile } from "./stat-tile";
import { SystemAdminDashboard } from "./system-admin-dashboard";

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

// Mock leaflet/maplibre component to avoid canvas rendering in jsdom
vi.mock("./dashboard-map-preview", () => ({
	DashboardMapPreview: () => (
		<div data-testid="mock-map-preview">Map Preview</div>
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

describe("Dashboard Components", () => {
	describe("StatTile", () => {
		it("renders label, value, subtext, and custom variant", () => {
			render(
				<StatTile
					title="Total Pipeline"
					value={42}
					description="Perusahaan aktif"
					icon={Activity}
					variant="emerald"
				/>,
			);

			expect(screen.getByText("Total Pipeline")).toBeDefined();
			expect(screen.getByText("42")).toBeDefined();
			expect(screen.getByText("Perusahaan aktif")).toBeDefined();
		});

		it("renders warning variant with alert tone", () => {
			render(
				<StatTile
					title="Perlu Tindakan"
					value={3}
					description="Perlu revisi data"
					icon={Activity}
					variant="amber"
				/>,
			);

			expect(screen.getByText("Perlu Tindakan")).toBeDefined();
			expect(screen.getByText("3")).toBeDefined();
		});
	});

	describe("SalesAreaDashboard", () => {
		const mockSalesData: SalesAreaDashboardDto = {
			returnedWorkItems: [
				{
					companyId: "c1",
					companyNomor: "1-35-001",
					companyName: "PT Maju Bersama",
					action: "Revisi",
					returnReason: "Lengkapi data tekanan operasi gas.",
					actorRoleLabel: "Area Head Surabaya",
					returnedAt: new Date().toISOString(),
				},
			],
			stageCounts: {
				"1": 5,
				"2": 3,
				"3": 2,
				"4": 1,
				"5": 0,
				"6": 1,
				"7": 0,
				"8": 4,
			},
			activeApprovalItems: [
				{
					companyId: "c2",
					companyNomor: "1-35-002",
					companyName: "PT Sejahtera Abadi",
					currentStage: 6,
					holderLabel: "Reviewer Teknis",
					submittedAt: new Date().toISOString(),
				},
			],
		};

		it("renders action callout for returned items with reviewer comments", () => {
			renderWithClient(
				<SalesAreaDashboard data={mockSalesData} areaName="Surabaya" />,
			);

			expect(screen.getAllByText(/Perlu Tindakan Anda/).length).toBeGreaterThan(
				0,
			);
			expect(screen.getByText("PT Maju Bersama")).toBeDefined();
			expect(
				screen.getByText(/Lengkapi data tekanan operasi gas/),
			).toBeDefined();
		});

		it("renders 8-stage interactive pipeline buttons", () => {
			renderWithClient(<SalesAreaDashboard data={mockSalesData} />);

			expect(screen.getByText("Pipeline Penjualan Saya")).toBeDefined();
			expect(screen.getByText("Survei KK0")).toBeDefined();
			expect(screen.getByText("Permohonan NOL")).toBeDefined();
		});

		it("renders active approval items table", () => {
			renderWithClient(<SalesAreaDashboard data={mockSalesData} />);

			expect(screen.getByText("PT Sejahtera Abadi")).toBeDefined();
			expect(screen.getByText("Reviewer Teknis")).toBeDefined();
		});
	});

	describe("ApproverDashboard", () => {
		const mockApproverData: ApproverDashboardDto = {
			pendingApprovals: [
				{
					companyId: "c3",
					companyNomor: "1-35-003",
					companyName: "PT Industri Makmur",
					stage: 6,
					submittedByName: "Budi Sales",
					waitingSince: new Date().toISOString(),
				},
			],
			totalActiveRecords: 20,
			totalPendingApprovals: 1,
			nolIssuedThisMonth: 5,
			performance: {
				averageTurnaroundDays: 2.4,
				approvedThisMonth: 15,
				revisedThisMonth: 2,
			},
			recentActivity: [
				{
					occurredAt: new Date().toISOString(),
					actorName: "Agus Approver",
					action: "Setuju",
					companyName: "PT Gas Lestari",
					nextHolderLabel: "Reviewer Teknis",
				},
			],
		};

		it("renders pending approval queue and performance metrics", () => {
			renderWithClient(
				<ApproverDashboard data={mockApproverData} roleTitle="Area Head" />,
			);

			expect(screen.getByText("PT Industri Makmur")).toBeDefined();
			expect(
				screen.getAllByText(/Menunggu Persetujuan Anda/).length,
			).toBeGreaterThan(0);
			expect(screen.getByText(/2.4 hari/)).toBeDefined(); // avg turnaround days
			expect(screen.getByText("PT Gas Lestari")).toBeDefined();
		});
	});

	describe("RegionalAdminDashboard", () => {
		const mockRegionalData: RegionalAdminDashboardDto = {
			stuckTasks: [
				{
					companyId: "c5",
					companyNomor: "1-35-005",
					companyName: "PT Terhenti Lama",
					reason: "Menunggu persetujuan lebih dari 7 hari.",
					waitingDays: 9,
				},
			],
			pendingMyActionCount: 4,
			totalWaitingActionCount: 8,
			regionFunnelCounts: {
				"1": 10,
				"2": 8,
				"3": 5,
				"4": 4,
				"5": 3,
				"6": 2,
				"7": 1,
				"8": 1,
			},
			oldestWaitingItem: {
				companyId: "c5",
				companyNomor: "1-35-005",
				companyName: "PT Terhenti Lama",
				waitingDays: 9,
			},
		};

		it("renders stuck tasks alert and oldest waiting record banner", () => {
			renderWithClient(
				<RegionalAdminDashboard
					data={mockRegionalData}
					regionName="Region 3"
				/>,
			);

			expect(screen.getByText(/Berkas Tertahan Terlama/)).toBeDefined();
			expect(screen.getAllByText(/PT Terhenti Lama/).length).toBeGreaterThan(0);
			expect(screen.getAllByText(/9 hari/).length).toBeGreaterThan(0);
		});
	});

	describe("SystemAdminDashboard", () => {
		const mockSystemData: SystemAdminDashboardDto = {
			healthItems: [
				{
					key: "gsize",
					title: "Ukuran Meter (G-Size)",
					isHealthy: true,
					description: "8 G-Size aktif terkonfigurasi.",
				},
				{
					key: "refdocs",
					title: "Dokumen Acuan",
					isHealthy: false,
					description: "Belum ada dokumen acuan terdaftar.",
				},
			],
			activeUsersCount: 24,
			activeRegionsCount: 3,
			activeAreasCount: 12,
			documentTemplatesCount: 5,
		};

		it("renders master data health checklist and summary counters", () => {
			renderWithClient(<SystemAdminDashboard data={mockSystemData} />);

			expect(
				screen.getByText("Status Kelengkapan & Kesehatan Master Data"),
			).toBeDefined();
			expect(screen.getByText("Ukuran Meter (G-Size)")).toBeDefined();
			expect(
				screen.getByText("Belum ada dokumen acuan terdaftar."),
			).toBeDefined();
			expect(screen.getByText("24")).toBeDefined(); // activeUsersCount
		});
	});
});
