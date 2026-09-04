import { Link, useRouterState } from "@tanstack/react-router";
import { Home } from "lucide-react";
import * as React from "react";
import {
	Breadcrumb,
	BreadcrumbItem,
	BreadcrumbLink,
	BreadcrumbList,
	BreadcrumbPage,
	BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";

const ROUTE_LABELS: Record<string, string> = {
	"": "Beranda",
	directory: "Direktori",
	plotting: "Plotting",
	map: "Peta GIS",
	tasks: "Tugas Saya",
	blocked: "Tugas Tertahan",
	history: "Riwayat Tindakan",
	reports: "Laporan",
	"gas-demand": "Kebutuhan Gas",
	ageing: "Durasi Proses & SLA",
	funnel: "Sales Funnel",
	"survey-productivity": "Produktivitas Survei",
	"nol-outcomes": "Hasil NOL / RL",
	master: "Master Data",
	admin: "Administrasi",
	organisation: "Organisasi",
	users: "Pengguna",
	countries: "Negara",
	"industry-types": "Jenis Industri",
	segments: "Segmen",
	"fuel-types": "Jenis Bahan Bakar",
	units: "Satuan",
	"meter-sizes": "G-Size / Meter",
	"mrs-specs": "Spesifikasi MRS",
	"reference-documents": "Dokumen Acuan Kerja",
	"reason-categories": "Kategori Alasan",
	"stuck-steps": "Langkah Tertahan",
	"break-glass": "Akses Darurat",
	companies: "Perusahaan",
	new: "Tambah Baru",
	"change-password": "Ubah Kata Sandi",
	"access-denied": "Akses Ditolak",
	"sign-in": "Masuk",
};

export function Breadcrumbs({ className }: { className?: string }) {
	const routerState = useRouterState();
	const pathname = routerState.location.pathname;

	const segments = pathname.split("/").filter(Boolean);

	if (segments.length === 0) {
		return (
			<Breadcrumb className={className}>
				<BreadcrumbList className="text-xs flex-nowrap whitespace-nowrap">
					<BreadcrumbItem>
						<BreadcrumbPage className="font-medium text-foreground inline-flex items-center gap-1.5">
							<Home className="size-3.5 -translate-y-0.5 text-muted-foreground shrink-0" />
							<span>Beranda</span>
						</BreadcrumbPage>
					</BreadcrumbItem>
				</BreadcrumbList>
			</Breadcrumb>
		);
	}

	let accumulatedPath = "";

	return (
		<Breadcrumb className={className}>
			<BreadcrumbList className="text-xs flex-nowrap whitespace-nowrap">
				<BreadcrumbItem>
					<BreadcrumbLink asChild>
						<Link
							to="/"
							className="inline-flex items-center gap-1.5 text-muted-foreground hover:text-foreground transition-colors"
						>
							<Home className="size-3.5 -translate-y-0.5 shrink-0" />
							<span className="hidden sm:inline">Beranda</span>
						</Link>
					</BreadcrumbLink>
				</BreadcrumbItem>

				{segments.map((segment, index) => {
					accumulatedPath += `/${segment}`;
					const isLast = index === segments.length - 1;
					const isUuid =
						/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
							segment,
						);
					const label =
						ROUTE_LABELS[segment] ||
						(isUuid
							? "Detail Berkas"
							: segment.length > 20
								? `${segment.slice(0, 10)}...`
								: segment);

					return (
						<React.Fragment key={accumulatedPath}>
							<BreadcrumbSeparator className="opacity-60" />
							<BreadcrumbItem>
								{isLast ? (
									<BreadcrumbPage className="font-semibold text-foreground">
										{label}
									</BreadcrumbPage>
								) : (
									<BreadcrumbLink asChild>
										<Link
											to={accumulatedPath}
											className="text-muted-foreground hover:text-foreground transition-colors"
										>
											{label}
										</Link>
									</BreadcrumbLink>
								)}
							</BreadcrumbItem>
						</React.Fragment>
					);
				})}
			</BreadcrumbList>
		</Breadcrumb>
	);
}
