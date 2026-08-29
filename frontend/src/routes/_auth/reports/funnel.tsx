import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowRight, BarChart3, Loader2 } from "lucide-react";
import { $api } from "@/api/client";
import { ReportLayout } from "@/components/reports/report-layout";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
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

export const Route = createFileRoute("/_auth/reports/funnel")({
	component: FunnelReportPage,
});

function FunnelReportPage() {
	const {
		data: report,
		isLoading,
		error,
	} = $api.useQuery("get", "/api/reports/funnel");

	return (
		<ReportLayout
			title="Laporan Corong Penjualan (Sales Funnel)"
			description="Analisis konversi perpindahan antar tahapan dari Direktori hingga Terbit Surat NOL."
			exportEndpoint="/api/reports/export/funnel"
			exportFileName="Laporan_Corong_Penjualan.xlsx"
		>
			{isLoading ? (
				<div className="flex flex-col items-center justify-center min-h-[300px] gap-2">
					<Loader2 className="size-8 animate-spin text-primary" />
					<p className="text-sm text-muted-foreground">
						Memuat data corong penjualan...
					</p>
				</div>
			) : error || !report ? (
				<div className="text-center py-10 text-destructive text-sm">
					Gagal memuat data laporan corong penjualan.
				</div>
			) : (
				<div className="space-y-6">
					{/* KPI Header Bar */}
					<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
						<Card className="shadow-xs">
							<CardContent className="p-4 flex items-center justify-between">
								<div>
									<p className="text-xs font-semibold text-muted-foreground uppercase">
										Total Record Masuk
									</p>
									<p className="text-2xl font-bold mt-1 text-foreground">
										{report.totalRecords}
									</p>
								</div>
								<Badge variant="secondary">Semua Tahap</Badge>
							</CardContent>
						</Card>
						<Card className="shadow-xs">
							<CardContent className="p-4 flex items-center justify-between">
								<div>
									<p className="text-xs font-semibold text-muted-foreground uppercase">
										Tingkat Konversi Akhir (NOL)
									</p>
									<p className="text-2xl font-bold mt-1 text-emerald-600 dark:text-emerald-400">
										{Number(report.overallConversionRatePct).toFixed(1)}%
									</p>
								</div>
								<Badge
									variant="outline"
									className="border-emerald-500 text-emerald-600"
								>
									Target Conversion
								</Badge>
							</CardContent>
						</Card>
					</div>

					{/* Funnel Visual Bars Card */}
					<Card className="shadow-xs">
						<CardHeader className="pb-3">
							<CardTitle className="text-base font-semibold flex items-center gap-2">
								<BarChart3 className="size-4 text-primary" />
								Visualisasi Konversi Antar Tahap
							</CardTitle>
							<CardDescription className="text-xs">
								Persentase konversi dihitung terhadap jumlah record pada tahap
								sebelumnya untuk mendeteksi potensi hambatan (pipeline
								drop-off).
							</CardDescription>
						</CardHeader>
						<CardContent className="space-y-3">
							{report.stages.map((stage) => {
								const total = Number(report.totalRecords) || 0;
								const count = Number(stage.recordCount) || 0;
								const maxCount = total > 0 ? total : 1;
								const barWidthPct = Math.max(
									4,
									Math.round((count / maxCount) * 100),
								);

								return (
									<div
										key={String(stage.stage)}
										className="space-y-1.5 p-2 rounded-lg hover:bg-muted/30 transition-colors"
									>
										<div className="flex items-center justify-between text-xs font-medium">
											<div className="flex items-center gap-2">
												<span className="font-mono text-muted-foreground w-14">
													Tahap {stage.stage}
												</span>
												<span className="font-semibold text-foreground">
													{stage.stageName}
												</span>
											</div>
											<div className="flex items-center gap-3">
												<span className="font-bold text-foreground">
													{stage.recordCount} record
												</span>
												<Badge
													variant={
														Number(stage.stage) === 1 ? "secondary" : "outline"
													}
													className="text-[11px] font-mono min-w-16 text-center justify-center"
												>
													{Number(stage.stage) === 1
														? "Baseline"
														: `${Number(stage.conversionRatePct).toFixed(1)}%`}
												</Badge>
											</div>
										</div>
										<div className="h-3 w-full bg-muted rounded-full overflow-hidden flex">
											<div
												className="h-full bg-primary rounded-full transition-all duration-300"
												style={{ width: `${barWidthPct}%` }}
											/>
										</div>
									</div>
								);
							})}
						</CardContent>
					</Card>

					{/* Detail Table */}
					<Card className="shadow-xs">
						<CardHeader className="pb-3">
							<CardTitle className="text-base font-semibold">
								Rincian Data Konversi & Turnaround Time
							</CardTitle>
						</CardHeader>
						<CardContent className="p-0">
							<Table>
								<TableHeader>
									<TableRow>
										<TableHead className="w-16">Tahap</TableHead>
										<TableHead>Nama Tahapan</TableHead>
										<TableHead className="text-right">Jumlah Record</TableHead>
										<TableHead className="text-right">
											Tingkat Konversi (%)
										</TableHead>
										<TableHead className="text-right">
											Rata-Rata Waktu
										</TableHead>
										<TableHead className="text-right">Aksi</TableHead>
									</TableRow>
								</TableHeader>
								<TableBody>
									{report.stages.map((st) => (
										<TableRow key={String(st.stage)}>
											<TableCell className="font-mono font-medium">
												{st.stage}
											</TableCell>
											<TableCell className="font-semibold text-foreground">
												{st.stageName}
											</TableCell>
											<TableCell className="text-right font-medium">
												{st.recordCount}
											</TableCell>
											<TableCell className="text-right font-mono">
												{Number(st.stage) === 1
													? "-"
													: `${Number(st.conversionRatePct).toFixed(1)}%`}
											</TableCell>
											<TableCell className="text-right text-muted-foreground text-xs">
												{Number(st.avgTurnaroundDays) > 0
													? `${Number(st.avgTurnaroundDays).toFixed(1)} hari`
													: "-"}
											</TableCell>
											<TableCell className="text-right">
												<Button
													asChild
													size="sm"
													variant="ghost"
													className="h-7 text-xs"
												>
													<Link
														to="/directory"
														search={{ stage: Number(st.stage) }}
													>
														Lihat
														<ArrowRight className="size-3 ml-1" />
													</Link>
												</Button>
											</TableCell>
										</TableRow>
									))}
								</TableBody>
							</Table>
						</CardContent>
					</Card>
				</div>
			)}
		</ReportLayout>
	);
}
