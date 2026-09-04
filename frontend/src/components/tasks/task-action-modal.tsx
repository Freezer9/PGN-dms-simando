import { useForm } from "@tanstack/react-form";
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
import { Textarea } from "@/components/ui/textarea";
import { formatRole } from "@/lib/roles";
import {
	type TaskActionModalFormValues,
	taskActionModalSchema,
} from "@/lib/schemas";

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

	const invalidateAllTaskQueries = React.useCallback(() => {
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
			queryClient.invalidateQueries({
				queryKey: [
					"get",
					"/api/companies/{id}/timeline",
					{ params: { path: { id: task.companyId } } },
				],
			});
		}
	}, [queryClient, task?.companyId]);

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
				toast.error(error.detail || error.title || "Gagal memproses tindakan");
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
					error.detail || error.title || "Gagal menugaskan ulang reviewer",
				);
			},
		},
	);

	const form = useForm({
		defaultValues: {
			comment: "",
			newUserId: "",
		} as TaskActionModalFormValues,
		validators: {
			onSubmit: taskActionModalSchema,
		},
		onSubmit: async ({ value }) => {
			if (!task) return;

			if (actionType === "Reassign") {
				if (!value.newUserId) {
					toast.error("Reviewer baru wajib dipilih!");
					return;
				}
				await reassignMutation.mutateAsync({
					params: { path: { stepId: task.stepId } },
					body: {
						newUserId: value.newUserId,
						reason: value.comment?.trim() || null,
					},
				});
				return;
			}

			if (actionType === "Revisi") {
				if (!value.comment?.trim()) {
					toast.error("Catatan revisi wajib diisi!");
					return;
				}
				await actOnStepMutation.mutateAsync({
					params: { path: { stepId: task.stepId } },
					body: {
						action: "Revisi",
						comment: value.comment.trim(),
					},
				});
				return;
			}

			if (actionType === "Tolak") {
				if (!value.comment?.trim()) {
					toast.error("Alasan penolakan wajib diisi!");
					return;
				}
				await actOnStepMutation.mutateAsync({
					params: { path: { stepId: task.stepId } },
					body: {
						action: "Tolak",
						comment: value.comment.trim(),
					},
				});
				return;
			}

			if (actionType === "Setuju") {
				await actOnStepMutation.mutateAsync({
					params: { path: { stepId: task.stepId } },
					body: {
						action: "Setuju",
						comment: value.comment?.trim() || null,
					},
				});
			}
		},
	});

	// Reset form when opened
	React.useEffect(() => {
		if (isOpen) {
			form.reset({
				comment: "",
				newUserId: "",
			});
		}
	}, [isOpen, form]);

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

				<form
					onSubmit={(e) => {
						e.preventDefault();
						e.stopPropagation();
						form.handleSubmit();
					}}
					className="space-y-4 py-2"
				>
					{actionType === "Reassign" && (
						<form.Field name="newUserId">
							{(field) => {
								const error = field.state.meta.errors[0]?.message;
								return (
									<FormField
										label="Pilih Reviewer Baru"
										htmlFor="new-reviewer-select"
										required
										error={error}
									>
										<Combobox
											id="new-reviewer-select"
											value={field.state.value || ""}
											onValueChange={(val) => field.handleChange(val)}
											options={reviewers.map((r) => ({
												value: r.id,
												label: `${r.fullName} (${formatRole(r.role)}) - ${r.email}`,
											}))}
											placeholder="-- Pilih Reviewer --"
											searchPlaceholder="Cari reviewer..."
											emptyText="Reviewer tidak ditemukan."
											className="h-9 text-xs"
										/>
									</FormField>
								);
							}}
						</form.Field>
					)}

					<form.Field name="comment">
						{(field) => {
							const error = field.state.meta.errors[0]?.message;
							const isCommentRequired =
								actionType === "Revisi" || actionType === "Tolak";
							const labelText =
								actionType === "Setuju"
									? "Catatan Persetujuan"
									: actionType === "Revisi"
										? "Catatan Revisi"
										: actionType === "Tolak"
											? "Alasan Penolakan"
											: "Alasan Penugasan Ulang";

							return (
								<FormField
									label={labelText}
									htmlFor="action-comment"
									required={isCommentRequired}
									error={error}
								>
									<Textarea
										id="action-comment"
										name={field.name}
										rows={3}
										value={field.state.value || ""}
										onBlur={field.handleBlur}
										onChange={(e) => field.handleChange(e.target.value)}
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
								</FormField>
							);
						}}
					</form.Field>

					{actionType === "Tolak" && (
						<div className="flex items-start gap-2.5 p-2.5 rounded-lg bg-rose-50 dark:bg-rose-950/40 border border-rose-200 dark:border-rose-800 text-xs text-rose-700 dark:text-rose-300">
							<AlertTriangle className="h-4 w-4 shrink-0 mt-0.5" />
							<span>
								Tindakan ini tidak dapat dibatalkan secara otomatis. Status
								berkas akan dialihkan ke Regional Admin untuk ditindaklanjuti.
							</span>
						</div>
					)}

					<DialogFooter className="gap-2 sm:gap-0 pt-2">
						<Button type="button" variant="outline" onClick={onClose}>
							Batal
						</Button>
						<form.Subscribe
							selector={(state) => [state.canSubmit, state.isSubmitting]}
						>
							{([canSubmit, isSubmitting]) => (
								<Button
									type="submit"
									variant={
										actionType === "Tolak"
											? "destructive"
											: actionType === "Setuju"
												? "default"
												: "secondary"
									}
									disabled={!canSubmit || isSubmitting}
								>
									{isSubmitting && (
										<Loader2 className="h-4 w-4 mr-2 animate-spin" />
									)}
									{actionType === "Setuju" && "Setujui Permohonan"}
									{actionType === "Revisi" && "Minta Revisi"}
									{actionType === "Tolak" && "Tolak Berkas"}
									{actionType === "Reassign" && "Tugaskan Reviewer"}
								</Button>
							)}
						</form.Subscribe>
					</DialogFooter>
				</form>
			</DialogContent>
		</Dialog>
	);
}
