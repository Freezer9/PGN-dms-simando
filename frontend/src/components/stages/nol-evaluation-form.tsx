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
	NolEvaluationScenarioDetail,
	SaveNolEvaluationRequest,
	SkemaPembayaran,
	StatusRkap,
} from "@/api/types";
import { DocumentDownloadButton } from "@/components/documents/document-download-buttons";
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

interface NolEvaluationFormProps {
	companyId: string;
	initialData?: NolEvaluationDetail | null;
	canEdit?: boolean;
	canChooseReviewers?: boolean;
	onSaved?: () => void;
	onReviewersChosen?: () => void;
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

	// Form State
	const [feedStatus, setFeedStatus] = React.useState<FeedStatus>(
		initialData?.feedStatus || "Selesai",
	);
	const [feedCompletedAt, setFeedCompletedAt] = React.useState<string>(
		initialData?.feedCompletedAt || "",
	);
	const [statusRkap, setStatusRkap] = React.useState<StatusRkap | "">(
		initialData?.statusRkap || "Rkap",
	);

	// Pipeline Specs
	const [pipaIndukPanjangM, setPipaIndukPanjangM] = React.useState<string>(
		initialData?.pipaIndukPanjangM != null
			? String(initialData.pipaIndukPanjangM)
			: "",
	);
	const [pipaIndukDiameter, setPipaIndukDiameter] = React.useState<string>(
		initialData?.pipaIndukDiameter != null
			? String(initialData.pipaIndukDiameter)
			: "",
	);
	const [pipaIndukDiameterUnit, setPipaIndukDiameterUnit] = React.useState<
		DiameterUnit | ""
	>(initialData?.pipaIndukDiameterUnit || "Inch");

	const [pipaServicePanjangM, setPipaServicePanjangM] = React.useState<string>(
		initialData?.pipaServicePanjangM != null
			? String(initialData.pipaServicePanjangM)
			: "",
	);
	const [pipaServiceDiameter, setPipaServiceDiameter] = React.useState<string>(
		initialData?.pipaServiceDiameter != null
			? String(initialData.pipaServiceDiameter)
			: "",
	);
	const [pipaServiceDiameterUnit, setPipaServiceDiameterUnit] = React.useState<
		DiameterUnit | ""
	>(initialData?.pipaServiceDiameterUnit || "Inch");

	// Meter & MRS
	const [spesifikasiMrs, setSpesifikasiMrs] = React.useState<string>(
		initialData?.spesifikasiMrs || "",
	);
	const [gSize, setGSize] = React.useState<string>(initialData?.gSize || "");
	const [maksKapasitasMeterM3Jam, setMaksKapasitasMeterM3Jam] =
		React.useState<string>(
			initialData?.maksKapasitasMeterM3Jam != null
				? String(initialData.maksKapasitasMeterM3Jam)
				: "",
		);
	const [tekanan, setTekanan] = React.useState<string>(
		initialData?.tekanan != null ? String(initialData.tekanan) : "",
	);
	const [maksFlowrate, setMaksFlowrate] = React.useState<string>(
		initialData?.maksFlowrate != null ? String(initialData.maksFlowrate) : "",
	);

	// Commercial & Financials
	const [skemaPembayaran, setSkemaPembayaran] = React.useState<
		SkemaPembayaran | ""
	>(initialData?.skemaPembayaran || "JaminanPembayaran");
	const [jaminanStatus, setJaminanStatus] = React.useState<string>(
		initialData?.jaminanStatus || "",
	);
	const [jaminanJenis, setJaminanJenis] = React.useState<string>(
		initialData?.jaminanJenis || "",
	);
	const [jaminanMasaBerlaku, setJaminanMasaBerlaku] = React.useState<string>(
		initialData?.jaminanMasaBerlaku || "",
	);
	const [jaminanPenerbit, setJaminanPenerbit] = React.useState<string>(
		initialData?.jaminanPenerbit || "",
	);
	const [ketersediaanPasokanBbtud, setKetersediaanPasokanBbtud] =
		React.useState<string>(
			initialData?.ketersediaanPasokanBbtud != null
				? String(initialData.ketersediaanPasokanBbtud)
				: "",
		);
	const [capexFinal, setCapexFinal] = React.useState<string>(
		initialData?.capexFinal != null ? String(initialData.capexFinal) : "",
	);
	const [durasiPelaksanaanBulan, setDurasiPelaksanaanBulan] =
		React.useState<string>(
			initialData?.durasiPelaksanaanBulan != null
				? String(initialData.durasiPelaksanaanBulan)
				: "",
		);

