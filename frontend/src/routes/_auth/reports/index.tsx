import { createFileRoute, Link } from "@tanstack/react-router";
import {
	ArrowRight,
	BarChart3,
	CheckCircle2,
	ClipboardCheck,
	Clock,
	FileSpreadsheet,
	Flame,
	ShieldCheck,
} from "lucide-react";
import * as React from "react";
import { PageHeader } from "@/components/common";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardFooter,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { useAuth } from "@/lib/auth";

export const Route = createFileRoute("/_auth/reports/")({
	component: ReportsHubPage,
});

function ReportsHubPage() {
	const { user } = useAuth();
	const hasPiiCapability = user?.capabilities?.includes("ExportContactDataPii");
	const [includePii, setIncludePii] = React.useState(false);
	const [isExportingDir, setIsExportingDir] = React.useState(false);

	const handleExportDirectory = async () => {
		try {
			setIsExportingDir(true);
			const url = `/api/reports/export/directory?includePii=${includePii && hasPiiCapability}`;
			const response = await fetch(url, { credentials: "include" });
			if (!response.ok) throw new Error("Gagal mengunduh direktori.");

			const blob = await response.blob();
			const blobUrl = window.URL.createObjectURL(blob);
			const link = document.createElement("a");
			link.href = blobUrl;
			link.download = `Direktori_Perusahaan_${new Date().toISOString().slice(0, 10)}.xlsx`;
			document.body.appendChild(link);
			link.click();
			document.body.removeChild(link);
			window.URL.revokeObjectURL(blobUrl);
		} catch (error) {
			console.error("Download error:", error);
		} finally {
			setIsExportingDir(false);
		}
	};

	const reports = [
		{
			id: "funnel",
			title: "Sales Funnel",
			description:
				"Analisis tingkat konversi per tahapan (Stage 1 s.d. 8) dan identifikasi hambatan penurunan berkas calon pelanggan gas.",
			href: "/reports/funnel",
			icon: BarChart3,
			badge: "Stage Conversion",
			exportUrl: "/api/reports/export/funnel",
			exportName: "Laporan_Sales_Funnel.xlsx",
		},
		{
			id: "ageing",
			title: "Durasi Proses & Ageing",
			description:
				"Laporan waktu tunggu berkas aktif di seluruh tahapan persetujuan, diurutkan dari waktu tunggu terlama.",
			href: "/reports/ageing",
			icon: Clock,
			badge: "SLA Tracker",
			exportUrl: "/api/reports/export/ageing",
			exportName: "Laporan_Ageing_Workflow.xlsx",
		},
		{
			id: "gas-demand",
			title: "Potensi Kebutuhan Gas",
			description:
				"Agregasi total volume kebutuhan gas (MMBtu) dikelompokkan berdasarkan Tahap, Wilayah, dan Sektor Industri.",
			href: "/reports/gas-demand",
			icon: Flame,
			badge: "Volume Demand",
			exportUrl: "/api/reports/export/gas-demand",
			exportName: "Laporan_Potensi_Kebutuhan_Gas.xlsx",
		},
		{
			id: "survey-productivity",
			title: "Produktivitas Survei",
			description:
				"Rekapitulasi jumlah formulir KK0 yang diselesaikan per Sales Representative per bulan beserta rata-rata durasi survei.",
			href: "/reports/survey-productivity",
			icon: ClipboardCheck,
			badge: "Sales Output",
			exportUrl: "/api/reports/export/survey-productivity",
			exportName: "Laporan_Produktivitas_Survei.xlsx",
		},
		{
			id: "nol-outcomes",
			title: "Hasil NOL / RL",
			description:
				"Rasio persetujuan Surat NOL vs Surat Penolakan (RL) beserta kategorisasi alasan penolakan atau revisi syarat teknis.",
			href: "/reports/nol-outcomes",
			icon: CheckCircle2,
			badge: "Approval Ratio",
			exportUrl: "/api/reports/export/nol-outcomes",
			exportName: "Laporan_Hasil_NOL_RL.xlsx",
		},
	];

	return (
		<div className="space-y-8">
			{/* Page Header */}
			<PageHeader
				title="Pusat Laporan & Analitik"
				description="Akses laporan standar operasional, metrik konversi pipeline, pemantauan SLA & ageing berkas, dan ekspor spreadsheet terformat."
			/>

			{/* 5 Reports Cards Grid */}
			<div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
				{reports.map((r) => {
					const Icon = r.icon;
					return (
						<Card
							key={r.id}
							className="shadow-xs hover:border-primary/50 transition-all flex flex-col justify-between"
						>
							<CardHeader className="space-y-2">
								<div className="flex items-center justify-between">
									<div className="p-2.5 rounded-lg bg-primary/10 text-primary">
										<Icon className="size-5" />
									</div>
									<Badge variant="secondary" className="text-xs">
										{r.badge}
									</Badge>
								</div>
								<CardTitle className="text-lg">{r.title}</CardTitle>
								<CardDescription className="text-xs line-clamp-3 leading-relaxed">
									{r.description}
								</CardDescription>
							</CardHeader>
							<CardFooter className="py-3 px-6 flex items-center justify-between gap-2 border-t bg-muted/20">
								<Button
									asChild
									variant="outline"
									size="sm"
									className="h-8 gap-1.5 text-emerald-700 dark:text-emerald-400 hover:text-emerald-800 text-xs border-emerald-200 dark:border-emerald-800 hover:bg-emerald-50 dark:hover:bg-emerald-950/40"
								>
									<a
										href={r.exportUrl}
										download={r.exportName}
										className="flex items-center gap-1.5"
									>
										<FileSpreadsheet className="size-3.5" />
										<span>Excel</span>
									</a>
								</Button>
								<Button asChild size="sm" className="h-8 gap-1.5 shadow-xs">
									<Link to={r.href}>
										<span>Buka Laporan</span>
										<ArrowRight className="size-3.5" />
									</Link>
								</Button>
							</CardFooter>
						</Card>
					);
				})}
			</div>

			{/* Directory Spreadsheet Export Banner */}
			<Card className="border-emerald-200 bg-emerald-50/40 dark:border-emerald-900/50 dark:bg-emerald-950/20 shadow-xs">
				<CardContent className="p-5 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
					<div className="space-y-1">
						<div className="flex items-center gap-2">
							<ShieldCheck className="size-5 text-emerald-700 dark:text-emerald-400" />
							<h3 className="font-semibold text-base text-emerald-950 dark:text-emerald-100">
								Ekspor Data Direktori Pelanggan & Prospek
							</h3>
						</div>
						<p className="text-xs text-emerald-800/80 dark:text-emerald-300/80">
							Unduh seluruh basis data perusahaan dalam format ClosedXML
							(.xlsx). Sesuai ketentuan UU Perlindungan Data Pribadi (UU
							27/2022), data kontak nomor telepon dan email disamarkan (masked)
							secara default.
						</p>
						{hasPiiCapability && (
							<label
								htmlFor="include-pii"
								className="flex items-center gap-2 mt-2 cursor-pointer text-xs font-medium text-emerald-950 dark:text-emerald-100 select-none"
							>
								<Checkbox
									id="include-pii"
									checked={includePii}
									onCheckedChange={(checked) => setIncludePii(Boolean(checked))}
								/>
								<span>Sertakan Data Kontak (Akses PII)</span>
							</label>
						)}
					</div>
					<Button
						onClick={handleExportDirectory}
						disabled={isExportingDir}
						size="sm"
						className="bg-emerald-700 hover:bg-emerald-800 text-white shrink-0 gap-1.5 h-9 shadow-xs"
					>
						<FileSpreadsheet className="size-4" />
						{isExportingDir ? "Menyiapkan File..." : "Unduh Direktori (.xlsx)"}
					</Button>
				</CardContent>
			</Card>
		</div>
	);
}
