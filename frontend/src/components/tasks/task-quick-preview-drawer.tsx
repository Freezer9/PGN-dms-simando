import { Link } from "@tanstack/react-router";
import {
	Building2,
	CheckCircle2,
	ExternalLink,
	Loader2,
	MapPin,
	Phone,
	RotateCcw,
	User,
	UserCheck,
	XCircle,
} from "lucide-react";
import { $api } from "@/api/client";
import type { TaskListItem } from "@/api/types";
import { SlaClockBadge } from "@/components/tasks/sla-clock-badge";
import type { TaskActionModalType } from "@/components/tasks/task-action-modal";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Sheet,
	SheetContent,
	SheetDescription,
	SheetFooter,
	SheetHeader,
	SheetTitle,
} from "@/components/ui/sheet";

export interface TaskQuickPreviewDrawerProps {
	task: TaskListItem | null;
	isOpen: boolean;
	onClose: () => void;
	onTakeAction: (
		task: TaskListItem,
		actionType: NonNullable<TaskActionModalType>,
	) => void;
}

export function TaskQuickPreviewDrawer({
	task,
	isOpen,
	onClose,
	onTakeAction,
}: TaskQuickPreviewDrawerProps) {
	// Query full company record when drawer is open
	const { data: company, isLoading } = $api.useQuery(
		"get",
		"/api/companies/{id}",
		{
			params: { path: { id: task?.companyId ?? "" } },
		},
		{
			enabled: isOpen && !!task?.companyId,
		},
	);

	if (!task) return null;

	return (
		<Sheet open={isOpen} onOpenChange={(open) => !open && onClose()}>
			<SheetContent
				side="right"
				className="flex flex-col sm:max-w-xl overflow-y-auto"
			>
				<SheetHeader className="pb-4 border-b">
					<div className="flex items-center justify-between gap-2 pr-6">
						<Badge
							variant="outline"
							className="font-mono text-xs text-muted-foreground"
						>
							{task.nomor}
						</Badge>
						<SlaClockBadge waitingSince={task.waitingSince} />
					</div>
					<SheetTitle className="text-xl font-bold text-foreground flex items-center gap-2 mt-1">
						<Building2 className="h-5 w-5 text-primary shrink-0" />
						<span>{task.namaPerusahaan}</span>
					</SheetTitle>
					<SheetDescription className="flex items-center gap-2 text-xs">
						<span>{task.industryTypeName || "Industri"}</span>
						<span>•</span>
						<span>
							{task.areaName} ({task.regionName})
						</span>
					</SheetDescription>
				</SheetHeader>

				<div className="flex-1 py-4 space-y-5">
					{/* Status & Review Turn Summary */}
					<div className="p-3.5 bg-muted/50 rounded-xl border space-y-2">
						<div className="flex items-center justify-between text-xs">
							<span className="text-muted-foreground font-medium">
								Tahap Verifikasi
							</span>
							<Badge variant="secondary" className="font-medium">
								{task.stepKind ?? "Persetujuan"}
							</Badge>
						</div>
						<div className="flex items-center justify-between text-xs">
							<span className="text-muted-foreground font-medium">
								Diajukan Oleh
							</span>
							<span className="font-medium text-foreground">
								{task.submittedByName || "-"}
							</span>
						</div>
						<div className="flex items-center justify-between text-xs">
							<span className="text-muted-foreground font-medium">
								Menunggu Sejak
							</span>
							<span className="text-foreground">
								{new Date(task.waitingSince).toLocaleString("id-ID", {
									dateStyle: "medium",
									timeStyle: "short",
								})}
							</span>
						</div>
					</div>

					{/* Loading State */}
					{isLoading && (
						<div className="flex items-center justify-center py-12 text-muted-foreground">
							<Loader2 className="h-6 w-6 animate-spin mr-2" />
							<span>Memuat rincian perusahaan...</span>
						</div>
					)}

					{/* Detailed Content */}
					{company && (
						<div className="space-y-4 text-sm">
							{/* Location and Address */}
							<div className="space-y-1.5">
								<h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
									Lokasi & Alamat
								</h4>
								<div className="p-3 rounded-lg border bg-card/60 space-y-1.5 text-xs">
									<div className="flex items-start gap-2 text-muted-foreground">
										<MapPin className="h-3.5 w-3.5 mt-0.5 shrink-0 text-primary" />
										<span>{company.alamat || "Alamat belum diisi"}</span>
									</div>
									<div className="flex items-center gap-2 text-muted-foreground pl-5.5">
										<span>
											{company.villageName}, {company.districtName},{" "}
											{company.regencyName}, {company.provinceName}
										</span>
									</div>
								</div>
							</div>

							{/* Contact Information */}
							{company.contact && (
								<div className="space-y-1.5">
									<h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
										Kontak Person (PIC)
									</h4>
									<div className="p-3 rounded-lg border bg-card/60 space-y-2 text-xs">
										<div className="flex items-center justify-between">
											<div className="flex items-center gap-2 font-medium text-foreground">
												<User className="h-3.5 w-3.5 text-primary" />
												<span>{company.contact.nama}</span>
											</div>
											{company.contact.jabatan && (
												<Badge variant="outline" className="text-[10px]">
													{company.contact.jabatan}
												</Badge>
											)}
										</div>
										{company.contact.telp && (
											<div className="flex items-center gap-2 text-muted-foreground">
												<Phone className="h-3.5 w-3.5" />
												<span>{company.contact.telp}</span>
											</div>
										)}
									</div>
								</div>
							)}

							{/* Stage & Progress Info */}
							<div className="space-y-1.5">
								<h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
									Informasi Tahapan (Stage Gate)
								</h4>
								<div className="p-3 rounded-lg border bg-card/60 space-y-2 text-xs">
									<div className="flex items-center justify-between">
										<span className="text-muted-foreground">
											Tahap Saat Ini
										</span>
										<span className="font-semibold text-primary">
											Tahap {company.currentStage}:{" "}
											{getStageName(company.currentStage)}
										</span>
									</div>
									<div className="flex items-center justify-between">
										<span className="text-muted-foreground">
											Status Rekaman
										</span>
										<Badge variant="secondary">{company.status}</Badge>
									</div>
								</div>
							</div>
						</div>
					)}
				</div>

				<SheetFooter className="pt-4 border-t flex flex-col gap-2 sm:gap-2">
					<div className="grid grid-cols-3 gap-2 w-full">
						<Button
							size="sm"
							variant="outline"
							className="text-rose-600 border-rose-200 hover:bg-rose-50 dark:hover:bg-rose-950/50"
							onClick={() => onTakeAction(task, "Tolak")}
						>
							<XCircle className="h-4 w-4 mr-1.5" />
							Tolak
						</Button>
						<Button
							size="sm"
							variant="outline"
							className="text-amber-600 border-amber-200 hover:bg-amber-50 dark:hover:bg-amber-950/50"
							onClick={() => onTakeAction(task, "Revisi")}
						>
							<RotateCcw className="h-4 w-4 mr-1.5" />
							Revisi
						</Button>
						<Button
							size="sm"
							className="bg-emerald-600 hover:bg-emerald-700 text-white"
							onClick={() => onTakeAction(task, "Setuju")}
						>
							<CheckCircle2 className="h-4 w-4 mr-1.5" />
							Setuju
						</Button>
					</div>

					<div className="flex items-center justify-between w-full pt-1">
						<Button
							size="sm"
							variant="ghost"
							className="text-xs text-muted-foreground"
							onClick={() => onTakeAction(task, "Reassign")}
						>
							<UserCheck className="h-3.5 w-3.5 mr-1" />
							Tugaskan Ulang
						</Button>

						<Button
							asChild
							size="sm"
							variant="link"
							className="text-xs p-0 h-auto"
						>
							<Link
								to="/directory/$companyId"
								params={{ companyId: task.companyId }}
								onClick={onClose}
							>
								Buka Halaman Lengkap
								<ExternalLink className="h-3 w-3 ml-1" />
							</Link>
						</Button>
					</div>
				</SheetFooter>
			</SheetContent>
		</Sheet>
	);
}

function getStageName(stage: number): string {
	switch (stage) {
		case 1:
			return "Calon Pelanggan";
		case 2:
			return "Kontak & Profiling";
		case 3:
			return "Plotting & Geotagging";
		case 4:
			return "Survei KK0";
		case 5:
			return "Registrasi A1";
		case 6:
			return "Permohonan NOL";
		case 7:
			return "Evaluasi NOL";
		case 8:
			return "Penerbitan Surat NOL";
		default:
			return `Tahap ${stage}`;
	}
}
