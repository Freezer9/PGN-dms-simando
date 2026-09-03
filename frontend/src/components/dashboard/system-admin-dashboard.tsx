import { Link } from "@tanstack/react-router";
import {
	AlertTriangle,
	CheckCircle2,
	Factory,
	FileCode2,
	Fuel,
	Gauge,
	Globe,
	MapPinned,
	Network,
	OctagonAlert,
	ShieldAlert,
	Users,
} from "lucide-react";
import type { SystemAdminDashboardDto } from "@/api/types";
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
import { StatTile } from "./stat-tile";

interface SystemAdminDashboardProps {
	data: SystemAdminDashboardDto;
}

export function SystemAdminDashboard({ data }: SystemAdminDashboardProps) {
	const allHealthy = data.healthItems.every((item) => item.isHealthy);

	return (
		<div className="space-y-6">
			{/* Hero & Quick Action Bar */}
			<PageHeader
				title="Beranda Administrator Sistem"
				description="Pengelolaan integritas master data, pemeliharaan katalog sistem, pengguna, dan audit keamanan."
				badge={
					<Badge variant="outline" className="font-semibold">
						Administrator Sistem
					</Badge>
				}
				actions={
					<>
						<Button asChild size="sm" variant="outline" className="gap-1.5">
							<Link to="/tasks/blocked">
								<OctagonAlert className="size-4" />
								Langkah Tertahan
							</Link>
						</Button>
						<Button asChild size="sm" variant="outline" className="gap-1.5">
							<Link to="/admin/break-glass">
								<ShieldAlert className="size-4" />
								Audit Break-Glass
							</Link>
						</Button>
						<Button asChild size="sm" className="gap-1.5 shadow-xs">
							<Link to="/master/users">
								<Users className="size-4" />
								Organisasi & Akses
							</Link>
						</Button>
					</>
				}
			/>

			{/* Top KPI Stat Cards */}
			<div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
				<StatTile
					title="Pengguna Aktif"
					value={data.activeUsersCount}
					description="Akun terdaftar dan aktif dalam sistem"
					icon={Users}
					variant="default"
				/>
				<StatTile
					title="Sales Area Terdaftar"
					value={data.activeAreasCount}
					description="Kantor area operasional terdaftar"
					icon={MapPinned}
					variant="default"
				/>
				<StatTile
					title="Wilayah Regional"
					value={data.activeRegionsCount}
					description="Struktur wilayah operasional"
					icon={Network}
					variant="default"
				/>
				<StatTile
					title="Kesehatan Master Data"
					value={allHealthy ? "100% OK" : "Perlu Perhatian"}
					description={
						allHealthy
							? "Seluruh katalog master data terisi lengkap"
							: "Terdapat master data yang belum diisi"
					}
					icon={allHealthy ? CheckCircle2 : AlertTriangle}
					variant={allHealthy ? "emerald" : "rose"}
					badge={allHealthy ? "Optimal" : "Periksa"}
				/>
			</div>

			{/* Master Data Health Checklist */}
			<Card className="shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-base font-semibold">
						Status Kelengkapan & Kesehatan Master Data
					</CardTitle>
					<CardDescription className="text-xs">
						Pemeriksaan otomatis ketersediaan data acuan yang diperlukan untuk
						operasional stage gate.
					</CardDescription>
				</CardHeader>
				<CardContent>
					<div className="grid gap-3 sm:grid-cols-2">
						{data.healthItems.map((item) => (
							<div
								key={item.key}
								className="flex items-start justify-between gap-3 p-3.5 rounded-lg border bg-card hover:bg-muted/30 transition-colors"
							>
								<div className="flex items-start gap-3">
									<div
										className={`p-1.5 rounded-full shrink-0 mt-0.5 ${
											item.isHealthy
												? "bg-emerald-100 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-300"
												: "bg-rose-100 text-rose-700 dark:bg-rose-950 dark:text-rose-300"
										}`}
									>
										{item.isHealthy ? (
											<CheckCircle2 className="size-4" />
										) : (
											<AlertTriangle className="size-4" />
										)}
									</div>
									<div>
										<h4 className="font-semibold text-sm text-foreground">
											{item.title}
										</h4>
										<p className="text-xs text-muted-foreground mt-0.5">
											{item.description}
										</p>
									</div>
								</div>
								<Badge
									variant={item.isHealthy ? "secondary" : "destructive"}
									className="text-[11px] shrink-0"
								>
									{item.isHealthy ? "Terkonfigurasi" : "Belum Diisi"}
								</Badge>
							</div>
						))}
					</div>
				</CardContent>
			</Card>

			{/* Quick Access to Master Data Modules */}
			<div className="space-y-3">
				<h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider">
					Modul Master Data Cepat
				</h3>
				<div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3">
					<Button
						asChild
						variant="outline"
						className="h-auto flex-col p-4 gap-2 text-center"
					>
						<Link to="/master/industry-types">
							<Factory className="size-5 text-primary" />
							<span className="text-xs font-semibold">Jenis Industri</span>
						</Link>
					</Button>
					<Button
						asChild
						variant="outline"
						className="h-auto flex-col p-4 gap-2 text-center"
					>
						<Link to="/master/fuel-types">
							<Fuel className="size-5 text-primary" />
							<span className="text-xs font-semibold">Bahan Bakar</span>
						</Link>
					</Button>
					<Button
						asChild
						variant="outline"
						className="h-auto flex-col p-4 gap-2 text-center"
					>
						<Link to="/master/meter-sizes">
							<Gauge className="size-5 text-primary" />
							<span className="text-xs font-semibold">Ukuran Meter</span>
						</Link>
					</Button>
					<Button
						asChild
						variant="outline"
						className="h-auto flex-col p-4 gap-2 text-center"
					>
						<Link to="/master/reference-documents">
							<FileCode2 className="size-5 text-primary" />
							<span className="text-xs font-semibold">Dokumen Acuan</span>
						</Link>
					</Button>
					<Button
						asChild
						variant="outline"
						className="h-auto flex-col p-4 gap-2 text-center"
					>
						<Link to="/master/countries">
							<Globe className="size-5 text-primary" />
							<span className="text-xs font-semibold">Negara</span>
						</Link>
					</Button>
					<Button
						asChild
						variant="outline"
						className="h-auto flex-col p-4 gap-2 text-center"
					>
						<Link to="/master/organisation">
							<Network className="size-5 text-primary" />
							<span className="text-xs font-semibold">Organisasi</span>
						</Link>
					</Button>
				</div>
			</div>
		</div>
	);
}
