import { createFileRoute, Link } from "@tanstack/react-router";
import {
	Building2,
	CheckCircle2,
	Eye,
	Inbox,
	Loader2,
	RotateCcw,
	Search,
	Sparkles,
	User,
	Users,
	XCircle,
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
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";

export const Route = createFileRoute("/_auth/tasks/")({
	component: TasksInboxPage,
});

type TaskScope = "my" | "region";
type SortOption = "sla_desc" | "sla_asc" | "name_asc";

function TasksInboxPage() {
	const [scope, setScope] = React.useState<TaskScope>("my");
	const [searchTerm, setSearchTerm] = React.useState("");
	const [stepFilter, setStepFilter] = React.useState<string>("all");
	const [areaFilter, setAreaFilter] = React.useState<string>("all");
	const [sortBy, setSortBy] = React.useState<SortOption>("sla_desc");

	// Active action modal state
	const [modalTask, setModalTask] = React.useState<TaskListItem | null>(null);
	const [modalAction, setModalAction] =
		React.useState<TaskActionModalType>(null);

	// Quick preview drawer state
	const [drawerTask, setDrawerTask] = React.useState<TaskListItem | null>(null);
	const [isDrawerOpen, setIsDrawerOpen] = React.useState(false);

	// Queries
	const {
		data: myTasks = [],
		isLoading: isLoadingMy,
		refetch: refetchMy,
	} = $api.useQuery("get", "/api/tasks/inbox");

	const {
		data: regionTasks = [],
		isLoading: isLoadingRegion,
		refetch: refetchRegion,
	} = $api.useQuery("get", "/api/tasks/region", undefined, {
		enabled: scope === "region",
	});

	const tasks = scope === "my" ? myTasks : regionTasks;
	const isLoading = scope === "my" ? isLoadingMy : isLoadingRegion;

	// Extract unique areas from tasks
	const availableAreas = React.useMemo(() => {
		const areas = new Set<string>();
		for (const t of tasks) {
			if (t.areaName) areas.add(t.areaName);
		}
		return Array.from(areas).sort();
	}, [tasks]);

	// Filter & sort logic
	const filteredTasks = React.useMemo(() => {
		let list = [...tasks];

		// Text search
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

		// Step filter
		if (stepFilter !== "all") {
			list = list.filter((t) => t.stepKind === stepFilter);
		}

		// Area filter
		if (areaFilter !== "all") {
			list = list.filter((t) => t.areaName === areaFilter);
		}

		// Sorting
		list.sort((a, b) => {
			if (sortBy === "sla_desc") {
				return (
					new Date(a.waitingSince).getTime() -
					new Date(b.waitingSince).getTime()
				);
			}
			if (sortBy === "sla_asc") {
				return (
					new Date(b.waitingSince).getTime() -
					new Date(a.waitingSince).getTime()
				);
			}
			if (sortBy === "name_asc") {
				return a.namaPerusahaan.localeCompare(b.namaPerusahaan);
			}
			return 0;
		});

		return list;
	}, [tasks, searchTerm, stepFilter, areaFilter, sortBy]);

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
			{/* Top Scope Selector & Controls */}
			<div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
				<Tabs
					value={scope}
					onValueChange={(val) => setScope(val as TaskScope)}
					className="w-full sm:w-auto"
				>
					<TabsList className="grid w-full sm:w-auto grid-cols-2">
						<TabsTrigger value="my" className="flex items-center gap-1.5 px-4">
							<Inbox className="h-4 w-4" />
							<span>Tugas Saya ({myTasks.length})</span>
						</TabsTrigger>
						<TabsTrigger
							value="region"
							className="flex items-center gap-1.5 px-4"
						>
							<Users className="h-4 w-4" />
							<span>Semua Tugas Wilayah ({regionTasks.length})</span>
						</TabsTrigger>
					</TabsList>
				</Tabs>

				<div className="text-xs text-muted-foreground self-end sm:self-auto">
					Menampilkan{" "}
					<strong className="text-foreground">{filteredTasks.length}</strong>{" "}
					tugas aktif
				</div>
			</div>

			{/* Filter & Search Bar */}
			<Card className="shadow-xs border bg-card/60">
				<CardContent className="p-3.5 space-y-3">
					<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
						{/* Search Input */}
						<div className="relative">
							<Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
							<Input
								placeholder="Cari perusahaan, nomor, pengaju..."
								value={searchTerm}
								onChange={(e) => setSearchTerm(e.target.value)}
								className="pl-9 h-9 text-xs"
							/>
						</div>

						{/* Step Kind Filter */}
						<Select value={stepFilter} onValueChange={setStepFilter}>
							<SelectTrigger className="h-9 text-xs">
								<SelectValue placeholder="Semua Tahap Verifikasi" />
							</SelectTrigger>
							<SelectContent>
								<SelectItem value="all">Semua Tahap Verifikasi</SelectItem>
								<SelectItem value="AreaHead">Area Head</SelectItem>
								<SelectItem value="RegionalAdmin">Regional Admin</SelectItem>
								<SelectItem value="Reviewer1">
									Reviewer 1 (Sales/Mkt)
								</SelectItem>
								<SelectItem value="Reviewer2">Reviewer 2 (Teknik)</SelectItem>
								<SelectItem value="Reviewer3">Reviewer 3 (Keuangan)</SelectItem>
								<SelectItem value="DivisionHead">Division Head</SelectItem>
							</SelectContent>
						</Select>

						{/* Area Filter */}
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

						{/* Sort Selector */}
						<Select
							value={sortBy}
							onValueChange={(val) => setSortBy(val as SortOption)}
						>
							<SelectTrigger className="h-9 text-xs">
								<SelectValue placeholder="Urutkan Berdasarkan" />
							</SelectTrigger>
							<SelectContent>
								<SelectItem value="sla_desc">
									Terlama Menunggu (Prioritas SLA)
								</SelectItem>
								<SelectItem value="sla_asc">Terbaru Diajukan</SelectItem>
								<SelectItem value="name_asc">Nama Perusahaan (A-Z)</SelectItem>
							</SelectContent>
						</Select>
					</div>
				</CardContent>
			</Card>

			{/* Task List Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table>
					<TableHeader className="bg-muted/40">
						<TableRow>
							<TableHead className="font-semibold text-xs">
								Perusahaan
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Tahap Verifikasi
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Wilayah & Area
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Diajukan Oleh
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Menunggu Sejak (SLA)
							</TableHead>
							<TableHead className="text-right font-semibold text-xs pr-4">
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
											Memuat antrean tugas...
										</span>
									</div>
								</TableCell>
							</TableRow>
						) : filteredTasks.length === 0 ? (
							<TableRow>
								<TableCell colSpan={6} className="text-center py-14">
									<div className="flex flex-col items-center justify-center gap-2.5 max-w-sm mx-auto text-muted-foreground">
										<div className="p-3 rounded-full bg-emerald-50 dark:bg-emerald-950/40 text-emerald-600 dark:text-emerald-400">
											<Sparkles className="h-6 w-6" />
										</div>
										<h4 className="font-semibold text-foreground text-base">
											Tidak Ada Tugas Menunggu
										</h4>
										<p className="text-xs text-muted-foreground text-center leading-relaxed">
											{searchTerm ||
											stepFilter !== "all" ||
											areaFilter !== "all"
												? "Tidak ada tugas yang sesuai dengan filter pencarian yang diterapkan."
												: scope === "my"
													? "Semua berkas pada antrean Anda telah selesai ditindaklanjuti. Kerja bagus!"
													: "Tidak ada berkas aktif yang sedang berproses pada wilayah kerja Anda."}
										</p>
									</div>
								</TableCell>
							</TableRow>
						) : (
							filteredTasks.map((task) => (
								<TableRow
									key={task.stepId}
									className="hover:bg-muted/30 transition-colors"
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
										<Badge variant="secondary" className="text-xs font-medium">
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
												className="h-8 px-2 text-xs text-amber-600 border-amber-200 hover:bg-amber-50 dark:hover:bg-amber-950/50"
												onClick={() => handleOpenActionModal(task, "Revisi")}
												title="Kembalikan untuk revisi"
											>
												<RotateCcw className="h-3.5 w-3.5 mr-1" />
												Revisi
											</Button>
											<Button
												size="sm"
												variant="outline"
												className="h-8 px-2 text-xs text-rose-600 border-rose-200 hover:bg-rose-50 dark:hover:bg-rose-950/50"
												onClick={() => handleOpenActionModal(task, "Tolak")}
												title="Tolak berkas"
											>
												<XCircle className="h-3.5 w-3.5 mr-1" />
												Tolak
											</Button>
											<Button
												size="sm"
												className="h-8 px-2.5 text-xs bg-emerald-600 hover:bg-emerald-700 text-white font-medium"
												onClick={() => handleOpenActionModal(task, "Setuju")}
												title="Setujui dan lanjutkan"
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

			{/* Action Modal (Setuju / Revisi / Tolak / Reassign) */}
			<TaskActionModal
				task={modalTask}
				actionType={modalAction}
				isOpen={!!modalAction}
				onClose={() => {
					setModalAction(null);
					setModalTask(null);
				}}
				onSuccess={() => {
					refetchMy();
					if (scope === "region") refetchRegion();
				}}
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
					handleOpenActionModal(task, actionType);
				}}
			/>
		</div>
	);
}
