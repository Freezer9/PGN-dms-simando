import { createFileRoute } from "@tanstack/react-router";
import { ClipboardCheck, Clock, Loader2, Search } from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import { ReportLayout } from "@/components/reports/report-layout";
import { Badge } from "@/components/ui/badge";
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

export const Route = createFileRoute("/_auth/reports/survey-productivity")({
	component: SurveyProductivityReportPage,
});

const MONTH_NAMES = [
	"",
	"Januari",
	"Februari",
	"Maret",
	"April",
	"Mei",
	"Juni",
	"Juli",
	"Agustus",
	"September",
	"Oktober",
	"November",
	"Desember",
];

function SurveyProductivityReportPage() {
	const currentYear = new Date().getFullYear();
	const [selectedYear, setSelectedYear] = React.useState<number>(currentYear);
	const [searchQuery, setSearchQuery] = React.useState("");

	const {
		data: report,
		isLoading,
		error,
	} = $api.useQuery("get", "/api/reports/survey-productivity", {
		params: {
			query: {
				year: selectedYear,
			},
		},
	});

	const filteredRows = React.useMemo(() => {
		if (!report?.rows) return [];
		if (!searchQuery.trim()) return report.rows;

		const q = searchQuery.toLowerCase();
		return report.rows.filter(
			(r) =>
				r.salesRepName.toLowerCase().includes(q) ||
				r.areaName.toLowerCase().includes(q),
		);
	}, [report, searchQuery]);

	const avgDaysOverall = React.useMemo(() => {
		if (!report?.rows || report.rows.length === 0) return 0;
		const totalDays = report.rows.reduce(
			(sum, r) => sum + Number(r.avgDaysPerSurvey),
			0,
		);
		return totalDays / report.rows.length;
	}, [report]);

	return (
		<ReportLayout
			title="Laporan Produktivitas Survei"
			description="Rekapitulasi output survei lapangan (Formulir KK0) per Sales Representative per bulan beserta rata-rata durasi penyelesaian."
			exportEndpoint={`/api/reports/export/survey-productivity?year=${selectedYear}`}
			exportFileName={`Laporan_Produktivitas_Survei_${selectedYear}.xlsx`}
			filterContent={
				<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
					<div className="flex flex-wrap items-center gap-3 flex-1">
						<div className="relative flex-1 max-w-xs">
							<Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
							<Input
								placeholder="Cari nama Sales Rep atau Area..."
								value={searchQuery}
								onChange={(e) => setSearchQuery(e.target.value)}
								className="pl-9 h-9 text-xs"
							/>
						</div>
						<div className="flex items-center gap-2">
							<span className="text-xs text-muted-foreground">Tahun:</span>
							<Select
								value={selectedYear.toString()}
								onValueChange={(val) => setSelectedYear(Number(val))}
							>
								<SelectTrigger className="w-28 h-9 text-xs">
									<SelectValue placeholder="Pilih Tahun" />
								</SelectTrigger>
								<SelectContent>
									{[currentYear, currentYear - 1, currentYear - 2].map((yr) => (
										<SelectItem key={yr} value={yr.toString()}>
											{yr}
										</SelectItem>
									))}
								</SelectContent>
							</Select>
						</div>
					</div>
					<Badge
						variant="outline"
						className="font-mono text-xs self-start sm:self-auto"
					>
						{filteredRows.length} entri bulanan
					</Badge>
				</div>
			}
			onResetFilters={() => {
				setSelectedYear(currentYear);
				setSearchQuery("");
			}}
		>
			{isLoading ? (
				<div className="flex flex-col items-center justify-center min-h-[300px] gap-2">
					<Loader2 className="size-8 animate-spin text-primary" />
					<p className="text-sm text-muted-foreground">
						Memuat data produktivitas survei...
					</p>
				</div>
			) : error || !report ? (
				<div className="text-center py-10 text-destructive text-sm">
					Gagal memuat data laporan produktivitas survei.
				</div>
			) : (
				<div className="space-y-6">
					{/* KPI Summary Cards */}
					<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
						<Card className="shadow-xs bg-linear-to-br from-blue-500/10 via-background to-background border-blue-200 dark:border-blue-900/50">
							<CardContent className="p-5 flex items-center justify-between">
								<div className="space-y-1">
									<p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
										Total KK0 Diselesaikan ({selectedYear})
									</p>
									<p className="text-3xl font-bold tracking-tight text-foreground">
										{report.totalSurveysCompleted}{" "}
										<span className="text-sm font-normal text-muted-foreground">
											Survei
										</span>
									</p>
								</div>
								<div className="p-3 bg-blue-500/15 text-blue-600 dark:text-blue-400 rounded-xl">
									<ClipboardCheck className="size-6" />
								</div>
							</CardContent>
						</Card>

						<Card className="shadow-xs">
							<CardContent className="p-5 flex items-center justify-between">
								<div className="space-y-1">
									<p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
										Rata-Rata Durasi Survei
									</p>
									<p className="text-3xl font-bold tracking-tight text-foreground">
										{avgDaysOverall.toFixed(1)}{" "}
										<span className="text-sm font-normal text-muted-foreground">
											Hari / KK0
										</span>
									</p>
								</div>
								<div className="p-3 bg-muted text-muted-foreground rounded-xl">
									<Clock className="size-6" />
								</div>
							</CardContent>
						</Card>
					</div>

					{/* Detail Table */}
					<Card className="shadow-xs">
						<CardContent className="p-0">
							{filteredRows.length === 0 ? (
								<div className="py-12 text-center text-xs text-muted-foreground">
									Tidak ada data survei untuk filter yang dipilih.
								</div>
							) : (
								<Table>
									<TableHeader>
										<TableRow>
											<TableHead>Sales Representative</TableHead>
											<TableHead>Sales Area</TableHead>
											<TableHead>Periode</TableHead>
											<TableHead className="text-right">
												Survei KK0 Selesai
											</TableHead>
											<TableHead className="text-right">
												Rata-Rata Waktu
											</TableHead>
										</TableRow>
									</TableHeader>
									<TableBody>
										{filteredRows.map((r) => (
											<TableRow key={`${r.userId}-${r.year}-${r.month}`}>
												<TableCell>
													<div className="flex items-center gap-2">
														<div className="size-7 rounded-full bg-primary/10 text-primary flex items-center justify-center text-xs font-semibold">
															{r.salesRepName ? r.salesRepName.charAt(0) : "S"}
														</div>
														<span className="font-semibold text-foreground text-sm">
															{r.salesRepName}
														</span>
													</div>
												</TableCell>
												<TableCell className="text-xs text-muted-foreground">
													{r.areaName}
												</TableCell>
												<TableCell className="text-xs font-medium">
													{MONTH_NAMES[Number(r.month)] || `Bulan ${r.month}`}{" "}
													{r.year}
												</TableCell>
												<TableCell className="text-right font-mono font-bold text-foreground">
													{r.surveysCompletedCount}
												</TableCell>
												<TableCell className="text-right text-xs">
													<span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-mono font-medium border bg-muted text-muted-foreground">
														{Number(r.avgDaysPerSurvey).toFixed(1)} hari
													</span>
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
