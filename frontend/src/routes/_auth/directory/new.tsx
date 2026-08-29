import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import {
	ArrowLeft,
	Building2,
	Globe,
	Info,
	Loader2,
	Mail,
	MapPin,
	Phone,
	Save,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type { CreateCompanyRequest } from "@/api/types";
import { Map, type MapCoordinates } from "@/components/map";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import { requireCapabilities } from "@/lib/auth-middleware";

export const Route = createFileRoute("/_auth/directory/new")({
	beforeLoad: requireCapabilities(["CreateCompany"]),
	component: CreateCompanyPage,
});

function CreateCompanyPage() {
	const navigate = useNavigate();

	// Form State
	const [namaPerusahaan, setNamaPerusahaan] = React.useState("");
	const [industryTypeId, setIndustryTypeId] = React.useState("");
	const [areaId, setAreaId] = React.useState("");
	const [npwp, setNpwp] = React.useState("");
	const [email, setEmail] = React.useState("");
	const [telp, setTelp] = React.useState("");
	const [website, setWebsite] = React.useState("");
	const [alamat, setAlamat] = React.useState("");
	const [kodePos, setKodePos] = React.useState("");

	// Cascading Geography state
	const [provinceId, setProvinceId] = React.useState("");
	const [regencyId, setRegencyId] = React.useState("");
	const [districtId, setDistrictId] = React.useState("");
	const [villageId, setVillageId] = React.useState("");

	// Location Coordinates state (Default to Jakarta coordinates)
	const [coordinates, setCoordinates] = React.useState<MapCoordinates>({
		latitude: -6.2088,
		longitude: 106.8456,
	});

	// Master Data Queries
	const { data: industryTypes } = $api.useQuery(
		"get",
		"/api/master/industry-types",
	);
	const { data: areas } = $api.useQuery("get", "/api/master/areas");

	// Cascading Geography Queries
	const { data: provinces } = $api.useQuery("get", "/api/geography/provinces");

	const { data: regencies, isLoading: loadingRegencies } = $api.useQuery(
		"get",
		"/api/geography/regencies",
		{
			params: {
				query: { provinceId: provinceId },
			},
		},
		{
			enabled: Boolean(provinceId),
		},
	);

	const { data: districts, isLoading: loadingDistricts } = $api.useQuery(
		"get",
		"/api/geography/districts",
		{
			params: {
				query: { regencyId: regencyId },
			},
		},
		{
			enabled: Boolean(regencyId),
		},
	);

	const { data: villages, isLoading: loadingVillages } = $api.useQuery(
		"get",
		"/api/geography/villages",
		{
			params: {
				query: { districtId: districtId },
			},
		},
		{
			enabled: Boolean(districtId),
		},
	);

	// Create Mutation
	const createCompanyMutation = $api.useMutation("post", "/api/companies", {
		onSuccess: (data) => {
			toast.success("Calon Pelanggan Berhasil Didaftarkan", {
				description: `Nomor Registrasi: ${data.nomor}`,
			});
			navigate({
				to: "/directory/$companyId",
				params: { companyId: data.companyId },
			});
		},
		onError: (error) => {
			toast.error("Gagal Menyimpan Calon Pelanggan", {
				description:
					error.detail || "Terjadi kesalahan saat memproses pendaftaran.",
			});
		},
	});

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();

		if (!namaPerusahaan.trim()) {
			toast.error("Nama Perusahaan wajib diisi");
			return;
		}

		if (!industryTypeId) {
			toast.error("Sektor Industri wajib dipilih");
			return;
		}

		if (!areaId) {
			toast.error("Wilayah Operasional Area wajib dipilih");
			return;
		}

		if (!villageId) {
			toast.error(
				"Hierarki Lokasi Administratif (Kelurahan/Desa) wajib dipilih lengkap",
			);
			return;
		}

		if (!alamat.trim()) {
			toast.error("Alamat lengkap wajib diisi");
			return;
		}

		const payload: CreateCompanyRequest = {
			namaPerusahaan: namaPerusahaan.trim(),
			industryTypeId,
			areaId,
			villageId,
			alamat: alamat.trim(),
			latitude: coordinates.latitude,
			longitude: coordinates.longitude,
			npwp: npwp.trim() || null,
			email: email.trim() || null,
			telp: telp.trim() || null,
			website: website.trim() || null,
			kodePos: kodePos.trim() || null,
		};

		createCompanyMutation.mutate({ body: payload });
	};

	return (
		<div className="max-w-5xl mx-auto space-y-6 pb-12">
			{/* Page Header */}
			<div className="flex items-center justify-between border-b pb-4">
				<div className="flex items-center gap-3">
					<Button variant="ghost" size="icon" asChild className="h-9 w-9">
						<Link to="/directory">
							<ArrowLeft className="size-4" />
						</Link>
					</Button>
					<div>
						<h1 className="text-2xl font-bold tracking-tight text-foreground flex items-center gap-2">
							<Building2 className="size-6 text-primary" />
							Pendaftaran Calon Pelanggan Baru
						</h1>
						<p className="text-xs text-muted-foreground">
							Formulir registrasi tahap 1 direktori calon pelanggan industri &
							komersial PGN
						</p>
					</div>
				</div>
				<Badge variant="outline" className="text-xs px-3 py-1 font-mono">
					Tahap 1: Calon Pelanggan
				</Badge>
			</div>

			<form onSubmit={handleSubmit} className="space-y-6">
				{/* Section 1: Data Identitas Perusahaan */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3">
						<CardTitle className="text-base font-semibold flex items-center gap-2">
							<Building2 className="size-4 text-primary" />
							Identitas & Klasifikasi Perusahaan
						</CardTitle>
						<CardDescription className="text-xs">
							Lengkapi data legalitas nama dan sektor industri calon pelanggan
						</CardDescription>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
							{/* Nama Perusahaan */}
							<div className="space-y-1.5 md:col-span-2">
								<Label htmlFor="namaPerusahaan" className="text-xs font-medium">
									Nama Perusahaan <span className="text-destructive">*</span>
								</Label>
								<Input
									id="namaPerusahaan"
									placeholder="Contoh: PT Sumber Pangan Nusantara"
									value={namaPerusahaan}
									onChange={(e) => setNamaPerusahaan(e.target.value)}
									className="text-xs h-9"
									required
								/>
							</div>

							{/* Sektor Industri */}
							<div className="space-y-1.5">
								<Label htmlFor="industryType" className="text-xs font-medium">
									Sektor Industri <span className="text-destructive">*</span>
								</Label>
								<Select
									value={industryTypeId}
									onValueChange={setIndustryTypeId}
								>
									<SelectTrigger id="industryType" className="text-xs h-9">
										<SelectValue placeholder="Pilih Sektor Industri" />
									</SelectTrigger>
									<SelectContent>
										{industryTypes?.map((it) => (
											<SelectItem key={it.id} value={it.id}>
												{it.name}
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>

							{/* Area Kerja PGN */}
							<div className="space-y-1.5">
								<Label htmlFor="area" className="text-xs font-medium">
									Wilayah Area Kerja PGN{" "}
									<span className="text-destructive">*</span>
								</Label>
								<Select value={areaId} onValueChange={setAreaId}>
									<SelectTrigger id="area" className="text-xs h-9">
										<SelectValue placeholder="Pilih Area Kerja" />
									</SelectTrigger>
									<SelectContent>
										{areas?.map((a) => (
											<SelectItem key={a.id} value={a.id}>
												{a.name} ({a.code})
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>

							{/* NPWP */}
							<div className="space-y-1.5">
								<Label htmlFor="npwp" className="text-xs font-medium">
									Nomor NPWP Perusahaan
								</Label>
								<Input
									id="npwp"
									placeholder="Contoh: 01.234.567.8-901.000"
									value={npwp}
									onChange={(e) => setNpwp(e.target.value)}
									className="text-xs h-9"
								/>
							</div>

							{/* Website */}
							<div className="space-y-1.5">
								<Label
									htmlFor="website"
									className="text-xs font-medium flex items-center gap-1"
								>
									<Globe className="size-3 text-muted-foreground" /> Website
								</Label>
								<Input
									id="website"
									type="url"
									placeholder="https://www.perusahaan.co.id"
									value={website}
									onChange={(e) => setWebsite(e.target.value)}
									className="text-xs h-9"
								/>
							</div>

							{/* Email Perusahaan */}
							<div className="space-y-1.5">
								<Label
									htmlFor="email"
									className="text-xs font-medium flex items-center gap-1"
								>
									<Mail className="size-3 text-muted-foreground" /> Email Kontak
								</Label>
								<Input
									id="email"
									type="email"
									placeholder="contact@perusahaan.co.id"
									value={email}
									onChange={(e) => setEmail(e.target.value)}
									className="text-xs h-9"
								/>
							</div>

							{/* Telepon Perusahaan */}
							<div className="space-y-1.5">
								<Label
									htmlFor="telp"
									className="text-xs font-medium flex items-center gap-1"
								>
									<Phone className="size-3 text-muted-foreground" /> No. Telepon
									/ Hunting
								</Label>
								<Input
									id="telp"
									placeholder="Contoh: 021-5551234"
									value={telp}
									onChange={(e) => setTelp(e.target.value)}
									className="text-xs h-9"
								/>
							</div>
						</div>
					</CardContent>
				</Card>

				{/* Section 2: Lokasi Administratif (Cascading Dropdowns) */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3">
						<CardTitle className="text-base font-semibold flex items-center gap-2">
							<MapPin className="size-4 text-primary" />
							Lokasi Administratif & Alamat Pabrik / Plant
						</CardTitle>
						<CardDescription className="text-xs">
							Hierarki administratif wilayah BPS untuk penomoran sequence
							registrasi
						</CardDescription>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
							{/* Provinsi */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Provinsi <span className="text-destructive">*</span>
								</Label>
								<Select
									value={provinceId}
									onValueChange={(val) => {
										setProvinceId(val);
										setRegencyId("");
										setDistrictId("");
										setVillageId("");
									}}
								>
									<SelectTrigger className="text-xs h-9">
										<SelectValue placeholder="Pilih Provinsi" />
									</SelectTrigger>
									<SelectContent>
										{provinces?.map((p) => (
											<SelectItem key={p.id} value={p.id}>
												{p.name}
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>

							{/* Kota / Kabupaten */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Kota / Kabupaten <span className="text-destructive">*</span>
								</Label>
								<Select
									value={regencyId}
									disabled={!provinceId || loadingRegencies}
									onValueChange={(val) => {
										setRegencyId(val);
										setDistrictId("");
										setVillageId("");
									}}
								>
									<SelectTrigger className="text-xs h-9">
										<SelectValue
											placeholder={
												loadingRegencies
													? "Memuat..."
													: !provinceId
														? "Pilih Provinsi Dahulu"
														: "Pilih Kota/Kabupaten"
											}
										/>
									</SelectTrigger>
									<SelectContent>
										{regencies?.map((r) => (
											<SelectItem key={r.id} value={r.id}>
												{r.name}
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>

							{/* Kecamatan */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Kecamatan <span className="text-destructive">*</span>
								</Label>
								<Select
									value={districtId}
									disabled={!regencyId || loadingDistricts}
									onValueChange={(val) => {
										setDistrictId(val);
										setVillageId("");
									}}
								>
									<SelectTrigger className="text-xs h-9">
										<SelectValue
											placeholder={
												loadingDistricts
													? "Memuat..."
													: !regencyId
														? "Pilih Kota/Kab Dahulu"
														: "Pilih Kecamatan"
											}
										/>
									</SelectTrigger>
									<SelectContent>
										{districts?.map((d) => (
											<SelectItem key={d.id} value={d.id}>
												{d.name}
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>

							{/* Kelurahan / Desa */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Kelurahan / Desa <span className="text-destructive">*</span>
								</Label>
								<Select
									value={villageId}
									disabled={!districtId || loadingVillages}
									onValueChange={setVillageId}
								>
									<SelectTrigger className="text-xs h-9">
										<SelectValue
											placeholder={
												loadingVillages
													? "Memuat..."
													: !districtId
														? "Pilih Kecamatan Dahulu"
														: "Pilih Kelurahan/Desa"
											}
										/>
									</SelectTrigger>
									<SelectContent>
										{villages?.map((v) => (
											<SelectItem key={v.id} value={v.id}>
												{v.name}
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>
						</div>

						{/* Alamat Lengkap & Kode Pos */}
						<div className="grid grid-cols-1 md:grid-cols-4 gap-3 pt-2">
							<div className="space-y-1.5 md:col-span-3">
								<Label htmlFor="alamat" className="text-xs font-medium">
									Alamat Lengkap (Jalan, Kawasan, Blok, No){" "}
									<span className="text-destructive">*</span>
								</Label>
								<Input
									id="alamat"
									placeholder="Contoh: Jl. Industri Raya No. 45, Kawasan Industri Jababeka V"
									value={alamat}
									onChange={(e) => setAlamat(e.target.value)}
									className="text-xs h-9"
									required
								/>
							</div>
							<div className="space-y-1.5">
								<Label htmlFor="kodePos" className="text-xs font-medium">
									Kode Pos
								</Label>
								<Input
									id="kodePos"
									placeholder="Contoh: 17530"
									value={kodePos}
									onChange={(e) => setKodePos(e.target.value)}
									className="text-xs h-9"
								/>
							</div>
						</div>
					</CardContent>
				</Card>

				{/* Section 3: Titik Koordinat Spasial (Map Pin Drop) */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3">
						<div className="flex items-center justify-between">
							<div>
								<CardTitle className="text-base font-semibold flex items-center gap-2">
									<MapPin className="size-4 text-primary" />
									Penetapan Titik Koordinat Lokasi (Pin Drop)
								</CardTitle>
								<CardDescription className="text-xs">
									Klik pada peta atau geser pin penanda untuk menentukan
									koordinat presisi pabrik / calon pelanggan
								</CardDescription>
							</div>
							<div className="flex items-center gap-2">
								<Badge variant="outline" className="font-mono text-xs">
									Lat: {coordinates.latitude.toFixed(6)}, Lng:{" "}
									{coordinates.longitude.toFixed(6)}
								</Badge>
							</div>
						</div>
					</CardHeader>
					<CardContent className="space-y-3">
						<div className="h-[360px] w-full rounded-md overflow-hidden border">
							<Map
								center={[coordinates.longitude, coordinates.latitude]}
								selectedCoordinates={coordinates}
								onCoordinateSelect={(coords) => setCoordinates(coords)}
								className="h-full w-full"
							/>
						</div>
						<div className="flex items-center justify-between text-xs text-muted-foreground">
							<div className="flex items-center gap-1.5">
								<Info className="size-3.5 text-primary" />
								<span>
									Koordinat ini digunakan untuk analisis jarak jaringan pipa gas
									existing pada Tahap 2 (Plotting).
								</span>
							</div>
						</div>
					</CardContent>
				</Card>

				{/* Form Submission Action Buttons */}
				<div className="flex items-center justify-end gap-3 pt-4 border-t">
					<Button variant="outline" asChild className="text-xs h-9 px-4">
						<Link to="/directory">Batal</Link>
					</Button>
					<Button
						type="submit"
						disabled={createCompanyMutation.isPending}
						className="text-xs h-9 px-5 flex items-center gap-2"
					>
						{createCompanyMutation.isPending ? (
							<>
								<Loader2 className="size-4 animate-spin" />
								<span>Menyimpan...</span>
							</>
						) : (
							<>
								<Save className="size-4" />
								<span>Daftarkan Calon Pelanggan</span>
							</>
						)}
					</Button>
				</div>
			</form>
		</div>
	);
}
