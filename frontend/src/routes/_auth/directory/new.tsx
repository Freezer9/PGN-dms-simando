import { useForm, useStore } from "@tanstack/react-form";
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
import { toast } from "sonner";
import { $api } from "@/api/client";
import type { CreateCompanyRequest } from "@/api/types";
import { FormField } from "@/components/form/form-field";
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
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import { requireCapabilities } from "@/lib/auth-middleware";
import {
	type CreateCompanyFormValues,
	createCompanySchema,
} from "@/lib/schemas";

function getFieldError(errors: unknown[] | undefined): string | undefined {
	if (!errors || errors.length === 0) return undefined;
	const first = errors[0];
	if (typeof first === "string") return first;
	if (first && typeof first === "object" && "message" in first) {
		return (first as { message?: string }).message;
	}
	return undefined;
}

export const Route = createFileRoute("/_auth/directory/new")({
	beforeLoad: requireCapabilities(["CreateCompany"]),
	component: CreateCompanyPage,
});

function CreateCompanyPage() {
	const navigate = useNavigate();

	// Master Data Queries
	const { data: industryTypes } = $api.useQuery(
		"get",
		"/api/master/industry-types",
	);
	const { data: areas } = $api.useQuery("get", "/api/master/areas");

	// Cascading Geography Queries
	const { data: provinces } = $api.useQuery("get", "/api/geography/provinces");

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

	const form = useForm({
		defaultValues: {
			namaPerusahaan: "",
			industryTypeId: "",
			areaId: "",
			npwp: "",
			email: "",
			telp: "",
			website: "",
			alamat: "",
			kodePos: "",
			provinceId: "",
			regencyId: "",
			districtId: "",
			villageId: "",
			latitude: -6.2088,
			longitude: 106.8456,
		} as CreateCompanyFormValues,
		validators: {
			onChange: createCompanySchema,
		},
		onSubmit: async ({ value }) => {
			const payload: CreateCompanyRequest = {
				namaPerusahaan: value.namaPerusahaan.trim(),
				industryTypeId: value.industryTypeId,
				areaId: value.areaId,
				villageId: value.villageId,
				alamat: value.alamat.trim(),
				latitude: value.latitude,
				longitude: value.longitude,
				npwp: value.npwp?.trim() || null,
				email: value.email?.trim() || null,
				telp: value.telp?.trim() || null,
				website: value.website?.trim() || null,
				kodePos: value.kodePos?.trim() || null,
			};

			await createCompanyMutation.mutateAsync({ body: payload });
		},
	});

	// Subscribe to geography field values for cascading queries
	const provinceId = useStore(form.store, (state) => state.values.provinceId);
	const regencyId = useStore(form.store, (state) => state.values.regencyId);
	const districtId = useStore(form.store, (state) => state.values.districtId);
	const latitude = useStore(form.store, (state) => state.values.latitude);
	const longitude = useStore(form.store, (state) => state.values.longitude);

	const { data: regencies, isLoading: loadingRegencies } = $api.useQuery(
		"get",
		"/api/geography/regencies",
		{
			params: {
				query: { provinceId },
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
				query: { regencyId },
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
				query: { districtId },
			},
		},
		{
			enabled: Boolean(districtId),
		},
	);

	const coordinates: MapCoordinates = {
		latitude,
		longitude,
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

			<form
				onSubmit={(e) => {
					e.preventDefault();
					e.stopPropagation();
					form.handleSubmit();
				}}
				className="space-y-6"
			>
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
							<div className="md:col-span-2">
								<form.Field name="namaPerusahaan">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField label="Nama Perusahaan" required error={error}>
												<Input
													id={field.name}
													name={field.name}
													placeholder="Contoh: PT Sumber Pangan Nusantara"
													value={field.state.value}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</form.Field>
							</div>

							{/* Sektor Industri */}
							<div>
								<form.Field name="industryTypeId">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField label="Sektor Industri" required error={error}>
												<Select
													value={field.state.value}
													onValueChange={(val) => field.handleChange(val)}
												>
													<SelectTrigger
														id={field.name}
														className="text-xs h-9"
													>
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
											</FormField>
										);
									}}
								</form.Field>
							</div>

							{/* Area Kerja PGN */}
							<div>
								<form.Field name="areaId">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField
												label="Wilayah Area Kerja PGN"
												required
												error={error}
											>
												<Select
													value={field.state.value}
													onValueChange={(val) => field.handleChange(val)}
												>
													<SelectTrigger
														id={field.name}
														className="text-xs h-9"
													>
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
											</FormField>
										);
									}}
								</form.Field>
							</div>

							{/* NPWP */}
							<div>
								<form.Field name="npwp">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField label="Nomor NPWP Perusahaan" error={error}>
												<Input
													id={field.name}
													name={field.name}
													placeholder="Contoh: 01.234.567.8-901.000"
													value={field.state.value || ""}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</form.Field>
							</div>

							{/* Website */}
							<div>
								<form.Field name="website">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField
												label={
													<span className="flex items-center gap-1">
														<Globe className="size-3 text-muted-foreground" />{" "}
														Website
													</span>
												}
												error={error}
											>
												<Input
													id={field.name}
													name={field.name}
													type="url"
													placeholder="https://www.perusahaan.co.id"
													value={field.state.value || ""}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</form.Field>
							</div>

							{/* Email Perusahaan */}
							<div>
								<form.Field name="email">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField
												label={
													<span className="flex items-center gap-1">
														<Mail className="size-3 text-muted-foreground" />{" "}
														Email Kontak
													</span>
												}
												error={error}
											>
												<Input
													id={field.name}
													name={field.name}
													type="email"
													placeholder="contact@perusahaan.co.id"
													value={field.state.value || ""}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</form.Field>
							</div>

							{/* Telepon Perusahaan */}
							<div>
								<form.Field name="telp">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField
												label={
													<span className="flex items-center gap-1">
														<Phone className="size-3 text-muted-foreground" />{" "}
														No. Telepon / Hunting
													</span>
												}
												error={error}
											>
												<Input
													id={field.name}
													name={field.name}
													placeholder="Contoh: 021-5551234"
													value={field.state.value || ""}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</form.Field>
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
							<div>
								<form.Field name="provinceId">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField label="Provinsi" required error={error}>
												<Select
													value={field.state.value}
													onValueChange={(val) => {
														field.handleChange(val);
														form.setFieldValue("regencyId", "");
														form.setFieldValue("districtId", "");
														form.setFieldValue("villageId", "");
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
											</FormField>
										);
									}}
								</form.Field>
							</div>

							{/* Kota / Kabupaten */}
							<div>
								<form.Field name="regencyId">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField
												label="Kota / Kabupaten"
												required
												error={error}
											>
												<Select
													value={field.state.value}
													disabled={!provinceId || loadingRegencies}
													onValueChange={(val) => {
														field.handleChange(val);
														form.setFieldValue("districtId", "");
														form.setFieldValue("villageId", "");
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
											</FormField>
										);
									}}
								</form.Field>
							</div>

							{/* Kecamatan */}
							<div>
								<form.Field name="districtId">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField label="Kecamatan" required error={error}>
												<Select
													value={field.state.value}
													disabled={!regencyId || loadingDistricts}
													onValueChange={(val) => {
														field.handleChange(val);
														form.setFieldValue("villageId", "");
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
											</FormField>
										);
									}}
								</form.Field>
							</div>

							{/* Kelurahan / Desa */}
							<div>
								<form.Field name="villageId">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField
												label="Kelurahan / Desa"
												required
												error={error}
											>
												<Select
													value={field.state.value}
													disabled={!districtId || loadingVillages}
													onValueChange={(val) => field.handleChange(val)}
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
											</FormField>
										);
									}}
								</form.Field>
							</div>
						</div>

						{/* Alamat Lengkap & Kode Pos */}
						<div className="grid grid-cols-1 md:grid-cols-4 gap-3 pt-2">
							<div className="md:col-span-3">
								<form.Field name="alamat">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField
												label="Alamat Lengkap (Jalan, Kawasan, Blok, No)"
												required
												error={error}
											>
												<Input
													id={field.name}
													name={field.name}
													placeholder="Contoh: Jl. Industri Raya No. 45, Kawasan Industri Jababeka V"
													value={field.state.value}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</form.Field>
							</div>
							<div>
								<form.Field name="kodePos">
									{(field) => {
										const error = getFieldError(field.state.meta.errors);
										return (
											<FormField label="Kode Pos" error={error}>
												<Input
													id={field.name}
													name={field.name}
													placeholder="Contoh: 17530"
													value={field.state.value || ""}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</form.Field>
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
								onCoordinateSelect={(coords) => {
									form.setFieldValue("latitude", coords.latitude);
									form.setFieldValue("longitude", coords.longitude);
								}}
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
					<form.Subscribe
						selector={(state) => [state.canSubmit, state.isSubmitting]}
					>
						{([canSubmit, isSubmitting]) => (
							<Button
								type="submit"
								disabled={!canSubmit || isSubmitting}
								className="text-xs h-9 px-5 flex items-center gap-2"
							>
								{isSubmitting ? (
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
						)}
					</form.Subscribe>
				</div>
			</form>
		</div>
	);
}
