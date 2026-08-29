import { useQueryClient } from "@tanstack/react-query";
import {
	Archive,
	Download,
	File,
	FileSpreadsheet,
	FileText,
	FileType,
	Image as ImageIcon,
	Loader2,
	Plus,
	Trash2,
	UploadCloud,
	User,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type { AttachmentKind } from "@/api/types";
import { ATTACHMENT_KIND_LABELS } from "@/components/attachments/attachment-upload-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/components/ui/dialog";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";

interface AttachmentListProps {
	companyId: string;
	filterKind?: AttachmentKind | AttachmentKind[];
	showUploadButton?: boolean;
	onUploadClick?: () => void;
	canDelete?: boolean;
	emptyMessage?: string;
}

export function AttachmentList({
	companyId,
	filterKind,
	showUploadButton = false,
	onUploadClick,
	canDelete = true,
	emptyMessage,
}: AttachmentListProps) {
	const queryClient = useQueryClient();
	const [deletingId, setDeletingId] = React.useState<string | null>(null);
	const [isConfirmOpen, setIsConfirmOpen] = React.useState(false);
	const [isDeleting, setIsDeleting] = React.useState(false);

	const { data: attachments = [], isLoading } = $api.useQuery(
		"get",
		"/api/companies/{companyId}/attachments",
		{
			params: {
				path: {
					companyId,
				},
			},
		},
	);

	const filteredAttachments = React.useMemo(() => {
		if (!filterKind) return attachments;
		if (Array.isArray(filterKind)) {
			const set = new Set(filterKind);
			return attachments.filter((a) => set.has(a.kind));
		}
		return attachments.filter((a) => a.kind === filterKind);
	}, [attachments, filterKind]);

	const handleDelete = async () => {
		if (!deletingId) return;
		setIsDeleting(true);

		try {
			const response = await fetch(`/api/attachments/${deletingId}`, {
				method: "DELETE",
			});

			if (!response.ok) {
				throw new Error("Gagal menghapus berkas lampiran.");
			}

			toast.success("Berkas lampiran berhasil dihapus.");
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
			setIsConfirmOpen(false);
			setDeletingId(null);
		} catch (err: unknown) {
			const message =
				err instanceof Error ? err.message : "Gagal menghapus berkas lampiran.";
			toast.error(message);
		} finally {
			setIsDeleting(false);
		}
	};

	const formatSize = (bytes: number | string) => {
		const num = Number(bytes);
		if (num < 1024) return `${num} B`;
		if (num < 1024 * 1024) return `${(num / 1024).toFixed(1)} KB`;
		return `${(num / (1024 * 1024)).toFixed(1)} MB`;
	};

	const getFileIcon = (filename: string, mimeType: string) => {
		const ext = filename.split(".").pop()?.toLowerCase();

		if (ext === "pdf" || mimeType.includes("pdf")) {
			return <FileText className="h-4 w-4 text-rose-500 shrink-0" />;
		}
		if (["docx", "doc"].includes(ext || "") || mimeType.includes("word")) {
			return <FileType className="h-4 w-4 text-blue-500 shrink-0" />;
		}
		if (
			["xlsx", "xls", "csv"].includes(ext || "") ||
			mimeType.includes("sheet") ||
			mimeType.includes("excel")
		) {
			return <FileSpreadsheet className="h-4 w-4 text-emerald-500 shrink-0" />;
		}
		if (
			["jpg", "jpeg", "png", "webp", "gif"].includes(ext || "") ||
			mimeType.startsWith("image/")
		) {
			return <ImageIcon className="h-4 w-4 text-purple-500 shrink-0" />;
		}
		if (["zip", "rar", "7z", "tar", "gz"].includes(ext || "")) {
			return <Archive className="h-4 w-4 text-amber-500 shrink-0" />;
		}
		return <File className="h-4 w-4 text-muted-foreground shrink-0" />;
	};

	return (
		<div className="space-y-3">
			{/* Top Header / Upload action if enabled */}
			{showUploadButton && (
				<div className="flex items-center justify-between">
					<div className="text-xs text-muted-foreground">
						Total:{" "}
						<strong className="text-foreground">
							{filteredAttachments.length}
						</strong>{" "}
						berkas lampiran
					</div>
					<Button
						size="sm"
						onClick={onUploadClick}
						className="h-8 px-3 text-xs bg-primary text-primary-foreground font-medium"
					>
						<Plus className="h-3.5 w-3.5 mr-1" />
						Unggah Dokumen
					</Button>
				</div>
			)}

			{/* Attachments Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table>
					<TableHeader className="bg-muted/40">
						<TableRow>
							<TableHead className="font-semibold text-xs">
								Nama Berkas
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Jenis Dokumen
							</TableHead>
							<TableHead className="font-semibold text-xs">Versi</TableHead>
							<TableHead className="font-semibold text-xs">Ukuran</TableHead>
							<TableHead className="font-semibold text-xs">
								Pengunggah
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Waktu Unggah
							</TableHead>
							<TableHead className="text-right font-semibold text-xs pr-4">
								Tindakan
							</TableHead>
						</TableRow>
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableRow>
								<TableCell colSpan={7} className="text-center py-8">
									<div className="flex flex-col items-center justify-center gap-2 text-muted-foreground">
										<Loader2 className="h-5 w-5 animate-spin text-primary" />
										<span className="text-xs">Memuat lampiran dokumen...</span>
									</div>
								</TableCell>
							</TableRow>
						) : filteredAttachments.length === 0 ? (
							<TableRow>
								<TableCell colSpan={7} className="text-center py-10">
									<div className="flex flex-col items-center justify-center gap-2 max-w-sm mx-auto text-muted-foreground">
										<div className="p-2.5 rounded-full bg-muted text-muted-foreground">
											<UploadCloud className="h-5 w-5" />
										</div>
										<p className="text-xs text-muted-foreground text-center">
											{emptyMessage ||
												"Belum ada berkas lampiran yang diunggah untuk kategori ini."}
										</p>
										{onUploadClick && (
											<Button
												size="sm"
												variant="outline"
												className="h-7 text-xs mt-1"
												onClick={onUploadClick}
											>
												<Plus className="h-3 w-3 mr-1" />
												Unggah Berkas Sekarang
											</Button>
										)}
									</div>
								</TableCell>
							</TableRow>
						) : (
							filteredAttachments.map((item) => (
								<TableRow
									key={item.id}
									className="hover:bg-muted/30 transition-colors"
								>
									{/* Nama Berkas */}
									<TableCell className="py-2.5">
										<div className="flex items-center gap-2">
											{getFileIcon(item.filename, item.mimeType)}
											<span
												className="font-medium text-foreground text-xs truncate max-w-xs"
												title={item.filename}
											>
												{item.filename}
											</span>
										</div>
									</TableCell>

									{/* Jenis Dokumen */}
									<TableCell className="py-2.5">
										<Badge
											variant="outline"
											className="text-[11px] font-normal"
										>
											{ATTACHMENT_KIND_LABELS[item.kind] || item.kind}
										</Badge>
									</TableCell>

									{/* Versi & Signature Method */}
									<TableCell className="py-2.5">
										<div className="flex items-center gap-1.5">
											<Badge
												variant="secondary"
												className="text-[11px] font-mono font-medium"
											>
												v{item.version}
											</Badge>
											{item.signatureMethod && (
												<Badge
													variant="outline"
													className="text-[10px] bg-primary/5 text-primary border-primary/20"
												>
													{item.signatureMethod === "Digital" ? "TTE" : "Basah"}
												</Badge>
											)}
										</div>
									</TableCell>

									{/* Ukuran */}
									<TableCell className="py-2.5 text-xs text-muted-foreground font-mono">
										{formatSize(item.sizeBytes)}
									</TableCell>

									{/* Pengunggah */}
									<TableCell className="py-2.5">
										<div className="flex items-center gap-1 text-xs text-foreground">
											<User className="h-3 w-3 text-muted-foreground shrink-0" />
											<span>{item.uploadedByName || "-"}</span>
										</div>
									</TableCell>

									{/* Waktu Unggah */}
									<TableCell className="py-2.5 text-xs text-muted-foreground">
										{new Date(item.uploadedAt).toLocaleString("id-ID", {
											dateStyle: "medium",
											timeStyle: "short",
										})}
									</TableCell>

									{/* Tindakan (Download & Delete) */}
									<TableCell className="py-2.5 text-right pr-4">
										<div className="flex items-center justify-end gap-1">
											<Button
												asChild
												size="sm"
												variant="ghost"
												className="h-7 px-2 text-xs text-primary hover:text-primary hover:bg-primary/10"
												title="Unduh Berkas"
											>
												<a
													href={`/api/attachments/${item.id}/download`}
													download={item.filename}
												>
													<Download className="h-3.5 w-3.5 mr-1" />
													Unduh
												</a>
											</Button>
											{canDelete && (
												<Button
													size="sm"
													variant="ghost"
													className="h-7 px-2 text-xs text-destructive hover:text-destructive hover:bg-destructive/10"
													onClick={() => {
														setDeletingId(item.id);
														setIsConfirmOpen(true);
													}}
													title="Hapus Berkas"
												>
													<Trash2 className="h-3.5 w-3.5" />
												</Button>
											)}
										</div>
									</TableCell>
								</TableRow>
							))
						)}
					</TableBody>
				</Table>
			</div>

			{/* Delete Confirmation Modal */}
			<Dialog open={isConfirmOpen} onOpenChange={setIsConfirmOpen}>
				<DialogContent className="sm:max-w-md">
					<DialogHeader>
						<DialogTitle className="text-foreground">
							Konfirmasi Hapus Lampiran
						</DialogTitle>
						<DialogDescription>
							Apakah Anda yakin ingin menghapus berkas lampiran ini? Tindakan
							ini tidak dapat dibatalkan.
						</DialogDescription>
					</DialogHeader>
					<DialogFooter className="pt-2">
						<Button
							variant="outline"
							size="sm"
							onClick={() => setIsConfirmOpen(false)}
							disabled={isDeleting}
						>
							Batal
						</Button>
						<Button
							variant="destructive"
							size="sm"
							onClick={handleDelete}
							disabled={isDeleting}
						>
							{isDeleting ? (
								<>
									<Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" />
									Menghapus...
								</>
							) : (
								<>
									<Trash2 className="h-3.5 w-3.5 mr-1.5" />
									Hapus Berkas
								</>
							)}
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</div>
	);
}
