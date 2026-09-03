import {
	ChevronDown,
	FileDown,
	FileSpreadsheet,
	FileText,
	FileType,
	Loader2,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuLabel,
	DropdownMenuSeparator,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

export type DocumentType =
	| "kk0"
	| "a1"
	| "nol-request"
	| "evaluation"
	| "nol-issuance";

export const DOCUMENT_TYPE_CONFIG: Record<
	DocumentType,
	{ label: string; endpoint: string; defaultFilename: string }
> = {
	kk0: {
		label: "Formulir Survei KK0 (.docx)",
		endpoint: "kk0",
		defaultFilename: "Formulir_Survei_KK0.docx",
	},
	a1: {
		label: "Formulir Registrasi A1 (.docx)",
		endpoint: "a1",
		defaultFilename: "Formulir_Registrasi_A1.docx",
	},
	"nol-request": {
		label: "Permohonan Surat NOL (.docx)",
		endpoint: "nol-request",
		defaultFilename: "Permohonan_NOL.docx",
	},
	evaluation: {
		label: "Resume Evaluasi Kelayakan (.docx)",
		endpoint: "evaluation",
		defaultFilename: "Resume_Evaluasi.docx",
	},
	"nol-issuance": {
		label: "Surat Penerbitan NOL (.docx)",
		endpoint: "nol-issuance",
		defaultFilename: "Surat_Penerbitan_NOL.docx",
	},
};

async function downloadDocumentFile(companyId: string, type: DocumentType) {
	const config = DOCUMENT_TYPE_CONFIG[type];
	const url = `/api/documents/company/${companyId}/${config.endpoint}`;

	try {
		const response = await fetch(url);
		if (!response.ok) {
			let errorMsg = `Gagal mengunduh dokumen ${config.label}`;
			if (response.status === 403) {
				errorMsg = "Anda tidak memiliki izin untuk mengunduh dokumen ini.";
			} else if (response.status === 404) {
				errorMsg = "Data dokumen tidak ditemukan.";
			}
			throw new Error(errorMsg);
		}

		// Extract filename from Content-Disposition header if available
		const disposition = response.headers.get("Content-Disposition");
		let filename = config.defaultFilename;
		if (disposition?.includes("filename=")) {
			const matches = disposition.match(
				/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/,
			);
			if (matches?.[1]) {
				filename = matches[1].replace(/['"]/g, "");
			}
		}

		const blob = await response.blob();
		const downloadUrl = window.URL.createObjectURL(blob);
		const a = document.createElement("a");
		a.href = downloadUrl;
		a.download = filename;
		document.body.appendChild(a);
		a.click();
		window.URL.revokeObjectURL(downloadUrl);
		a.remove();

		toast.success(`Dokumen ${filename} berhasil diunduh.`);
	} catch (err: unknown) {
		const message =
			err instanceof Error ? err.message : "Gagal mengunduh dokumen.";
		toast.error(message);
		throw err;
	}
}

interface DocumentDownloadDropdownProps {
	companyId: string;
	className?: string;
}

export function DocumentDownloadDropdown({
	companyId,
	className,
}: DocumentDownloadDropdownProps) {
	const [downloadingType, setDownloadingType] =
		React.useState<DocumentType | null>(null);

	const handleDownload = async (type: DocumentType) => {
		setDownloadingType(type);
		try {
			await downloadDocumentFile(companyId, type);
		} finally {
			setDownloadingType(null);
		}
	};

	return (
		<DropdownMenu>
			<DropdownMenuTrigger asChild>
				<Button
					variant="outline"
					size="sm"
					className={className}
					disabled={downloadingType !== null}
				>
					{downloadingType ? (
						<Loader2 className="h-4 w-4 animate-spin mr-1.5" />
					) : (
						<FileDown className="h-4 w-4 mr-1.5 text-primary" />
					)}
					<span>Unduh Dokumen Resmi</span>
					<ChevronDown className="h-3.5 w-3.5 ml-1.5 opacity-60" />
				</Button>
			</DropdownMenuTrigger>
			<DropdownMenuContent align="end" className="w-64">
				<DropdownMenuLabel className="text-xs font-semibold text-muted-foreground">
					Generate Template Word (.docx)
				</DropdownMenuLabel>
				<DropdownMenuSeparator />
				<DropdownMenuItem
					onClick={() => handleDownload("kk0")}
					disabled={downloadingType !== null}
					className="text-xs cursor-pointer"
				>
					<FileText className="h-3.5 w-3.5 mr-2 text-primary" />
					<span>Formulir Survei KK0</span>
				</DropdownMenuItem>
				<DropdownMenuItem
					onClick={() => handleDownload("a1")}
					disabled={downloadingType !== null}
					className="text-xs cursor-pointer"
				>
					<FileType className="h-3.5 w-3.5 mr-2 text-primary" />
					<span>Formulir Registrasi A1</span>
				</DropdownMenuItem>
				<DropdownMenuItem
					onClick={() => handleDownload("nol-request")}
					disabled={downloadingType !== null}
					className="text-xs cursor-pointer"
				>
					<FileDown className="h-3.5 w-3.5 mr-2 text-primary" />
					<span>Permohonan Surat NOL</span>
				</DropdownMenuItem>
				<DropdownMenuItem
					onClick={() => handleDownload("evaluation")}
					disabled={downloadingType !== null}
					className="text-xs cursor-pointer"
				>
					<FileSpreadsheet className="h-3.5 w-3.5 mr-2 text-primary" />
					<span>Resume Evaluasi Kelayakan</span>
				</DropdownMenuItem>
				<DropdownMenuItem
					onClick={() => handleDownload("nol-issuance")}
					disabled={downloadingType !== null}
					className="text-xs cursor-pointer"
				>
					<FileText className="h-3.5 w-3.5 mr-2 text-primary" />
					<span>Surat Penerbitan NOL</span>
				</DropdownMenuItem>
			</DropdownMenuContent>
		</DropdownMenu>
	);
}

interface DocumentDownloadButtonProps
	extends Omit<React.ComponentProps<typeof Button>, "onClick"> {
	companyId: string;
	documentType: DocumentType;
	label?: string;
}

export function DocumentDownloadButton({
	companyId,
	documentType,
	label,
	variant = "outline",
	size = "sm",
	className,
	...props
}: DocumentDownloadButtonProps) {
	const [isDownloading, setIsDownloading] = React.useState(false);
	const config = DOCUMENT_TYPE_CONFIG[documentType];

	const handleDownload = async () => {
		setIsDownloading(true);
		try {
			await downloadDocumentFile(companyId, documentType);
		} finally {
			setIsDownloading(false);
		}
	};

	return (
		<Button
			variant={variant}
			size={size}
			onClick={handleDownload}
			disabled={isDownloading}
			className={className}
			{...props}
		>
			{isDownloading ? (
				<Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" />
			) : (
				<FileDown className="h-3.5 w-3.5 mr-1.5 text-primary" />
			)}
			<span>{label || config.label}</span>
		</Button>
	);
}
