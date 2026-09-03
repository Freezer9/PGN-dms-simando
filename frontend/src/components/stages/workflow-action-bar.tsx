import { useForm } from "@tanstack/react-form";
import { useQueryClient } from "@tanstack/react-query";
import {
	AlertOctagon,
	CheckCircle2,
	Loader2,
	RotateCcw,
	Undo2,
	XCircle,
	Zap,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type { CompanyRecordDto } from "@/api/types";
import { FormField } from "@/components/form/form-field";
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
import { Textarea } from "@/components/ui/textarea";
import {
	type WorkflowActionBarFormValues,
	workflowActionBarSchema,
} from "@/lib/schemas";

interface WorkflowActionBarProps {
	company: CompanyRecordDto;
	onActionSuccess?: () => void;
}

type ActionModalType =
	| "approve"
	| "revise"
	| "reject"
	| "rework"
	| "discontinue"
	| null;

export function WorkflowActionBar({
	company,
	onActionSuccess,
}: WorkflowActionBarProps) {
	const queryClient = useQueryClient();
	const currentStepId = company.currentStepId;

	const [activeModal, setActiveModal] = React.useState<ActionModalType>(null);

	// Invalidate company query helper
	const invalidateCompany = React.useCallback(() => {
		queryClient.invalidateQueries({
			queryKey: [
				"get",
				"/api/companies/{id}",
				{ params: { path: { id: company.id } } },
			],
		});
		onActionSuccess?.();
	}, [queryClient, company.id, onActionSuccess]);

	// Step Action Mutation (Approve, Revise, Reject)
	const actOnStepMutation = $api.useMutation(
		"post",
		"/api/workflow/steps/{stepId}/act",
		{
			onSuccess: () => {
				toast.success("Aksi berhasil diproses!");
				setActiveModal(null);
				invalidateCompany();
			},
			onError: (error) => {
				toast.error(
					error.detail || error.title || "Gagal memproses aksi langkah",
				);
			},
		},
	);

	// Rework Mutation
	const reworkMutation = $api.useMutation(
		"post",
		"/api/companies/{id}/workflow/rework",
		{
			onSuccess: () => {
				toast.success("Proses dikembalikan ke Rework!");
				setActiveModal(null);
				invalidateCompany();
			},
			onError: (error) => {
				toast.error(error.detail || error.title || "Gagal melakukan Rework");
			},
		},
	);

	// Discontinue Mutation
	const discontinueMutation = $api.useMutation(
		"post",
		"/api/companies/{id}/workflow/discontinue",
		{
			onSuccess: () => {
				toast.warning("Proses resmi dihentikan (Discontinued)!");
				setActiveModal(null);
				invalidateCompany();
			},
			onError: (error) => {
				toast.error(error.detail || error.title || "Gagal menghentikan proses");
			},
		},
	);

	const form = useForm({
		defaultValues: {
			comment: "",
		} as WorkflowActionBarFormValues,
		validators: {
			onSubmit: workflowActionBarSchema,
		},
		onSubmit: async ({ value }) => {
			const commentTrimmed = value.comment?.trim() || "";

			if (activeModal === "approve" && currentStepId) {
				await actOnStepMutation.mutateAsync({
					params: { path: { stepId: currentStepId } },
					body: { action: "Setuju", comment: commentTrimmed || null },
				});
				return;
			}

			if (activeModal === "revise" && currentStepId) {
				if (!commentTrimmed) {
					toast.error("Wajib mengisi catatan/alasan revisi!");
					return;
				}
				await actOnStepMutation.mutateAsync({
					params: { path: { stepId: currentStepId } },
					body: { action: "Revisi", comment: commentTrimmed },
				});
				return;
			}

			if (activeModal === "reject" && currentStepId) {
				if (!commentTrimmed) {
					toast.error("Wajib mengisi alasan penolakan!");
					return;
				}
				await actOnStepMutation.mutateAsync({
					params: { path: { stepId: currentStepId } },
					body: { action: "Tolak", comment: commentTrimmed },
				});
				return;
			}

			if (activeModal === "rework") {
				await reworkMutation.mutateAsync({
					params: { path: { id: company.id } },
					body: { comment: commentTrimmed || null },
				});
				return;
			}

			if (activeModal === "discontinue") {
				if (!commentTrimmed) {
					toast.error("Wajib memberikan alasan penghentian proses!");
					return;
				}
				await discontinueMutation.mutateAsync({
					params: { path: { id: company.id } },
					body: { comment: commentTrimmed },
				});
			}
		},
	});

	// Reset form when modal opens
	React.useEffect(() => {
		if (activeModal !== null) {
			form.reset({
				comment: "",
			});
		}
	}, [activeModal, form]);

	// Determine available actions
	const canAct = company.canAct && !!currentStepId;
	const canRework =
		company.status !== "Draft" &&
		company.status !== "Discontinued" &&
		company.canAct;
	const canDiscontinue = company.status !== "Discontinued" && company.canAct;

	// Only show action banner when the current user has authority to act
	if (!canAct && !canRework) {
		return null;
	}

	const isCommentRequired =
		activeModal === "revise" ||
		activeModal === "reject" ||
		activeModal === "discontinue";

	return (
		<>
			<div className="flex flex-wrap items-center justify-between gap-3 p-3.5 bg-amber-500/10 dark:bg-amber-950/30 border border-amber-500/30 rounded-xl shadow-xs">
				<div className="flex items-center gap-3">
					<div className="size-8 rounded-lg bg-amber-500/20 text-amber-700 dark:text-amber-400 flex items-center justify-center shrink-0">
						<Zap className="size-4" />
					</div>
					<div>
						<div className="flex items-center gap-2">
							<span className="text-xs font-bold text-amber-900 dark:text-amber-300">
								⚡ TINDAKAN DIPERLUKAN
							</span>
							{company.currentStepKind && (
								<Badge
									variant="outline"
									className="text-[10px] bg-background text-amber-800 dark:text-amber-300 border-amber-400 font-mono"
								>
									Tahap: {company.currentStepKind}
								</Badge>
							)}
						</div>
						<p className="text-[11px] text-muted-foreground mt-0.5">
							Berkas ini memerlukan tindakan evaluasi atau persetujuan Anda
							untuk melanjutkan ke proses berikutnya.
						</p>
					</div>
				</div>

				<div className="flex flex-wrap items-center gap-2">
					{/* Act on Step Buttons */}
					{canAct && (
						<>
							<Button
								type="button"
								size="sm"
								variant="outline"
								onClick={() => setActiveModal("reject")}
								className="h-8 text-xs text-destructive border-destructive/30 hover:bg-destructive/10"
							>
								<XCircle className="size-3.5" /> Tolak
							</Button>

							<Button
								type="button"
								size="sm"
								variant="outline"
								onClick={() => setActiveModal("revise")}
								className="h-8 text-xs text-amber-700 border-amber-300 dark:text-amber-400 dark:border-amber-700 hover:bg-amber-100 dark:hover:bg-amber-950/40"
							>
								<RotateCcw className="size-3.5" /> Minta Revisi
							</Button>

							<Button
								type="button"
								size="sm"
								onClick={() => setActiveModal("approve")}
								className="h-8 text-xs bg-emerald-600 hover:bg-emerald-700 text-white shadow-xs"
							>
								<CheckCircle2 className="size-3.5" /> Setujui Langkah
							</Button>
						</>
					)}

					{/* Rework Button */}
					{canRework && !canAct && (
						<Button
							type="button"
							size="sm"
							variant="outline"
							onClick={() => setActiveModal("rework")}
							className="h-8 text-xs text-amber-700 border-amber-300 hover:bg-amber-50"
						>
							<Undo2 className="size-3.5" /> Rework
						</Button>
					)}

					{/* Discontinue Button */}
					{canDiscontinue && (
						<Button
							type="button"
							size="sm"
							variant="ghost"
							onClick={() => setActiveModal("discontinue")}
							className="h-8 text-xs text-muted-foreground hover:text-destructive ml-1"
							title="Hentikan Proses Berkas"
						>
							<AlertOctagon className="size-3.5" />
						</Button>
					)}
				</div>
			</div>

			{/* ACTION CONFIRMATION DIALOG */}
			<Dialog
				open={activeModal !== null}
				onOpenChange={(open) => !open && setActiveModal(null)}
			>
				<DialogContent className="sm:max-w-md">
					<DialogHeader>
						<DialogTitle className="text-base flex items-center gap-2">
							{activeModal === "approve" && (
								<>
									<CheckCircle2 className="size-4 text-emerald-600" />
									Konfirmasi Persetujuan Langkah
								</>
							)}
							{activeModal === "revise" && (
								<>
									<RotateCcw className="size-4 text-amber-600" />
									Permintaan Revisi Berkas
								</>
							)}
							{activeModal === "reject" && (
								<>
									<XCircle className="size-4 text-destructive" />
									Konfirmasi Penolakan Berkas
								</>
							)}
							{activeModal === "rework" && (
								<>
									<Undo2 className="size-4 text-amber-600" />
									Kembalikan ke Rework (Perbaikan Sales)
								</>
							)}
							{activeModal === "discontinue" && (
								<>
									<AlertOctagon className="size-4 text-destructive" />
									Hentikan Proses Pelanggan (Discontinue)
								</>
							)}
						</DialogTitle>
						<DialogDescription className="text-xs">
							{activeModal === "approve" &&
								"Apakah Anda yakin ingin menyetujui langkah evaluasi ini dan melanjutkannya ke tahap berikutnya?"}
							{activeModal === "revise" &&
								"Masukkan catatan revisi yang jelas agar tim sales/evaluator dapat memperbaiki data."}
							{activeModal === "reject" &&
								"Masukkan alasan penolakan berkas permohonan pelanggan ini."}
							{activeModal === "rework" &&
								"Berkas akan dikembalikan ke status Rework untuk diperbaiki oleh Sales/Admin."}
							{activeModal === "discontinue" &&
								"Tindakan ini akan menghentikan seluruh proses berlangganan calon pelanggan ini. Masukkan alasan resmi penghentian."}
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
						<form.Field name="comment">
							{(field) => {
								const error = field.state.meta.errors[0]?.message;
								const labelText =
									activeModal === "approve"
										? "Catatan Tambahan (Opsional)"
										: "Catatan / Alasan Keputusan";

								return (
									<FormField
										label={labelText}
										htmlFor="workflow-action-comment"
										required={isCommentRequired}
										error={error}
									>
										<Textarea
											id="workflow-action-comment"
											name={field.name}
											value={field.state.value || ""}
											onBlur={field.handleBlur}
											onChange={(e) => field.handleChange(e.target.value)}
											placeholder={
												activeModal === "approve"
													? "Tuliskan catatan opsional..."
													: "Tuliskan alasan/keterangan yang jelas..."
											}
											className="text-xs min-h-[80px]"
										/>
									</FormField>
								);
							}}
						</form.Field>

						<DialogFooter className="flex items-center justify-end gap-2 pt-2">
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => setActiveModal(null)}
								className="text-xs"
							>
								Batal
							</Button>
							<form.Subscribe
								selector={(state) => [state.canSubmit, state.isSubmitting]}
							>
								{([canSubmit, isSubmitting]) => (
									<Button
										type="submit"
										size="sm"
										disabled={!canSubmit || isSubmitting}
										className={`text-xs text-white ${
											activeModal === "approve"
												? "bg-emerald-600 hover:bg-emerald-700"
												: activeModal === "revise" || activeModal === "rework"
													? "bg-amber-600 hover:bg-amber-700"
													: "bg-destructive hover:bg-destructive/90"
										}`}
									>
										{isSubmitting && (
											<Loader2 className="size-3 animate-spin mr-1.5" />
										)}
										{activeModal === "approve" && "Setujui"}
										{activeModal === "revise" && "Kirim Permintaan Revisi"}
										{activeModal === "reject" && "Tolak Berkas"}
										{activeModal === "rework" && "Proses Rework"}
										{activeModal === "discontinue" && "Hentikan Proses"}
									</Button>
								)}
							</form.Subscribe>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>
		</>
	);
}
