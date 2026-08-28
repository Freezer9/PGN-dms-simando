import { useQueryClient } from "@tanstack/react-query";
import {
	AlertTriangle,
	CheckCircle2,
	Loader2,
	RotateCcw,
	UserCheck,
	XCircle,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type { TaskListItem } from "@/api/types";
import { Button } from "@/components/ui/button";
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
import { Textarea } from "@/components/ui/textarea";

export type TaskActionModalType =
	| "Setuju"
	| "Revisi"
	| "Tolak"
	| "Reassign"
	| null;

export interface TaskActionModalProps {
	task: TaskListItem | null;
	actionType: TaskActionModalType;
	isOpen: boolean;
	onClose: () => void;
	onSuccess?: () => void;
}

export function TaskActionModal({
	task,
	actionType,
	isOpen,
	onClose,
	onSuccess,
}: TaskActionModalProps) {
	const queryClient = useQueryClient();
	const [comment, setComment] = React.useState("");
	const [newUserId, setNewUserId] = React.useState<string>("");

	// Reset form when opened
	React.useEffect(() => {
		if (isOpen) {
			setComment("");
			setNewUserId("");
		}
	}, [isOpen]);

	// Master Reviewers query for Reassign
	const { data: reviewers = [] } = $api.useQuery(
		"get",
		"/api/master/reviewers",
		undefined,
		{
			enabled: isOpen && actionType === "Reassign",
		},
	);

	// Act on step mutation (Setuju, Revisi, Tolak)
	const actOnStepMutation = $api.useMutation(
		"post",
		"/api/workflow/steps/{stepId}/act",
		{
			onSuccess: () => {
				toast.success(
					actionType === "Setuju"
						? "Persetujuan berhasil diproses!"
						: actionType === "Revisi"
							? "Permintaan revisi telah dikirim!"
							: "Penolakan telah diproses dan dialihkan!",
				);
				invalidateAllTaskQueries();
				onClose();
				onSuccess?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error ? error.message : "Gagal memproses tindakan",
				);
			},
		},
	);

	// Reassign step mutation
	const reassignMutation = $api.useMutation(
		"post",
		"/api/workflow/steps/{stepId}/reassign",
		{
			onSuccess: () => {
				toast.success("Reviewer berhasil ditugaskan ulang!");
				invalidateAllTaskQueries();
				onClose();
				onSuccess?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error
						? error.message
						: "Gagal menugaskan ulang reviewer",
				);
			},
		},
	);

	const invalidateAllTaskQueries = () => {
		queryClient.invalidateQueries({ queryKey: ["get", "/api/tasks/inbox"] });
		queryClient.invalidateQueries({ queryKey: ["get", "/api/tasks/region"] });
		queryClient.invalidateQueries({ queryKey: ["get", "/api/tasks/blocked"] });
		queryClient.invalidateQueries({ queryKey: ["get", "/api/tasks/history"] });
		queryClient.invalidateQueries({ queryKey: ["get", "/api/tasks/summary"] });
		if (task?.companyId) {
			queryClient.invalidateQueries({
				queryKey: [
					"get",
					"/api/companies/{id}",
					{ params: { path: { id: task.companyId } } },
				],
			});
		}
	};

	const isPending = actOnStepMutation.isPending || reassignMutation.isPending;

	const handleConfirm = () => {
		if (!task) return;

		if (actionType === "Reassign") {
			if (!newUserId) {
				toast.error("Wajib memilih reviewer baru!");
				return;
			}
			reassignMutation.mutate({
				params: { path: { stepId: task.stepId } },
				body: {
					newUserId,
					reason: comment.trim() || null,
				},
			});
			return;
		}

		if (actionType === "Revisi") {
			if (!comment.trim()) {
				toast.error("Wajib mengisi catatan/alasan revisi!");
				return;
			}
			actOnStepMutation.mutate({
				params: { path: { stepId: task.stepId } },
				body: {
					action: "Revisi",
					comment: comment.trim(),
				},
			});
			return;
		}

		if (actionType === "Tolak") {
			if (!comment.trim()) {
				toast.error("Wajib mengisi alasan penolakan!");
				return;
			}
			actOnStepMutation.mutate({
				params: { path: { stepId: task.stepId } },
				body: {
					action: "Tolak",
					comment: comment.trim(),
				},
			});
			return;
		}

		if (actionType === "Setuju") {
			actOnStepMutation.mutate({
				params: { path: { stepId: task.stepId } },
				body: {
					action: "Setuju",
					comment: comment.trim() || null,
				},
			});
		}
	};

	if (!actionType || !task) {
		return null;
	}

	return (
		<Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
			<DialogContent className="sm:max-w-md">
				<DialogHeader>
					<div className="flex items-center gap-2 mb-1">
						{actionType === "Setuju" && (
							<div className="p-2 rounded-full bg-emerald-100 dark:bg-emerald-950 text-emerald-600 dark:text-emerald-400">
								<CheckCircle2 className="h-5 w-5" />
							</div>
						)}
						{actionType === "Revisi" && (
							<div className="p-2 rounded-full bg-amber-100 dark:bg-amber-950 text-amber-600 dark:text-amber-400">
								<RotateCcw className="h-5 w-5" />
							</div>
						)}
						{actionType === "Tolak" && (
							<div className="p-2 rounded-full bg-rose-100 dark:bg-rose-950 text-rose-600 dark:text-rose-400">
								<XCircle className="h-5 w-5" />
							</div>
						)}
						{actionType === "Reassign" && (
							<div className="p-2 rounded-full bg-blue-100 dark:bg-blue-950 text-blue-600 dark:text-blue-400">
								<UserCheck className="h-5 w-5" />
							</div>
						)}
						<DialogTitle>
							{actionType === "Setuju" && "Konfirmasi Persetujuan"}
							{actionType === "Revisi" && "Permintaan Revisi Berkas"}
							{actionType === "Tolak" && "Konfirmasi Penolakan"}
							{actionType === "Reassign" && "Tugaskan Ulang Reviewer"}
						</DialogTitle>
					</div>
					<DialogDescription>
						{actionType === "Setuju" && (
							<>
								Menyetujui permohonan untuk{" "}
								<strong className="text-foreground">
									{task.namaPerusahaan}
								</strong>{" "}
								({task.nomor}). Tahapan akan berlanjut ke verifikator
								berikutnya.
							</>
						)}
						{actionType === "Revisi" && (
							<>
								Mengembalikan pengajuan{" "}
								<strong className="text-foreground">
									{task.namaPerusahaan}
								</strong>{" "}
								({task.nomor}) ke tahap sebelumnya agar pemohon dapat
								memperbaiki data.
							</>
						)}
						{actionType === "Tolak" && (
							<>
								Menolak pengajuan{" "}
								<strong className="text-foreground">
									{task.namaPerusahaan}
								</strong>{" "}
								({task.nomor}). Berkas akan dialihkan ke Regional Admin untuk
								tinjauan lebih lanjut.
							</>
						)}
						{actionType === "Reassign" && (
							<>
								Memindahkan penugasan review{" "}
								<strong className="text-foreground">
									{task.namaPerusahaan}
								</strong>{" "}
								({task.nomor}) ke personil lain.
							</>
						)}
					</DialogDescription>
				</DialogHeader>

				<div className="space-y-4 py-2">
					{actionType === "Reassign" && (
						<div className="space-y-2">
							<Label htmlFor="new-reviewer-select">
								Pilih Reviewer Baru <span className="text-rose-500">*</span>
							</Label>
							<Select value={newUserId} onValueChange={setNewUserId}>
								<SelectTrigger id="new-reviewer-select">
									<SelectValue placeholder="-- Pilih Reviewer --" />
								</SelectTrigger>
								<SelectContent>
									{reviewers.map((r) => (
										<SelectItem key={r.id} value={r.id}>
											{r.name} ({r.role}) - {r.email}
										</SelectItem>
									))}
								</SelectContent>
							</Select>
						</div>
					)}

					<div className="space-y-2">
						<Label htmlFor="action-comment">
							{actionType === "Setuju" && "Catatan Persetujuan (Opsional)"}
							{actionType === "Revisi" && (
								<>
									Catatan Revisi / Poin Perbaikan{" "}
									<span className="text-rose-500">*</span>
								</>
							)}
							{actionType === "Tolak" && (
								<>
									Alasan Penolakan <span className="text-rose-500">*</span>
								</>
							)}
							{actionType === "Reassign" && "Alasan Penugasan Ulang (Opsional)"}
						</Label>
						<Textarea
							id="action-comment"
							rows={3}
							value={comment}
							onChange={(e) => setComment(e.target.value)}
							placeholder={
								actionType === "Setuju"
									? "Tulis catatan tambahan jika ada..."
									: actionType === "Revisi"
										? "Jelaskan data atau berkas yang wajib diperbaiki oleh pemohon..."
										: actionType === "Tolak"
											? "Jelaskan alasan pengajuan tidak dapat disetujui..."
											: "Tulis alasan pengalihan tugas..."
							}
						/>
					</div>

					{actionType === "Tolak" && (
						<div className="flex items-start gap-2.5 p-2.5 rounded-lg bg-rose-50 dark:bg-rose-950/40 border border-rose-200 dark:border-rose-800 text-xs text-rose-700 dark:text-rose-300">
							<AlertTriangle className="h-4 w-4 shrink-0 mt-0.5" />
							<span>
								Tindakan ini tidak dapat dibatalkan secara otomatis. Status
								berkas akan dialihkan ke Regional Admin untuk ditindaklanjuti.
							</span>
						</div>
					)}
				</div>

				<DialogFooter className="gap-2 sm:gap-0">
					<Button variant="outline" onClick={onClose} disabled={isPending}>
						Batal
					</Button>
					<Button
						variant={
							actionType === "Tolak"
								? "destructive"
								: actionType === "Revisi"
									? "secondary"
									: "default"
						}
						className={
							actionType === "Setuju"
								? "bg-emerald-600 hover:bg-emerald-700 text-white font-medium"
								: actionType === "Revisi"
									? "bg-amber-600 hover:bg-amber-700 text-white font-medium"
									: undefined
						}
						onClick={handleConfirm}
						disabled={isPending}
					>
						{isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
						{actionType === "Setuju" && "Konfirmasi Setuju"}
						{actionType === "Revisi" && "Kirim Permintaan Revisi"}
						{actionType === "Tolak" && "Konfirmasi Tolak"}
						{actionType === "Reassign" && "Simpan Penugasan"}
					</Button>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	);
}
