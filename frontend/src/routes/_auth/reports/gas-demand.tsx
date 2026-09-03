import { createFileRoute } from "@tanstack/react-router";
import { Flame, Loader2 } from "lucide-react";
import { $api } from "@/api/client";
import { ReportLayout } from "@/components/reports/report-layout";
import { Badge } from "@/components/ui/badge";
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

export const Route = createFileRoute("/_auth/reports/gas-demand")({
	component: GasDemandReportPage,
});

function GasDemandReportPage() {
	const {
		data: report,
		isLoading,
		error,
	} = $api.useQuery("get", "/api/reports/gas-demand");

	return (
		<ReportLayout
			title="Laporan Potensi Kebutuhan Gas"
			description="Agregasi estimasi volume kebutuhan gas (MMBtu) yang dikelompokkan berdasarkan Tahapan, Wilayah Regional, dan Sektor Industri."
			exportEndpoint="/api/reports/export/gas-demand"
			exportFileName="Laporan_Potensi_Kebutuhan_Gas.xlsx"
		>
			{isLoading ? (
				<div className="flex flex-col items-center justify-center min-h-[300px] gap-2">
					<Loader2 className="size-8 animate-spin text-primary" />
					<p className="text-sm text-muted-foreground">
						Memuat data potensi kebutuhan gas...
					</p>
				</div>
			) : error || !report ? (
				<div className="text-center py-10 text-destructive text-sm">
					Gagal memuat data laporan kebutuhan gas.
				</div>
			) : (
				<div className="space-y-6">
					{/* KPI Summary Cards */}
					<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
						<Card className="shadow-xs bg-linear-to-br from-amber-500/10 via-background to-background border-amber-200 dark:border-amber-900/50">
							<CardContent className="p-5 flex items-center justify-between">
								<div className="space-y-1">
									<p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
										Total Potensi Kebutuhan Gas
									</p>
									<p className="text-3xl font-bold tracking-tight text-foreground">
										{Number(report.grandTotalDemandMMBtu).toLocaleString(
											"id-ID",
											{
												maximumFractionDigits: 2,
											},
										)}{" "}
										<span className="text-sm font-normal text-muted-foreground">
											MMBtu
										</span>
									</p>
								</div>
								<div className="p-3 bg-amber-500/15 text-amber-600 dark:text-amber-400 rounded-xl">
									<Flame className="size-6" />
								</div>
							</CardContent>
						</Card>

						<Card className="shadow-xs">
							<CardContent className="p-5 flex items-center justify-between">
								<div className="space-y-1">
									<p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
										Cakupan Seluruh Tahapan
									</p>
									<p className="text-3xl font-bold tracking-tight text-foreground">
										{report.byStage.reduce(
											(sum, s) => sum + Number(s.recordCount),
											0,
										)}{" "}
										<span className="text-sm font-normal text-muted-foreground">
											Perusahaan
										</span>
									</p>
								</div>
								<Badge variant="secondary" className="font-mono text-xs">
									Stage 1 s.d. 8
								</Badge>
							</CardContent>
						</Card>
					</div>

					{/* Breakdown Tabs */}
					<Tabs defaultValue="stage" className="space-y-4">
						<TabsList className="grid grid-cols-3 w-full sm:w-[450px]">
							<TabsTrigger value="stage" className="text-xs">
								Per Tahap
							</TabsTrigger>
							<TabsTrigger value="region" className="text-xs">
								Per Wilayah
							</TabsTrigger>
							<TabsTrigger value="industry" className="text-xs">
								Per Industri
							</TabsTrigger>
						</TabsList>

						{/* 1. By Stage */}
						<TabsContent value="stage" className="space-y-4">
							<Card className="shadow-xs">
								<CardHeader className="pb-3">
									<CardTitle className="text-base font-semibold">
										Distribusi Kebutuhan Gas per Tahapan Pipeline
									</CardTitle>
									<CardDescription className="text-xs">
										Volume akumulatif di setiap tahapan progress pelanggan.
									</CardDescription>
								</CardHeader>
								<CardContent className="p-0">
									<Table>
										<TableHeader>
											<TableRow>
												<TableHead className="w-16">Tahap</TableHead>
												<TableHead>Nama Tahapan</TableHead>
												<TableHead className="text-right">
													Jumlah Perusahaan
												</TableHead>
												<TableHead className="text-right">
													Total Kebutuhan (MMBtu)
												</TableHead>
												<TableHead className="text-right">Porsi (%)</TableHead>
											</TableRow>
										</TableHeader>
										<TableBody>
											{report.byStage.map((st) => {
												const grandTotal =
													Number(report.grandTotalDemandMMBtu) || 1;
												const stageDemand = Number(st.totalDemandMMBtu) || 0;
												const pct = (stageDemand / grandTotal) * 100;

												return (
													<TableRow key={st.stage}>
														<TableCell className="font-mono font-medium">
															{st.stage}
														</TableCell>
														<TableCell className="font-semibold text-foreground">
															{st.stageName}
														</TableCell>
														<TableCell className="text-right font-medium">
															{st.recordCount}
														</TableCell>
														<TableCell className="text-right font-mono font-semibold">
															{stageDemand.toLocaleString("id-ID", {
																maximumFractionDigits: 2,
															})}
														</TableCell>
														<TableCell className="text-right font-mono text-xs text-muted-foreground">
															{pct.toFixed(1)}%
														</TableCell>
													</TableRow>
												);
											})}
										</TableBody>
									</Table>
								</CardContent>
							</Card>
						</TabsContent>

						{/* 2. By Region */}
						<TabsContent value="region" className="space-y-4">
							<Card className="shadow-xs">
								<CardHeader className="pb-3">
									<CardTitle className="text-base font-semibold">
										Distribusi Kebutuhan Gas per Wilayah Regional
									</CardTitle>
									<CardDescription className="text-xs">
										Rekapitulasi volume berdasarkan unit regional operasi PGN.
									</CardDescription>
								</CardHeader>
								<CardContent className="p-0">
									{report.byRegion.length === 0 ? (
										<div className="py-8 text-center text-xs text-muted-foreground">
											Belum ada data regional.
										</div>
									) : (
										<Table>
											<TableHeader>
												<TableRow>
													<TableHead>Wilayah Regional</TableHead>
													<TableHead className="text-right">
														Jumlah Perusahaan
													</TableHead>
													<TableHead className="text-right">
														Total Kebutuhan (MMBtu)
													</TableHead>
													<TableHead className="text-right">
														Porsi (%)
													</TableHead>
												</TableRow>
											</TableHeader>
											<TableBody>
												{report.byRegion.map((rg) => {
													const grandTotal =
														Number(report.grandTotalDemandMMBtu) || 1;
													const regDemand = Number(rg.totalDemandMMBtu) || 0;
													const pct = (regDemand / grandTotal) * 100;

													return (
														<TableRow key={rg.regionName}>
															<TableCell className="font-semibold text-foreground">
																{rg.regionName}
															</TableCell>
															<TableCell className="text-right font-medium">
																{rg.recordCount}
															</TableCell>
															<TableCell className="text-right font-mono font-semibold">
																{regDemand.toLocaleString("id-ID", {
																	maximumFractionDigits: 2,
																})}
															</TableCell>
															<TableCell className="text-right font-mono text-xs text-muted-foreground">
																{pct.toFixed(1)}%
															</TableCell>
														</TableRow>
													);
												})}
											</TableBody>
										</Table>
									)}
								</CardContent>
							</Card>
						</TabsContent>

						{/* 3. By Industry */}
						<TabsContent value="industry" className="space-y-4">
							<Card className="shadow-xs">
								<CardHeader className="pb-3">
									<CardTitle className="text-base font-semibold">
										Distribusi Kebutuhan Gas per Sektor Industri
									</CardTitle>
									<CardDescription className="text-xs">
										Segmentasi volume kebutuhan gas berdasarkan klasifikasi
										industri pelanggan.
									</CardDescription>
								</CardHeader>
								<CardContent className="p-0">
									{report.byIndustry.length === 0 ? (
										<div className="py-8 text-center text-xs text-muted-foreground">
											Belum ada data sektor industri.
										</div>
									) : (
										<Table>
											<TableHeader>
												<TableRow>
													<TableHead>Sektor Industri</TableHead>
													<TableHead className="text-right">
														Jumlah Perusahaan
													</TableHead>
													<TableHead className="text-right">
														Total Kebutuhan (MMBtu)
													</TableHead>
													<TableHead className="text-right">
														Porsi (%)
													</TableHead>
												</TableRow>
											</TableHeader>
											<TableBody>
												{report.byIndustry.map((ind) => {
													const grandTotal =
														Number(report.grandTotalDemandMMBtu) || 1;
													const indDemand = Number(ind.totalDemandMMBtu) || 0;
													const pct = (indDemand / grandTotal) * 100;

													return (
														<TableRow key={ind.industryTypeName}>
															<TableCell className="font-semibold text-foreground">
																{ind.industryTypeName}
															</TableCell>
															<TableCell className="text-right font-medium">
																{ind.recordCount}
															</TableCell>
															<TableCell className="text-right font-mono font-semibold">
																{indDemand.toLocaleString("id-ID", {
																	maximumFractionDigits: 2,
																})}
															</TableCell>
															<TableCell className="text-right font-mono text-xs text-muted-foreground">
																{pct.toFixed(1)}%
															</TableCell>
														</TableRow>
													);
												})}
											</TableBody>
										</Table>
									)}
								</CardContent>
							</Card>
						</TabsContent>
					</Tabs>
				</div>
			)}
		</ReportLayout>
	);
}
