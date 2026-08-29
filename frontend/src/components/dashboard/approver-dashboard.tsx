import { Link } from "@tanstack/react-router";
import {
	Activity,
	ArrowRight,
	Building2,
	CheckCircle2,
	Clock,
	FileCheck,
	ListChecks,
	Sparkles,
	TrendingUp,
} from "lucide-react";
import type { ApproverDashboardDto } from "@/api/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { Table, TableBody, TableCell, TableRow } from "@/components/ui/table";
import { STAGE_CONFIG } from "@/lib/directory-utils";
import { StatTile } from "./stat-tile";

interface ApproverDashboardProps {
	data: ApproverDashboardDto;
	roleTitle?: string;
}

export function ApproverDashboard({
	data,
	roleTitle = "Approver & Reviewer",
}: ApproverDashboardProps) {
	const pendingCount = data.pendingApprovals?.length || 0;
	const totalPending = Number(data.totalPendingApprovals);

	return (
		<div className="space-y-6">
			{/* Hero & Quick Action Bar */}
			<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-card p-5 rounded-xl border shadow-xs">
				<div>
					<div className="flex items-center gap-2">
						<h1 className="text-2xl font-bold tracking-tight">
							Beranda Persetujuan
						</h1>
						<Badge variant="secondary" className="font-semibold">
							{roleTitle}
						</Badge>
					</div>
					<p className="text-muted-foreground mt-1 text-sm">
						Daftar tugas verifikasi berkas, persetujuan syarat teknis/komersial,
						dan metrik waktu tinjau.
					</p>
				</div>
				<div className="flex flex-wrap items-center gap-2.5">
					<Button asChild size="sm" className="gap-1.5 shadow-xs">
						<Link to="/tasks">
							<ListChecks className="size-4" />
							Antrean Tugas Saya
							{pendingCount > 0 && (
								<Badge
									variant="secondary"
									className="ml-1 bg-primary-foreground text-primary font-mono text-[10px]"
								>
									{pendingCount}
								</Badge>
							)}
						</Link>
					</Button>
					<Button asChild variant="outline" size="sm" className="gap-1.5">
						<Link to="/directory">
							<Building2 className="size-4" />
							Direktori
						</Link>
					</Button>
				</div>
			</div>

			{/* Top KPI Stat Cards */}
			<div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
				<StatTile
					title="Menunggu Persetujuan Anda"
					value={data.totalPendingApprovals}
					description="Tugas verifikasi aktif dalam antrean"
					icon={Clock}
					variant={totalPending > 0 ? "amber" : "default"}
					badge={totalPending > 0 ? "Pending" : "Selesai"}
				/>
				<StatTile
					title="Total Record Aktif"
					value={data.totalActiveRecords}
					description="Record dalam cakupan kewenangan"
					icon={Building2}
					variant="default"
				/>
				<StatTile
					title="Terbit NOL Bulan Ini"
					value={data.nolIssuedThisMonth}
					description="Persetujuan final berhasil diterbitkan"
					icon={FileCheck}
					variant="emerald"
				/>
				<StatTile
					title="Disetujui Bulan Ini"
					value={data.performance.approvedThisMonth}
					description={`Rata-rata waktu tinjau ${Number(data.performance.averageTurnaroundDays).toFixed(1)} hari`}
					icon={TrendingUp}
					variant="blue"
				/>
			</div>

			{/* Menunggu Persetujuan Anda Table */}
			<Card className="shadow-xs">
				<CardHeader className="flex flex-row items-center justify-between pb-3">
					<div>
						<CardTitle className="text-base font-semibold flex items-center gap-2">
							<Clock className="size-4 text-amber-500" />
							Menunggu Persetujuan Anda ({pendingCount})
						</CardTitle>
						<CardDescription className="text-xs">
							Berkas yang dialokasikan kepada Anda untuk diverifikasi atau
							disetujui.
						</CardDescription>
					</div>
					<Button variant="ghost" size="sm" asChild className="text-xs">
						<Link to="/tasks">Lihat Semua di Tugas Saya →</Link>
					</Button>
				</CardHeader>
				<CardContent className="p-0">
					{pendingCount === 0 ? (
						<div className="py-12 text-center text-xs text-muted-foreground flex flex-col items-center gap-2">
							<CheckCircle2 className="size-8 text-emerald-500/80" />
							<p className="font-medium text-sm text-foreground">
								Tidak ada tugas yang menunggu persetujuan
							</p>
							<p>Semua berkas yang dialokasikan telah selesai diproses.</p>
						</div>
					) : (
						<Table>
							<TableBody>
								{data.pendingApprovals.map((item) => {
									const waitingDays = Math.max(
										0,
										Math.floor(
											(Date.now() - new Date(item.waitingSince).getTime()) /
												(1000 * 60 * 60 * 24),
										),
									);
									const stageObj =
										STAGE_CONFIG[Number(item.stage)] || STAGE_CONFIG[1];
									const durationVariant =
										waitingDays > 7
											? "text-rose-600 bg-rose-50 border-rose-200 dark:bg-rose-950/40"
											: waitingDays >= 3
												? "text-amber-600 bg-amber-50 border-amber-200 dark:bg-amber-950/40"
												: "text-emerald-600 bg-emerald-50 border-emerald-200 dark:bg-emerald-950/40";

									return (
										<TableRow key={item.companyId}>
											<TableCell className="font-medium">
												<div>
													<Link
														to="/directory/$companyId"
														params={{ companyId: item.companyId }}
														className="hover:underline text-foreground font-semibold text-sm"
													>
														{item.companyName}
													</Link>
													<div className="text-xs font-mono text-muted-foreground mt-0.5">
														{item.companyNomor}
													</div>
												</div>
											</TableCell>
											<TableCell>
												<Badge variant="outline" className="text-xs">
													Tahap {item.stage}: {stageObj?.shortName || "Tahapan"}
												</Badge>
											</TableCell>
											<TableCell className="text-xs text-muted-foreground">
												<div>Diajukan oleh:</div>
												<div className="font-medium text-foreground">
													{item.submittedByName}
												</div>
											</TableCell>
											<TableCell>
												<span
													className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium border ${durationVariant}`}
												>
													⏱ {waitingDays} hari
												</span>
											</TableCell>
											<TableCell className="text-right">
												<Button
													asChild
													size="sm"
													variant="default"
													className="gap-1"
												>
													<Link
														to="/directory/$companyId"
														params={{ companyId: item.companyId }}
													>
														Tinjau
														<ArrowRight className="size-3.5" />
													</Link>
												</Button>
											</TableCell>
										</TableRow>
									);
								})}
							</TableBody>
						</Table>
					)}
				</CardContent>
			</Card>

			{/* Kinerja Persetujuan & Aktivitas Terbaru Grid */}
			<div className="grid gap-6 lg:grid-cols-2">
				{/* Kinerja Persetujuan Card */}
				<Card className="shadow-xs">
					<CardHeader className="pb-3">
						<CardTitle className="text-base font-semibold flex items-center gap-2">
							<Sparkles className="size-4 text-primary" />
							Kinerja Persetujuan Saya
						</CardTitle>
						<CardDescription className="text-xs">
							Metrik produktivitas dan kecepatan review bulan ini.
						</CardDescription>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="grid grid-cols-2 gap-3">
							<div className="p-3.5 rounded-lg border bg-muted/30">
								<p className="text-xs text-muted-foreground">
									Rata-rata Waktu Tinjau
								</p>
								<p className="text-2xl font-bold mt-1 text-foreground">
									{Number(data.performance.averageTurnaroundDays).toFixed(1)}{" "}
									<span className="text-sm font-normal text-muted-foreground">
										hari
									</span>
								</p>
							</div>
							<div className="p-3.5 rounded-lg border bg-muted/30">
								<p className="text-xs text-muted-foreground">
									Disetujui Bulan Ini
								</p>
								<p className="text-2xl font-bold mt-1 text-emerald-600 dark:text-emerald-400">
									{data.performance.approvedThisMonth}
								</p>
							</div>
						</div>
						<div className="p-3.5 rounded-lg border bg-muted/30 flex items-center justify-between">
							<div>
								<p className="text-xs text-muted-foreground">
									Dikembalikan (Revisi / Tolak)
								</p>
								<p className="text-sm font-semibold mt-0.5 text-foreground">
									{data.performance.revisedThisMonth} berkas
								</p>
							</div>
							<Badge variant="outline">Quality Check</Badge>
						</div>
					</CardContent>
				</Card>

				{/* Aktivitas Terbaru Timeline Card */}
				<Card className="shadow-xs">
					<CardHeader className="pb-3">
						<CardTitle className="text-base font-semibold flex items-center gap-2">
							<Activity className="size-4 text-primary" />
							Aktivitas Terbaru
						</CardTitle>
						<CardDescription className="text-xs">
							Pergerakan riwayat status berkas di wilayah kerja Anda.
						</CardDescription>
					</CardHeader>
					<CardContent className="p-0">
						{!data.recentActivity || data.recentActivity.length === 0 ? (
							<div className="py-10 text-center text-xs text-muted-foreground">
								Belum ada aktivitas persetujuan terbaru.
							</div>
						) : (
							<div className="divide-y max-h-[260px] overflow-y-auto">
								{data.recentActivity.map((act) => (
									<div
										key={`${act.occurredAt}-${act.companyName}`}
										className="p-3 hover:bg-muted/30 transition-colors flex items-start justify-between gap-2 text-xs"
									>
										<div className="space-y-0.5">
											<div className="flex items-center gap-1.5 font-medium">
												<span className="font-semibold text-foreground">
													{act.actorName}
												</span>
												<span className="text-muted-foreground">melakukan</span>
												<Badge variant="secondary" className="text-[10px]">
													{act.action}
												</Badge>
											</div>
											<p className="text-muted-foreground">
												Perusahaan:{" "}
												<span className="font-medium text-foreground">
													{act.companyName}
												</span>
											</p>
										</div>
										<span className="text-[10px] text-muted-foreground whitespace-nowrap">
											{new Date(act.occurredAt).toLocaleTimeString("id-ID", {
												hour: "2-digit",
												minute: "2-digit",
											})}
										</span>
									</div>
								))}
							</div>
						)}
					</CardContent>
				</Card>
			</div>
		</div>
	);
}
