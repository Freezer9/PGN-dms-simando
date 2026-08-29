import { Link } from "@tanstack/react-router";
import {
	AlertTriangle,
	ArrowRight,
	Building2,
	CheckCircle2,
	Clock,
	FilePlus2,
	MapPin,
	Send,
} from "lucide-react";
import type { SalesAreaDashboardDto } from "@/api/types";
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
import { DashboardMapPreview } from "./dashboard-map-preview";
import { StatTile } from "./stat-tile";

interface SalesAreaDashboardProps {
	data: SalesAreaDashboardDto;
	areaName?: string;
}

export function SalesAreaDashboard({
	data,
	areaName = "Sales Area",
}: SalesAreaDashboardProps) {
	const returnedCount = data.returnedWorkItems?.length || 0;
	const activeApprovalCount = data.activeApprovalItems?.length || 0;
	const totalPipeline = Object.values(data.stageCounts || {}).reduce(
		(a, b) => Math.max(Number(a), Number(b)),
		0,
	);
	const nolIssuedCount = Number(data.stageCounts?.["8"] || 0);

	return (
		<div className="space-y-6">
			{/* Hero & Quick Action Bar */}
			<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-card p-5 rounded-xl border shadow-xs">
				<div>
					<div className="flex items-center gap-2">
						<h1 className="text-2xl font-bold tracking-tight">
							Beranda Sales Area
						</h1>
						<Badge variant="secondary" className="font-semibold">
							{areaName}
						</Badge>
					</div>
					<p className="text-muted-foreground mt-1 text-sm">
						Ringkasan tugas lapangan, progres verifikasi berkas, dan pipeline
						pelanggan gas.
					</p>
				</div>
				<div className="flex flex-wrap items-center gap-2.5">
					<Button asChild size="sm" className="gap-1.5 shadow-xs">
						<Link to="/directory/new">
							<FilePlus2 className="size-4" />
							Tambah Perusahaan Baru
						</Link>
					</Button>
					<Button asChild variant="outline" size="sm" className="gap-1.5">
						<Link to="/map">
							<MapPin className="size-4" />
							Peta Explorer
						</Link>
					</Button>
				</div>
			</div>

			{/* Top KPI Stat Cards */}
			<div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
				<StatTile
					title="Total Pipeline"
					value={totalPipeline}
					description="Perusahaan terdaftar di Area"
					icon={Building2}
					variant="default"
				/>
				<StatTile
					title="Perlu Tindakan Anda"
					value={returnedCount}
					description={
						returnedCount > 0
							? `${returnedCount} berkas perlu perbaikan`
							: "Semua berkas berjalan lancar"
					}
					icon={AlertTriangle}
					variant={returnedCount > 0 ? "rose" : "emerald"}
					badge={returnedCount > 0 ? "Revisi / Tolak" : "Clear"}
				/>
				<StatTile
					title="Dalam Persetujuan"
					value={activeApprovalCount}
					description="Menunggu tindakan Reviewer / Admin"
					icon={Clock}
					variant={activeApprovalCount > 0 ? "amber" : "default"}
				/>
				<StatTile
					title="Terbit NOL (RL)"
					value={nolIssuedCount}
					description="Pelanggan siap berlangganan gas"
					icon={CheckCircle2}
					variant="emerald"
				/>
			</div>

			{/* Perlu Tindakan Anda Alert Panel */}
			{returnedCount > 0 && (
				<Card className="border-rose-200 bg-rose-50/40 dark:border-rose-900/50 dark:bg-rose-950/20 shadow-xs">
					<CardHeader className="pb-3">
						<div className="flex items-center gap-2">
							<div className="p-1 rounded-md bg-rose-100 dark:bg-rose-900/50 text-rose-600 dark:text-rose-300">
								<AlertTriangle className="size-4" />
							</div>
							<CardTitle className="text-base text-rose-900 dark:text-rose-100 font-semibold">
								Perlu Tindakan Anda ({returnedCount})
							</CardTitle>
						</div>
						<CardDescription className="text-rose-700/80 dark:text-rose-300/80 text-xs">
							Berkas yang dikembalikan untuk revisi atau ditolak. Mohon periksa
							catatan reviewer dan lakukan perbaikan.
						</CardDescription>
					</CardHeader>
					<CardContent className="space-y-3 pt-0">
						{data.returnedWorkItems.map((item) => (
							<div
								key={item.companyId}
								className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-3.5 rounded-lg bg-background border border-rose-100 dark:border-rose-900/40 shadow-xs"
							>
								<div className="space-y-1 min-w-0">
									<div className="flex items-center gap-2 flex-wrap">
										<span className="font-semibold text-sm">
											{item.companyName}
										</span>
										<Badge variant="outline" className="text-xs font-mono">
											{item.companyNomor}
										</Badge>
										<Badge
											variant={
												item.action === "Tolak" ? "destructive" : "secondary"
											}
											className="text-[11px]"
										>
											{item.action === "Tolak" ? "Ditolak" : "Revisi"} oleh{" "}
											{item.actorRoleLabel}
										</Badge>
									</div>
									<p className="text-xs text-muted-foreground bg-muted/60 p-2 rounded-md font-mono line-clamp-2">
										"{item.returnReason}"
									</p>
								</div>
								<Button
									asChild
									size="sm"
									variant="default"
									className="shrink-0 gap-1"
								>
									<Link
										to="/directory/$companyId"
										params={{ companyId: item.companyId }}
									>
										Buka Record
										<ArrowRight className="size-3.5" />
									</Link>
								</Button>
							</div>
						))}
					</CardContent>
				</Card>
			)}

			{/* Pipeline Penjualan Saya (Stage Bar) */}
			<Card className="shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-base font-semibold">
						Pipeline Penjualan Saya
					</CardTitle>
					<CardDescription className="text-xs">
						Distribusi progres per tahap proses berlangganan gas. Klik tahapan
						untuk membuka direktori.
					</CardDescription>
				</CardHeader>
				<CardContent>
					<div className="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-8 gap-2">
						{Object.values(STAGE_CONFIG).map((stage) => {
							const count = Number(
								data.stageCounts?.[stage.stage.toString()] ?? 0,
							);
							return (
								<Link
									key={stage.stage}
									to="/directory"
									search={{ stage: stage.stage }}
									className="group flex flex-col items-center justify-between p-3 rounded-lg border bg-card hover:bg-accent/50 hover:border-primary/50 transition-all text-center"
								>
									<span className="text-[11px] font-semibold text-muted-foreground group-hover:text-primary transition-colors">
										Tahap {stage.stage}
									</span>
									<span className="text-xl font-bold my-1 tracking-tight">
										{count}
									</span>
									<span className="text-[10px] text-muted-foreground truncate w-full">
										{stage.shortName}
									</span>
								</Link>
							);
						})}
					</div>
				</CardContent>
			</Card>

			{/* Dalam Proses Persetujuan & Map Preview */}
			<div className="grid gap-6 lg:grid-cols-2">
				{/* Approval Progress Card */}
				<Card className="shadow-xs flex flex-col">
					<CardHeader className="pb-3">
						<div className="flex items-center justify-between">
							<CardTitle className="text-base font-semibold flex items-center gap-2">
								<Send className="size-4 text-primary" />
								Dalam Proses Persetujuan
							</CardTitle>
							<Badge variant="outline" className="text-xs font-mono">
								{activeApprovalCount} berkas aktif
							</Badge>
						</div>
						<CardDescription className="text-xs">
							Daftar pengajuan yang sedang diproses oleh reviewer dan
							administrator.
						</CardDescription>
					</CardHeader>
					<CardContent className="flex-1 p-0">
						{activeApprovalCount === 0 ? (
							<div className="py-12 text-center text-xs text-muted-foreground">
								Tidak ada pengajuan yang sedang menunggu persetujuan.
							</div>
						) : (
							<div className="divide-y border-t">
								{data.activeApprovalItems.map((item) => (
									<div
										key={item.companyId}
										className="flex items-center justify-between p-3.5 hover:bg-muted/40 transition-colors"
									>
										<div className="min-w-0 pr-2">
											<Link
												to="/directory/$companyId"
												params={{ companyId: item.companyId }}
												className="text-sm font-medium hover:underline text-foreground block truncate"
											>
												{item.companyName}
											</Link>
											<div className="flex items-center gap-2 mt-0.5 text-xs text-muted-foreground">
												<span>Tahap {item.currentStage}</span>
												<span>•</span>
												<span className="font-mono text-[11px]">
													{item.companyNomor}
												</span>
											</div>
										</div>
										<div className="text-right shrink-0">
											<Badge variant="secondary" className="text-[11px]">
												{item.holderLabel}
											</Badge>
											<p className="text-[10px] text-muted-foreground mt-0.5">
												Diajukan{" "}
												{new Date(item.submittedAt).toLocaleDateString("id-ID")}
											</p>
										</div>
									</div>
								))}
							</div>
						)}
					</CardContent>
				</Card>

				{/* Live Map Preview */}
				<DashboardMapPreview />
			</div>
		</div>
	);
}
