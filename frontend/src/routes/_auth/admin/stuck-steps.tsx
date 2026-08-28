import { createFileRoute } from "@tanstack/react-router";
import {
	ArrowRightLeft,
	Building2,
	Clock,
	Loader2,
	MapPin,
	OctagonAlert,
	Search,
	ShieldAlert,
	UserCheck,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { StuckStepItemDto } from "@/api/types";
import { Alert, AlertDescription } from "@/components/ui/alert";
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
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";

export const Route = createFileRoute("/_auth/admin/stuck-steps")({
	component: StuckStepsPage,
});

function StuckStepsPage() {
	const [searchTerm, setSearchTerm] = React.useState("");
	const [selectedStep, setSelectedStep] =
		React.useState<StuckStepItemDto | null>(null);
	const [targetUserId, setTargetUserId] = React.useState<string>("");
	const [error, setError] = React.useState<string | null>(null);

	const {
		data: stuckSteps,
		isLoading,
		refetch,
	} = $api.useQuery("get", "/api/admin/stuck-steps");

	const { data: usersData } = $api.useQuery("get", "/api/admin/users");

	const reassignMutation = $api.useMutation(
		"post",
		"/api/admin/stuck-steps/reassign",
		{
			onSuccess: () => {
				setSelectedStep(null);
				setTargetUserId("");
				setError(null);
				refetch();
			},
			onError: (err: unknown) => {
				const errorObj = err as { error?: string; errors?: string[] };
				setError(
					errorObj?.error ||
						errorObj?.errors?.[0] ||
						"Gagal mengalihkan penugasan.",
				);
			},
		},
	);

	const stepList = stuckSteps || [];
	const allUsers = usersData || [];

	const filteredSteps = React.useMemo(() => {
		if (!searchTerm.trim()) return stepList;
		const q = searchTerm.toLowerCase();
		return stepList.filter((s) => {
			return (
				s.companyName.toLowerCase().includes(q) ||
				s.companyNomor.toLowerCase().includes(q) ||
				s.regionName.toLowerCase().includes(q) ||
				s.areaName.toLowerCase().includes(q) ||
				s.stepKind.toLowerCase().includes(q) ||
				s.assignedUserName.toLowerCase().includes(q)
			);
		});
	}, [stepList, searchTerm]);

	const handleOpenReassign = (step: StuckStepItemDto) => {
		setSelectedStep(step);
		setTargetUserId("");
		setError(null);
	};

	const handleReassignSubmit = (e: React.FormEvent) => {
		e.preventDefault();
		if (!selectedStep || !targetUserId) {
			setError("Silakan pilih pengguna tujuan pengalihan.");
			return;
		}

		reassignMutation.mutate({
			body: {
				stepId: selectedStep.stepId,
				targetUserId,
			},
		});
	};

	return (
		<div className="space-y-4">
			{/* Top Header */}
			<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
				<div className="space-y-0.5">
					<h2 className="text-lg font-bold tracking-tight text-foreground flex items-center gap-2">
						<OctagonAlert className="h-5 w-5 text-amber-600 dark:text-amber-400" />
						<span>Langkah Tertahan — Lintas Wilayah (System Admin)</span>
					</h2>
					<p className="text-xs text-muted-foreground">
						Pusat pengawasan dan pemulihan langkah workflow yang terhenti akibat
						petugas nonaktif atau kendala wilayah.
					</p>
				</div>

				<div className="flex items-center gap-2">
					<div className="relative w-64">
						<Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
						<Input
							placeholder="Cari perusahaan, wilayah, peran..."
							value={searchTerm}
							onChange={(e) => setSearchTerm(e.target.value)}
							className="pl-8 h-9 text-xs"
						/>
					</div>
				</div>
			</div>

			{/* Info Box */}
			<Alert className="bg-muted/40 border-muted text-muted-foreground py-2.5">
				<ShieldAlert className="h-4 w-4 text-primary shrink-0" />
				<AlertDescription className="text-xs leading-relaxed">
					System Admin dapat mengalihkan langkah persetujuan yang tertahan ke
					petugas aktif lainnya tanpa perlu membuka atau melihat rincian data
					komersial pelanggan.
				</AlertDescription>
			</Alert>

			{/* Stuck Steps Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table>
					<TableHeader className="bg-muted/40">
						<TableRow>
							<TableHead className="font-semibold text-xs py-3">
								Perusahaan & Nomor
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Wilayah & Area
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Langkah Workflow
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Penugasan Saat Ini
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Lama Tertahan
							</TableHead>
							<TableHead className="text-right font-semibold text-xs py-3 pr-4">
								Tindakan
							</TableHead>
						</TableRow>
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableRow>
								<TableCell colSpan={6} className="text-center py-12">
									<div className="flex flex-col items-center justify-center gap-2 text-muted-foreground">
										<Loader2 className="h-6 w-6 animate-spin text-primary" />
										<span className="text-sm font-medium">
											Memeriksa antrean workflow...
										</span>
									</div>
								</TableCell>
							</TableRow>
						) : filteredSteps.length === 0 ? (
							<TableRow>
								<TableCell colSpan={6} className="text-center py-12">
									<div className="flex flex-col items-center justify-center gap-1.5 text-muted-foreground">
										<UserCheck className="h-8 w-8 text-emerald-600/80" />
										<span className="text-sm font-medium text-foreground">
											Tidak Ada Langkah Tertahan
										</span>
										<span className="text-xs">
											Semua tahapan persetujuan berjalan normal tanpa antrean
											buntu.
										</span>
									</div>
								</TableCell>
							</TableRow>
						) : (
							filteredSteps.map((step) => (
								<TableRow
									key={step.stepId}
									className="hover:bg-muted/30 transition-colors"
								>
									{/* Company */}
									<TableCell className="py-3">
										<div className="flex flex-col">
											<span className="font-semibold text-foreground text-sm flex items-center gap-1.5">
												<Building2 className="h-3.5 w-3.5 text-primary shrink-0" />
												<span>{step.companyName}</span>
											</span>
											<span className="font-mono text-[11px] text-muted-foreground">
												{step.companyNomor}
											</span>
										</div>
									</TableCell>

									{/* Territory */}
									<TableCell className="py-3 text-xs">
										<div className="flex flex-col">
											<span className="font-medium text-foreground">
												{step.regionName}
											</span>
											<span className="text-muted-foreground flex items-center gap-1">
												<MapPin className="h-3 w-3" />
												<span>{step.areaName}</span>
											</span>
										</div>
									</TableCell>

									{/* Step Kind */}
									<TableCell className="py-3">
										<Badge variant="outline" className="font-mono text-xs">
											{step.stepKind}
										</Badge>
									</TableCell>

									{/* Current Assignee */}
									<TableCell className="py-3 text-xs">
										<span className="font-medium text-foreground">
											{step.assignedUserName}
										</span>
									</TableCell>

									{/* Elapsed Time */}
									<TableCell className="py-3 text-xs">
										<div className="flex items-center gap-1.5">
											<Clock className="h-3.5 w-3.5 text-amber-600" />
											<span className="font-semibold text-amber-700 dark:text-amber-400">
												{step.elapsedDays} hari
											</span>
										</div>
									</TableCell>

									{/* Action */}
									<TableCell className="py-3 text-right pr-4">
										<Button
											size="sm"
											variant="outline"
											onClick={() => handleOpenReassign(step)}
											className="h-8 gap-1.5 text-xs border-amber-300 hover:bg-amber-50 dark:border-amber-800 dark:hover:bg-amber-950/40"
										>
											<ArrowRightLeft className="h-3.5 w-3.5 text-amber-600" />
											<span>Alihkan Tugas</span>
										</Button>
									</TableCell>
								</TableRow>
							))
						)}
					</TableBody>
				</Table>
			</div>

			{/* Reassign Dialog */}
			<Dialog
				open={Boolean(selectedStep)}
				onOpenChange={(open) => {
					if (!open) setSelectedStep(null);
				}}
			>
				<DialogContent className="sm:max-w-[480px]">
					<DialogHeader>
						<DialogTitle className="flex items-center gap-2 text-sm font-semibold">
							<ArrowRightLeft className="h-4 w-4 text-primary" />
							<span>Alihkan Penugasan Langkah Workflow</span>
						</DialogTitle>
						<DialogDescription className="text-xs">
							Tetapkan petugas baru untuk menyelesaikan langkah persetujuan pada{" "}
							<strong className="text-foreground">
								{selectedStep?.companyName}
							</strong>
							.
						</DialogDescription>
					</DialogHeader>

					{error && (
						<Alert variant="destructive">
							<AlertDescription className="text-xs">{error}</AlertDescription>
						</Alert>
					)}

					<form onSubmit={handleReassignSubmit} className="space-y-4 py-2">
						<div className="rounded-lg border bg-muted/30 p-3 space-y-1.5 text-xs">
							<div className="flex justify-between">
								<span className="text-muted-foreground">Langkah Workflow:</span>
								<strong className="font-mono text-foreground">
									{selectedStep?.stepKind}
								</strong>
							</div>
							<div className="flex justify-between">
								<span className="text-muted-foreground">Wilayah:</span>
								<span className="text-foreground">
									{selectedStep?.regionName} ({selectedStep?.areaName})
								</span>
							</div>
							<div className="flex justify-between">
								<span className="text-muted-foreground">Petugas Saat Ini:</span>
								<span className="text-foreground">
									{selectedStep?.assignedUserName}
								</span>
							</div>
						</div>

						<div className="space-y-1.5">
							<Label htmlFor="target-user" className="text-xs font-medium">
								Pilih Petugas Baru <span className="text-destructive">*</span>
							</Label>
							<select
								id="target-user"
								value={targetUserId}
								onChange={(e) => setTargetUserId(e.target.value)}
								className="w-full h-9 px-3 rounded-md border bg-background text-xs"
								required
							>
								<option value="">-- Pilih Petugas Pengganti --</option>
								{allUsers
									.filter(
										(u) => u.active && u.id !== selectedStep?.assignedUserId,
									)
									.map((u) => (
										<option key={u.id} value={u.id}>
											{u.fullName} ({u.roles.map((r) => r.role).join(", ")})
										</option>
									))}
							</select>
						</div>

						<DialogFooter className="pt-2">
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => setSelectedStep(null)}
								disabled={reassignMutation.isPending}
							>
								Batal
							</Button>
							<Button
								type="submit"
								size="sm"
								disabled={reassignMutation.isPending}
							>
								{reassignMutation.isPending ? (
									<>
										<Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" />
										<span>Mengalihkan...</span>
									</>
								) : (
									"Alihkan Penugasan"
								)}
							</Button>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>
		</div>
	);
}
