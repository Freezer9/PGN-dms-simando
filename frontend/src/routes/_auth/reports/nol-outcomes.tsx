import { createFileRoute } from "@tanstack/react-router";
import { CheckCircle2, FileText, Loader2, XCircle } from "lucide-react";
import { $api } from "@/api/client";
import { ReportLayout } from "@/components/reports/report-layout";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";

export const Route = createFileRoute("/_auth/reports/nol-outcomes")({
	component: NolOutcomesReportPage,
});

function NolOutcomesReportPage() {
	const {
		data: report,
		isLoading,
		error,
	} = $api.useQuery("get", "/api/reports/nol-outcomes");

	return (
		<ReportLayout
			title="Laporan Hasil NOL / RL"
			description="Analisis rasio penerbitan Surat Persetujuan (NOL) vs Surat Penolakan (RL) beserta kategorisasi alasan teknis atau komersial."
			exportEndpoint="/api/reports/export/nol-outcomes"
			exportFileName="Laporan_Hasil_NOL_RL.xlsx"
		>
			{isLoading ? (
				<div className="flex flex-col items-center justify-center min-h-[300px] gap-2">
					<Loader2 className="size-8 animate-spin text-primary" />
					<p className="text-sm text-muted-foreground">
						Memuat data hasil evaluasi NOL / RL...
					</p>
				</div>
			) : error || !report ? (
				<div className="text-center py-10 text-destructive text-sm">
					Gagal memuat data laporan hasil evaluasi NOL / RL.
				</div>
			) : (
				<div className="space-y-6">
					{/* KPI Summary Cards */}
					<div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
						<Card className="shadow-xs">
							<CardContent className="p-5 flex items-center justify-between">
								<div className="space-y-1">
									<p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
										Total Permohonan Dievaluasi
									</p>
									<p className="text-3xl font-bold tracking-tight text-foreground">
										{report.totalEvaluated}
									</p>
								</div>
								<div className="p-3 bg-muted text-muted-foreground rounded-xl">
									<FileText className="size-6" />
								</div>
							</CardContent>
						</Card>

						<Card className="shadow-xs bg-linear-to-br from-emerald-500/10 via-background to-background border-emerald-200 dark:border-emerald-900/50">
							<CardContent className="p-5 flex items-center justify-between">
								<div className="space-y-1">
									<p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
										Surat NOL (Disetujui)
									</p>
									<div className="flex items-baseline gap-2">
										<p className="text-3xl font-bold tracking-tight text-emerald-600 dark:text-emerald-400">
											{report.nolCount}
										</p>
										<span className="text-sm font-semibold text-emerald-600/80">
											({Number(report.nolPercentage).toFixed(1)}%)
										</span>
									</div>
								</div>
								<div className="p-3 bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 rounded-xl">
									<CheckCircle2 className="size-6" />
								</div>
							</CardContent>
						</Card>

						<Card className="shadow-xs bg-linear-to-br from-rose-500/10 via-background to-background border-rose-200 dark:border-rose-900/50">
							<CardContent className="p-5 flex items-center justify-between">
								<div className="space-y-1">
									<p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
										Surat RL (Ditolak)
									</p>
									<div className="flex items-baseline gap-2">
										<p className="text-3xl font-bold tracking-tight text-rose-600 dark:text-rose-400">
											{report.rlCount}
										</p>
										<span className="text-sm font-semibold text-rose-600/80">
											({Number(report.rlPercentage).toFixed(1)}%)
										</span>
									</div>
								</div>
								<div className="p-3 bg-rose-500/15 text-rose-600 dark:text-rose-400 rounded-xl">
									<XCircle className="size-6" />
								</div>
							</CardContent>
						</Card>
					</div>

					{/* Ratio Bar Card */}
					<Card className="shadow-xs">
						<CardHeader className="pb-3">
							<CardTitle className="text-base font-semibold">
								Rasio Persetujuan Permohonan Gas
							</CardTitle>
						</CardHeader>
						<CardContent className="space-y-3">
							<div className="flex items-center justify-between text-xs font-medium">
								<div className="flex items-center gap-2 text-emerald-600 dark:text-emerald-400">
									<span className="size-2.5 rounded-full bg-emerald-500" />
									<span className="font-semibold">
										Terbit Surat NOL ({Number(report.nolPercentage).toFixed(1)}
										%)
									</span>
								</div>
								<div className="flex items-center gap-2 text-rose-600 dark:text-rose-400">
									<span className="size-2.5 rounded-full bg-rose-500" />
									<span className="font-semibold">
										Terbit Surat RL ({Number(report.rlPercentage).toFixed(1)}%)
									</span>
								</div>
							</div>
							<div className="h-4 w-full bg-muted rounded-full overflow-hidden flex">
								<div
									className="h-full bg-emerald-500 transition-all duration-300"
									style={{ width: `${Number(report.nolPercentage) || 0}%` }}
								/>
								<div
									className="h-full bg-rose-500 transition-all duration-300"
									style={{ width: `${Number(report.rlPercentage) || 0}%` }}
								/>
							</div>
						</CardContent>
					</Card>

					{/* Categorized Rejection Reasons Breakdown */}
					<Card className="shadow-xs">
						<CardHeader className="pb-3">
							<CardTitle className="text-base font-semibold">
								Kategori Alasan Penolakan (Surat RL)
							</CardTitle>
							<CardDescription className="text-xs">
								Frekuensi alasan teknis maupun komersial yang mendasari
								penolakan permohonan.
							</CardDescription>
						</CardHeader>
						<CardContent className="p-0">
							{report.rejectionReasons.length === 0 ? (
								<div className="py-12 text-center text-xs text-muted-foreground">
									Belum ada berkas penolakan dengan kategori terdaftar.
								</div>
							) : (
								<Table>
									<TableHeader>
										<TableRow>
											<TableHead>Kategori Alasan</TableHead>
											<TableHead className="text-right">Frekuensi</TableHead>
											<TableHead className="text-right">Porsi (%)</TableHead>
											<TableHead className="w-48 text-right">
												Distribusi
											</TableHead>
										</TableRow>
									</TableHeader>
									<TableBody>
										{report.rejectionReasons.map((reason) => (
											<TableRow key={reason.reasonCategoryName}>
												<TableCell className="font-semibold text-foreground">
													{reason.reasonCategoryName}
												</TableCell>
												<TableCell className="text-right font-mono font-bold">
													{reason.count}
												</TableCell>
												<TableCell className="text-right font-mono text-xs text-muted-foreground">
													{Number(reason.percentage).toFixed(1)}%
												</TableCell>
												<TableCell className="text-right">
													<div className="h-2.5 w-full bg-muted rounded-full overflow-hidden">
														<div
															className="h-full bg-rose-500 rounded-full"
															style={{
																width: `${Math.min(100, Math.max(5, Number(reason.percentage)))}%`,
															}}
														/>
													</div>
												</TableCell>
											</TableRow>
										))}
									</TableBody>
								</Table>
							)}
						</CardContent>
					</Card>
				</div>
			)}
		</ReportLayout>
	);
}
