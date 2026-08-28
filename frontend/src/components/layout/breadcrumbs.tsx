import { Link, useRouterState } from "@tanstack/react-router";
import { ChevronRight, Home } from "lucide-react";

const ROUTE_LABELS: Record<string, string> = {
	"": "Beranda",
	directory: "Direktori",
	plotting: "Plotting",
	map: "Peta",
	tasks: "Tugas Saya",
	blocked: "Tugas Tertahan",
	reports: "Laporan",
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
		<nav
			aria-label="Breadcrumb"
			className="flex items-center text-xs text-muted-foreground mb-4"
		>
			<Link
				to="/"
				className="flex items-center gap-1 hover:text-foreground transition-colors"
			>
				<Home className="size-3.5" />
				<span>Beranda</span>
			</Link>

			{segments.map((segment, index) => {
				accumulatedPath += `/${segment}`;
				const isLast = index === segments.length - 1;
				const label =
					ROUTE_LABELS[segment] ||
					(segment.length > 20 ? `${segment.slice(0, 10)}...` : segment);

				return (
					<div key={accumulatedPath} className="flex items-center">
						<ChevronRight className="size-3.5 mx-1.5 opacity-50" />
						{isLast ? (
							<span className="font-medium text-foreground">{label}</span>
						) : (
							<Link
								to={accumulatedPath}
								className="hover:text-foreground transition-colors"
							>
								{label}
							</Link>
						)}
					</div>
				);
			})}
		</nav>
	);
}
