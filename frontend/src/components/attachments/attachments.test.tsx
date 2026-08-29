import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import type * as React from "react";
import { describe, expect, it, vi } from "vitest";
import type { AttachmentDetail } from "@/api/types";
import {
	DOCUMENT_TYPE_CONFIG,
	DocumentDownloadButton,
	DocumentDownloadDropdown,
} from "@/components/documents/document-download-buttons";
import { AttachmentList } from "./attachment-list";
import {
	ATTACHMENT_KIND_LABELS,
	AttachmentUploadDialog,
} from "./attachment-upload-dialog";

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

const mockAttachments: AttachmentDetail[] = [
	{
		id: "11111111-1111-1111-1111-111111111111",
		companyId: "00000000-0000-0000-0000-000000000001",
		kind: "Kk0",
		filename: "Formulir_Survei_KK0_Signed.pdf",
		sizeBytes: 1048576, // 1 MB
		mimeType: "application/pdf",
		version: 1,
		signatureMethod: "Digital",
		uploadedByName: "Budi Sales",
		uploadedAt: "2026-08-28T10:00:00Z",
	},
	{
		id: "22222222-2222-2222-2222-222222222222",
		companyId: "00000000-0000-0000-0000-000000000001",
		kind: "Npwp",
		filename: "NPWP_Perusahaan.jpg",
		sizeBytes: 524288, // 512 KB
		mimeType: "image/jpeg",
		version: 1,
		signatureMethod: null,
		uploadedByName: "Budi Sales",
		uploadedAt: "2026-08-28T10:05:00Z",
	},
];

// Mock API client query
vi.mock("@/api/client", () => ({
	$api: {
		useQuery: (_method: string, path: string) => {
			if (path.includes("/attachments")) {
				return {
					data: mockAttachments,
					isLoading: false,
					error: null,
				};
			}
			return { data: undefined, isLoading: false };
		},
		useMutation: () => ({
			mutate: vi.fn(),
			isPending: false,
		}),
	},
}));

describe("Attachments & Document Components", () => {
	describe("Attachment Labels & Configuration", () => {
		it("contains definitions for all standard AttachmentKinds", () => {
			expect(ATTACHMENT_KIND_LABELS.Kk0).toBe("Formulir Survei Lapangan KK0");
			expect(ATTACHMENT_KIND_LABELS.A1).toBe(
				"Formulir Registrasi Pelanggan A1",
			);
			expect(ATTACHMENT_KIND_LABELS.MomSigas).toBe(
				"Minutes of Meeting (MoM) SiGas",
			);
			expect(ATTACHMENT_KIND_LABELS.Npwp).toBe("Salinan NPWP Perusahaan");
			expect(ATTACHMENT_KIND_LABELS.ResumeKelayakan).toBe(
				"Resume Kelayakan Investasi",
			);
		});

		it("contains proper document download configurations", () => {
			expect(DOCUMENT_TYPE_CONFIG.kk0.endpoint).toBe("kk0");
			expect(DOCUMENT_TYPE_CONFIG.a1.endpoint).toBe("a1");
			expect(DOCUMENT_TYPE_CONFIG["nol-request"].endpoint).toBe("nol-request");
			expect(DOCUMENT_TYPE_CONFIG.evaluation.endpoint).toBe("evaluation");
			expect(DOCUMENT_TYPE_CONFIG["nol-issuance"].endpoint).toBe(
				"nol-issuance",
			);
		});
	});

	describe("AttachmentList Component", () => {
		it("renders attachment list with correct details", () => {
			renderWithClient(
				<AttachmentList
					companyId="00000000-0000-0000-0000-000000000001"
					showUploadButton={true}
				/>,
			);

			expect(
				screen.getByText("Formulir_Survei_KK0_Signed.pdf"),
			).toBeInTheDocument();
			expect(screen.getByText("NPWP_Perusahaan.jpg")).toBeInTheDocument();
			expect(
				screen.getByText("Formulir Survei Lapangan KK0"),
			).toBeInTheDocument();
			expect(screen.getByText("Salinan NPWP Perusahaan")).toBeInTheDocument();
			expect(screen.getAllByText("v1").length).toBe(2);
			expect(screen.getByText("TTE")).toBeInTheDocument();
			expect(screen.getAllByText("Unduh").length).toBe(2);
		});

		it("renders with filterKind property", () => {
			renderWithClient(
				<AttachmentList
					companyId="00000000-0000-0000-0000-000000000001"
					filterKind="Kk0"
				/>,
			);

			expect(
				screen.getByText("Formulir_Survei_KK0_Signed.pdf"),
			).toBeInTheDocument();
			expect(screen.queryByText("NPWP_Perusahaan.jpg")).not.toBeInTheDocument();
		});
	});

	describe("AttachmentUploadDialog Component", () => {
		it("renders dialog content when open", () => {
			renderWithClient(
				<AttachmentUploadDialog
					companyId="00000000-0000-0000-0000-000000000001"
					isOpen={true}
					onClose={vi.fn()}
					defaultKind="A1"
				/>,
			);

			expect(screen.getByText("Unggah Berkas Lampiran")).toBeInTheDocument();
			expect(screen.getByText("Jenis Dokumen")).toBeInTheDocument();
			expect(
				screen.getAllByText(/Formulir Registrasi Pelanggan A1/i).length,
			).toBeGreaterThan(0);
			expect(
				screen.getByText("Metode Tanda Tangan (Opsional)"),
			).toBeInTheDocument();
			expect(
				screen.getByText(/Klik untuk memilih berkas atau tarik ke sini/i),
			).toBeInTheDocument();
			expect(
				screen.getByRole("button", { name: /Unggah Berkas/i }),
			).toBeInTheDocument();
		});
	});

	describe("Document Download Buttons", () => {
		it("renders DocumentDownloadButton with custom label", () => {
			renderWithClient(
				<DocumentDownloadButton
					companyId="00000000-0000-0000-0000-000000000001"
					documentType="kk0"
					label="Unduh KK0 Custom"
				/>,
			);

			expect(screen.getByText("Unduh KK0 Custom")).toBeInTheDocument();
		});

		it("renders DocumentDownloadDropdown trigger", () => {
			renderWithClient(
				<DocumentDownloadDropdown companyId="00000000-0000-0000-0000-000000000001" />,
			);

			expect(screen.getByText("Unduh Dokumen Resmi")).toBeInTheDocument();
		});
	});
});
