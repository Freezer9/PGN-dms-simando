import { useForm } from "@tanstack/react-form";
import { useQueryClient } from "@tanstack/react-query";
import {
	Activity,
	FileSearch,
	Gauge,
	LineChart,
	Loader2,
	Plus,
	Save,
	ShieldCheck,
	Trash2,
	UserPlus,
	Users,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	DiameterUnit,
	FeedStatus,
	NolEvaluationDetail,
	SaveNolEvaluationRequest,
	SaveNolEvaluationScenarioRequest,
	SkemaPembayaran,
	StatusRkap,
} from "@/api/types";
import { DocumentDownloadButton } from "@/components/documents/document-download-buttons";
import { FormField } from "@/components/form/form-field";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
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
import { Textarea } from "@/components/ui/textarea";
import {
	type NolEvaluationFormValues,
	nolEvaluationSchema,
} from "@/lib/schemas";

interface NolEvaluationFormProps {
	companyId: string;
	initialData?: NolEvaluationDetail | null;
	canEdit?: boolean;
	canChooseReviewers?: boolean;
	onSaved?: () => void;
	onReviewersChosen?: () => void;
}

function getDefaultValues(
	initialData?: NolEvaluationDetail | null,
): NolEvaluationFormValues {
	return {
		feedStatus: initialData?.feedStatus || "Selesai",
		feedCompletedAt: initialData?.feedCompletedAt || "",
		statusRkap: initialData?.statusRkap || "Rkap",
		pipaIndukPanjangM:
			initialData?.pipaIndukPanjangM != null
				? String(initialData.pipaIndukPanjangM)
				: "",
		pipaIndukDiameter:
			initialData?.pipaIndukDiameter != null
				? String(initialData.pipaIndukDiameter)
				: "",
		pipaIndukDiameterUnit: initialData?.pipaIndukDiameterUnit || "Inch",
		pipaServicePanjangM:
			initialData?.pipaServicePanjangM != null
				? String(initialData.pipaServicePanjangM)
				: "",
		pipaServiceDiameter:
			initialData?.pipaServiceDiameter != null
				? String(initialData.pipaServiceDiameter)
				: "",
		pipaServiceDiameterUnit: initialData?.pipaServiceDiameterUnit || "Inch",
		spesifikasiMrs: initialData?.spesifikasiMrs || "",
		gSize: initialData?.gSize || "",
		maksKapasitasMeterM3Jam:
			initialData?.maksKapasitasMeterM3Jam != null
				? String(initialData.maksKapasitasMeterM3Jam)
				: "",
		tekanan: initialData?.tekanan != null ? String(initialData.tekanan) : "",
		maksFlowrate:
			initialData?.maksFlowrate != null ? String(initialData.maksFlowrate) : "",
		skemaPembayaran: initialData?.skemaPembayaran || "JaminanPembayaran",
		jaminanStatus: initialData?.jaminanStatus || "",
		jaminanJenis: initialData?.jaminanJenis || "",
		jaminanMasaBerlaku: initialData?.jaminanMasaBerlaku || "",
		jaminanPenerbit: initialData?.jaminanPenerbit || "",
		ketersediaanPasokanBbtud:
			initialData?.ketersediaanPasokanBbtud != null
				? String(initialData.ketersediaanPasokanBbtud)
				: "",
		capexFinal:
			initialData?.capexFinal != null ? String(initialData.capexFinal) : "",
		durasiPelaksanaanBulan:
			initialData?.durasiPelaksanaanBulan != null
				? String(initialData.durasiPelaksanaanBulan)
				: "",
		analisisKomersial: initialData?.analisisKomersial || "",
		analisisKompetitor: initialData?.analisisKompetitor || "",
		radiusKompetitorKm:
			initialData?.radiusKompetitorKm != null
				? String(initialData.radiusKompetitorKm)
				: "",
		kesimpulan: initialData?.kesimpulan || "",
		scenarios: initialData?.scenarios?.map((s) => ({
			id: s.id || crypto.randomUUID(),
			label: s.label,
			irrPct: s.irrPct != null ? Number(s.irrPct) : null,
			npv: s.npv != null ? Number(s.npv) : null,
			paybackYears: s.paybackYears != null ? Number(s.paybackYears) : null,
			hasilAnalisis: s.hasilAnalisis || null,
		})) || [
			{
				id: crypto.randomUUID(),
				label: "Skenario Moderat",
				irrPct: 18.5,
				npv: 120000,
				paybackYears: 3.2,
				hasilAnalisis: "Layak (Feasible)",
			},
		],
	};
}

