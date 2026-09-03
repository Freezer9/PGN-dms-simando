import { useForm, useStore } from "@tanstack/react-form";
import { useQueryClient } from "@tanstack/react-query";
import { AlertCircle, FileUp, Loader2, UploadCloud, X } from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import type { AttachmentDetail, AttachmentKind } from "@/api/types";
import { FormField } from "@/components/form/form-field";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import {
	type AttachmentUploadFormValues,
	attachmentUploadSchema,
} from "@/lib/schemas";
import { cn } from "@/lib/utils";

export const ATTACHMENT_KIND_LABELS: Record<AttachmentKind, string> = {
	Kk0: "Formulir Survei Lapangan KK0",
	A1: "Formulir Registrasi Pelanggan A1",
	MomSigas: "Minutes of Meeting (MoM) SiGas",
	CapexPreGr3: "Rincian Capex Pre-GR3",
	BuktiKelayakan: "Bukti / Dokumen Kelayakan",
	SpreadsheetPeralatanGas: "Spreadsheet Peralatan Gas",
	GambarSituasiPabrik: "Gambar Situasi / Layout Lokasi Pelanggan",
	GambarPipaEksisting: "Gambar Jaringan Pipa Eksisting",
	TitikTaping: "Foto / Sketsa Titik Taping",
	DataKompetitor: "Data & Analisis Kompetitor",
	ResumeKelayakan: "Resume Kelayakan Investasi",
	GasBalance: "Neraca Gas / Gas Balance",
	Npwp: "Salinan NPWP Perusahaan",
	ReferenceDocument: "Dokumen Referensi / Ketentuan",
	Other: "Dokumen Pendukung Lainnya",
};

interface AttachmentUploadDialogProps {
	companyId: string;
	isOpen: boolean;
	onClose: () => void;
	defaultKind?: AttachmentKind;
	onSuccess?: (attachment: AttachmentDetail) => void;
}

const MAX_FILE_SIZE_BYTES = 25 * 1024 * 1024; // 25 MB
const ALLOWED_EXTENSIONS = [
	".pdf",
	".docx",
	".doc",
	".xlsx",
	".xls",
	".jpg",
	".jpeg",
	".png",
	".zip",
];