	// Narrative & Competitor
	const [analisisKomersial, setAnalisisKomersial] = React.useState<string>(
		initialData?.analisisKomersial || "",
	);
	const [analisisKompetitor, setAnalisisKompetitor] = React.useState<string>(
		initialData?.analisisKompetitor || "",
	);
	const [radiusKompetitorKm, setRadiusKompetitorKm] = React.useState<string>(
		initialData?.radiusKompetitorKm != null
			? String(initialData.radiusKompetitorKm)
			: "",
	);
	const [kesimpulan, setKesimpulan] = React.useState<string>(
		initialData?.kesimpulan || "",
	);

	// Repeating Evaluation Scenarios
	const [scenarios, setScenarios] = React.useState<
		NolEvaluationScenarioDetail[]
	>(
		initialData?.scenarios?.map((s) => ({
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
	);

	// Synchronize when initialData changes
	React.useEffect(() => {
		if (initialData) {
			setFeedStatus(initialData.feedStatus || "Selesai");
			setFeedCompletedAt(initialData.feedCompletedAt || "");
			setStatusRkap(initialData.statusRkap || "Rkap");
			setPipaIndukPanjangM(
				initialData.pipaIndukPanjangM != null
					? String(initialData.pipaIndukPanjangM)
					: "",
			);
			setPipaIndukDiameter(
				initialData.pipaIndukDiameter != null
					? String(initialData.pipaIndukDiameter)
					: "",
			);
			setPipaIndukDiameterUnit(initialData.pipaIndukDiameterUnit || "Inch");
			setPipaServicePanjangM(
				initialData.pipaServicePanjangM != null
					? String(initialData.pipaServicePanjangM)
					: "",
			);
			setPipaServiceDiameter(
				initialData.pipaServiceDiameter != null
					? String(initialData.pipaServiceDiameter)
					: "",
			);
			setPipaServiceDiameterUnit(initialData.pipaServiceDiameterUnit || "Inch");
			setSpesifikasiMrs(initialData.spesifikasiMrs || "");
			setGSize(initialData.gSize || "");
			setMaksKapasitasMeterM3Jam(
				initialData.maksKapasitasMeterM3Jam != null
					? String(initialData.maksKapasitasMeterM3Jam)
					: "",
			);
			setTekanan(
				initialData.tekanan != null ? String(initialData.tekanan) : "",
			);
			setMaksFlowrate(
				initialData.maksFlowrate != null
					? String(initialData.maksFlowrate)
					: "",
			);
			setSkemaPembayaran(initialData.skemaPembayaran || "JaminanPembayaran");
			setJaminanStatus(initialData.jaminanStatus || "");
			setJaminanJenis(initialData.jaminanJenis || "");
			setJaminanMasaBerlaku(initialData.jaminanMasaBerlaku || "");
			setJaminanPenerbit(initialData.jaminanPenerbit || "");
			setKetersediaanPasokanBbtud(
				initialData.ketersediaanPasokanBbtud != null
					? String(initialData.ketersediaanPasokanBbtud)
					: "",
			);
			setCapexFinal(
				initialData.capexFinal != null ? String(initialData.capexFinal) : "",
			);
			setDurasiPelaksanaanBulan(
				initialData.durasiPelaksanaanBulan != null
					? String(initialData.durasiPelaksanaanBulan)
					: "",
			);
			setAnalisisKomersial(initialData.analisisKomersial || "");
			setAnalisisKompetitor(initialData.analisisKompetitor || "");
			setRadiusKompetitorKm(
				initialData.radiusKompetitorKm != null
					? String(initialData.radiusKompetitorKm)
					: "",
			);
			setKesimpulan(initialData.kesimpulan || "");

			if (initialData.scenarios && initialData.scenarios.length > 0) {
				setScenarios(
					initialData.scenarios.map((s) => ({
						id: s.id || crypto.randomUUID(),
						label: s.label,
						irrPct: s.irrPct != null ? Number(s.irrPct) : null,
						npv: s.npv != null ? Number(s.npv) : null,
						paybackYears:
							s.paybackYears != null ? Number(s.paybackYears) : null,
						hasilAnalisis: s.hasilAnalisis || null,
					})),
				);
			}
		}
	}, [initialData]);

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

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();

		const request: SaveNolEvaluationRequest = {
			feedStatus,
			feedCompletedAt: feedCompletedAt || null,
			capexFinal: capexFinal ? Number(capexFinal) : null,
			pipaIndukPanjangM: pipaIndukPanjangM ? Number(pipaIndukPanjangM) : null,
			pipaIndukDiameter: pipaIndukDiameter ? Number(pipaIndukDiameter) : null,
			pipaIndukDiameterUnit: pipaIndukDiameterUnit
				? (pipaIndukDiameterUnit as DiameterUnit)
				: null,
			pipaServicePanjangM: pipaServicePanjangM
				? Number(pipaServicePanjangM)
				: null,
			pipaServiceDiameter: pipaServiceDiameter
				? Number(pipaServiceDiameter)
				: null,
			pipaServiceDiameterUnit: pipaServiceDiameterUnit
				? (pipaServiceDiameterUnit as DiameterUnit)
				: null,
			spesifikasiMrs: spesifikasiMrs || null,
			gSize: gSize || null,
			tekanan: tekanan ? Number(tekanan) : null,
			maksFlowrate: maksFlowrate ? Number(maksFlowrate) : null,
			maksKapasitasMeterM3Jam: maksKapasitasMeterM3Jam
				? Number(maksKapasitasMeterM3Jam)
				: null,
			durasiPelaksanaanBulan: durasiPelaksanaanBulan
				? Number(durasiPelaksanaanBulan)
				: null,
			statusRkap: statusRkap ? (statusRkap as StatusRkap) : null,
			skemaPembayaran: skemaPembayaran
				? (skemaPembayaran as SkemaPembayaran)
				: null,
			jaminanStatus: jaminanStatus || null,
			jaminanJenis: jaminanJenis || null,
			jaminanMasaBerlaku: jaminanMasaBerlaku || null,
			jaminanPenerbit: jaminanPenerbit || null,
			ketersediaanPasokanBbtud: ketersediaanPasokanBbtud
				? Number(ketersediaanPasokanBbtud)
				: null,
			analisisKomersial: analisisKomersial || null,
			analisisKompetitor: analisisKompetitor || null,
			kesimpulan: kesimpulan || null,
			radiusKompetitorKm: radiusKompetitorKm
				? Number(radiusKompetitorKm)
				: null,
			scenarios: scenarios.map((s) => ({
				id: s.id || crypto.randomUUID(),
				label: s.label,
				irrPct: s.irrPct != null ? Number(s.irrPct) : null,
				npv: s.npv != null ? Number(s.npv) : null,
				paybackYears: s.paybackYears != null ? Number(s.paybackYears) : null,
				hasilAnalisis: s.hasilAnalisis || null,
			})),
		};

		saveMutation.mutate({
			params: { path: { id: companyId } },
			body: request,
		});
	};

	const addScenarioRow = () => {
		setScenarios([
			...scenarios,
			{
				id: crypto.randomUUID(),
				label: `Skenario ${scenarios.length + 1}`,
				irrPct: null,
				npv: null,
				paybackYears: null,
				hasilAnalisis: "Layak",
			},
		]);
	};

	const removeScenarioRow = (index: number) => {
		setScenarios(scenarios.filter((_, i) => i !== index));
	};

	const toggleReviewer = (id: string) => {
		if (selectedReviewerIds.includes(id)) {
			setSelectedReviewerIds(selectedReviewerIds.filter((r) => r !== id));
		} else {
			setSelectedReviewerIds([...selectedReviewerIds, id]);
		}
	};

	const handleAssignReviewers = () => {
		if (selectedReviewerIds.length === 0) {
			toast.error("Pilih minimal 1 reviewer.");
			return;
		}
		chooseReviewersMutation.mutate({
			params: { path: { id: companyId } },
			body: { reviewerUserIds: selectedReviewerIds },
		});
	};

	return (
		<form onSubmit={handleSubmit} className="space-y-6">
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
						<Button
							type="submit"
							size="sm"
							disabled={saveMutation.isPending}
							className="h-9 text-xs flex items-center gap-1.5 bg-purple-600 hover:bg-purple-700 text-white"
						>
							{saveMutation.isPending ? (
								<Loader2 className="size-3.5 animate-spin" />
							) : (
								<Save className="size-3.5" />
							)}
							Simpan Resume Evaluasi
						</Button>
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
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Status FEED</Label>
							<Select
								value={feedStatus}
								onValueChange={(val) => setFeedStatus(val as FeedStatus)}
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
						</div>

						{/* Tanggal Selesai FEED */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Tanggal Selesai FEED
							</Label>
							<Input
								type="date"
								value={feedCompletedAt}
								onChange={(e) => setFeedCompletedAt(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Status RKAP */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Status RKAP</Label>
							<Select
								value={statusRkap || "NONE"}
								onValueChange={(val) =>
									setStatusRkap(val === "NONE" ? "" : (val as StatusRkap))
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
						</div>

						{/* Capex Final */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Capex Final (USD)</Label>
							<Input
								type="number"
								step="0.01"
								value={capexFinal}
								onChange={(e) => setCapexFinal(e.target.value)}
								placeholder="contoh: 85000"
								disabled={!canEdit}
								className="text-xs h-9 font-mono"
							/>
						</div>
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
							<div className="space-y-1.5">
								<Label className="text-[11px]">Panjang Pipa (Meter)</Label>
								<Input
									type="number"
									placeholder="contoh: 500"
									value={pipaIndukPanjangM}
									onChange={(e) => setPipaIndukPanjangM(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-8"
								/>
							</div>
							<div className="grid grid-cols-2 gap-2">
								<div className="space-y-1.5">
									<Label className="text-[11px]">Diameter</Label>
									<Input
										type="number"
										step="0.5"
										placeholder="contoh: 4"
										value={pipaIndukDiameter}
										onChange={(e) => setPipaIndukDiameter(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-8"
									/>
								</div>
								<div className="space-y-1.5">
									<Label className="text-[11px]">Satuan</Label>
									<Select
										value={pipaIndukDiameterUnit || "Inch"}
										onValueChange={(val) =>
											setPipaIndukDiameterUnit(val as DiameterUnit)
										}
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
								</div>
							</div>
						</div>

						{/* Pipa Servis */}
						<div className="p-3 border rounded-md space-y-2 bg-muted/20">
							<span className="text-xs font-semibold text-muted-foreground">
								Pipa Servis Pelanggan
							</span>
							<div className="space-y-1.5">
								<Label className="text-[11px]">Panjang Pipa (Meter)</Label>
								<Input
									type="number"
									placeholder="contoh: 25"
									value={pipaServicePanjangM}
									onChange={(e) => setPipaServicePanjangM(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-8"
								/>
							</div>
							<div className="grid grid-cols-2 gap-2">
								<div className="space-y-1.5">
									<Label className="text-[11px]">Diameter</Label>
									<Input
										type="number"
										step="0.5"
										placeholder="contoh: 2"
										value={pipaServiceDiameter}
										onChange={(e) => setPipaServiceDiameter(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-8"
									/>
								</div>
								<div className="space-y-1.5">
									<Label className="text-[11px]">Satuan</Label>
									<Select
										value={pipaServiceDiameterUnit || "Inch"}
										onValueChange={(val) =>
											setPipaServiceDiameterUnit(val as DiameterUnit)
										}
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
								</div>
							</div>
						</div>

						{/* MRS & Metering */}
						<div className="p-3 border rounded-md space-y-2 bg-muted/20">
							<span className="text-xs font-semibold text-muted-foreground">
								Spesifikasi MRS & Meter
							</span>
							<div className="space-y-1.5">
								<Label className="text-[11px]">Tipe / Spesifikasi MRS</Label>
								<Select
									value={spesifikasiMrs || "NONE"}
									onValueChange={(val) =>
										setSpesifikasiMrs(val === "NONE" ? "" : val)
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
							</div>
							<div className="grid grid-cols-2 gap-2">
								<div className="space-y-1.5">
									<Label className="text-[11px]">Ukuran Meter</Label>
									<Select
										value={gSize || "NONE"}
										onValueChange={(val) => setGSize(val === "NONE" ? "" : val)}
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
								</div>
								<div className="space-y-1.5">
									<Label className="text-[11px]">Kapasitas (m3/jam)</Label>
									<Input
										type="number"
										placeholder="contoh: 250"
										value={maksKapasitasMeterM3Jam}
										onChange={(e) => setMaksKapasitasMeterM3Jam(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-8"
									/>
								</div>
							</div>
						</div>
					</div>

					<div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-2 border-t">
						{/* Tekanan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Tekanan (Barg)</Label>
							<Input
								type="number"
								step="0.1"
								placeholder="contoh: 3.0"
								value={tekanan}
								onChange={(e) => setTekanan(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Maks Flowrate */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Maksimum Flowrate (m3/jam)
							</Label>
							<Input
								type="number"
								placeholder="contoh: 180"
								value={maksFlowrate}
								onChange={(e) => setMaksFlowrate(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Durasi Pelaksanaan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Durasi Pelaksanaan (Bulan)
							</Label>
							<Input
								type="number"
								placeholder="contoh: 3"
								value={durasiPelaksanaanBulan}
								onChange={(e) => setDurasiPelaksanaanBulan(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>
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
							onClick={addScenarioRow}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" />
							Tambah Skenario
						</Button>
					)}
				</CardHeader>
				<CardContent>
					<div className="rounded-lg border overflow-hidden">
						<Table>
							<TableHeader className="bg-muted/40">
								<TableRow>
									<TableHead className="text-xs">Nama Skenario</TableHead>
									<TableHead className="text-xs">IRR (%)</TableHead>
									<TableHead className="text-xs">NPV (USD)</TableHead>
									<TableHead className="text-xs">Payback (Tahun)</TableHead>
									<TableHead className="text-xs">Kesimpulan / Status</TableHead>
									{canEdit && (
										<TableHead className="text-xs text-center w-12">
											Hapus
										</TableHead>
									)}
								</TableRow>
							</TableHeader>
							<TableBody>
								{scenarios.map((row, idx) => (
									// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
									<TableRow key={idx}>
										<TableCell>
											<Input
												value={row.label}
												onChange={(e) => {
													const next = [...scenarios];
													next[idx].label = e.target.value;
													setScenarios(next);
												}}
												disabled={!canEdit}
												className="text-xs h-8 font-medium"
											/>
										</TableCell>
										<TableCell>
											<Input
												type="number"
												step="0.1"
												value={row.irrPct ?? ""}
												onChange={(e) => {
													const next = [...scenarios];
													next[idx].irrPct = e.target.value
														? Number(e.target.value)
														: null;
													setScenarios(next);
												}}
												placeholder="18.5"
												disabled={!canEdit}
												className="text-xs h-8 font-mono"
											/>
										</TableCell>
										<TableCell>
											<Input
												type="number"
												value={row.npv ?? ""}
												onChange={(e) => {
													const next = [...scenarios];
													next[idx].npv = e.target.value
														? Number(e.target.value)
														: null;
													setScenarios(next);
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
												value={row.paybackYears ?? ""}
												onChange={(e) => {
													const next = [...scenarios];
													next[idx].paybackYears = e.target.value
														? Number(e.target.value)
														: null;
													setScenarios(next);
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
													const next = [...scenarios];
													next[idx].hasilAnalisis = e.target.value || null;
													setScenarios(next);
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
													onClick={() => removeScenarioRow(idx)}
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
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Skema Pembayaran</Label>
							<Select
								value={skemaPembayaran || "NONE"}
								onValueChange={(val) =>
									setSkemaPembayaran(
										val === "NONE" ? "" : (val as SkemaPembayaran),
									)
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
						</div>

						{/* Ketersediaan Pasokan (BBTUD) */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Ketersediaan Pasokan Gas (BBTUD)
							</Label>
							<Input
								type="number"
								step="0.01"
								value={ketersediaanPasokanBbtud}
								onChange={(e) => setKetersediaanPasokanBbtud(e.target.value)}
								placeholder="contoh: 1.5"
								disabled={!canEdit}
								className="text-xs h-9 font-mono"
							/>
						</div>

						{/* Radius Kompetitor (km) */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Jarak Kompetitor Terdekat (Km)
							</Label>
							<Input
								type="number"
								step="0.1"
								value={radiusKompetitorKm}
								onChange={(e) => setRadiusKompetitorKm(e.target.value)}
								placeholder="contoh: 2.5"
								disabled={!canEdit}
								className="text-xs h-9 font-mono"
							/>
						</div>
					</div>

					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 pt-2 border-t">
						<div className="space-y-1.5">
							<Label className="text-[11px]">Status Jaminan</Label>
							<Input
								value={jaminanStatus}
								onChange={(e) => setJaminanStatus(e.target.value)}
								placeholder="contoh: Siap diterbitkan"
								disabled={!canEdit}
								className="text-xs h-8"
							/>
						</div>
						<div className="space-y-1.5">
							<Label className="text-[11px]">Jenis Jaminan</Label>
							<Input
								value={jaminanJenis}
								onChange={(e) => setJaminanJenis(e.target.value)}
								placeholder="contoh: Bank Garansi"
								disabled={!canEdit}
								className="text-xs h-8"
							/>
						</div>
						<div className="space-y-1.5">
							<Label className="text-[11px]">Masa Berlaku</Label>
							<Input
								value={jaminanMasaBerlaku}
								onChange={(e) => setJaminanMasaBerlaku(e.target.value)}
								placeholder="contoh: 12 Bulan"
								disabled={!canEdit}
								className="text-xs h-8"
							/>
						</div>
						<div className="space-y-1.5">
							<Label className="text-[11px]">Bank / Penerbit Jaminan</Label>
							<Input
								value={jaminanPenerbit}
								onChange={(e) => setJaminanPenerbit(e.target.value)}
								placeholder="contoh: Bank Mandiri"
								disabled={!canEdit}
								className="text-xs h-8"
							/>
						</div>
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
					<div className="space-y-1.5">
						<Label className="text-xs font-medium">
							Analisis Komersial & Pasar
						</Label>
						<Textarea
							value={analisisKomersial}
							onChange={(e) => setAnalisisKomersial(e.target.value)}
							placeholder="Evaluasi profil kebutuhan gas, proyeksi pertumbuhan, dan kepatuhan tarif..."
							disabled={!canEdit}
							className="text-xs min-h-[60px]"
						/>
					</div>

					<div className="space-y-1.5">
						<Label className="text-xs font-medium">
							Analisis Kompetitor & Ancaman Bahan Bakar Alternatif
						</Label>
						<Textarea
							value={analisisKompetitor}
							onChange={(e) => setAnalisisKompetitor(e.target.value)}
							placeholder="Analisis harga energi kompetitor (CNG, LPG, Batubara)..."
							disabled={!canEdit}
							className="text-xs min-h-[60px]"
						/>
					</div>

					<div className="space-y-1.5">
						<Label className="text-xs font-medium font-semibold text-primary">
							Kesimpulan & Rekomendasi Tim Evaluator
						</Label>
						<Textarea
							value={kesimpulan}
							onChange={(e) => setKesimpulan(e.target.value)}
							placeholder="Rekomendasi penerbitan Surat NOL atau RL bersyarat..."
							disabled={!canEdit}
							className="text-xs min-h-[70px]"
						/>
					</div>
				</CardContent>
			</Card>

			{canEdit && (
				<div className="flex justify-end pt-2">
					<Button
						type="submit"
						disabled={saveMutation.isPending}
						className="h-9 text-xs flex items-center gap-1.5 bg-purple-600 hover:bg-purple-700 text-white"
					>
						{saveMutation.isPending ? (
							<Loader2 className="size-3.5 animate-spin" />
						) : (
							<Save className="size-3.5" />
						)}
						Simpan Resume Evaluasi
					</Button>
				</div>
			)}

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
