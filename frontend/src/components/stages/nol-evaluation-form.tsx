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
import { Badge } from "@/components/ui/badge";
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
import { Switch } from "@/components/ui/switch";
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
	const [feedStatus, setFeedStatus] = React.useState<FeedStatus | "">(
		initialData?.feedStatus || "Done",
	);
	const [feedTanggalSelesai, setFeedTanggalSelesai] = React.useState<string>(
		initialData?.feedTanggalSelesai || "",
	);
	const [isRkap, setIsRkap] = React.useState<StatusRkap | "">(
		initialData?.isRkap || "Rkap",
	);

	// Pipeline Specs
	const [pipaIndukPanjangM, setPipaIndukPanjangM] = React.useState<string>(
		initialData?.pipaIndukPanjangM ? String(initialData.pipaIndukPanjangM) : "",
	);
	const [pipaIndukDiameter, setPipaIndukDiameter] = React.useState<string>(
		initialData?.pipaIndukDiameter ? String(initialData.pipaIndukDiameter) : "",
	);
	const [pipaIndukDiameterUnit, setPipaIndukDiameterUnit] = React.useState<
		DiameterUnit | ""
	>(initialData?.pipaIndukDiameterUnit || "Inch");

	const [pipaServicePanjangM, setPipaServicePanjangM] = React.useState<string>(
		initialData?.pipaServicePanjangM
			? String(initialData.pipaServicePanjangM)
			: "",
	);
	const [pipaServiceDiameter, setPipaServiceDiameter] = React.useState<string>(
		initialData?.pipaServiceDiameter
			? String(initialData.pipaServiceDiameter)
			: "",
	);
	const [pipaServiceDiameterUnit, setPipaServiceDiameterUnit] = React.useState<
		DiameterUnit | ""
	>(initialData?.pipaServiceDiameterUnit || "Inch");

	// Meter & MRS
	const [mrsSpecId, setMrsSpecId] = React.useState<string>(
		initialData?.mrsSpecId || "",
	);
	const [meterSizeId, setMeterSizeId] = React.useState<string>(
		initialData?.meterSizeId || "",
	);
	const [kapasitasMeterM3H, setKapasitasMeterM3H] = React.useState<string>(
		initialData?.kapasitasMeterM3H ? String(initialData.kapasitasMeterM3H) : "",
	);
	const [tekananInletBarg, setTekananInletBarg] = React.useState<string>(
		initialData?.tekananInletBarg ? String(initialData.tekananInletBarg) : "",
	);
	const [tekananOutletBarg, setTekananOutletBarg] = React.useState<string>(
		initialData?.tekananOutletBarg ? String(initialData.tekananOutletBarg) : "",
	);
	const [maxFlowrateM3H, setMaxFlowrateM3H] = React.useState<string>(
		initialData?.maxFlowrateM3H ? String(initialData.maxFlowrateM3H) : "",
	);

	// Commercial & Bank Guarantee
	const [skemaPembayaran, setSkemaPembayaran] = React.useState<
		SkemaPembayaran | ""
	>(initialData?.skemaPembayaran || "Pascabayar");
	const [jaminanPembayaranStatus, setJaminanPembayaranStatus] =
		React.useState<boolean>(initialData?.jaminanPembayaranStatus ?? true);
	const [jaminanPembayaranJenis, setJaminanPembayaranJenis] =
		React.useState<string>(initialData?.jaminanPembayaranJenis || "");
	const [jaminanPembayaranMasaBerlaku, setJaminanPembayaranMasaBerlaku] =
		React.useState<string>(initialData?.jaminanPembayaranMasaBerlaku || "");
	const [jaminanPembayaranPenerbit, setJaminanPembayaranPenerbit] =
		React.useState<string>(initialData?.jaminanPembayaranPenerbit || "");

	const [pasokanBbtud, setPasokanBbtud] = React.useState<string>(
		initialData?.pasokanBbtud ? String(initialData.pasokanBbtud) : "",
	);
	const [radiusKompetitorKm, setRadiusKompetitorKm] = React.useState<string>(
		initialData?.radiusKompetitorKm
			? String(initialData.radiusKompetitorKm)
			: "",
	);
	const [keteranganResume, setKeteranganResume] = React.useState<string>(
		initialData?.keteranganResume || "",
	);

	// Scenarios
	const [scenarios, setScenarios] = React.useState<
		SaveNolEvaluationScenarioRequest[]
	>(
		initialData?.scenarios?.map((s) => ({
			scenarioName: s.scenarioName || undefined,
			irrPct: s.irrPct != null ? Number(s.irrPct) : undefined,
			npvIdr: s.npvIdr != null ? Number(s.npvIdr) : undefined,
			paybackPeriodYears:
				s.paybackPeriodYears != null ? Number(s.paybackPeriodYears) : undefined,
			kesimpulan: s.kesimpulan || undefined,
		})) || [],
	);

	// Sync initialData
	React.useEffect(() => {
		if (initialData) {
			setFeedStatus(initialData.feedStatus || "Done");
			setFeedTanggalSelesai(initialData.feedTanggalSelesai || "");
			setIsRkap(initialData.isRkap || "Rkap");
			setPipaIndukPanjangM(
				initialData.pipaIndukPanjangM
					? String(initialData.pipaIndukPanjangM)
					: "",
			);
			setPipaIndukDiameter(
				initialData.pipaIndukDiameter
					? String(initialData.pipaIndukDiameter)
					: "",
			);
			setPipaIndukDiameterUnit(initialData.pipaIndukDiameterUnit || "Inch");
			setPipaServicePanjangM(
				initialData.pipaServicePanjangM
					? String(initialData.pipaServicePanjangM)
					: "",
			);
			setPipaServiceDiameter(
				initialData.pipaServiceDiameter
					? String(initialData.pipaServiceDiameter)
					: "",
			);
			setPipaServiceDiameterUnit(initialData.pipaServiceDiameterUnit || "Inch");
			setMrsSpecId(initialData.mrsSpecId || "");
			setMeterSizeId(initialData.meterSizeId || "");
			setKapasitasMeterM3H(
				initialData.kapasitasMeterM3H
					? String(initialData.kapasitasMeterM3H)
					: "",
			);
			setTekananInletBarg(
				initialData.tekananInletBarg
					? String(initialData.tekananInletBarg)
					: "",
			);
			setTekananOutletBarg(
				initialData.tekananOutletBarg
					? String(initialData.tekananOutletBarg)
					: "",
			);
			setMaxFlowrateM3H(
				initialData.maxFlowrateM3H ? String(initialData.maxFlowrateM3H) : "",
			);
			setSkemaPembayaran(initialData.skemaPembayaran || "Pascabayar");
			setJaminanPembayaranStatus(initialData.jaminanPembayaranStatus ?? true);
			setJaminanPembayaranJenis(initialData.jaminanPembayaranJenis || "");
			setJaminanPembayaranMasaBerlaku(
				initialData.jaminanPembayaranMasaBerlaku || "",
			);
			setJaminanPembayaranPenerbit(initialData.jaminanPembayaranPenerbit || "");
			setPasokanBbtud(
				initialData.pasokanBbtud ? String(initialData.pasokanBbtud) : "",
			);
			setRadiusKompetitorKm(
				initialData.radiusKompetitorKm
					? String(initialData.radiusKompetitorKm)
					: "",
			);
			setKeteranganResume(initialData.keteranganResume || "");

			if (initialData.scenarios) {
				setScenarios(
					initialData.scenarios.map((s) => ({
						scenarioName: s.scenarioName || undefined,
						irrPct: s.irrPct != null ? Number(s.irrPct) : undefined,
						npvIdr: s.npvIdr != null ? Number(s.npvIdr) : undefined,
						paybackPeriodYears:
							s.paybackPeriodYears != null
								? Number(s.paybackPeriodYears)
								: undefined,
						kesimpulan: s.kesimpulan || undefined,
					})),
				);
			}
		}
	}, [initialData]);

	// Auto fill MRS Spec Name & Meter GSize from lookup
	const selectedMrsSpec = mrsSpecs?.find((m) => m.id === mrsSpecId);
	const selectedMeterSize = meterSizes?.find((m) => m.id === meterSizeId);

	// Save Mutation
	const saveMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/nol-evaluation",
		{
			onSuccess: () => {
				toast.success("Data Evaluasi & Resume NOL berhasil disimpan!");
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
						: "Gagal menyimpan Evaluasi NOL",
				);
			},
		},
	);

	// Choose Reviewers Mutation
	const chooseReviewersMutation = $api.useMutation(
		"post",
		"/api/companies/{id}/workflow/choose-reviewers",
		{
			onSuccess: (res) => {
				toast.success(
					`Reviewer berhasil ditetapkan! Status: ${res.currentStatus}`,
				);
				setIsReviewerDialogOpen(false);
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
				onReviewersChosen?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error ? error.message : "Gagal menetapkan Reviewer",
				);
			},
		},
	);

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();

		const request: SaveNolEvaluationRequest = {
			feedStatus: feedStatus ? (feedStatus as FeedStatus) : null,
			feedTanggalSelesai: feedTanggalSelesai || null,
			isRkap: isRkap ? (isRkap as StatusRkap) : null,
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
			mrsSpecId: mrsSpecId || null,
			mrsSpecName: selectedMrsSpec?.name || null,
			meterSizeId: meterSizeId || null,
			meterGSize: selectedMeterSize?.gSize || null,
			kapasitasMeterM3H: kapasitasMeterM3H ? Number(kapasitasMeterM3H) : null,
			tekananInletBarg: tekananInletBarg ? Number(tekananInletBarg) : null,
			tekananOutletBarg: tekananOutletBarg ? Number(tekananOutletBarg) : null,
			maxFlowrateM3H: maxFlowrateM3H ? Number(maxFlowrateM3H) : null,
			skemaPembayaran: skemaPembayaran
				? (skemaPembayaran as SkemaPembayaran)
				: null,
			jaminanPembayaranStatus,
			jaminanPembayaranJenis: jaminanPembayaranJenis || null,
			jaminanPembayaranMasaBerlaku: jaminanPembayaranMasaBerlaku || null,
			jaminanPembayaranPenerbit: jaminanPembayaranPenerbit || null,
			pasokanBbtud: pasokanBbtud ? Number(pasokanBbtud) : null,
			radiusKompetitorKm: radiusKompetitorKm
				? Number(radiusKompetitorKm)
				: null,
			keteranganResume: keteranganResume || null,
			scenarios,
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
				scenarioName: `Skenario ${scenarios.length + 1}`,
				irrPct: undefined,
				npvIdr: undefined,
				paybackPeriodYears: undefined,
				kesimpulan: "Layak",
			},
		]);
	};

	const removeScenarioRow = (index: number) => {
		setScenarios(scenarios.filter((_, i) => i !== index));
	};

	const toggleReviewerSelection = (userId: string) => {
		if (selectedReviewerIds.includes(userId)) {
			setSelectedReviewerIds(selectedReviewerIds.filter((id) => id !== userId));
		} else {
			setSelectedReviewerIds([...selectedReviewerIds, userId]);
		}
	};

	const handleConfirmReviewers = () => {
		if (selectedReviewerIds.length === 0) {
			toast.error("Pilih minimal 1 Reviewer!");
			return;
		}
		chooseReviewersMutation.mutate({
			params: { path: { id: companyId } },
			body: { reviewerUserIds: selectedReviewerIds },
		});
	};

	return (
		<>
			<form onSubmit={handleSubmit} className="space-y-6">
				{/* Top Bar Summary / Save / Choose Reviewers */}
				<div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 bg-muted/40 rounded-lg border">
					<div className="flex items-center gap-3">
						<div className="size-10 rounded-full bg-cyan-100 dark:bg-cyan-900/40 text-cyan-600 flex items-center justify-center">
							<FileSearch className="size-5" />
						</div>
						<div>
							<h3 className="text-sm font-semibold">
								Evaluasi Teknis & Komersial Surat NOL (Stage 7)
							</h3>
							<p className="text-xs text-muted-foreground">
								Spesifikasi FEED, jaringan pipa, MRS, analisis kelayakan
								finansial, dan penunjukan tim penelaah
							</p>
						</div>
					</div>

					<div className="flex items-center gap-2">
						<DocumentDownloadButton
							companyId={companyId}
							documentType="evaluation"
							label="Unduh Resume Evaluasi (.docx)"
						/>
						{canEdit && (
							<Button
								type="submit"
								size="sm"
								disabled={saveMutation.isPending}
								className="h-9 text-xs flex items-center gap-1.5 bg-cyan-600 hover:bg-cyan-700 text-white"
							>
								{saveMutation.isPending ? (
									<Loader2 className="size-3.5 animate-spin" />
								) : (
									<Save className="size-3.5" />
								)}
								Simpan Evaluasi NOL
							</Button>
						)}

						{canChooseReviewers && (
							<Button
								type="button"
								size="sm"
								onClick={() => setIsReviewerDialogOpen(true)}
								className="h-9 text-xs flex items-center gap-1.5 bg-indigo-600 hover:bg-indigo-700 text-white"
							>
								<UserPlus className="size-3.5" />
								Tetapkan Reviewer
							</Button>
						)}
					</div>
				</div>

				{/* SECTION 1: FEED & INFRASTRUKTUR PIPA */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3">
						<CardTitle className="text-sm font-semibold flex items-center gap-2">
							<Gauge className="size-4 text-cyan-500" />
							1. Status FEED & Spesifikasi Jaringan Pipa
						</CardTitle>
						<CardDescription className="text-xs">
							Kesiapan Front-End Engineering Design, status anggaran RKAP, dan
							dimensi pipa
						</CardDescription>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
							{/* Status FEED */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">Status FEED</Label>
								<Select
									value={feedStatus || "Done"}
									onValueChange={(val) => setFeedStatus(val as FeedStatus)}
									disabled={!canEdit}
								>
									<SelectTrigger className="text-xs h-9">
										<SelectValue />
									</SelectTrigger>
									<SelectContent>
										<SelectItem value="Done">Selesai (Done)</SelectItem>
										<SelectItem value="InProgress">
											Dalam Pengerjaan (In Progress)
										</SelectItem>
										<SelectItem value="NotYetStarted">Belum Dimulai</SelectItem>
										<SelectItem value="NotRequired">
											Tidak Diperlukan
										</SelectItem>
									</SelectContent>
								</Select>
							</div>

							{/* Tanggal Selesai FEED */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Tanggal Target / Selesai FEED
								</Label>
								<Input
									type="date"
									value={feedTanggalSelesai}
									onChange={(e) => setFeedTanggalSelesai(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>

							{/* Status RKAP */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Status Anggaran RKAP
								</Label>
								<Select
									value={isRkap || "Rkap"}
									onValueChange={(val) => setIsRkap(val as StatusRkap)}
									disabled={!canEdit}
								>
									<SelectTrigger className="text-xs h-9">
										<SelectValue />
									</SelectTrigger>
									<SelectContent>
										<SelectItem value="Rkap">
											Termasuk RKAP (Approved Budget)
										</SelectItem>
										<SelectItem value="NonRkap">
											Non-RKAP (Unbudgeted / Addendum)
										</SelectItem>
									</SelectContent>
								</Select>
							</div>

							{/* Pipa Induk - Panjang (m) */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Panjang Pipa Induk (m)
								</Label>
								<Input
									type="number"
									step="0.1"
									value={pipaIndukPanjangM}
									onChange={(e) => setPipaIndukPanjangM(e.target.value)}
									placeholder="contoh: 450"
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>

							{/* Pipa Induk - Diameter */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Diameter Pipa Induk
								</Label>
								<div className="flex gap-2">
									<Input
										type="number"
										step="0.1"
										value={pipaIndukDiameter}
										onChange={(e) => setPipaIndukDiameter(e.target.value)}
										placeholder="contoh: 6"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
									<Select
										value={pipaIndukDiameterUnit || "Inch"}
										onValueChange={(val) =>
											setPipaIndukDiameterUnit(val as DiameterUnit)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9 w-24">
											<SelectValue />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="Inch">Inch</SelectItem>
											<SelectItem value="Mm">mm</SelectItem>
										</SelectContent>
									</Select>
								</div>
							</div>

							{/* Pipa Service - Panjang (m) */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Panjang Pipa Service (m)
								</Label>
								<Input
									type="number"
									step="0.1"
									value={pipaServicePanjangM}
									onChange={(e) => setPipaServicePanjangM(e.target.value)}
									placeholder="contoh: 25"
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>

							{/* Pipa Service - Diameter */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Diameter Pipa Service
								</Label>
								<div className="flex gap-2">
									<Input
										type="number"
										step="0.1"
										value={pipaServiceDiameter}
										onChange={(e) => setPipaServiceDiameter(e.target.value)}
										placeholder="contoh: 2"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
									<Select
										value={pipaServiceDiameterUnit || "Inch"}
										onValueChange={(val) =>
											setPipaServiceDiameterUnit(val as DiameterUnit)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9 w-24">
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
					</CardContent>
				</Card>

				{/* SECTION 2: SPESIFIKASI MRS & METERING */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3">
						<CardTitle className="text-sm font-semibold flex items-center gap-2">
							<Activity className="size-4 text-emerald-500" />
							2. Spesifikasi MRS & Alat Ukur Metering
						</CardTitle>
						<CardDescription className="text-xs">
							Tipe stasiun pengukur tekanan (MRS), ukuran turbin/rotary meter,
							dan parameter operasi
						</CardDescription>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
							{/* MRS Spec */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Tipe / Spesifikasi MRS
								</Label>
								<Select
									value={mrsSpecId || "NONE"}
									onValueChange={(val) =>
										setMrsSpecId(val === "NONE" ? "" : val)
									}
									disabled={!canEdit}
								>
									<SelectTrigger className="text-xs h-9">
										<SelectValue placeholder="Pilih Spesifikasi MRS" />
									</SelectTrigger>
									<SelectContent>
										<SelectItem value="NONE">Belum Dipilih</SelectItem>
										{mrsSpecs?.map((m) => (
											<SelectItem key={m.id} value={m.id}>
												{m.name}
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>

							{/* Meter Size */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Ukuran Meter (G-Size)
								</Label>
								<Select
									value={meterSizeId || "NONE"}
									onValueChange={(val) =>
										setMeterSizeId(val === "NONE" ? "" : val)
									}
									disabled={!canEdit}
								>
									<SelectTrigger className="text-xs h-9">
										<SelectValue placeholder="Pilih Ukuran Meter" />
									</SelectTrigger>
									<SelectContent>
										<SelectItem value="NONE">Belum Dipilih</SelectItem>
										{meterSizes?.map((s) => (
											<SelectItem key={s.id} value={s.id}>
												{s.gSize} (QMax: {s.qMaxM3H} m³/h)
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>

							{/* Kapasitas Meter m3/h */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Kapasitas Meter (m³/h)
								</Label>
								<Input
									type="number"
									step="0.1"
									value={kapasitasMeterM3H}
									onChange={(e) => setKapasitasMeterM3H(e.target.value)}
									placeholder="contoh: 250"
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>

							{/* Tekanan Inlet (barg) */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Tekanan Inlet (barg)
								</Label>
								<Input
									type="number"
									step="0.01"
									value={tekananInletBarg}
									onChange={(e) => setTekananInletBarg(e.target.value)}
									placeholder="contoh: 4.0"
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>

							{/* Tekanan Outlet (barg) */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Tekanan Outlet (barg)
								</Label>
								<Input
									type="number"
									step="0.01"
									value={tekananOutletBarg}
									onChange={(e) => setTekananOutletBarg(e.target.value)}
									placeholder="contoh: 2.0"
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>

							{/* Max Flowrate (m3/h) */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Max Flowrate (m³/h)
								</Label>
								<Input
									type="number"
									step="0.1"
									value={maxFlowrateM3H}
									onChange={(e) => setMaxFlowrateM3H(e.target.value)}
									placeholder="contoh: 180"
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>
						</div>
					</CardContent>
				</Card>

				{/* SECTION 3: KETENTUAN KOMERSIAL & JAMINAN */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3">
						<CardTitle className="text-sm font-semibold flex items-center gap-2">
							<ShieldCheck className="size-4 text-indigo-500" />
							3. Parameter Komersial, Jaminan Pembayaran & Pasokan
						</CardTitle>
						<CardDescription className="text-xs">
							Skema penagihan, mitigasi risiko bank guarantee, pasokan BBTUD,
							dan radius kompetitor
						</CardDescription>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
							{/* Skema Pembayaran */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">Skema Pembayaran</Label>
								<Select
									value={skemaPembayaran || "Pascabayar"}
									onValueChange={(val) =>
										setSkemaPembayaran(val as SkemaPembayaran)
									}
									disabled={!canEdit}
								>
									<SelectTrigger className="text-xs h-9">
										<SelectValue />
									</SelectTrigger>
									<SelectContent>
										<SelectItem value="Pascabayar">
											Pascabayar (Postpaid)
										</SelectItem>
										<SelectItem value="Prabayar">Prabayar (Prepaid)</SelectItem>
									</SelectContent>
								</Select>
							</div>

							{/* Pasokan BBTUD */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Alokasi Pasokan Gas (BBTUD)
								</Label>
								<Input
									type="number"
									step="0.001"
									value={pasokanBbtud}
									onChange={(e) => setPasokanBbtud(e.target.value)}
									placeholder="contoh: 0.05"
									disabled={!canEdit}
									className="text-xs h-9 font-mono"
								/>
							</div>

							{/* Radius Kompetitor */}
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Radius Kompetitor Terdekat (km)
								</Label>
								<Input
									type="number"
									step="0.1"
									value={radiusKompetitorKm}
									onChange={(e) => setRadiusKompetitorKm(e.target.value)}
									placeholder="contoh: 12.5"
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>
						</div>

						{/* Jaminan Pembayaran Box */}
						<div className="p-3 border rounded-lg bg-muted/20 space-y-3">
							<div className="flex items-center space-x-3">
								<Switch
									id="bg-status"
									checked={jaminanPembayaranStatus}
									onCheckedChange={setJaminanPembayaranStatus}
									disabled={!canEdit}
								/>
								<div>
									<Label
										htmlFor="bg-status"
										className="text-xs font-semibold cursor-pointer"
									>
										Wajib Jaminan Pembayaran (Bank Guarantee / Deposito)
									</Label>
									<p className="text-[11px] text-muted-foreground">
										Diperlukan sebagai agunan pembayaran tagihan pemakaian gas
										bumi
									</p>
								</div>
							</div>

							{jaminanPembayaranStatus && (
								<div className="grid grid-cols-1 sm:grid-cols-3 gap-3 pt-2">
									<div className="space-y-1">
										<Label className="text-[11px]">Jenis Jaminan</Label>
										<Input
											value={jaminanPembayaranJenis}
											onChange={(e) =>
												setJaminanPembayaranJenis(e.target.value)
											}
											placeholder="contoh: Bank Garansi"
											disabled={!canEdit}
											className="text-xs h-8"
										/>
									</div>
									<div className="space-y-1">
										<Label className="text-[11px]">Masa Berlaku</Label>
										<Input
											value={jaminanPembayaranMasaBerlaku}
											onChange={(e) =>
												setJaminanPembayaranMasaBerlaku(e.target.value)
											}
											placeholder="contoh: 12 Bulan"
											disabled={!canEdit}
											className="text-xs h-8"
										/>
									</div>
									<div className="space-y-1">
										<Label className="text-[11px]">
											Bank Penerbit / Penjamin
										</Label>
										<Input
											value={jaminanPembayaranPenerbit}
											onChange={(e) =>
												setJaminanPembayaranPenerbit(e.target.value)
											}
											placeholder="contoh: Bank Mandiri"
											disabled={!canEdit}
											className="text-xs h-8"
										/>
									</div>
								</div>
							)}
						</div>
					</CardContent>
				</Card>

				{/* SECTION 4: SKENARIO KELAYAKAN FINANSIAL */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-sm font-semibold flex items-center gap-2">
								<LineChart className="size-4 text-emerald-500" />
								4. Analisis Kelayakan Finansial & Skenario Investasi
							</CardTitle>
							<CardDescription className="text-xs">
								Perhitungan IRR (%), NPV (IDR), dan Payback Period investasi
								jaringan pipa
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
								<Plus className="size-3.5" /> Tambah Skenario
							</Button>
						)}
					</CardHeader>
					<CardContent>
						<div className="border rounded-lg overflow-x-auto">
							<Table>
								<TableHeader className="bg-muted/50">
									<TableRow>
										<TableHead className="text-xs font-semibold min-w-[140px]">
											Nama Skenario
										</TableHead>
										<TableHead className="text-xs font-semibold min-w-[110px]">
											IRR (%)
										</TableHead>
										<TableHead className="text-xs font-semibold min-w-[140px]">
											NPV (IDR)
										</TableHead>
										<TableHead className="text-xs font-semibold min-w-[120px]">
											Payback (Thn)
										</TableHead>
										<TableHead className="text-xs font-semibold min-w-[130px]">
											Kesimpulan
										</TableHead>
										{canEdit && (
											<TableHead className="text-xs font-semibold w-12 text-center">
												Aksi
											</TableHead>
										)}
									</TableRow>
								</TableHeader>
								<TableBody>
									{scenarios.length === 0 ? (
										<TableRow>
											<TableCell
												colSpan={canEdit ? 6 : 5}
												className="h-20 text-center text-xs text-muted-foreground"
											>
												Belum ada skenario kelayakan. Klik "+ Tambah Skenario"
												untuk menambahkan.
											</TableCell>
										</TableRow>
									) : (
										scenarios.map((row, idx) => (
											// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
											<TableRow key={idx}>
												<TableCell>
													<Input
														value={row.scenarioName ?? ""}
														onChange={(e) => {
															const next = [...scenarios];
															next[idx].scenarioName =
																e.target.value || undefined;
															setScenarios(next);
														}}
														placeholder="Skenario Base"
														disabled={!canEdit}
														className="text-xs h-8 font-medium"
													/>
												</TableCell>
												<TableCell>
													<Input
														type="number"
														step="0.01"
														value={row.irrPct ?? ""}
														onChange={(e) => {
															const next = [...scenarios];
															next[idx].irrPct = e.target.value
																? Number(e.target.value)
																: undefined;
															setScenarios(next);
														}}
														placeholder="15.5"
														disabled={!canEdit}
														className="text-xs h-8 font-mono text-emerald-600 font-semibold"
													/>
												</TableCell>
												<TableCell>
													<Input
														type="number"
														value={row.npvIdr ?? ""}
														onChange={(e) => {
															const next = [...scenarios];
															next[idx].npvIdr = e.target.value
																? Number(e.target.value)
																: undefined;
															setScenarios(next);
														}}
														placeholder="500000000"
														disabled={!canEdit}
														className="text-xs h-8 font-mono"
													/>
												</TableCell>
												<TableCell>
													<Input
														type="number"
														step="0.1"
														value={row.paybackPeriodYears ?? ""}
														onChange={(e) => {
															const next = [...scenarios];
															next[idx].paybackPeriodYears = e.target.value
																? Number(e.target.value)
																: undefined;
															setScenarios(next);
														}}
														placeholder="3.5"
														disabled={!canEdit}
														className="text-xs h-8 font-mono"
													/>
												</TableCell>
												<TableCell>
													<Input
														value={row.kesimpulan ?? ""}
														onChange={(e) => {
															const next = [...scenarios];
															next[idx].kesimpulan =
																e.target.value || undefined;
															setScenarios(next);
														}}
														placeholder="Layak / Rekomendasi"
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
										))
									)}
								</TableBody>
							</Table>
						</div>
					</CardContent>
				</Card>

				{/* Resume Catatan */}
				<div className="space-y-1.5">
					<Label className="text-xs font-medium">
						Catatan / Resume Evaluator
					</Label>
					<Textarea
						placeholder="Kesimpulan teknis & komersial tim penelaah..."
						value={keteranganResume}
						onChange={(e) => setKeteranganResume(e.target.value)}
						disabled={!canEdit}
						className="text-xs min-h-[60px]"
					/>
				</div>

				{canEdit && (
					<div className="flex justify-end pt-2">
						<Button
							type="submit"
							disabled={saveMutation.isPending}
							className="h-9 text-xs flex items-center gap-1.5 bg-cyan-600 hover:bg-cyan-700 text-white"
						>
							{saveMutation.isPending ? (
								<Loader2 className="size-3.5 animate-spin" />
							) : (
								<Save className="size-3.5" />
							)}
							Simpan Evaluasi NOL
						</Button>
					</div>
				)}
			</form>

			{/* REGIONAL ADMIN: CHOOSE REVIEWERS DIALOG MODAL */}
			<Dialog
				open={isReviewerDialogOpen}
				onOpenChange={setIsReviewerDialogOpen}
			>
				<DialogContent className="sm:max-w-md">
					<DialogHeader>
						<DialogTitle className="text-base flex items-center gap-2">
							<Users className="size-4 text-indigo-600" />
							Tetapkan Tim Reviewer Evaluasi NOL
						</DialogTitle>
						<DialogDescription className="text-xs">
							Pilih satu atau lebih pejabat/ahli teknis & komersial untuk
							menelaah berkas permohonan ini.
						</DialogDescription>
					</DialogHeader>

					<div className="space-y-2 max-h-72 overflow-y-auto py-2">
						{reviewerCandidates?.map((u) => {
							const isChecked = selectedReviewerIds.includes(u.id);
							return (
								<button
									key={u.id}
									type="button"
									onClick={() => toggleReviewerSelection(u.id)}
									className={`w-full flex items-center justify-between p-3 rounded-lg border text-left cursor-pointer transition-colors ${
										isChecked
											? "bg-indigo-50 border-indigo-300 dark:bg-indigo-950/40 dark:border-indigo-800"
											: "hover:bg-muted/40"
									}`}
								>
									<div className="flex items-center space-x-3">
										<Checkbox
											checked={isChecked}
											onCheckedChange={() => toggleReviewerSelection(u.id)}
											className="pointer-events-none"
										/>
										<div>
											<p className="text-xs font-semibold">{u.fullName}</p>
											<p className="text-[11px] text-muted-foreground">
												{u.email}
											</p>
										</div>
									</div>
									<Badge variant="outline" className="text-[10px]">
										{u.role}
									</Badge>
								</button>
							);
						})}
					</div>

					<DialogFooter className="flex items-center justify-end gap-2 pt-2">
						<Button
							variant="outline"
							size="sm"
							onClick={() => setIsReviewerDialogOpen(false)}
							className="text-xs"
						>
							Batal
						</Button>
						<Button
							size="sm"
							disabled={
								chooseReviewersMutation.isPending ||
								selectedReviewerIds.length === 0
							}
							onClick={handleConfirmReviewers}
							className="text-xs bg-indigo-600 hover:bg-indigo-700 text-white flex items-center gap-1.5"
						>
							{chooseReviewersMutation.isPending && (
								<Loader2 className="size-3 animate-spin" />
							)}
							Tetapkan ({selectedReviewerIds.length}) Reviewer
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</>
	);
}
