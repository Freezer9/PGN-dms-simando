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
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
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
			title: "Corong Penjualan (Funnel)",
			description:
				"Analisis tingkat konversi per tahapan (Stage 1 s.d. 8) dan identifikasi kebocoran pipeline penjualan gas.",
			href: "/reports/funnel",
			icon: BarChart3,
			badge: "Stage Conversion",
			exportUrl: "/api/reports/export/funnel",
			exportName: "Laporan_Corong_Penjualan.xlsx",
		},
		{
			id: "ageing",
			title: "Penuaan Proses (Ageing)",
			description:
				"Laporan waktu tunggu berkas aktif di seluruh tahapan persetujuan, diurutkan dari wait time terlama.",
			href: "/reports/ageing",
			icon: Clock,
			badge: "SLA Tracker",
			exportUrl: "/api/reports/export/ageing",
			exportName: "Laporan_Penuaan_Workflow.xlsx",
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
			<div className="bg-card p-6 rounded-xl border shadow-xs space-y-1">
				<h1 className="text-2xl sm:text-3xl font-bold tracking-tight">
					Pusat Laporan & Analitik
				</h1>
				<p className="text-muted-foreground text-sm">
					Akses laporan standar operasional, metrik konversi pipeline,
					pemantauan SLA penuaan berkas, dan ekspor spreadsheet terformat.
				</p>
			</div>

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
							<CardContent className="pt-2 flex items-center justify-between gap-2 border-t">
								<Button
									asChild
									variant="ghost"
									size="sm"
									className="gap-1 px-2"
								>
									<a
										href={r.exportUrl}
										download={r.exportName}
										className="text-emerald-700 dark:text-emerald-400 text-xs hover:underline flex items-center gap-1.5"
									>
										<FileSpreadsheet className="size-3.5" />
										Excel
									</a>
								</Button>
								<Button asChild size="sm" className="gap-1.5 shadow-xs">
									<Link to={r.href}>
										Buka Laporan
										<ArrowRight className="size-3.5" />
									</Link>
								</Button>
							</CardContent>
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
							<label className="flex items-center gap-2 mt-2 cursor-pointer text-xs font-medium text-emerald-950 dark:text-emerald-100">
								<input
									type="checkbox"
									checked={includePii}
									onChange={(e) => setIncludePii(e.target.checked)}
									className="rounded border-emerald-300 text-emerald-600 focus:ring-emerald-500 size-3.5"
								/>
								Sertakan Data Kontak Lengkap (Akses PII Berwenang)
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
