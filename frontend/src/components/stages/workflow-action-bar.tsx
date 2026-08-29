import { useQueryClient } from "@tanstack/react-query";
import {
	AlertOctagon,
	CheckCircle2,
	Loader2,
	MessageSquare,
	RotateCcw,
	Undo2,
	XCircle,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type { CompanyRecordDto } from "@/api/types";
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
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";

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
	const [comment, setComment] = React.useState<string>("");

	// Step Action Mutation (Approve, Revise, Reject)
	const actOnStepMutation = $api.useMutation(
		"post",
		"/api/workflow/steps/{stepId}/act",
		{
			onSuccess: () => {
				toast.success("Aksi berhasil diproses!");
				setActiveModal(null);
				setComment("");
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}",
						{ params: { path: { id: company.id } } },
					],
				});
				onActionSuccess?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error
						? error.message
						: "Gagal memproses aksi langkah",
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
				setComment("");
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}",
						{ params: { path: { id: company.id } } },
					],
				});
				onActionSuccess?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error ? error.message : "Gagal melakukan Rework",
				);
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
				setComment("");
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}",
						{ params: { path: { id: company.id } } },
					],
				});
				onActionSuccess?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error ? error.message : "Gagal menghentikan proses",
				);
			},
		},
	);

	const isPending =
		actOnStepMutation.isPending ||
		reworkMutation.isPending ||
		discontinueMutation.isPending;

	// Handle Action Execution
	const handleConfirmAction = () => {
		if (activeModal === "approve" && currentStepId) {
			actOnStepMutation.mutate({
				params: { path: { stepId: currentStepId } },
				body: { action: "Setuju", comment: comment || null },
			});
		} else if (activeModal === "revise" && currentStepId) {
			if (!comment.trim()) {
				toast.error("Wajib mengisi catatan/alasan revisi!");
				return;
			}
			actOnStepMutation.mutate({
				params: { path: { stepId: currentStepId } },
				body: { action: "Revisi", comment },
			});
		} else if (activeModal === "reject" && currentStepId) {
			if (!comment.trim()) {
				toast.error("Wajib mengisi alasan penolakan!");
				return;
			}
			actOnStepMutation.mutate({
				params: { path: { stepId: currentStepId } },
				body: { action: "Tolak", comment },
			});
		} else if (activeModal === "rework") {
			reworkMutation.mutate({
				params: { path: { id: company.id } },
				body: { comment: comment || null },
			});
		} else if (activeModal === "discontinue") {
			if (!comment.trim()) {
				toast.error("Wajib memberikan alasan penghentian proses!");
				return;
			}
			discontinueMutation.mutate({
				params: { path: { id: company.id } },
				body: { comment },
			});
		}
	};

	// Determine available actions
	const canAct = company.canAct && !!currentStepId;
	const canRework =
		company.status !== "Draft" && company.status !== "Discontinued";
	const canDiscontinue = company.status !== "Discontinued";

	if (!canAct && !canRework && !canDiscontinue) {
		return null;
	}

	return (
		<>
			<div className="flex flex-wrap items-center justify-between gap-3 p-3.5 bg-card/95 backdrop-blur-xs border border-primary/20 rounded-xl shadow-sm">
				<div className="flex items-center gap-3">
					<div className="size-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center">
						<MessageSquare className="size-4" />
					</div>
					<div>
						<div className="flex items-center gap-2">
							<span className="text-xs font-semibold text-foreground">
								Aksi Alur Kerja (Workflow Gate)
							</span>
							{company.currentStepKind && (
								<Badge
									variant="outline"
									className="text-[10px] bg-primary/5 text-primary border-primary/20"
								>
									Tahap: {company.currentStepKind}
								</Badge>
							)}
						</div>
						<p className="text-[11px] text-muted-foreground">
							{canAct
								? "Anda memiliki wewenang untuk meninjau dan mengambil keputusan pada tahap ini"
								: "Menu tindakan lanjutan untuk status berkas pelanggan"}
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
								onClick={() => {
									setActiveModal("reject");
									setComment("");
								}}
								className="h-8 text-xs text-destructive border-destructive/30 hover:bg-destructive/10"
							>
								<XCircle className="size-3.5 mr-1" /> Tolak
							</Button>

							<Button
								type="button"
								size="sm"
								variant="outline"
								onClick={() => {
									setActiveModal("revise");
									setComment("");
								}}
								className="h-8 text-xs text-amber-600 border-amber-300 dark:text-amber-400 dark:border-amber-700 hover:bg-amber-50 dark:hover:bg-amber-950/30"
							>
								<RotateCcw className="size-3.5 mr-1" /> Minta Revisi
							</Button>

							<Button
								type="button"
								size="sm"
								onClick={() => {
									setActiveModal("approve");
									setComment("");
								}}
								className="h-8 text-xs bg-emerald-600 hover:bg-emerald-700 text-white shadow-xs"
							>
								<CheckCircle2 className="size-3.5 mr-1" /> Setujui Langkah
							</Button>
						</>
					)}

					{/* Rework Button */}
					{canRework && !canAct && (
						<Button
							type="button"
							size="sm"
							variant="outline"
							onClick={() => {
								setActiveModal("rework");
								setComment("");
							}}
							className="h-8 text-xs text-amber-600 border-amber-300 hover:bg-amber-50"
						>
							<Undo2 className="size-3.5 mr-1" /> Rework
						</Button>
					)}

					{/* Discontinue Button */}
					{canDiscontinue && (
						<Button
							type="button"
							size="sm"
							variant="ghost"
							onClick={() => {
								setActiveModal("discontinue");
								setComment("");
							}}
							className="h-8 text-xs text-muted-foreground hover:text-destructive"
						>
							<AlertOctagon className="size-3.5 mr-1" /> Hentikan Proses
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

					<div className="space-y-2 py-2">
						<Label className="text-xs font-medium">
							{activeModal === "approve"
								? "Catatan Tambahan (Opsional)"
								: "Catatan / Alasan Keputusan (Wajib)"}
						</Label>
						<Textarea
							value={comment}
							onChange={(e) => setComment(e.target.value)}
							placeholder={
								activeModal === "approve"
									? "Tuliskan catatan opsional..."
									: "Tuliskan alasan/keterangan yang jelas..."
							}
							className="text-xs min-h-[80px]"
						/>
					</div>

					<DialogFooter className="flex items-center justify-end gap-2 pt-2">
						<Button
							variant="outline"
							size="sm"
							onClick={() => setActiveModal(null)}
							disabled={isPending}
							className="text-xs"
						>
							Batal
						</Button>
						<Button
							size="sm"
							disabled={isPending}
							onClick={handleConfirmAction}
							className={`text-xs text-white ${
								activeModal === "approve"
									? "bg-emerald-600 hover:bg-emerald-700"
									: activeModal === "revise" || activeModal === "rework"
										? "bg-amber-600 hover:bg-amber-700"
										: "bg-destructive hover:bg-destructive/90"
							}`}
						>
							{isPending && <Loader2 className="size-3 animate-spin mr-1.5" />}
							{activeModal === "approve" && "Setujui"}
							{activeModal === "revise" && "Kirim Permintaan Revisi"}
							{activeModal === "reject" && "Tolak Berkas"}
							{activeModal === "rework" && "Proses Rework"}
							{activeModal === "discontinue" && "Hentikan Proses"}
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</>
	);
}