export function AttachmentUploadDialog({
	companyId,
	isOpen,
	onClose,
	defaultKind = "Other",
	onSuccess,
}: AttachmentUploadDialogProps) {
	const queryClient = useQueryClient();
	const fileInputRef = React.useRef<HTMLInputElement | null>(null);

	const [selectedFile, setSelectedFile] = React.useState<File | null>(null);
	const [isDragging, setIsDragging] = React.useState(false);
	const [errorMsg, setErrorMsg] = React.useState<string | null>(null);

	const form = useForm({
		defaultValues: {
			kind: defaultKind,
			signatureMethod: "",
		} as AttachmentUploadFormValues,
		validators: {
			onSubmit: attachmentUploadSchema,
		},
		onSubmit: async ({ value }) => {
			if (!selectedFile) {
				setErrorMsg("Silakan pilih berkas yang akan diunggah.");
				return;
			}

			setErrorMsg(null);

			try {
				const formData = new FormData();
				formData.append("file", selectedFile);
				formData.append("kind", value.kind);
				if (value.signatureMethod) {
					formData.append("signatureMethod", value.signatureMethod);
				}

				const response = await fetch(
					`/api/companies/${companyId}/attachments`,
					{
						method: "POST",
						body: formData,
					},
				);

				if (!response.ok) {
					let errorMessage = "Gagal mengunggah berkas.";
					try {
						const errorData = await response.json();
						if (errorData.detail) errorMessage = errorData.detail;
						else if (errorData.title) errorMessage = errorData.title;
					} catch {
						// Fallback default message
					}
					throw new Error(errorMessage);
				}

				const uploaded: AttachmentDetail = await response.json();

				toast.success("Berkas lampiran berhasil diunggah!");
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{companyId}/attachments",
						{ params: { path: { companyId } } },
					],
				});
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}",
						{ params: { path: { id: companyId } } },
					],
				});

				onSuccess?.(uploaded);
				onClose();
			} catch (err: unknown) {
				const message =
					err instanceof Error
						? err.message
						: "Terjadi kesalahan saat mengunggah berkas.";
				setErrorMsg(message);
				toast.error(message);
			}
		},
	});

	const kindValue = useStore(form.store, (state) => state.values.kind);

	React.useEffect(() => {
		if (isOpen) {
			form.reset({
				kind: defaultKind,
				signatureMethod: "",
			});
			setSelectedFile(null);
			setErrorMsg(null);
		}
	}, [isOpen, defaultKind, form]);

	const validateAndSetFile = (file: File) => {
		setErrorMsg(null);
		if (file.size > MAX_FILE_SIZE_BYTES) {
			setErrorMsg("Ukuran berkas melebihi batas maksimum 25 MB.");
			return;
		}

		const ext = `.${file.name.split(".").pop()?.toLowerCase()}`;
		if (!ALLOWED_EXTENSIONS.includes(ext)) {
			setErrorMsg(
				`Format file ${ext} tidak didukung. Format yang diizinkan: PDF, Word, Excel, Gambar, atau ZIP.`,
			);
			return;
		}

		setSelectedFile(file);
	};

	const handleDragOver = (e: React.DragEvent) => {
		e.preventDefault();
		e.stopPropagation();
		setIsDragging(true);
	};

	const handleDragLeave = (e: React.DragEvent) => {
		e.preventDefault();
		e.stopPropagation();
		setIsDragging(false);
	};

	const handleDrop = (e: React.DragEvent) => {
		e.preventDefault();
		e.stopPropagation();
		setIsDragging(false);
		if (e.dataTransfer.files?.[0]) {
			validateAndSetFile(e.dataTransfer.files[0]);
		}
	};

	const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		if (e.target.files?.[0]) {
			validateAndSetFile(e.target.files[0]);
		}
	};

	const formatSize = (bytes: number) => {
		if (bytes < 1024 * 1024) {
			return `${(bytes / 1024).toFixed(1)} KB`;
		}
		return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
	};

	return (
		<Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
			<DialogContent className="sm:max-w-lg">
				<DialogHeader>
					<DialogTitle className="flex items-center gap-2 text-foreground">
						<FileUp className="h-5 w-5 text-primary" />
						<span>Unggah Berkas Lampiran</span>
					</DialogTitle>
					<DialogDescription>
						Unggah dokumen resmi, formulir tanda tangan, atau berkas pendukung
						calon pelanggan.
					</DialogDescription>
				</DialogHeader>

				<form
					onSubmit={(e) => {
						e.preventDefault();
						e.stopPropagation();
						form.handleSubmit();
					}}
					className="space-y-4 pt-2"
				>
					{/* Jenis Dokumen / Kind */}
					<form.Field name="kind">
						{(field) => {
							const error = field.state.meta.errors[0]?.message;
							const kindOptions = (
								Object.entries(ATTACHMENT_KIND_LABELS) as [
									AttachmentKind,
									string,
								][]
							).map(([key, label]) => ({
								value: key,
								label,
							}));

							return (
								<FormField
									label="Jenis Dokumen"
									htmlFor="attachment-kind"
									required
									error={error}
								>
									<Combobox
										id="attachment-kind"
										value={field.state.value}
										onValueChange={(val) =>
											field.handleChange(val as AttachmentKind)
										}
										options={kindOptions}
										placeholder="Pilih jenis dokumen"
										searchPlaceholder="Cari jenis dokumen..."
										emptyText="Jenis dokumen tidak ditemukan."
										className="h-9 text-xs"
									/>
								</FormField>
							);
						}}
					</form.Field>

					{/* Signature Method (for A1 or KK0 signed forms) */}
					{(kindValue === "A1" || kindValue === "Kk0") && (
						<form.Field name="signatureMethod">
							{(field) => (
								<div className="space-y-1.5 p-3 rounded-lg bg-muted/40 border">
									<Label
										htmlFor="signature-method"
										className="text-xs font-semibold"
									>
										Metode Tanda Tangan (Opsional)
									</Label>
									<Select
										value={field.state.value || undefined}
										onValueChange={(val) => field.handleChange(val)}
									>
										<SelectTrigger
											id="signature-method"
											className="text-xs bg-background"
										>
											<SelectValue placeholder="Pilih metode tanda tangan (opsional)" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="Digital" className="text-xs">
												Digital Certificate / TTE Elektronik
											</SelectItem>
											<SelectItem value="Wet" className="text-xs">
												Tanda Tangan Basah / Manual Stamp
											</SelectItem>
										</SelectContent>
									</Select>
								</div>
							)}
						</form.Field>
					)}

					{/* Dropzone Area */}
					<div className="space-y-1.5">
						<Label className="text-xs font-semibold">
							Berkas Dokumen <span className="text-destructive">*</span>
						</Label>
						<input
							type="file"
							ref={fileInputRef}
							onChange={handleFileInputChange}
							accept=".pdf,.docx,.doc,.xlsx,.xls,.jpg,.jpeg,.png,.zip"
							className="hidden"
						/>
						{selectedFile ? (
							<div className="flex items-center justify-between p-3 rounded-lg border bg-card text-xs">
								<div className="flex items-center gap-2.5 overflow-hidden">
									<UploadCloud className="h-5 w-5 text-primary shrink-0" />
									<div className="overflow-hidden">
										<p className="font-medium text-foreground truncate">
											{selectedFile.name}
										</p>
										<p className="text-[11px] text-muted-foreground">
											{formatSize(selectedFile.size)}
										</p>
									</div>
								</div>
								<Button
									type="button"
									variant="ghost"
									size="sm"
									className="h-7 w-7 p-0 text-muted-foreground hover:text-foreground"
									onClick={() => setSelectedFile(null)}
								>
									<X className="h-4 w-4" />
								</Button>
							</div>
						) : (
							<button
								type="button"
								onDragOver={handleDragOver}
								onDragLeave={handleDragLeave}
								onDrop={handleDrop}
								onClick={() => fileInputRef.current?.click()}
								className={cn(
									"w-full flex flex-col items-center justify-center p-6 border-2 border-dashed rounded-xl cursor-pointer transition-colors text-center",
									isDragging
										? "border-primary bg-primary/5"
										: "border-muted-foreground/30 hover:border-primary/60 hover:bg-muted/30",
								)}
							>
								<UploadCloud className="h-8 w-8 text-muted-foreground mb-2" />
								<p className="text-xs font-medium text-foreground">
									Klik untuk memilih berkas atau tarik ke sini
								</p>
								<p className="text-[11px] text-muted-foreground mt-1">
									Mendukung PDF, DOCX, XLSX, JPG, PNG, ZIP (Maks. 25 MB)
								</p>
							</button>
						)}
					</div>

					{/* Error Alert */}
					{errorMsg && (
						<div className="flex items-start gap-2 p-3 rounded-lg bg-destructive/10 border border-destructive/20 text-destructive text-xs">
							<AlertCircle className="h-4 w-4 shrink-0 mt-0.5" />
							<span>{errorMsg}</span>
						</div>
					)}

					<DialogFooter className="pt-2">
						<Button type="button" variant="outline" size="sm" onClick={onClose}>
							Batal
						</Button>
						<form.Subscribe
							selector={(state) => [state.canSubmit, state.isSubmitting]}
						>
							{([canSubmit, isSubmitting]) => (
								<Button
									type="submit"
									size="sm"
									disabled={!selectedFile || !canSubmit || isSubmitting}
									className="bg-primary text-primary-foreground font-medium"
								>
									{isSubmitting ? (
										<>
											<Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" />
											Mengunggah...
										</>
									) : (
										<>
											<FileUp className="h-3.5 w-3.5 mr-1.5" />
											Unggah Berkas
										</>
									)}
								</Button>
							)}
						</form.Subscribe>
					</DialogFooter>
				</form>
			</DialogContent>
		</Dialog>
	);
}
