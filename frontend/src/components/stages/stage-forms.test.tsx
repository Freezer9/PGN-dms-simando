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
	const queryClient = createTestQueryClient();
	return render(
		<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
	);
}

describe("Stage Forms and Workflow Components", () => {
	it("renders SurveyKk0Form with default sections", () => {
		renderWithClient(
			<SurveyKk0Form
				companyId="00000000-0000-0000-0000-000000000001"
				canEdit={true}
			/>,
		);

		expect(
			screen.getByText(/Formulir Survei Calon Pelanggan \(KK0/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/1\. Data Pelaksanaan Survei & Jam Kerja Operasional/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/2\. Kebutuhan Energi & Kedekatan Jalur Pipa/i),
		).toBeInTheDocument();
		expect(
			screen.getAllByRole("button", { name: /Simpan Data Survei KK0/i }).length,
		).toBeGreaterThan(0);
	});

	it("renders A1RegistrationForm with customer subscription fields", () => {
		renderWithClient(
			<A1RegistrationForm
				companyId="00000000-0000-0000-0000-000000000001"
				canEdit={true}
			/>,
		);

		expect(
			screen.getByText(/Formulir Berlangganan Gas Bumi \(Formulir A1\)/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/1\. Data Kontak Person in Charge \(PIC\)/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/3\. Periode & Profil Pemakaian Gas/i),
		).toBeInTheDocument();
		expect(
			screen.getAllByRole("button", { name: /Simpan Formulir A1/i }).length,
		).toBeGreaterThan(0);
	});

	it("renders NolRequestForm with Capex and connection costs", () => {
		renderWithClient(
			<NolRequestForm
				companyId="00000000-0000-0000-0000-000000000001"
				canEdit={true}
				canSubmit={true}
			/>,
		);

		expect(
			screen.getByText(
				/Permohonan Surat Kesiapan Gas \/ Notice of Letter \(NOL\)/i,
			),
		).toBeInTheDocument();
		expect(
			screen.getByText(/2\. Rincian Biaya Penyambungan Gas/i),
		).toBeInTheDocument();
		expect(
			screen.getAllByRole("button", { name: /Simpan Permohonan NOL/i }).length,
		).toBeGreaterThan(0);
		expect(
			screen.getByRole("button", { name: /Ajukan ke Evaluasi NOL/i }),
		).toBeInTheDocument();
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
			screen.getByText(/Evaluasi Teknis & Komersial Surat NOL/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/1\. Status FEED & Spesifikasi Jaringan Pipa/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(
				/4\. Analisis Kelayakan Finansial & Skenario Investasi/i,
			),
		).toBeInTheDocument();
		expect(
			screen.getByRole("button", { name: /Tetapkan Reviewer/i }),
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
			screen.getByText(/Penerbitan Surat Kesiapan Pasokan Gas \(Surat NOL\)/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/1\. Keputusan Akhir & Administrasi Surat Resmi/i),
		).toBeInTheDocument();
		expect(
			screen.getByText(/2\. Ketentuan Volume Gas yang Disetujui/i),
		).toBeInTheDocument();
		expect(
			screen.getAllByRole("button", { name: /Simpan Penerbitan NOL/i }).length,
		).toBeGreaterThan(0);
	});

	it("renders WorkflowActionBar when user has action rights", () => {
		const mockCompany: CompanyRecordDto = {
			id: "00000000-0000-0000-0000-000000000001",
			name: "PT Industri Gas Jaya",
			businessSector: "Manufacturing",
			address: "Jl. Industri No. 12",
			latitude: -6.2,
			longitude: 106.8,
			stage: 6,
			status: "NeedReview",
			salesRepName: "Sales User",
			createdAt: "2026-08-28T00:00:00Z",
			userCanAct: true,
			userCanSubmit: false,
			userCanChooseReviewers: false,
			userCanRework: true,
			userCanDiscontinue: true,
			activeWorkflowStep: {
				id: "step-1",
				instanceId: "inst-1",
				stepType: "Review",
				stepName: "Review Teknis",
				status: "Pending",
			},
		};

		renderWithClient(<WorkflowActionBar company={mockCompany} />);

		expect(
			screen.getByText(/Aksi Alur Kerja \(Workflow Gate\)/i),
		).toBeInTheDocument();
		expect(screen.getByText(/Tahap: Review Teknis/i)).toBeInTheDocument();
		expect(
			screen.getByRole("button", { name: /Setujui Langkah/i }),
		).toBeInTheDocument();
		expect(
			screen.getByRole("button", { name: /Minta Revisi/i }),
		).toBeInTheDocument();
		expect(screen.getByRole("button", { name: /Tolak/i })).toBeInTheDocument();
	});
});
