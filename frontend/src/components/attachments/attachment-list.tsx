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
	User,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type { AttachmentDetail, AttachmentKind } from "@/api/types";
import { ATTACHMENT_KIND_LABELS } from "@/components/attachments/attachment-upload-dialog";
import { IconButton } from "@/components/common";
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
import { TableEmptyState } from "@/components/ui/table-empty-state";
import { TablePagination } from "@/components/ui/table-pagination";
import { TableSkeleton } from "@/components/ui/table-skeleton";

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

	const [page, setPage] = React.useState(1);
	const [pageSize, setPageSize] = React.useState(10);

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

	const filteredAttachments = React.useMemo<AttachmentDetail[]>(() => {
		if (!filterKind) return attachments;
		if (Array.isArray(filterKind)) {
			const set = new Set(filterKind);
			return attachments.filter((a) => set.has(a.kind));
		}
		return attachments.filter((a) => a.kind === filterKind);
	}, [attachments, filterKind]);

	const totalCount = filteredAttachments.length;
	const totalPages = Math.ceil(totalCount / pageSize) || 1;
	const paginatedAttachments = React.useMemo(() => {
		const start = (page - 1) * pageSize;
		return filteredAttachments.slice(start, start + pageSize);
	}, [filteredAttachments, page, pageSize]);

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
							<TableHead className="font-semibold text-xs py-3">
								Nama Berkas
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Jenis Dokumen
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Versi
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Ukuran
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Pengunggah
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Waktu Unggah
							</TableHead>
							<TableHead className="text-right font-semibold text-xs py-3 pr-4">
								Aksi
							</TableHead>
						</TableRow>
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableSkeleton columns={7} rows={4} />
						) : filteredAttachments.length === 0 ? (
							<TableEmptyState
								colSpan={7}
								icon="folder"
								title="Belum Ada Berkas Lampiran"
								description={
									emptyMessage ||
									"Belum ada berkas lampiran yang diunggah untuk kategori ini."
								}
								action={
									onUploadClick ? (
										<Button
											size="sm"
											variant="outline"
											className="h-8 text-xs gap-1.5"
											onClick={onUploadClick}
										>
											<Plus className="size-3.5" />
											<span>Unggah Berkas Sekarang</span>
										</Button>
									) : undefined
								}
							/>
						) : (
							paginatedAttachments.map((item) => (
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
											<IconButton
												asChild
												tooltip="Unduh Berkas"
												className="size-7"
												aria-label="Unduh Berkas"
											>
												<a
													href={`/api/attachments/${item.id}/download`}
													download={item.filename}
												>
													<Download className="size-3.5" />
												</a>
											</IconButton>
											{canDelete && (
												<IconButton
													tooltip="Hapus Berkas"
													danger
													className="size-7"
													onClick={() => {
														setDeletingId(item.id);
														setIsConfirmOpen(true);
													}}
													aria-label="Hapus Berkas"
												>
													<Trash2 className="size-3.5" />
												</IconButton>
											)}
										</div>
									</TableCell>
								</TableRow>
							))
						)}
					</TableBody>
				</Table>

				{/* Pagination if multiple pages */}
				{filteredAttachments.length > 10 && (
					<TablePagination
						pageIndex={page - 1}
						page={page}
						pageSize={pageSize}
						totalCount={totalCount}
						totalPages={totalPages}
						onPageChange={(newPage) => setPage(newPage)}
						onPageSizeChange={(newSize) => {
							setPageSize(newSize);
							setPage(1);
						}}
						pageSizeOptions={[10, 25, 50]}
						className="border-t px-4"
					/>
				)}
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
					<DialogFooter className="flex justify-end gap-2">
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
								<Loader2 className="h-4 w-4 animate-spin mr-1" />
							) : null}
							Hapus Lampiran
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</div>
	);
}