export function NolEvaluationForm({
	companyId,
	initialData,
	canEdit = true,
	canChooseReviewers = false,
	onSaved,
	onReviewersChosen,
}: NolEvaluationFormProps) {
	const queryClient = useQueryClient();

	// Master data
	const { data: mrsSpecs } = $api.useQuery("get", "/api/master/mrs-specs");
	const { data: meterSizes } = $api.useQuery("get", "/api/master/meter-sizes");
	const { data: reviewerCandidates } = $api.useQuery(
		"get",
		"/api/master/reviewers",
	);

	// Reviewer Dialog State
	const [isReviewerDialogOpen, setIsReviewerDialogOpen] =
		React.useState<boolean>(false);
	const [selectedReviewerIds, setSelectedReviewerIds] = React.useState<
		string[]
	>([]);

	// Save Mutation
	const saveMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/nol-evaluation",
		{
			onSuccess: () => {
				toast.success("Data Resume Evaluasi NOL berhasil disimpan!");
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}",
						{ params: { path: { id: companyId } } },
					],
				});
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}/nol-evaluation",
						{ params: { path: { id: companyId } } },
					],
				});
				onSaved?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error
						? error.message
						: "Gagal menyimpan evaluasi NOL",
				);
			},
		},
	);

	// Choose Reviewers Mutation
	const chooseReviewersMutation = $api.useMutation(
		"post",
		"/api/companies/{id}/workflow/choose-reviewers",
		{
			onSuccess: () => {
				toast.success("Reviewer evaluasi berhasil ditugaskan!");
				setIsReviewerDialogOpen(false);
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}",
						{ params: { path: { id: companyId } } },
					],
				});
				onReviewersChosen?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error ? error.message : "Gagal menugaskan reviewer",
				);
			},
		},
	);

	const form = useForm({
		defaultValues: getDefaultValues(initialData),
		validators: {
			onChange: nolEvaluationSchema,
		},
		onSubmit: async ({ value }) => {
			const scenarios: SaveNolEvaluationScenarioRequest[] = (
				value.scenarios || []
			).map((s) => ({
				id: s.id || crypto.randomUUID(),
				label: s.label,
				irrPct: s.irrPct != null ? Number(s.irrPct) : null,
				npv: s.npv != null ? Number(s.npv) : null,
				paybackYears: s.paybackYears != null ? Number(s.paybackYears) : null,
				hasilAnalisis: s.hasilAnalisis || null,
			}));

			const request: SaveNolEvaluationRequest = {
				feedStatus: (value.feedStatus as FeedStatus) || "Selesai",
				feedCompletedAt: value.feedCompletedAt || null,
				capexFinal: value.capexFinal ? Number(value.capexFinal) : null,
				pipaIndukPanjangM: value.pipaIndukPanjangM
					? Number(value.pipaIndukPanjangM)
					: null,
				pipaIndukDiameter: value.pipaIndukDiameter
					? Number(value.pipaIndukDiameter)
					: null,
				pipaIndukDiameterUnit: value.pipaIndukDiameterUnit
					? (value.pipaIndukDiameterUnit as DiameterUnit)
					: null,
				pipaServicePanjangM: value.pipaServicePanjangM
					? Number(value.pipaServicePanjangM)
					: null,
				pipaServiceDiameter: value.pipaServiceDiameter
					? Number(value.pipaServiceDiameter)
					: null,
				pipaServiceDiameterUnit: value.pipaServiceDiameterUnit
					? (value.pipaServiceDiameterUnit as DiameterUnit)
					: null,
				spesifikasiMrs: value.spesifikasiMrs || null,
				gSize: value.gSize || null,
				tekanan: value.tekanan ? Number(value.tekanan) : null,
				maksFlowrate: value.maksFlowrate ? Number(value.maksFlowrate) : null,
				maksKapasitasMeterM3Jam: value.maksKapasitasMeterM3Jam
					? Number(value.maksKapasitasMeterM3Jam)
					: null,
				durasiPelaksanaanBulan: value.durasiPelaksanaanBulan
					? Number(value.durasiPelaksanaanBulan)
					: null,
				statusRkap: value.statusRkap ? (value.statusRkap as StatusRkap) : null,
				skemaPembayaran: value.skemaPembayaran
					? (value.skemaPembayaran as SkemaPembayaran)
					: null,
				jaminanStatus: value.jaminanStatus || null,
				jaminanJenis: value.jaminanJenis || null,
				jaminanMasaBerlaku: value.jaminanMasaBerlaku || null,
				jaminanPenerbit: value.jaminanPenerbit || null,
				ketersediaanPasokanBbtud: value.ketersediaanPasokanBbtud
					? Number(value.ketersediaanPasokanBbtud)
					: null,
				analisisKomersial: value.analisisKomersial || null,
				analisisKompetitor: value.analisisKompetitor || null,
				kesimpulan: value.kesimpulan || null,
				radiusKompetitorKm: value.radiusKompetitorKm
					? Number(value.radiusKompetitorKm)
					: null,
				scenarios,
			};

			await saveMutation.mutateAsync({
				params: { path: { id: companyId } },
				body: request,
			});
		},
	});

	// Synchronize when initialData changes
	React.useEffect(() => {
		if (initialData) {
			form.reset(getDefaultValues(initialData));
		}
	}, [initialData, form]);

	const toggleReviewer = (id: string) => {
		if (selectedReviewerIds.includes(id)) {
			setSelectedReviewerIds(selectedReviewerIds.filter((r) => r !== id));
		} else {
			setSelectedReviewerIds([...selectedReviewerIds, id]);
		}
	};

	const handleAssignReviewers = async () => {
		if (selectedReviewerIds.length === 0) {
			toast.error("Pilih minimal 1 reviewer.");
			return;
		}
		await chooseReviewersMutation.mutateAsync({
			params: { path: { id: companyId } },
			body: { reviewerUserIds: selectedReviewerIds },
		});
	};

	return (
		<form
			onSubmit={(e) => {
				e.preventDefault();
				e.stopPropagation();
				form.handleSubmit();
			}}
			className="space-y-6"
		>
			{/* Top Bar Summary / Save */}
			<div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 bg-muted/40 rounded-lg border">
				<div className="flex items-center gap-3">
					<div className="size-10 rounded-full bg-purple-100 dark:bg-purple-900/40 text-purple-600 flex items-center justify-center">
						<FileSearch className="size-5" />
					</div>
					<div>
						<h3 className="text-sm font-semibold">
							Resume Evaluasi Kelayakan Calon Pelanggan
						</h3>
						<p className="text-xs text-muted-foreground">
							Kajian teknis FEED, analisis finansial/IRR/NPV, mitigasi risiko,
							dan verifikasi reviewer
						</p>
					</div>
				</div>

				<div className="flex items-center gap-2">
					<DocumentDownloadButton
						companyId={companyId}
						documentType="evaluation"
						label="Unduh Resume Evaluasi (.docx)"
					/>
					{canChooseReviewers && (
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={() => setIsReviewerDialogOpen(true)}
							className="h-9 text-xs flex items-center gap-1.5 border-purple-300 text-purple-700 dark:text-purple-300"
						>
							<Users className="size-3.5" />
							Pilih Reviewer
						</Button>
					)}
					{canEdit && (
						<form.Subscribe
							selector={(state) => [state.canSubmit, state.isSubmitting]}
						>
							{([canSubmit, isSubmitting]) => (
								<Button
									type="submit"
									size="sm"
									disabled={!canSubmit || isSubmitting}
									className="h-9 text-xs flex items-center gap-1.5 bg-purple-600 hover:bg-purple-700 text-white"
								>
									{isSubmitting ? (
										<Loader2 className="size-3.5 animate-spin" />
									) : (
										<Save className="size-3.5" />
									)}
									Simpan Resume Evaluasi
								</Button>
							)}
						</form.Subscribe>
					)}
				</div>
			</div>

			{/* SECTION 1: STATUS FEED & PERENCANAAN */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<Activity className="size-4 text-purple-500" />
						1. Status FEED & Status Penganggaran RKAP
					</CardTitle>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
						{/* Status FEED */}
						<form.Field name="feedStatus">
							{(field) => (
								<FormField label="Status FEED">
									<Select
										value={field.state.value || "Selesai"}
										onValueChange={(val) => field.handleChange(val)}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="Belum">Belum Dimulai</SelectItem>
											<SelectItem value="DalamProses">Dalam Proses</SelectItem>
											<SelectItem value="Selesai">Selesai (Done)</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Tanggal Selesai FEED */}
						<form.Field name="feedCompletedAt">
							{(field) => (
								<FormField label="Tanggal Selesai FEED">
									<Input
										type="date"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Status RKAP */}
						<form.Field name="statusRkap">
							{(field) => (
								<FormField label="Status RKAP">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Status RKAP" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											<SelectItem value="Rkap">Masuk RKAP</SelectItem>
											<SelectItem value="NonRkap">Non-RKAP</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Capex Final */}
						<form.Field name="capexFinal">
							{(field) => (
								<FormField label="Capex Final (USD)">
									<Input
										type="number"
										step="0.01"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 85000"
										disabled={!canEdit}
										className="text-xs h-9 font-mono"
									/>
								</FormField>
							)}
						</form.Field>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 2: JALUR PIPA & INFRASTRUKTUR TEKNIS */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<Gauge className="size-4 text-blue-500" />
						2. Spesifikasi Pipa Induk, Pipa Servis & MRS
					</CardTitle>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
						{/* Pipa Induk */}
						<div className="p-3 border rounded-md space-y-2 bg-muted/20">
							<span className="text-xs font-semibold text-muted-foreground">
								Pipa Induk / Distribusi
							</span>
							<form.Field name="pipaIndukPanjangM">
								{(field) => (
									<FormField label="Panjang Pipa (Meter)">
										<Input
											type="number"
											placeholder="contoh: 500"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-8"
										/>
									</FormField>
								)}
							</form.Field>

							<div className="grid grid-cols-2 gap-2">
								<form.Field name="pipaIndukDiameter">
									{(field) => (
										<FormField label="Diameter">
											<Input
												type="number"
												step="0.5"
												placeholder="contoh: 4"
												value={field.state.value}
												onChange={(e) => field.handleChange(e.target.value)}
												disabled={!canEdit}
												className="text-xs h-8"
											/>
										</FormField>
									)}
								</form.Field>

								<form.Field name="pipaIndukDiameterUnit">
									{(field) => (
										<FormField label="Satuan">
											<Select
												value={field.state.value || "Inch"}
												onValueChange={(val) => field.handleChange(val)}
												disabled={!canEdit}
											>
												<SelectTrigger className="text-xs h-8">
													<SelectValue />
												</SelectTrigger>
												<SelectContent>
													<SelectItem value="Inch">Inch</SelectItem>
													<SelectItem value="Mm">mm</SelectItem>
												</SelectContent>
											</Select>
										</FormField>
									)}
								</form.Field>
							</div>
						</div>

						{/* Pipa Servis */}
						<div className="p-3 border rounded-md space-y-2 bg-muted/20">
							<span className="text-xs font-semibold text-muted-foreground">
								Pipa Servis Pelanggan
							</span>
							<form.Field name="pipaServicePanjangM">
								{(field) => (
									<FormField label="Panjang Pipa (Meter)">
										<Input
											type="number"
											placeholder="contoh: 25"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-8"
										/>
									</FormField>
								)}
							</form.Field>

							<div className="grid grid-cols-2 gap-2">
								<form.Field name="pipaServiceDiameter">
									{(field) => (
										<FormField label="Diameter">
											<Input
												type="number"
												step="0.5"
												placeholder="contoh: 2"
												value={field.state.value}
												onChange={(e) => field.handleChange(e.target.value)}
												disabled={!canEdit}
												className="text-xs h-8"
											/>
										</FormField>
									)}
								</form.Field>

								<form.Field name="pipaServiceDiameterUnit">
									{(field) => (
										<FormField label="Satuan">
											<Select
												value={field.state.value || "Inch"}
												onValueChange={(val) => field.handleChange(val)}
												disabled={!canEdit}
											>
												<SelectTrigger className="text-xs h-8">
													<SelectValue />
												</SelectTrigger>
												<SelectContent>
													<SelectItem value="Inch">Inch</SelectItem>
													<SelectItem value="Mm">mm</SelectItem>
												</SelectContent>
											</Select>
										</FormField>
									)}
								</form.Field>
							</div>
						</div>

						{/* MRS & Metering */}
						<div className="p-3 border rounded-md space-y-2 bg-muted/20">
							<span className="text-xs font-semibold text-muted-foreground">
								Spesifikasi MRS & Meter
							</span>
							<form.Field name="spesifikasiMrs">
								{(field) => (
									<FormField label="Tipe / Spesifikasi MRS">
										<Select
											value={field.state.value || "NONE"}
											onValueChange={(val) =>
												field.handleChange(val === "NONE" ? "" : val)
											}
											disabled={!canEdit}
										>
											<SelectTrigger className="text-xs h-8">
												<SelectValue placeholder="Pilih Spesifikasi MRS" />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="NONE">Belum Dipilih</SelectItem>
												{mrsSpecs?.map((m) => (
													<SelectItem key={m.id} value={m.name}>
														{m.name}
													</SelectItem>
												))}
											</SelectContent>
										</Select>
									</FormField>
								)}
							</form.Field>

							<div className="grid grid-cols-2 gap-2">
								<form.Field name="gSize">
									{(field) => (
										<FormField label="Ukuran Meter">
											<Select
												value={field.state.value || "NONE"}
												onValueChange={(val) =>
													field.handleChange(val === "NONE" ? "" : val)
												}
												disabled={!canEdit}
											>
												<SelectTrigger className="text-xs h-8">
													<SelectValue placeholder="Pilih G-Size" />
												</SelectTrigger>
												<SelectContent>
													<SelectItem value="NONE">Belum Dipilih</SelectItem>
													{meterSizes?.map((m) => (
														<SelectItem key={m.id} value={m.gSize}>
															{m.gSize}
														</SelectItem>
													))}
												</SelectContent>
											</Select>
										</FormField>
									)}
								</form.Field>

								<form.Field name="maksKapasitasMeterM3Jam">
									{(field) => (
										<FormField label="Kapasitas (m3/jam)">
											<Input
												type="number"
												placeholder="contoh: 250"
												value={field.state.value}
												onChange={(e) => field.handleChange(e.target.value)}
												disabled={!canEdit}
												className="text-xs h-8"
											/>
										</FormField>
									)}
								</form.Field>
							</div>
						</div>
					</div>

					<div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-2 border-t">
						{/* Tekanan */}
						<form.Field name="tekanan">
							{(field) => (
								<FormField label="Tekanan (Barg)">
									<Input
										type="number"
										step="0.1"
										placeholder="contoh: 3.0"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Maks Flowrate */}
						<form.Field name="maksFlowrate">
							{(field) => (
								<FormField label="Maksimum Flowrate (m3/jam)">
									<Input
										type="number"
										placeholder="contoh: 180"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Durasi Pelaksanaan */}
						<form.Field name="durasiPelaksanaanBulan">
							{(field) => (
								<FormField label="Durasi Pelaksanaan (Bulan)">
									<Input
										type="number"
										placeholder="contoh: 3"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 3: SKENARIO FINANSIAL & KEEKONOMIAN */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold flex items-center gap-2">
							<LineChart className="size-4 text-emerald-500" />
							3. Analisis Finansial & Skenario Keekonomian Proyek
						</CardTitle>
						<CardDescription className="text-xs">
							Perhitungan NPV, IRR, dan Payback Period untuk setiap skenario
						</CardDescription>
					</div>
					{canEdit && (
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={() => {
								const current = form.getFieldValue("scenarios") || [];
								form.setFieldValue("scenarios", [
									...current,
									{
										id: crypto.randomUUID(),
										label: `Skenario ${current.length + 1}`,
										irrPct: null,
										npv: null,
										paybackYears: null,
										hasilAnalisis: "Layak",
									},
								]);
							}}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" />
							Tambah Skenario
						</Button>
					)}
				</CardHeader>
				<CardContent>
					<form.Field name="scenarios">
						{(field) => {
							const scenarioList = field.state.value || [];
							return (
								<div className="rounded-lg border overflow-hidden">
									<Table>
										<TableHeader className="bg-muted/40">
											<TableRow>
												<TableHead className="text-xs">Nama Skenario</TableHead>
												<TableHead className="text-xs">IRR (%)</TableHead>
												<TableHead className="text-xs">NPV (USD)</TableHead>
												<TableHead className="text-xs">
													Payback (Tahun)
												</TableHead>
												<TableHead className="text-xs">
													Kesimpulan / Status
												</TableHead>
												{canEdit && (
													<TableHead className="text-xs text-center w-12">
														Hapus
													</TableHead>
												)}
											</TableRow>
										</TableHeader>
										<TableBody>
											{scenarioList.map((row, idx) => (
												<TableRow key={row.id || `scenario-${idx}`}>
													<TableCell>
														<Input
															value={row.label}
															onChange={(e) => {
																const next = [...scenarioList];
																next[idx] = { ...row, label: e.target.value };
																field.handleChange(next);
															}}
															disabled={!canEdit}
															className="text-xs h-8 font-medium"
														/>
													</TableCell>
													<TableCell>
														<Input
															type="number"
															step="0.1"
															value={
																row.irrPct != null ? String(row.irrPct) : ""
															}
															onChange={(e) => {
																const next = [...scenarioList];
																next[idx] = {
																	...row,
																	irrPct: e.target.value
																		? Number(e.target.value)
																		: null,
																};
																field.handleChange(next);
															}}
															placeholder="18.5"
															disabled={!canEdit}
															className="text-xs h-8 font-mono"
														/>
													</TableCell>
													<TableCell>
														<Input
															type="number"
															value={row.npv != null ? String(row.npv) : ""}
															onChange={(e) => {
																const next = [...scenarioList];
																next[idx] = {
																	...row,
																	npv: e.target.value
																		? Number(e.target.value)
																		: null,
																};
																field.handleChange(next);
															}}
															placeholder="150000"
															disabled={!canEdit}
															className="text-xs h-8 font-mono"
														/>
													</TableCell>
													<TableCell>
														<Input
															type="number"
															step="0.1"
															value={
																row.paybackYears != null
																	? String(row.paybackYears)
																	: ""
															}
															onChange={(e) => {
																const next = [...scenarioList];
																next[idx] = {
																	...row,
																	paybackYears: e.target.value
																		? Number(e.target.value)
																		: null,
																};
																field.handleChange(next);
															}}
															placeholder="3.5"
															disabled={!canEdit}
															className="text-xs h-8 font-mono"
														/>
													</TableCell>
													<TableCell>
														<Input
															value={row.hasilAnalisis || ""}
															onChange={(e) => {
																const next = [...scenarioList];
																next[idx] = {
																	...row,
																	hasilAnalisis: e.target.value || null,
																};
																field.handleChange(next);
															}}
															placeholder="Layak"
															disabled={!canEdit}
															className="text-xs h-8"
														/>
													</TableCell>
													{canEdit && (
														<TableCell className="text-center">
															<Button
																type="button"
																variant="ghost"
																size="icon"
																onClick={() => {
																	field.handleChange(
																		scenarioList.filter((_, i) => i !== idx),
																	);
																}}
																className="size-7 text-destructive hover:text-destructive"
															>
																<Trash2 className="size-3.5" />
															</Button>
														</TableCell>
													)}
												</TableRow>
											))}
										</TableBody>
									</Table>
								</div>
							);
						}}
					</form.Field>
				</CardContent>
			</Card>

			{/* SECTION 4: KETENTUAN KOMERSIAL & JAMINAN */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<ShieldCheck className="size-4 text-amber-500" />
						4. Ketentuan Pembayaran & Jaminan Pasokan
					</CardTitle>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Skema Pembayaran */}
						<form.Field name="skemaPembayaran">
							{(field) => (
								<FormField label="Skema Pembayaran">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Skema Pembayaran" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											<SelectItem value="JaminanPembayaran">
												Jaminan Pembayaran (Bank Garansi)
											</SelectItem>
											<SelectItem value="PembayaranDimuka">
												Pembayaran Dimuka (Advance Payment)
											</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Ketersediaan Pasokan (BBTUD) */}
						<form.Field name="ketersediaanPasokanBbtud">
							{(field) => (
								<FormField label="Ketersediaan Pasokan Gas (BBTUD)">
									<Input
										type="number"
										step="0.01"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 1.5"
										disabled={!canEdit}
										className="text-xs h-9 font-mono"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Radius Kompetitor (km) */}
						<form.Field name="radiusKompetitorKm">
							{(field) => (
								<FormField label="Jarak Kompetitor Terdekat (Km)">
									<Input
										type="number"
										step="0.1"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 2.5"
										disabled={!canEdit}
										className="text-xs h-9 font-mono"
									/>
								</FormField>
							)}
						</form.Field>
					</div>

					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 pt-2 border-t">
						<form.Field name="jaminanStatus">
							{(field) => (
								<FormField label="Status Jaminan">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: Siap diterbitkan"
										disabled={!canEdit}
										className="text-xs h-8"
									/>
								</FormField>
							)}
						</form.Field>

						<form.Field name="jaminanJenis">
							{(field) => (
								<FormField label="Jenis Jaminan">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: Bank Garansi"
										disabled={!canEdit}
										className="text-xs h-8"
									/>
								</FormField>
							)}
						</form.Field>

						<form.Field name="jaminanMasaBerlaku">
							{(field) => (
								<FormField label="Masa Berlaku">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 12 Bulan"
										disabled={!canEdit}
										className="text-xs h-8"
									/>
								</FormField>
							)}
						</form.Field>

						<form.Field name="jaminanPenerbit">
							{(field) => (
								<FormField label="Bank / Penerbit Jaminan">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: Bank Mandiri"
										disabled={!canEdit}
										className="text-xs h-8"
									/>
								</FormField>
							)}
						</form.Field>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 5: NARASI EVALUASI & KESIMPULAN */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold">
						5. Narasi Analisis & Kesimpulan Evaluasi
					</CardTitle>
				</CardHeader>
				<CardContent className="space-y-4">
					<form.Field name="analisisKomersial">
						{(field) => (
							<FormField label="Analisis Komersial & Pasar">
								<Textarea
									value={field.state.value}
									onChange={(e) => field.handleChange(e.target.value)}
									placeholder="Evaluasi profil kebutuhan gas, proyeksi pertumbuhan, dan kepatuhan tarif..."
									disabled={!canEdit}
									className="text-xs min-h-[60px]"
								/>
							</FormField>
						)}
					</form.Field>

					<form.Field name="analisisKompetitor">
						{(field) => (
							<FormField label="Analisis Kompetitor & Ancaman Bahan Bakar Alternatif">
								<Textarea
									value={field.state.value}
									onChange={(e) => field.handleChange(e.target.value)}
									placeholder="Analisis harga energi kompetitor (CNG, LPG, Batubara)..."
									disabled={!canEdit}
									className="text-xs min-h-[60px]"
								/>
							</FormField>
						)}
					</form.Field>

					<form.Field name="kesimpulan">
						{(field) => (
							<FormField
								label="Kesimpulan & Rekomendasi Tim Evaluator"
								className="text-primary font-semibold"
							>
								<Textarea
									value={field.state.value}
									onChange={(e) => field.handleChange(e.target.value)}
									placeholder="Rekomendasi penerbitan Surat NOL atau RL bersyarat..."
									disabled={!canEdit}
									className="text-xs min-h-[70px]"
								/>
							</FormField>
						)}
					</form.Field>
				</CardContent>
			</Card>

			{/* Reviewer Choice Modal Dialog */}
			<Dialog
				open={isReviewerDialogOpen}
				onOpenChange={setIsReviewerDialogOpen}
			>
				<DialogContent className="sm:max-w-md">
					<DialogHeader>
						<DialogTitle className="text-base font-semibold flex items-center gap-2">
							<UserPlus className="size-4 text-purple-600" />
							Pilih Reviewer Evaluasi NOL
						</DialogTitle>
						<DialogDescription className="text-xs">
							Pilih pejabat reviewer teknis/komersial yang akan memverifikasi
							resume evaluasi ini.
						</DialogDescription>
					</DialogHeader>

					<div className="space-y-3 py-2 max-h-[300px] overflow-y-auto">
						{reviewerCandidates?.map((r) => (
							<div
								key={r.id}
								className="flex items-center space-x-3 p-3 rounded-lg border bg-card hover:bg-muted/40 transition-colors"
							>
								<Checkbox
									id={`reviewer-${r.id}`}
									checked={selectedReviewerIds.includes(r.id)}
									onCheckedChange={() => toggleReviewer(r.id)}
								/>
								<div className="space-y-0.5 leading-none">
									<Label
										htmlFor={`reviewer-${r.id}`}
										className="text-xs font-semibold cursor-pointer"
									>
										{r.fullName}
									</Label>
									<p className="text-[11px] text-muted-foreground font-mono">
										{r.email} ({r.role})
									</p>
								</div>
							</div>
						))}
					</div>

					<DialogFooter className="gap-2 sm:gap-0">
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={() => setIsReviewerDialogOpen(false)}
						>
							Batal
						</Button>
						<Button
							type="button"
							size="sm"
							disabled={
								chooseReviewersMutation.isPending ||
								selectedReviewerIds.length === 0
							}
							onClick={handleAssignReviewers}
							className="bg-purple-600 hover:bg-purple-700 text-white"
						>
							{chooseReviewersMutation.isPending && (
								<Loader2 className="size-3.5 mr-1.5 animate-spin" />
							)}
							Tugaskan Reviewer ({selectedReviewerIds.length})
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</form>
	);
}
