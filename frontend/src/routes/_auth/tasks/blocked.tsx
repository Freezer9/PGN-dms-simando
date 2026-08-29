import { createFileRoute, Link } from "@tanstack/react-router";
import {
	AlertOctagon,
	Building2,
	CheckCircle2,
	Eye,
	Loader2,
	Search,
	ShieldAlert,
	User,
	UserCheck,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { TaskListItem } from "@/api/types";
import { SlaClockBadge } from "@/components/tasks/sla-clock-badge";
import {
	TaskActionModal,
	type TaskActionModalType,
} from "@/components/tasks/task-action-modal";
import { TaskQuickPreviewDrawer } from "@/components/tasks/task-quick-preview-drawer";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";

export const Route = createFileRoute("/_auth/tasks/blocked")({
	component: BlockedTasksPage,
});

function BlockedTasksPage() {
	const [searchTerm, setSearchTerm] = React.useState("");
	const [areaFilter, setAreaFilter] = React.useState<string>("all");

	// Active modal and drawer states
	const [modalTask, setModalTask] = React.useState<TaskListItem | null>(null);
	const [modalAction, setModalAction] =
		React.useState<TaskActionModalType>(null);

	const [drawerTask, setDrawerTask] = React.useState<TaskListItem | null>(null);
	const [isDrawerOpen, setIsDrawerOpen] = React.useState(false);

	// Query blocked tasks
	const {
		data: blockedTasks = [],
		isLoading,
		refetch,
	} = $api.useQuery("get", "/api/tasks/blocked");

	// Extract unique areas
	const availableAreas = React.useMemo(() => {
		const areas = new Set<string>();
		for (const t of blockedTasks) {
			if (t.areaName) areas.add(t.areaName);
		}
		return Array.from(areas).sort();
	}, [blockedTasks]);

	// Filter & sort (prioritize longest waiting)
	const filteredTasks = React.useMemo(() => {
		let list = [...blockedTasks];

		if (searchTerm.trim()) {
			const q = searchTerm.toLowerCase();
			list = list.filter(
				(t) =>
					t.namaPerusahaan?.toLowerCase().includes(q) ||
					t.nomor?.toLowerCase().includes(q) ||
					t.submittedByName?.toLowerCase().includes(q) ||
					t.areaName?.toLowerCase().includes(q),
			);
		}

		if (areaFilter !== "all") {
			list = list.filter((t) => t.areaName === areaFilter);
		}

		// Sort by longest waiting duration first
		list.sort(
			(a, b) =>
				new Date(a.waitingSince).getTime() - new Date(b.waitingSince).getTime(),
		);

		return list;
	}, [blockedTasks, searchTerm, areaFilter]);

	const handleOpenActionModal = (
		task: TaskListItem,
		action: NonNullable<TaskActionModalType>,
	) => {
		setModalTask(task);
		setModalAction(action);
	};

	const handleOpenDrawer = (task: TaskListItem) => {
		setDrawerTask(task);
		setIsDrawerOpen(true);
	};

	return (
		<div className="space-y-4">
			{/* SLA Escalation Notice Banner */}
			<div className="flex items-start gap-3 p-4 rounded-xl bg-rose-50/80 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-900/60 shadow-xs">
				<div className="p-2 rounded-lg bg-rose-100 dark:bg-rose-900/50 text-rose-600 dark:text-rose-400 shrink-0">
					<AlertOctagon className="h-5 w-5" />
				</div>
				<div className="space-y-1 text-xs">
					<h3 className="font-semibold text-rose-900 dark:text-rose-200 text-sm">
						Pemantauan Berkas Tertahan & Kemacetan (Bottleneck SLA)
					</h3>
					<p className="text-rose-700 dark:text-rose-300/90 leading-relaxed">
						Halaman ini menampilkan seluruh berkas pengajuan pelanggan yang
						telah menunggu lebih dari <strong>7 hari kalender</strong> pada
						suatu tahap atau belum memiliki reviewer yang ditugaskan. Regional
						Admin atau System Admin dapat melakukan penugasan ulang (reassign)
						untuk mempercepat kelancaran proses.
					</p>
				</div>
			</div>

			{/* Filter Bar */}
			<Card className="shadow-xs border bg-card/60">
				<CardContent className="p-3.5 space-y-3">
					<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
						<div className="relative">
							<Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
							<Input
								placeholder="Cari berkas tertahan..."
								value={searchTerm}
								onChange={(e) => setSearchTerm(e.target.value)}
								className="pl-9 h-9 text-xs"
							/>
						</div>

						<Select value={areaFilter} onValueChange={setAreaFilter}>
							<SelectTrigger className="h-9 text-xs">
								<SelectValue placeholder="Semua Area" />
							</SelectTrigger>
							<SelectContent>
								<SelectItem value="all">Semua Area</SelectItem>
								{availableAreas.map((area) => (
									<SelectItem key={area} value={area}>
										{area}
									</SelectItem>
								))}
							</SelectContent>
						</Select>

						<div className="flex items-center justify-end text-xs text-muted-foreground">
							Total Tertahan:{" "}
							<strong className="text-rose-600 dark:text-rose-400 ml-1 font-bold">
								{filteredTasks.length} berkas
							</strong>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* Blocked Tasks Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table>
					<TableHeader className="bg-muted/40">
						<TableRow>
							<TableHead className="font-semibold text-xs">
								Perusahaan
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Tahap Tertahan
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Wilayah & Area
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Diajukan Oleh
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Durasi Keterlambatan
							</TableHead>
							<TableHead className="text-right font-semibold text-xs pr-4">
								Tindakan Eskalasi
							</TableHead>
						</TableRow>
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableRow>
								<TableCell colSpan={6} className="text-center py-12">
									<div className="flex flex-col items-center justify-center gap-2 text-muted-foreground">
										<Loader2 className="h-6 w-6 animate-spin text-rose-500" />
										<span className="text-sm font-medium">
											Memuat berkas tertahan...
										</span>
									</div>
								</TableCell>
							</TableRow>
						) : filteredTasks.length === 0 ? (
							<TableRow>
								<TableCell colSpan={6} className="text-center py-14">
									<div className="flex flex-col items-center justify-center gap-2.5 max-w-sm mx-auto text-muted-foreground">
										<div className="p-3 rounded-full bg-emerald-50 dark:bg-emerald-950/40 text-emerald-600 dark:text-emerald-400">
											<ShieldAlert className="h-6 w-6" />
										</div>
										<h4 className="font-semibold text-foreground text-base">
											Tidak Ada Berkas Tertahan
										</h4>
										<p className="text-xs text-muted-foreground text-center leading-relaxed">
											Semua berkas permohonan berjalan sesuai dengan target SLA
											proses bisnis (&lt; 7 hari).
										</p>
									</div>
								</TableCell>
							</TableRow>
						) : (
							filteredTasks.map((task) => (
								<TableRow
									key={task.stepId}
									className="hover:bg-rose-50/20 dark:hover:bg-rose-950/20 transition-colors"
								>
									{/* Perusahaan Info */}
									<TableCell className="py-3">
										<div className="flex flex-col gap-0.5">
											<Link
												to="/directory/$companyId"
												params={{ companyId: task.companyId }}
												className="font-medium text-foreground hover:text-primary transition-colors text-sm flex items-center gap-1.5"
											>
												<Building2 className="h-3.5 w-3.5 text-primary shrink-0" />
												<span>{task.namaPerusahaan}</span>
											</Link>
											<div className="flex items-center gap-2 text-xs text-muted-foreground">
												<span className="font-mono text-[11px]">
													{task.nomor}
												</span>
												<span>•</span>
												<span>{task.industryTypeName || "Industri"}</span>
											</div>
										</div>
									</TableCell>

									{/* Tahap Step */}
									<TableCell className="py-3">
										<Badge
											variant="secondary"
											className="text-xs font-medium bg-rose-100 text-rose-800 dark:bg-rose-950/60 dark:text-rose-300"
										>
											{task.stepKind ?? "Persetujuan"}
										</Badge>
									</TableCell>

									{/* Wilayah & Area */}
									<TableCell className="py-3">
										<div className="flex flex-col gap-0.5 text-xs">
											<span className="font-medium text-foreground">
												{task.areaName}
											</span>
											<span className="text-muted-foreground text-[11px]">
												{task.regionName}
											</span>
										</div>
									</TableCell>

									{/* Submitter */}
									<TableCell className="py-3">
										<div className="flex items-center gap-1.5 text-xs text-foreground">
											<User className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
											<span>{task.submittedByName || "-"}</span>
										</div>
									</TableCell>

									{/* SLA Duration */}
									<TableCell className="py-3">
										<SlaClockBadge waitingSince={task.waitingSince} />
									</TableCell>

									{/* Actions */}
									<TableCell className="py-3 text-right pr-4">
										<div className="flex items-center justify-end gap-1.5">
											<Button
												size="sm"
												variant="ghost"
												className="h-8 px-2 text-xs"
												onClick={() => handleOpenDrawer(task)}
												title="Tinjau Cepat"
											>
												<Eye className="h-3.5 w-3.5 mr-1" />
												Tinjau
											</Button>
											<Button
												size="sm"
												variant="outline"
												className="h-8 px-2.5 text-xs text-blue-600 border-blue-200 hover:bg-blue-50 dark:hover:bg-blue-950/50"
												onClick={() => handleOpenActionModal(task, "Reassign")}
												title="Tugaskan ulang reviewer"
											>
												<UserCheck className="h-3.5 w-3.5 mr-1" />
												Tugaskan Ulang
											</Button>
											<Button
												size="sm"
												className="h-8 px-2.5 text-xs bg-emerald-600 hover:bg-emerald-700 text-white font-medium"
												onClick={() => handleOpenActionModal(task, "Setuju")}
												title="Setujui"
											>
												<CheckCircle2 className="h-3.5 w-3.5 mr-1" />
												Setuju
											</Button>
										</div>
									</TableCell>
								</TableRow>
							))
						)}
					</TableBody>
				</Table>
			</div>

			{/* Action Modal */}
			<TaskActionModal
				task={modalTask}
				actionType={modalAction}
				isOpen={!!modalAction}
				onClose={() => {
					setModalAction(null);
					setModalTask(null);
				}}
				onSuccess={() => refetch()}
			/>

			{/* Quick Preview Drawer */}
			<TaskQuickPreviewDrawer
				task={drawerTask}
				isOpen={isDrawerOpen}
				onClose={() => {
					setIsDrawerOpen(false);
					setDrawerTask(null);
				}}
				onTakeAction={(task, actionType) => {
					setIsDrawerOpen(false);
					if (actionType) {
						handleOpenActionModal(task, actionType);
					}
				}}
			/>
		</div>
	);
}
