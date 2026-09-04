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
	XCircle,
} from "lucide-react";
import { $api } from "@/api/client";
import type { TaskListItem } from "@/api/types";
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
import { getStageInfo } from "@/lib/directory-utils";
import { formatRole } from "@/lib/roles";

interface TaskQuickPreviewDrawerProps {
	task: TaskListItem | null;
	isOpen: boolean;
	onClose: () => void;
	onTakeAction: (task: TaskListItem, action: TaskActionModalType) => void;
}

export function TaskQuickPreviewDrawer({
	task,
	isOpen,
	onClose,
	onTakeAction,
}: TaskQuickPreviewDrawerProps) {
	// Fetch company details for quick inspection
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

	if (!task) {
		return null;
	}

	const primaryContact =
		company?.contacts?.find((c) => c.isPrimary) || company?.contacts?.[0];

	return (
		<Sheet open={isOpen} onOpenChange={(open) => !open && onClose()}>
			<SheetContent className="sm:max-w-md w-full overflow-y-auto">
				<SheetHeader className="pb-4 border-b">
					<div className="flex items-center gap-2">
						<Building2 className="h-5 w-5 text-primary" />
						<SheetTitle className="text-base font-bold text-foreground">
							{task.namaPerusahaan ||
								company?.namaPerusahaan ||
								"Pratinjau Pelanggan"}
						</SheetTitle>
					</div>
					<SheetDescription className="text-xs">
						No. Registrasi:{" "}
						<span className="font-mono font-semibold text-foreground">
							{task.nomor || company?.nomor || "-"}
						</span>
					</SheetDescription>
				</SheetHeader>

				<div className="py-4 space-y-6">
					{/* Task Context Card */}
					<div className="p-3.5 rounded-xl bg-muted/40 border space-y-2.5">
						<div className="flex items-center justify-between text-xs">
							<span className="text-muted-foreground font-medium">
								Tugas / Langkah:
							</span>
							<Badge variant="outline" className="font-semibold text-primary">
								{formatRole(task.stepKind) || "Pemeriksaan Berkas"}
							</Badge>
						</div>
						<div className="flex items-center justify-between text-xs">
							<span className="text-muted-foreground">Menunggu Sejak:</span>
							<span className="text-foreground font-medium">
								{task.waitingSince
									? new Date(task.waitingSince).toLocaleDateString("id-ID", {
											day: "numeric",
											month: "short",
											year: "numeric",
											hour: "2-digit",
											minute: "2-digit",
										})
									: "-"}
							</span>
						</div>
						<div className="flex items-center justify-between text-xs">
							<span className="text-muted-foreground">Area / Regional:</span>
							<span className="text-foreground font-medium">
								{task.areaName} / {task.regionName}
							</span>
						</div>
						<div className="flex items-center justify-between text-xs">
							<span className="text-muted-foreground">Diajukan Oleh:</span>
							<span className="text-foreground font-medium">
								{task.submittedByName || "-"}
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
							{primaryContact && (
								<div className="space-y-1.5">
									<h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
										Kontak PIC Pelanggan
									</h4>
									<div className="p-3 rounded-lg border bg-card/60 space-y-2 text-xs">
										<div className="flex items-center justify-between">
											<div className="flex items-center gap-2 font-medium text-foreground">
												<User className="h-3.5 w-3.5 text-primary" />
												<span>{primaryContact.nama}</span>
											</div>
											{primaryContact.jabatan && (
												<Badge variant="outline" className="text-[10px]">
													{primaryContact.jabatan}
												</Badge>
											)}
										</div>
										{(primaryContact.noHp || primaryContact.email) && (
											<div className="flex items-center gap-2 text-muted-foreground">
												<Phone className="h-3.5 w-3.5" />
												<span>
													{primaryContact.noHp || primaryContact.email}
												</span>
											</div>
										)}
									</div>
								</div>
							)}

							{/* Stage & Progress Info */}
							<div className="space-y-1.5">
								<h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
									Informasi Tahapan Pipeline
								</h4>
								<div className="p-3 rounded-lg border bg-card/60 space-y-2 text-xs">
									<div className="flex items-center justify-between">
										<span className="text-muted-foreground">
											Tahap Saat Ini
										</span>
										<span className="font-semibold text-primary">
											{getStageInfo(Number(company.currentStage)).name}
										</span>
									</div>
									<div className="flex items-center justify-between">
										<span className="text-muted-foreground">Status Berkas</span>
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
							<XCircle className="size-4" />
							Tolak
						</Button>
						<Button
							size="sm"
							variant="outline"
							className="text-amber-600 border-amber-200 hover:bg-amber-50 dark:hover:bg-amber-950/50"
							onClick={() => onTakeAction(task, "Revisi")}
						>
							<RotateCcw className="size-4" />
							Minta Revisi
						</Button>
						<Button
							size="sm"
							className="bg-emerald-600 hover:bg-emerald-700 text-white"
							onClick={() => onTakeAction(task, "Setuju")}
						>
							<CheckCircle2 className="size-4" />
							Setujui
						</Button>
					</div>

					<Button
						asChild
						variant="ghost"
						size="sm"
						className="w-full text-xs text-muted-foreground hover:text-foreground"
					>
						<Link
							to="/directory/$companyId"
							params={{ companyId: task.companyId }}
						>
							<ExternalLink className="size-3.5" />
							Buka Berkas Lengkap Pelanggan
						</Link>
					</Button>
				</SheetFooter>
			</SheetContent>
		</Sheet>
	);
}
