import { Link } from "@tanstack/react-router";
import {
	AlertOctagon,
	ArrowRight,
	Building2,
	Clock,
	Layers,
	ListOrdered,
	OctagonAlert,
} from "lucide-react";
import type { RegionalAdminDashboardDto } from "@/api/types";
import { PageHeader } from "@/components/common";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { STAGE_CONFIG } from "@/lib/directory-utils";
import { StatTile } from "./stat-tile";

interface RegionalAdminDashboardProps {
	data: RegionalAdminDashboardDto;
}

export function RegionalAdminDashboard({ data }: RegionalAdminDashboardProps) {
	const stuckCount = data.stuckTasks?.length || 0;
	const totalPipeline: number = Object.values(
		data.regionFunnelCounts || {},
	).reduce<number>((a, b) => Math.max(Number(a), Number(b)), 0);
	const pendingMyAction = Number(data.pendingMyActionCount);

	return (
		<div className="space-y-6">
			{/* Hero & Quick Action Bar */}
			<PageHeader
				title="Beranda Admin Regional"
				description="Pengawasan seluruh Sales Area di wilayah, penanganan tugas tertahan, dan kelancaran SLA persetujuan."
				actions={
					<>
						<Button asChild size="sm" className="gap-1.5 shadow-xs">
							<Link to="/tasks/blocked">
								<AlertOctagon className="size-4 text-rose-300" />
								Kelola Tugas Tertahan
								{stuckCount > 0 && (
									<Badge
										variant="destructive"
										className="ml-1 text-[10px] font-mono px-1.5 py-0"
									>
										{stuckCount}
									</Badge>
								)}
							</Link>
						</Button>
						<Button asChild variant="outline" size="sm" className="gap-1.5">
							<Link to="/reports/ageing">
								<Clock className="size-4" />
								Laporan Durasi Proses (Ageing)
							</Link>
						</Button>
						<Button asChild variant="outline" size="sm" className="gap-1.5">
							<Link to="/directory">
								<Building2 className="size-4" />
								Direktori Wilayah
							</Link>
						</Button>
					</>
				}
			/>

			{/* Top KPI Stat Cards */}
			<div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
				<StatTile
					title="Total Pipeline Wilayah"
					value={totalPipeline}
					description="Akumulasi seluruh Sales Area"
					icon={Layers}
					variant="default"
				/>
				<StatTile
					title="Tugas Tertahan"
					value={stuckCount}
					description={
						stuckCount > 0
							? `${stuckCount} berkas perlu penugasan ulang atau tindak lanjut`
							: "Tidak ada tugas tertahan"
					}
					icon={OctagonAlert}
					variant={stuckCount > 0 ? "rose" : "emerald"}
					badge={stuckCount > 0 ? "Perhatian" : "Optimal"}
				/>
				<StatTile
					title="Menunggu Tindakan Saya"
					value={data.pendingMyActionCount}
					description="Verifikasi Tahap 7 / Verifikasi Evaluasi"
					icon={Clock}
					variant={pendingMyAction > 0 ? "amber" : "default"}
				/>
				<StatTile
					title="Total Berkas Berjalan"
					value={data.totalWaitingActionCount}
					description="Aktif dalam siklus workflow persetujuan"
					icon={ListOrdered}
					variant="blue"
				/>
			</div>

			{/* Oldest Waiting Record Alert */}
			{data.oldestWaitingItem && (
				<Card className="border-amber-200 bg-amber-50/50 dark:border-amber-900/50 dark:bg-amber-950/20 shadow-xs">
					<CardContent className="p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
						<div className="flex items-center gap-3">
							<div className="p-2 rounded-lg bg-amber-100 dark:bg-amber-900/50 text-amber-700 dark:text-amber-300">
								<Clock className="size-5" />
							</div>
							<div>
								<div className="flex items-center gap-2">
									<h4 className="font-semibold text-sm text-amber-950 dark:text-amber-100">
										Berkas Tertahan Terlama:{" "}
										{data.oldestWaitingItem.companyName}
									</h4>
									<Badge
										variant="outline"
										className="border-amber-300 text-amber-800 dark:text-amber-200 text-xs"
									>
										⏱ {data.oldestWaitingItem.waitingDays} hari
									</Badge>
								</div>
								<p className="text-xs text-amber-800/80 dark:text-amber-300/80 mt-0.5 font-mono">
									Nomor: {data.oldestWaitingItem.companyNomor}
								</p>
							</div>
						</div>
						<div className="flex items-center gap-2 shrink-0">
							<Button asChild size="sm" variant="default" className="gap-1">
								<Link
									to="/directory/$companyId"
									params={{ companyId: data.oldestWaitingItem.companyId }}
								>
									Periksa Berkas
									<ArrowRight className="size-3.5" />
								</Link>
							</Button>
							<Button asChild size="sm" variant="outline">
								<Link to="/reports/ageing">Buka Laporan Ageing</Link>
							</Button>
						</div>
					</CardContent>
				</Card>
			)}

			{/* Attention Grid: Stuck Tasks Queue & Regional Funnel */}
			<div className="grid gap-6 lg:grid-cols-2">
				{/* Stuck Tasks Queue */}
				<Card className="shadow-xs">
					<CardHeader className="pb-3">
						<div className="flex items-center justify-between">
							<CardTitle className="text-base font-semibold">
								Daftar Tugas Tertahan & Perlu Intervensi
							</CardTitle>
							<Badge
								variant={stuckCount > 0 ? "destructive" : "secondary"}
								className="text-xs font-mono"
							>
								{stuckCount}
							</Badge>
						</div>
						<CardDescription className="text-xs">
							Berkas yang ditolak atau mengalami kendala workflow yang
							membutuhkan penanganan admin.
						</CardDescription>
					</CardHeader>
					<CardContent className="p-0">
						{stuckCount === 0 ? (
							<div className="py-12 text-center text-xs text-muted-foreground">
								Tidak ada tugas yang tertahan di wilayah ini.
							</div>
						) : (
							<div className="divide-y max-h-[320px] overflow-y-auto">
								{data.stuckTasks.map((task) => (
									<div
										key={task.companyId}
										className="p-3.5 hover:bg-muted/40 transition-colors flex items-center justify-between gap-3 text-xs"
									>
										<div className="min-w-0 space-y-1">
											<div className="flex items-center gap-2">
												<span className="font-semibold text-foreground">
													{task.companyName}
												</span>
												<span className="font-mono text-[11px] text-muted-foreground">
													{task.companyNomor}
												</span>
											</div>
											<p className="text-muted-foreground text-[11px] line-clamp-1">
												{task.reason}
											</p>
										</div>
										<div className="flex items-center gap-2 shrink-0">
											<span className="text-[11px] font-medium text-rose-600 bg-rose-50 px-2 py-0.5 rounded border border-rose-200 dark:bg-rose-950/40">
												{task.waitingDays} hari
											</span>
											<Button asChild size="sm" variant="ghost" className="h-7">
												<Link
													to="/directory/$companyId"
													params={{ companyId: task.companyId }}
												>
													Buka
												</Link>
											</Button>
										</div>
									</div>
								))}
							</div>
						)}
					</CardContent>
				</Card>

				{/* Regional Funnel Breakdown */}
				<Card className="shadow-xs flex flex-col justify-between">
					<CardHeader className="pb-3">
						<div className="flex items-center justify-between">
							<CardTitle className="text-base font-semibold">
								Pipeline Penjualan Wilayah
							</CardTitle>
							<Button variant="ghost" size="sm" asChild className="text-xs">
								<Link to="/reports/funnel">Laporan Lengkap →</Link>
							</Button>
						</div>
						<CardDescription className="text-xs">
							Distribusi kumulatif jumlah perusahaan per tahapan gas.
						</CardDescription>
					</CardHeader>
					<CardContent className="space-y-2">
						{Object.values(STAGE_CONFIG).map((stage) => {
							const count = Number(
								data.regionFunnelCounts?.[stage.stage.toString()] ?? 0,
							);
							const percentage =
								totalPipeline > 0
									? Math.round((count / totalPipeline) * 100)
									: 0;

							return (
								<div key={stage.stage} className="space-y-1">
									<div className="flex justify-between text-xs font-medium">
										<span className="text-muted-foreground">
											Tahap {stage.stage}: {stage.shortName}
										</span>
										<span className="font-semibold text-foreground">
											{count}{" "}
											<span className="text-[11px] text-muted-foreground">
												({percentage}%)
											</span>
										</span>
									</div>
									<div className="h-2 w-full bg-muted rounded-full overflow-hidden">
										<div
											className="h-full bg-primary rounded-full transition-all duration-300"
											style={{ width: `${Math.min(100, percentage)}%` }}
										/>
									</div>
								</div>
							);
						})}
					</CardContent>
				</Card>
			</div>
		</div>
	);
}
