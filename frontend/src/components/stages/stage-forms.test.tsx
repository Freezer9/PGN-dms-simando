import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import type * as React from "react";
import { describe, expect, it } from "vitest";
import type { CompanyRecordDto } from "@/api/types";
import { A1RegistrationForm } from "./a1-registration-form";
import { NolEvaluationForm } from "./nol-evaluation-form";
import { NolIssuanceForm } from "./nol-issuance-form";
import { NolRequestForm } from "./nol-request-form";
import { SurveyKk0Form } from "./survey-kk0-form";
import { WorkflowActionBar } from "./workflow-action-bar";

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
	const testQueryClient = createTestQueryClient();
	return render(
		<QueryClientProvider client={testQueryClient}>{ui}</QueryClientProvider>,
	);
}

describe("Stage Form Components", () => {
	it("renders SurveyKk0Form with main section headers", () => {
		renderWithClient(
			<SurveyKk0Form
				companyId="00000000-0000-0000-0000-000000000001"
				canEdit={true}
			/>,
		);

		expect(
			screen.getByText(/Formulir Survei Calon Pelanggan/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/1\. Data Pelaksanaan Survei & Jam Kerja Operasional/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/2\. Kebutuhan Energi & Kedekatan Jalur Pipa/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/3\. Rencana Pemanfaatan Gas & Keekonomian/i),
		).toBeInTheDocument();
		expect(
			screen.getAllByRole("button", { name: /Simpan Data Survei KK0/i }).length,
		).toBeGreaterThan(0);
	});

	it("renders A1RegistrationForm with commercial fields", () => {
		renderWithClient(
			<A1RegistrationForm
				companyId="00000000-0000-0000-0000-000000000001"
				canEdit={true}
			/>,
		);

		expect(
			screen.getByText(/Formulir Berlangganan Gas Bumi/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/1\. Data Penanggung Jawab & Pendaftaran/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/2\. Skema Harga, Kontrak & Status Bangunan/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/3\. Periode & Volume Penggunaan Gas/i),
		).toBeInTheDocument();
		expect(
			screen.getAllByRole("button", { name: /Simpan Formulir A1/i }).length,
		).toBeGreaterThan(0);
	});

	it("renders NolRequestForm with financial calculation section", () => {
		renderWithClient(
			<NolRequestForm
				companyId="00000000-0000-0000-0000-000000000001"
				canEdit={true}
				canSubmit={true}
			/>,
		);

		expect(
			screen.getByText(/Formulir Permohonan Surat NOL/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/1\. Data Nota Dinas & Status Registrasi/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/2\. Biaya Penyambungan & Capex Pre-GR3/i),
		).toBeInTheDocument();
		expect(
			screen.getAllByRole("button", { name: /Simpan Permohonan NOL/i }).length,
		).toBeGreaterThan(0);
	});

	it("renders NolEvaluationForm with pipeline and scenario analysis", () => {
		renderWithClient(
			<NolEvaluationForm
				companyId="00000000-0000-0000-0000-000000000001"
				canEdit={true}
				canChooseReviewers={true}
			/>,
		);

		expect(
			screen.getByText(/Resume Evaluasi Kelayakan Calon Pelanggan/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/1\. Status FEED & Status Penganggaran RKAP/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/3\. Analisis Finansial & Skenario Keekonomian Proyek/i),
		).toBeInTheDocument();
		expect(
			screen.getByRole("button", { name: /Pilih Reviewer/i }),
		).toBeInTheDocument();
	});

	it("renders NolIssuanceForm with official outcome options", () => {
		renderWithClient(
			<NolIssuanceForm
				companyId="00000000-0000-0000-0000-000000000001"
				canEdit={true}
			/>,
		);

		expect(
			screen.getByText(
				/Penerbitan Surat Kesiapan Pasokan Gas \(Surat NOL \/ RL\)/i,
			),
		).toBeInTheDocument();
		expect(
			screen.getByText(/1\. Keputusan Akhir & Administrasi Surat Resmi/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/2\. Ketentuan Volume Gas yang Disetujui/i),
		).toBeInTheDocument();
		expect(
			screen.getAllByRole("button", { name: /Simpan Penerbitan/i }).length,
		).toBeGreaterThan(0);
	});

	it("renders WorkflowActionBar when user has action rights", () => {
		const mockCompany: CompanyRecordDto = {
			id: "00000000-0000-0000-0000-000000000001",
			nomor: "REG-2026-001",
			namaPerusahaan: "PT Industri Gas Jaya",
			website: null,
			alamat: "Jl. Industri No. 12",
			villageId: "00000000-0000-0000-0000-000000000002",
			villageName: "Cilincing",
			districtId: "00000000-0000-0000-0000-000000000003",
			districtName: "Cilincing",
			regencyId: "00000000-0000-0000-0000-000000000004",
			regencyName: "Jakarta Utara",
			provinceId: "00000000-0000-0000-0000-000000000005",
			provinceName: "DKI Jakarta",
			locationLabel: "Jakarta Utara, DKI Jakarta",
			industryTypeId: "00000000-0000-0000-0000-000000000006",
			industryTypeName: "Manufacturing",
			npwp: null,
			email: null,
			kodePos: null,
			telp: null,
			areaId: "00000000-0000-0000-0000-000000000007",
			areaName: "Area Jakarta",
			regionId: "00000000-0000-0000-0000-000000000008",
			regionName: "Region Barat",
			currentStage: 6,
			status: "Reviewer1",
			createdBy: "00000000-0000-0000-0000-000000000009",
			salesRepName: "Sales User",
			createdAt: "2026-08-28T00:00:00Z",
			updatedAt: null,
			latitude: -6.2,
			longitude: 106.8,
			holderLabel: "Reviewer",
			holderName: "Tech Reviewer",
			statusSince: "2026-08-28T00:00:00Z",
			currentStepId: "step-1",
			currentStepKind: "Reviewer1",
			workflowInstanceId: "inst-1",
			canSubmit: false,
			canAct: true,
			canChooseReviewers: false,
			contacts: [],
		};

		renderWithClient(<WorkflowActionBar company={mockCompany} />);

		expect(screen.getByText(/TINDAKAN DIPERLUKAN/i)).toBeInTheDocument();
		expect(screen.getByText(/Tahap: Reviewer1/i)).toBeInTheDocument();
		expect(
			screen.getByRole("button", { name: /Setujui Langkah/i }),
		).toBeInTheDocument();
		expect(
			screen.getByRole("button", { name: /Minta Revisi/i }),
		).toBeInTheDocument();
		expect(screen.getByRole("button", { name: /Tolak/i })).toBeInTheDocument();
	});
});
