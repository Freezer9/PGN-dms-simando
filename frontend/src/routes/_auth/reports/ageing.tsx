import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowRight, Loader2, Search } from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import { IconButton } from "@/components/common";
import { ReportLayout } from "@/components/reports/report-layout";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";

export const Route = createFileRoute("/_auth/reports/ageing")({
	component: AgeingReportPage,
});

function AgeingReportPage() {
	const {
		data: rows,
		isLoading,
		error,
	} = $api.useQuery("get", "/api/reports/ageing");

	const [searchQuery, setSearchQuery] = React.useState("");

	const filteredRows = React.useMemo(() => {
		if (!rows) return [];
		if (!searchQuery.trim()) return rows;

		const q = searchQuery.toLowerCase();
		return rows.filter(
			(r) =>
				r.namaPerusahaan.toLowerCase().includes(q) ||
				r.nomor.toLowerCase().includes(q) ||
				r.areaName.toLowerCase().includes(q) ||
				r.actorLabel.toLowerCase().includes(q),
		);
	}, [rows, searchQuery]);

	return (
		<ReportLayout
			title="Laporan Durasi Proses & Ageing Berkas"
			description="Pemantauan berkas aktif di alur kerja persetujuan yang diurutkan berdasarkan waktu tunggu terlama."
			exportEndpoint="/api/reports/export/ageing"
			exportFileName="Laporan_Durasi_Ageing_Berkas.xlsx"
			filterContent={
				<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
					<div className="relative flex-1 max-w-md">
						<Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
						<Input
							placeholder="Cari perusahaan, nomor register, atau penanggung jawab..."
							value={searchQuery}
							onChange={(e) => setSearchQuery(e.target.value)}
							className="pl-9 h-9 text-xs"
						/>
					</div>
					<Badge
						variant="outline"
						className="font-mono text-xs self-start sm:self-auto"
					>
						{filteredRows.length} berkas dalam proses
					</Badge>
				</div>
			}
			onResetFilters={() => setSearchQuery("")}
		>
			{isLoading ? (
				<div className="flex flex-col items-center justify-center min-h-[300px] gap-2">
					<Loader2 className="size-8 animate-spin text-primary" />
					<p className="text-sm text-muted-foreground">
						Memuat data durasi proses berkas...
					</p>
				</div>
			) : error || !rows ? (
				<div className="text-center py-10 text-destructive text-sm">
					Gagal memuat data laporan durasi proses berkas.
				</div>
			) : (
				<Card className="shadow-xs">
					<CardContent className="p-0">
						{filteredRows.length === 0 ? (
							<div className="py-12 text-center text-xs text-muted-foreground">
								Tidak ada berkas yang cocok dengan filter pencarian.
							</div>
						) : (
							<Table>
								<TableHeader>
									<TableRow>
										<TableHead>Perusahaan</TableHead>
										<TableHead>Sektor Industri</TableHead>
										<TableHead>Tahap Berjalan</TableHead>
										<TableHead>Area / Regional</TableHead>
										<TableHead>Menunggu di (Actor)</TableHead>
										<TableHead className="text-right">Menunggu Sejak</TableHead>
										<TableHead className="text-right">Durasi Tunggu</TableHead>
										<TableHead className="text-right">Aksi</TableHead>
									</TableRow>
								</TableHeader>
								<TableBody>
									{filteredRows.map((r) => {
										const waitingDays = Math.max(
											0,
											Math.floor(
												(Date.now() - new Date(r.waitingSince).getTime()) /
													(1000 * 60 * 60 * 24),
											),
										);
										const durationStyle =
											waitingDays > 7
												? "text-rose-700 bg-rose-50 border-rose-200 dark:bg-rose-950/40 dark:text-rose-300"
												: waitingDays >= 3
													? "text-amber-700 bg-amber-50 border-amber-200 dark:bg-amber-950/40 dark:text-amber-300"
													: "text-emerald-700 bg-emerald-50 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300";

										return (
											<TableRow key={r.companyId}>
												<TableCell>
													<div>
														<Link
															to="/directory/$companyId"
															params={{ companyId: r.companyId }}
															className="font-semibold text-sm hover:underline text-foreground"
														>
															{r.namaPerusahaan}
														</Link>
														<div className="text-xs font-mono text-muted-foreground mt-0.5">
															{r.nomor}
														</div>
													</div>
												</TableCell>
												<TableCell className="text-xs text-muted-foreground">
													{r.industryTypeName || "Industri"}
												</TableCell>
												<TableCell>
													<Badge variant="secondary" className="text-xs">
														{r.stepKind || "Approval"}
													</Badge>
												</TableCell>
												<TableCell className="text-xs">
													<div className="font-medium text-foreground">
														{r.areaName}
													</div>
													<div className="text-[11px] text-muted-foreground">
														{r.regionName}
													</div>
												</TableCell>
												<TableCell>
													<div className="font-medium text-xs text-foreground">
														{r.actorLabel}
													</div>
												</TableCell>
												<TableCell className="text-right text-xs text-muted-foreground">
													{new Date(r.waitingSince).toLocaleDateString(
														"id-ID",
														{
															day: "numeric",
															month: "short",
															year: "numeric",
														},
													)}
												</TableCell>
												<TableCell className="text-right">
													<span
														className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium border font-mono ${durationStyle}`}
													>
														⏱ {waitingDays} hari
													</span>
												</TableCell>
												<TableCell className="text-right pr-4">
													<IconButton tooltip="Periksa Berkas" asChild>
														<Link
															to="/directory/$companyId"
															params={{ companyId: r.companyId }}
															aria-label="Periksa berkas perusahaan"
														>
															<ArrowRight className="size-4" />
														</Link>
													</IconButton>
												</TableCell>
											</TableRow>
										);
									})}
								</TableBody>
							</Table>
						)}
					</CardContent>
				</Card>
			)}
		</ReportLayout>
	);
}
