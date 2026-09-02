import { Link, useRouterState } from "@tanstack/react-router";
import { Home } from "lucide-react";
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
	history: "Riwayat Tugas",
	reports: "Laporan",
	"gas-demand": "Kebutuhan Gas",
	ageing: "Umur Proses & SLA",
	funnel: "Konversi Funnel",
	"survey-productivity": "Produktivitas Survei",
	"nol-outcomes": "Hasil Surat NOL",
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

export function Breadcrumbs() {
	const routerState = useRouterState();
	const pathname = routerState.location.pathname;

	const segments = pathname.split("/").filter(Boolean);

	if (segments.length === 0) {
		return null;
	}

	let accumulatedPath = "";

	return (
		<Breadcrumb className="mb-4">
			<BreadcrumbList className="text-xs">
				<BreadcrumbItem>
					<BreadcrumbLink asChild>
						<Link to="/" className="flex items-center gap-1">
							<Home className="size-3.5" />
							<span>Beranda</span>
						</Link>
					</BreadcrumbLink>
				</BreadcrumbItem>

				{segments.map((segment, index) => {
					accumulatedPath += `/${segment}`;
					const isLast = index === segments.length - 1;
					const label =
						ROUTE_LABELS[segment] ||
						(segment.length > 20 ? `${segment.slice(0, 10)}...` : segment);

					return (
						<div
							key={accumulatedPath}
							className="inline-flex items-center gap-1.5"
						>
							<BreadcrumbSeparator />
							<BreadcrumbItem>
								{isLast ? (
									<BreadcrumbPage>{label}</BreadcrumbPage>
								) : (
									<BreadcrumbLink asChild>
										<Link to={accumulatedPath}>{label}</Link>
									</BreadcrumbLink>
								)}
							</BreadcrumbItem>
						</div>
					);
				})}
			</BreadcrumbList>
		</Breadcrumb>
	);
}
