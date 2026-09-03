import { useForm } from "@tanstack/react-form";
import { useQueryClient } from "@tanstack/react-query";
import { createFileRoute, Link } from "@tanstack/react-router";
import {
	Activity,
	AlertCircle,
	ArrowLeft,
	Building2,
	DollarSign,
	Edit2,
	Flame,
	Loader2,
	MapPin,
	Paperclip,
	Plus,
	Save,
	Star,
	Trash2,
	UserCheck,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	AttachmentKind,
	ContactDetail,
	Kawasan,
	PosisiPelanggan,
	SaveContactRequest,
	SavePlottingRequest,
} from "@/api/types";
import { AttachmentList } from "@/components/attachments/attachment-list";
import { AttachmentUploadDialog } from "@/components/attachments/attachment-upload-dialog";
import { IconButton } from "@/components/common";
import { DocumentDownloadDropdown } from "@/components/documents/document-download-buttons";
import { FormField } from "@/components/form/form-field";
import { A1RegistrationForm } from "@/components/stages/a1-registration-form";
import { NolEvaluationForm } from "@/components/stages/nol-evaluation-form";
import { NolIssuanceForm } from "@/components/stages/nol-issuance-form";
import { NolRequestForm } from "@/components/stages/nol-request-form";
import { StageStepper } from "@/components/stages/stage-stepper";
import { SurveyKk0Form } from "@/components/stages/survey-kk0-form";
import { WorkflowActionBar } from "@/components/stages/workflow-action-bar";
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
import { Combobox } from "@/components/ui/combobox";
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
	Map,
	MapControls,
	type MapCoordinates,
	MapMarker,
	MarkerContent,
} from "@/components/ui/map";
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { getStageInfo, getStatusLabel } from "@/lib/directory-utils";
import {
	type SaveContactFormValues,
	type SavePlottingFormValues,
	saveContactSchema,
	savePlottingSchema,
} from "@/lib/schemas";

export const Route = createFileRoute("/_auth/directory/$companyId")({
	component: CompanyRecordHubPage,
});

function CompanyRecordHubPage() {
	const { companyId } = Route.useParams();
	const queryClient = useQueryClient();

	// Active tab
	const [activeTab, setActiveTab] = React.useState("overview");

	// 1. Fetch Company Master Record
	const {
		data: company,
		isLoading: loadingCompany,
		error: companyError,
	} = $api.useQuery("get", "/api/companies/{id}", {
		params: { path: { id: companyId } },
	});

	// 2. Fetch Contacts
	const { data: contacts, isLoading: loadingContacts } = $api.useQuery(
		"get",
		"/api/companies/{id}/contacts",
		{
			params: { path: { id: companyId } },
		},
	);

	// 3. Fetch Plotting Detail
	const { data: plotting } = $api.useQuery(
		"get",
		"/api/companies/{id}/plotting",
		{
			params: { path: { id: companyId } },
		},
	);

	// 4. Fetch Master Data for dropdowns
	const { data: salesUsers } = $api.useQuery("get", "/api/master/sales-users");

	// 5. Fetch Timeline
	const { data: timeline } = $api.useQuery(
		"get",
		"/api/companies/{id}/timeline",
		{
			params: { path: { id: companyId } },
		},
	);

	// 6. Fetch Stage 4 - 8 Details
	const { data: surveyData } = $api.useQuery(
		"get",
		"/api/companies/{id}/survey",
		{
			params: { path: { id: companyId } },
		},
	);

	const { data: registrationData } = $api.useQuery(
		"get",
		"/api/companies/{id}/registration",
		{
			params: { path: { id: companyId } },
		},
	);

	const { data: nolRequestData } = $api.useQuery(
		"get",
		"/api/companies/{id}/nol-request",
		{
			params: { path: { id: companyId } },
		},
	);

	const { data: nolEvaluationData } = $api.useQuery(
		"get",
		"/api/companies/{id}/nol-evaluation",
		{
			params: { path: { id: companyId } },
		},
	);

	const { data: nolIssuanceData } = $api.useQuery(
		"get",
		"/api/companies/{id}/nol-issuance",
		{
			params: { path: { id: companyId } },
		},
	);

	// 7. Fetch Attachments
	const { data: attachments = [] } = $api.useQuery(
		"get",
		"/api/companies/{companyId}/attachments",
		{
			params: { path: { companyId } },
		},
	);

	// State for Upload Attachment Modal
	const [uploadDialogOpen, setUploadDialogOpen] = React.useState(false);
	const [uploadDialogKind, setUploadDialogKind] =
		React.useState<AttachmentKind>("Other");

	// State for Contact Modal (Add / Edit)
	const [contactModalOpen, setContactModalOpen] = React.useState(false);
	const [editingContact, setEditingContact] =
		React.useState<ContactDetail | null>(null);

	// State for Coordinates Adjuster
	const [currentCoords, setCurrentCoords] =
		React.useState<MapCoordinates | null>(null);
	const [hasChangedCoords, setHasChangedCoords] = React.useState(false);

	// Sync Coordinates when company loaded
	React.useEffect(() => {
		if (company && company.latitude != null && company.longitude != null) {
			setCurrentCoords({
				latitude: Number(company.latitude),
				longitude: Number(company.longitude),
			});
			setHasChangedCoords(false);
		}
	}, [company]);

	// Contact Mutations
	const addContactMutation = $api.useMutation(
		"post",
		"/api/companies/{id}/contacts",
		{
			onSuccess: () => {
				toast.success("Kontak berhasil ditambahkan");
				setContactModalOpen(false);
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}/contacts",
						{ params: { path: { id: companyId } } },
					],
				});
			},
			onError: (err) => {
				toast.error("Gagal menambah kontak", { description: err.detail });
			},
		},
	);

	const updateContactMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/contacts/{contactId}",
		{
			onSuccess: () => {
				toast.success("Kontak berhasil diperbarui");
				setContactModalOpen(false);
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}/contacts",
						{ params: { path: { id: companyId } } },
					],
				});
			},
			onError: (err) => {
				toast.error("Gagal mengubah kontak", { description: err.detail });
			},
		},
	);

	const deleteContactMutation = $api.useMutation(
		"delete",
		"/api/companies/{id}/contacts/{contactId}",
		{
			onSuccess: () => {
				toast.success("Kontak berhasil dihapus");
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}/contacts",
						{ params: { path: { id: companyId } } },
					],
				});
			},
			onError: (err) => {
				toast.error("Gagal menghapus kontak", { description: err.detail });
			},
		},
	);

	// Plotting Mutations
	const savePlottingMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/plotting",
		{
			onSuccess: () => {
				toast.success("Konfigurasi Plotting Berhasil Disimpan");
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
						"/api/companies/{id}/plotting",
						{ params: { path: { id: companyId } } },
					],
				});
			},
			onError: (err) => {
				toast.error("Gagal menyimpan plotting", { description: err.detail });
			},
		},
	);

	const updateLocationMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/location",
		{
			onSuccess: () => {
				toast.success("Titik Koordinat Lokasi Berhasil Diperbarui");
				setHasChangedCoords(false);
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}",
						{ params: { path: { id: companyId } } },
					],
				});
			},
			onError: (err) => {
				toast.error("Gagal memperbarui koordinat", {
					description: err.detail,
				});
			},
		},
	);

	const promoteToProspekMutation = $api.useMutation(
		"post",
		"/api/companies/{id}/promote-to-prospek",
		{
			onSuccess: () => {
				toast.success("Berhasil Dipromosikan ke Tahap 3 (Prospek)!");
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
						"/api/companies/{id}/timeline",
						{ params: { path: { id: companyId } } },
					],
				});
			},
			onError: (err) => {
				toast.error("Gagal mempromosikan ke prospek", {
					description: err.detail,
				});
			},
		},
	);

	// Plotting TanStack Form
	const plottingForm = useForm({
		defaultValues: {
			salesUserId: plotting?.salesUserId || "",
			posisiPelanggan: plotting?.posisiPelanggan || "",
			kawasan: plotting?.kawasan || "",
		} as SavePlottingFormValues,
		validators: {
			onSubmit: savePlottingSchema,
		},
		onSubmit: async ({ value }) => {
			const payload: SavePlottingRequest = {
				salesUserId: value.salesUserId,
				posisiPelanggan: (value.posisiPelanggan ||
					null) as PosisiPelanggan | null,
				kawasan: (value.kawasan || null) as Kawasan | null,
			};
			await savePlottingMutation.mutateAsync({
				params: { path: { id: companyId } },
				body: payload,
			});
		},
	});

	// Reset Plotting form when data loads
	React.useEffect(() => {
		if (plotting) {
			plottingForm.reset({
				salesUserId: plotting.salesUserId || "",
				posisiPelanggan: plotting.posisiPelanggan || "",
				kawasan: plotting.kawasan || "",
			});
		}
	}, [plotting, plottingForm]);

	// Contact TanStack Form
	const contactForm = useForm({
		defaultValues: {
			nama: "",
			jabatan: "",
			email: "",
			noHp: "",
			isPrimary: false,
		} as SaveContactFormValues,
		validators: {
			onSubmit: saveContactSchema,
		},
		onSubmit: async ({ value }) => {
			const payload: SaveContactRequest = {
				nama: value.nama.trim(),
				jabatan: value.jabatan?.trim() || "",
				email: value.email?.trim() || null,
				noHp: value.noHp?.trim() || null,
				linkedIn: null,
				instagram: null,
				facebook: null,
				isPrimary: Boolean(value.isPrimary),
			};

			if (editingContact) {
				await updateContactMutation.mutateAsync({
					params: { path: { id: companyId, contactId: editingContact.id } },
					body: payload,
				});
			} else {
				await addContactMutation.mutateAsync({
					params: { path: { id: companyId } },
					body: payload,
				});
			}
		},
	});

	// Handlers
	const handleOpenAddContact = () => {
		setEditingContact(null);
		contactForm.reset({
			nama: "",
			jabatan: "",
			email: "",
			noHp: "",
			isPrimary: false,
		});
		setContactModalOpen(true);
	};

	const handleOpenEditContact = (c: ContactDetail) => {
		setEditingContact(c);
		contactForm.reset({
			nama: c.nama,
			jabatan: c.jabatan || "",
			email: c.email || "",
			noHp: c.noHp || "",
			isPrimary: c.isPrimary,
		});
		setContactModalOpen(true);
	};

	const handleSaveLocation = () => {
		if (!currentCoords) return;
		updateLocationMutation.mutate({
			params: { path: { id: companyId } },
			body: {
				latitude: currentCoords.latitude,
				longitude: currentCoords.longitude,
			},
		});
	};

	if (loadingCompany) {
		return (
			<div className="flex flex-col items-center justify-center min-h-[400px] space-y-3">
				<Loader2 className="size-8 animate-spin text-primary" />
				<p className="text-xs text-muted-foreground">
					Memuat berkas pelanggan...
				</p>
			</div>
		);
	}

	if (companyError || !company) {
		return (
			<div className="max-w-md mx-auto my-12 text-center space-y-4">
				<AlertCircle className="size-12 text-destructive mx-auto" />
				<h2 className="text-lg font-bold">Berkas Pelanggan Tidak Ditemukan</h2>
				<p className="text-xs text-muted-foreground">
					Data perusahaan tidak tersedia atau Anda tidak memiliki hak akses pada
					lingkup area kerja ini.
				</p>
				<Button asChild size="sm">
					<Link to="/directory">Kembali ke Direktori</Link>
				</Button>
			</div>
		);
	}

	const stageInfo = getStageInfo(company.currentStage);
	const statusInfo = getStatusLabel(company.status);

	return (
		<div className="w-full space-y-6 pb-16">
			{/* Top Navigation & Breadcrumb */}
			<div className="flex items-center justify-between">
				<div className="flex items-center gap-2">
					<Button variant="ghost" size="sm" asChild className="h-8 px-2">
						<Link to="/directory" className="flex items-center gap-1.5 text-xs">
							<ArrowLeft className="size-3.5" /> Direktori
						</Link>
					</Button>
					<span className="text-muted-foreground text-xs">/</span>
					<span className="font-mono text-xs text-muted-foreground">
						{company.nomor}
					</span>
				</div>
				<div className="flex items-center gap-2">
					{/* Official PDF Document Downloads */}
					<DocumentDownloadDropdown companyId={company.id} />

					<Button
						variant="outline"
						size="sm"
						onClick={() => {
							setUploadDialogKind("Other");
							setUploadDialogOpen(true);
						}}
						className="h-8 text-xs flex items-center gap-1.5"
					>
						<Paperclip className="size-3.5" /> Unggah Berkas
					</Button>

					{company.currentStage === 2 && company.canAct && (
						<Button
							size="sm"
							onClick={() =>
								promoteToProspekMutation.mutate({
									params: { path: { id: company.id } },
								})
							}
							disabled={promoteToProspekMutation.isPending}
							className="h-8 text-xs bg-emerald-600 hover:bg-emerald-700 text-white flex items-center gap-1.5"
						>
							{promoteToProspekMutation.isPending ? (
								<Loader2 className="size-3.5 animate-spin" />
							) : (
								<UserCheck className="size-3.5" />
							)}
							Promosikan ke Tahap 3 (Prospek)
						</Button>
					)}
				</div>
			</div>

			{/* Workflow Action Bar (Gate / Decision Area) */}
			<WorkflowActionBar
				company={company}
				onActionSuccess={() => {
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
							"/api/companies/{id}/timeline",
							{ params: { path: { id: companyId } } },
						],
					});
				}}
			/>

			{/* Main Record Header Card */}
			<Card className="border-border/60 shadow-xs overflow-hidden">
				<div className="p-6">
					<div className="flex flex-col md:flex-row md:items-start justify-between gap-4">
						<div className="space-y-2">
							<div className="flex flex-wrap items-center gap-2">
								<Badge variant="outline" className="font-mono text-xs">
									{company.nomor}
								</Badge>
								<Badge
									variant="outline"
									className={`text-xs font-normal border ${statusInfo.badgeClass}`}
								>
									{statusInfo.label}
								</Badge>
								<Badge variant="secondary" className="text-xs">
									{company.areaName} ({company.regionName})
								</Badge>
								{company.industryTypeName && (
									<Badge variant="outline" className="text-xs">
										{company.industryTypeName}
									</Badge>
								)}
							</div>
							<h1 className="text-2xl font-bold tracking-tight text-foreground flex items-center gap-2">
								<Building2 className="size-6 text-primary shrink-0" />
								<span>{company.namaPerusahaan}</span>
							</h1>
							<p className="text-xs text-muted-foreground flex items-center gap-1.5">
								<MapPin className="size-3.5 shrink-0 text-muted-foreground" />
								<span>{company.alamat}</span>
							</p>
						</div>

						<div className="flex flex-col items-end gap-2 bg-muted/40 p-3 rounded-lg border text-right">
							<div className="text-[11px] text-muted-foreground font-medium">
								Tahapan Saat Ini
							</div>
							<div className="flex items-center gap-2">
								<span className="font-mono text-sm font-bold text-primary">
									Tahap {company.currentStage}:
								</span>
								<span className="text-sm font-semibold text-foreground">
									{stageInfo.shortName}
								</span>
							</div>
							<div className="text-[11px] text-muted-foreground">
								PIC:{" "}
								<strong className="text-foreground">
									{company.salesRepName || "Belum Ditugaskan"}
								</strong>
							</div>
						</div>
					</div>
				</div>

				{/* 8-Stage Visual Milestone Pipeline Stepper */}
				<div className="border-t bg-muted/20 px-4 sm:px-6 py-2">
					<StageStepper
						currentStage={Number(company.currentStage)}
						activeTab={activeTab}
						onSelectStage={(_stageNum, tabKey) => setActiveTab(tabKey)}
					/>
				</div>
			</Card>

			{/* 2-Column Responsive Layout: Left Tabs + Right Persistent Linimasa */}
			<div className="flex flex-col xl:flex-row gap-6 items-start w-full">
				{/* Left Column: Main Operational Stage Tabs */}
				<div className="flex-1 min-w-0 w-full space-y-4">
					<Tabs
						value={activeTab}
						onValueChange={setActiveTab}
						className="space-y-4"
					>
						<TabsList className="flex flex-wrap items-center gap-1.5 w-full group-data-[orientation=horizontal]/tabs:h-auto p-1.5 bg-muted/60 rounded-xl border border-border/50">
							<TabsTrigger
								value="overview"
								className="h-7 text-xs px-2.5 rounded-lg"
							>
								Ringkasan
							</TabsTrigger>
							<TabsTrigger
								value="contacts"
								className="h-7 text-xs px-2.5 rounded-lg"
							>
								Kontak ({contacts?.length || 0})
							</TabsTrigger>
							<TabsTrigger
								value="plotting"
								className="h-7 text-xs px-2.5 rounded-lg"
							>
								Plotting
							</TabsTrigger>
							<TabsTrigger
								value="survey"
								className="h-7 text-xs px-2.5 rounded-lg"
							>
								Survei KK0
							</TabsTrigger>
							<TabsTrigger
								value="registration"
								className="h-7 text-xs px-2.5 rounded-lg"
							>
								Registrasi A1
							</TabsTrigger>
							<TabsTrigger
								value="nol-req"
								className="h-7 text-xs px-2.5 rounded-lg"
							>
								Permohonan
							</TabsTrigger>
							<TabsTrigger
								value="nol-eval"
								className="h-7 text-xs px-2.5 rounded-lg"
							>
								Evaluasi
							</TabsTrigger>
							<TabsTrigger
								value="nol-issue"
								className="h-7 text-xs px-2.5 rounded-lg"
							>
								Penerbitan
							</TabsTrigger>
							<TabsTrigger
								value="attachments"
								className="h-7 text-xs px-2.5 rounded-lg flex items-center justify-center gap-1"
							>
								<span>Lampiran</span>
								{attachments.length > 0 && (
									<Badge
										variant="secondary"
										className="text-[10px] h-4 px-1 rounded-full font-mono"
									>
										{attachments.length}
									</Badge>
								)}
							</TabsTrigger>
						</TabsList>

						{/* TAB 1: RINGKASAN (OVERVIEW) */}
						<TabsContent value="overview" className="space-y-6 pt-4">
							{/* 4 Executive Commercial & Technical KPI Cards */}
							<div className="grid grid-cols-1 sm:grid-cols-2 2xl:grid-cols-4 gap-3">
								<Card className="border-border/60 shadow-xs">
									<CardContent className="p-4 space-y-1">
										<div className="flex items-center justify-between">
											<span className="text-xs font-medium text-muted-foreground">
												Est. Kebutuhan Gas
											</span>
											<Flame className="size-4 text-orange-500" />
										</div>
										<div className="text-base font-bold text-foreground truncate">
											{surveyData?.jumlahKebutuhanEnergi != null &&
											Number(surveyData.jumlahKebutuhanEnergi) > 0
												? `${Number(surveyData.jumlahKebutuhanEnergi).toLocaleString("id-ID")} MMBTU/bln`
												: "Belum Disurvei"}
										</div>
										<span className="text-[11px] text-muted-foreground block truncate">
											{surveyData?.rencanaPemanfaatanGas ||
												"Survei Teknis KK0 (Tahap 4)"}
										</span>
									</CardContent>
								</Card>

								<Card className="border-border/60 shadow-xs">
									<CardContent className="p-4 space-y-1">
										<div className="flex items-center justify-between">
											<span className="text-xs font-medium text-muted-foreground">
												Posisi & Kawasan
											</span>
											<MapPin className="size-4 text-primary" />
										</div>
										<div className="text-base font-bold text-foreground truncate">
											{plotting?.posisiPelanggan
												? plotting.posisiPelanggan === "JalurExisting"
													? "Jalur Existing"
													: "Pengembangan"
												: "Belum Ditetapkan"}
										</div>
										<span className="text-[11px] text-muted-foreground block truncate">
											{plotting?.kawasan
												? `Kawasan: ${plotting.kawasan}`
												: "Di luar kawasan industri"}
										</span>
									</CardContent>
								</Card>

								<Card className="border-border/60 shadow-xs">
									<CardContent className="p-4 space-y-1">
										<div className="flex items-center justify-between">
											<span className="text-xs font-medium text-muted-foreground">
												Indikasi Harga Gas
											</span>
											<DollarSign className="size-4 text-emerald-600" />
										</div>
										<div className="text-base font-bold text-foreground truncate">
											{registrationData?.hargaNilai != null
												? `${registrationData.hargaCurrency || "USD"} ${Number(registrationData.hargaNilai).toLocaleString("id-ID")}`
												: surveyData?.willingnessToPayUsdMmbtu != null
													? `USD ${Number(surveyData.willingnessToPayUsdMmbtu).toLocaleString("id-ID")} (WTP)`
													: "Belum Ditetapkan"}
										</div>
										<span className="text-[11px] text-muted-foreground block truncate">
											{registrationData?.skemaHarga
												? `Skema: ${registrationData.skemaHarga}`
												: "Menunggu Registrasi A1"}
										</span>
									</CardContent>
								</Card>

								<Card className="border-border/60 shadow-xs">
									<CardContent className="p-4 space-y-1">
										<div className="flex items-center justify-between">
											<span className="text-xs font-medium text-muted-foreground">
												Status Alur Kerja
											</span>
											<Activity className="size-4 text-primary" />
										</div>
										<div className="text-base font-bold text-foreground flex items-center gap-1.5 truncate">
											<Badge
												variant="outline"
												className={`text-[11px] font-normal border ${statusInfo.badgeClass}`}
											>
												{statusInfo.label}
											</Badge>
										</div>
										<span className="text-[11px] text-muted-foreground block truncate">
											Tahap {company.currentStage} · {stageInfo.shortName}
										</span>
									</CardContent>
								</Card>
							</div>

							{/* Profile Detail & Mini Map */}
							<div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
								{/* Left: Detail Data Perusahaan */}
								<Card className="border-border/60 shadow-xs">
									<CardHeader className="pb-3">
										<CardTitle className="text-sm font-semibold">
											Informasi Legal & Lokasi Administratif
										</CardTitle>
									</CardHeader>
									<CardContent className="space-y-3 text-xs">
										<div className="grid grid-cols-3 py-1 border-b">
											<span className="text-muted-foreground">
												Nomor Registrasi
											</span>
											<span className="col-span-2 font-mono font-semibold">
												{company.nomor}
											</span>
										</div>
										<div className="grid grid-cols-3 py-1 border-b">
											<span className="text-muted-foreground">
												NPWP Perusahaan
											</span>
											<span className="col-span-2 font-mono">
												{company.npwp || "-"}
											</span>
										</div>
										<div className="grid grid-cols-3 py-1 border-b">
											<span className="text-muted-foreground">
												Email Kontak
											</span>
											<span className="col-span-2">{company.email || "-"}</span>
										</div>
										<div className="grid grid-cols-3 py-1 border-b">
											<span className="text-muted-foreground">
												Nomor Telepon
											</span>
											<span className="col-span-2">{company.telp || "-"}</span>
										</div>
										<div className="grid grid-cols-3 py-1 border-b">
											<span className="text-muted-foreground">Website</span>
											<span className="col-span-2">
												{company.website ? (
													<a
														href={company.website}
														target="_blank"
														rel="noreferrer"
														className="text-primary hover:underline"
													>
														{company.website}
													</a>
												) : (
													"-"
												)}
											</span>
										</div>
										<div className="grid grid-cols-3 py-1 border-b">
											<span className="text-muted-foreground">
												Alamat Administratif
											</span>
											<span className="col-span-2">
												{company.locationLabel}
											</span>
										</div>
										<div className="grid grid-cols-3 py-1 border-b">
											<span className="text-muted-foreground">Kode Pos</span>
											<span className="col-span-2 font-mono">
												{company.kodePos || "-"}
											</span>
										</div>
										<div className="grid grid-cols-3 py-1">
											<span className="text-muted-foreground">
												Tanggal Daftar
											</span>
											<span className="col-span-2">
												{new Date(company.createdAt).toLocaleDateString(
													"id-ID",
													{
														dateStyle: "long",
													},
												)}
											</span>
										</div>
									</CardContent>
								</Card>

								{/* Right: Map Location Preview */}
								<Card className="border-border/60 shadow-xs">
									<CardHeader className="pb-3 flex flex-row items-center justify-between">
										<CardTitle className="text-sm font-semibold">
											Lokasi Geospasial Pelanggan
										</CardTitle>
										{currentCoords && (
											<Badge
												variant="outline"
												className="font-mono text-[10px]"
											>
												{currentCoords.latitude.toFixed(5)},{" "}
												{currentCoords.longitude.toFixed(5)}
											</Badge>
										)}
									</CardHeader>
									<CardContent>
										<div className="h-[260px] w-full rounded-md overflow-hidden border">
											{currentCoords ? (
												<Map
													center={[
														currentCoords.longitude,
														currentCoords.latitude,
													]}
													zoom={13}
													className="h-full w-full"
												>
													<MapControls />
													<MapMarker
														longitude={currentCoords.longitude}
														latitude={currentCoords.latitude}
													>
														<MarkerContent>
															<div className="size-6 rounded-full bg-primary flex items-center justify-center text-primary-foreground shadow-md border-2 border-white">
																<MapPin className="size-3.5" />
															</div>
														</MarkerContent>
													</MapMarker>
												</Map>
											) : (
												<div className="h-full flex items-center justify-center text-xs text-muted-foreground">
													Koordinat belum ditentukan
												</div>
											)}
										</div>
									</CardContent>
								</Card>
							</div>
						</TabsContent>

						{/* TAB 2: KONTAK PIC PELANGGAN */}
						<TabsContent value="contacts" className="space-y-4 pt-4">
							<Card className="border-border/60 shadow-xs">
								<CardHeader className="p-4 flex flex-row items-center justify-between">
									<div>
										<CardTitle className="text-base font-semibold">
											Daftar Kontak PIC Pelanggan
										</CardTitle>
										<CardDescription className="text-xs">
											Informasi penanggung jawab teknis, komersial, dan
											manajemen calon pelanggan
										</CardDescription>
									</div>
									<Button
										size="sm"
										onClick={handleOpenAddContact}
										className="h-8 text-xs flex items-center gap-1.5"
									>
										<Plus className="size-3.5" /> Tambah Kontak
									</Button>
								</CardHeader>
								<CardContent className="p-0">
									<Table>
										<TableHeader>
											<TableRow className="bg-muted/30">
												<TableHead className="text-xs font-semibold">
													Nama
												</TableHead>
												<TableHead className="text-xs font-semibold">
													Jabatan
												</TableHead>
												<TableHead className="text-xs font-semibold">
													Email
												</TableHead>
												<TableHead className="text-xs font-semibold">
													Telepon
												</TableHead>
												<TableHead className="text-xs font-semibold text-center">
													Kontak Utama
												</TableHead>
												<TableHead className="text-xs font-semibold text-right">
													Aksi
												</TableHead>
											</TableRow>
										</TableHeader>
										<TableBody>
											{loadingContacts ? (
												<TableRow>
													<TableCell
														colSpan={6}
														className="h-24 text-center text-xs text-muted-foreground"
													>
														Memuat kontak...
													</TableCell>
												</TableRow>
											) : contacts && contacts.length > 0 ? (
												contacts.map((c) => (
													<TableRow key={c.id}>
														<TableCell className="font-medium text-xs">
															{c.nama}
														</TableCell>
														<TableCell className="text-xs text-muted-foreground">
															{c.jabatan || "-"}
														</TableCell>
														<TableCell className="text-xs">
															{c.email || "-"}
														</TableCell>
														<TableCell className="text-xs">
															{c.noHp || "-"}
														</TableCell>
														<TableCell className="text-xs text-center">
															{c.isPrimary && (
																<Badge
																	variant="outline"
																	className="bg-amber-50 text-amber-700 border-amber-300 text-[10px]"
																>
																	<Star className="size-3 mr-1 fill-amber-500 text-amber-500" />{" "}
																	Utama
																</Badge>
															)}
														</TableCell>
														<TableCell className="text-right">
															<div className="flex items-center justify-end gap-1">
																<IconButton
																	tooltip="Ubah Kontak"
																	className="size-7"
																	onClick={() => handleOpenEditContact(c)}
																>
																	<Edit2 className="size-3.5" />
																</IconButton>
																<IconButton
																	tooltip="Hapus Kontak"
																	danger
																	className="size-7"
																	onClick={() =>
																		deleteContactMutation.mutate({
																			params: {
																				path: {
																					id: companyId,
																					contactId: c.id,
																				},
																			},
																		})
																	}
																>
																	<Trash2 className="size-3.5" />
																</IconButton>
															</div>
														</TableCell>
													</TableRow>
												))
											) : (
												<TableRow>
													<TableCell
														colSpan={6}
														className="h-24 text-center text-xs text-muted-foreground"
													>
														Belum ada kontak terdaftar. Silakan klik tombol
														Tambah Kontak.
													</TableCell>
												</TableRow>
											)}
										</TableBody>
									</Table>
								</CardContent>
							</Card>
						</TabsContent>

						{/* TAB 3: PLOTTING (STAGE 2) */}
						<TabsContent value="plotting" className="space-y-6 pt-4">
							<div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
								{/* Plotting Configuration Form */}
								<Card className="border-border/60 shadow-xs">
									<CardHeader className="pb-3">
										<CardTitle className="text-base font-semibold">
											Konfigurasi Plotting & Jalur Pipa
										</CardTitle>
										<CardDescription className="text-xs">
											Penetapan Sales Representative dan skema jalur pipa
											transmisi/distribusi
										</CardDescription>
									</CardHeader>
									<CardContent>
										<form
											onSubmit={(e) => {
												e.preventDefault();
												e.stopPropagation();
												plottingForm.handleSubmit();
											}}
											className="space-y-4"
										>
											{/* Sales Representative */}
											<div>
												<plottingForm.Field name="salesUserId">
													{(field) => {
														const error = field.state.meta.errors[0]?.message;
														return (
															<FormField
																label="Sales Representative Penanggung Jawab"
																required
																error={error}
															>
																<Combobox
																	id={field.name}
																	value={field.state.value || ""}
																	onValueChange={(val) =>
																		field.handleChange(val)
																	}
																	options={[
																		{ value: "", label: "Belum Ditugaskan" },
																		...(salesUsers?.map((u) => ({
																			value: u.id,
																			label: `${u.fullName} (${u.username})`,
																		})) || []),
																	]}
																	placeholder="Pilih Sales Representative"
																	searchPlaceholder="Cari sales representative..."
																	emptyText="Sales representative tidak ditemukan."
																	aria-label="Pilih Sales Representative"
																/>
															</FormField>
														);
													}}
												</plottingForm.Field>
											</div>

											{/* Posisi Pelanggan */}
											<div>
												<plottingForm.Field name="posisiPelanggan">
													{(field) => {
														const error = field.state.meta.errors[0]?.message;
														return (
															<FormField
																label="Posisi Pelanggan Terhadap Jalur Pipa"
																error={error}
															>
																<Select
																	value={field.state.value || "NONE"}
																	onValueChange={(val) =>
																		field.handleChange(
																			val === "NONE" ? "" : val,
																		)
																	}
																>
																	<SelectTrigger className="text-xs h-9">
																		<SelectValue placeholder="Pilih Posisi Pelanggan" />
																	</SelectTrigger>
																	<SelectContent>
																		<SelectItem value="NONE">
																			Belum Ditetapkan
																		</SelectItem>
																		<SelectItem value="JalurExisting">
																			Jalur Existing (Dekat Pipa Eksisting)
																		</SelectItem>
																		<SelectItem value="Pengembangan">
																			Pengembangan (Perlu Jaringan Baru)
																		</SelectItem>
																	</SelectContent>
																</Select>
															</FormField>
														);
													}}
												</plottingForm.Field>
											</div>

											{/* Kawasan */}
											<div>
												<plottingForm.Field name="kawasan">
													{(field) => {
														const error = field.state.meta.errors[0]?.message;
														return (
															<FormField
																label="Klasifikasi Kawasan"
																error={error}
															>
																<Select
																	value={field.state.value || "NONE"}
																	onValueChange={(val) =>
																		field.handleChange(
																			val === "NONE" ? "" : val,
																		)
																	}
																>
																	<SelectTrigger className="text-xs h-9">
																		<SelectValue placeholder="Pilih Klasifikasi Kawasan" />
																	</SelectTrigger>
																	<SelectContent>
																		<SelectItem value="NONE">
																			Belum Ditetapkan
																		</SelectItem>
																		<SelectItem value="KawasanIndustri">
																			Kawasan Industri
																		</SelectItem>
																		<SelectItem value="NonKawasanIndustri">
																			Non Kawasan Industri
																		</SelectItem>
																	</SelectContent>
																</Select>
															</FormField>
														);
													}}
												</plottingForm.Field>
											</div>

											<plottingForm.Subscribe
												selector={(state) => [
													state.canSubmit,
													state.isSubmitting,
												]}
											>
												{([canSubmit, isSubmitting]) => (
													<Button
														type="submit"
														disabled={!canSubmit || isSubmitting}
														className="text-xs h-9 w-full flex items-center justify-center gap-1.5 mt-2"
													>
														{isSubmitting ? (
															<Loader2 className="size-4 animate-spin" />
														) : (
															<Save className="size-4" />
														)}
														Simpan Konfigurasi Plotting
													</Button>
												)}
											</plottingForm.Subscribe>
										</form>
									</CardContent>
								</Card>

								{/* Coordinate Adjustment Card */}
								<Card className="border-border/60 shadow-xs">
									<CardHeader className="pb-3">
										<div className="flex items-center justify-between">
											<div>
												<CardTitle className="text-base font-semibold">
													Penyesuaian Koordinat Spasial
												</CardTitle>
												<CardDescription className="text-xs">
													Geser pin penanda pada peta untuk memperbarui
													koordinat
												</CardDescription>
											</div>
											{currentCoords && (
												<Badge variant="outline" className="font-mono text-xs">
													{currentCoords.latitude.toFixed(6)},{" "}
													{currentCoords.longitude.toFixed(6)}
												</Badge>
											)}
										</div>
									</CardHeader>
									<CardContent className="space-y-3">
										{currentCoords ? (
											<div className="h-[240px] w-full rounded-md overflow-hidden border">
												<Map
													center={[
														currentCoords.longitude,
														currentCoords.latitude,
													]}
													zoom={14}
													className="h-full w-full cursor-crosshair"
													onClick={(e) => {
														setCurrentCoords({
															longitude: Number(e.lngLat.lng.toFixed(6)),
															latitude: Number(e.lngLat.lat.toFixed(6)),
														});
														setHasChangedCoords(true);
													}}
												>
													<MapControls />
													<MapMarker
														longitude={currentCoords.longitude}
														latitude={currentCoords.latitude}
														draggable={true}
														onDragEnd={(coords) => {
															setCurrentCoords({
																longitude: coords.lng,
																latitude: coords.lat,
															});
															setHasChangedCoords(true);
														}}
													>
														<MarkerContent>
															<div className="size-6 rounded-full bg-primary flex items-center justify-center text-primary-foreground shadow-md border-2 border-white ring-2 ring-primary/40 cursor-grab active:cursor-grabbing">
																<MapPin className="size-3.5" />
															</div>
														</MarkerContent>
													</MapMarker>
												</Map>
											</div>
										) : null}

										<div className="flex items-center justify-end">
											<Button
												size="sm"
												onClick={handleSaveLocation}
												disabled={
													!hasChangedCoords || updateLocationMutation.isPending
												}
												className="h-8 text-xs flex items-center gap-1.5"
											>
												{updateLocationMutation.isPending ? (
													<Loader2 className="size-3.5 animate-spin" />
												) : (
													<MapPin className="size-3.5" />
												)}
												Perbarui Koordinat Lokasi
											</Button>
										</div>
									</CardContent>
								</Card>
							</div>
						</TabsContent>

						{/* TAB 4: SURVEI KK0 (STAGE 4) */}
						<TabsContent value="survey" className="pt-4">
							<SurveyKk0Form
								companyId={company.id}
								initialData={surveyData}
								onSaved={() => {
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}/survey",
											{ params: { path: { id: company.id } } },
										],
									});
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}",
											{ params: { path: { id: company.id } } },
										],
									});
								}}
							/>
						</TabsContent>

						{/* TAB 5: REGISTRASI A1 (STAGE 5) */}
						<TabsContent value="registration" className="pt-4">
							<A1RegistrationForm
								companyId={company.id}
								initialData={registrationData}
								onSaved={() => {
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}/registration",
											{ params: { path: { id: company.id } } },
										],
									});
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}",
											{ params: { path: { id: company.id } } },
										],
									});
								}}
							/>
						</TabsContent>

						{/* TAB 6: PERMOHONAN NOL (STAGE 6) */}
						<TabsContent value="nol-req" className="pt-4">
							<NolRequestForm
								companyId={company.id}
								initialData={nolRequestData}
								onSaved={() => {
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}/nol-request",
											{ params: { path: { id: company.id } } },
										],
									});
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}",
											{ params: { path: { id: company.id } } },
										],
									});
								}}
							/>
						</TabsContent>

						{/* TAB 7: EVALUASI NOL (STAGE 7) */}
						<TabsContent value="nol-eval" className="pt-4">
							<NolEvaluationForm
								companyId={company.id}
								initialData={nolEvaluationData}
								onSaved={() => {
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}/nol-evaluation",
											{ params: { path: { id: company.id } } },
										],
									});
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}",
											{ params: { path: { id: company.id } } },
										],
									});
								}}
							/>
						</TabsContent>

						{/* TAB 8: PENERBITAN NOL (STAGE 8) */}
						<TabsContent value="nol-issue" className="pt-4">
							<NolIssuanceForm
								companyId={company.id}
								initialData={nolIssuanceData}
								onSaved={() => {
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}/nol-issuance",
											{ params: { path: { id: company.id } } },
										],
									});
									queryClient.invalidateQueries({
										queryKey: [
											"get",
											"/api/companies/{id}",
											{ params: { path: { id: company.id } } },
										],
									});
								}}
							/>
						</TabsContent>

						{/* TAB 9: LAMPIRAN & BERKAS (ATTACHMENTS) */}
						<TabsContent value="attachments" className="pt-4 space-y-4">
							<div className="flex items-center justify-between">
								<div>
									<h3 className="text-base font-semibold">
										Berkas & Lampiran Resmi Pelanggan
									</h3>
									<p className="text-xs text-muted-foreground">
										Dokumen pendukung legalitas, formulir bertanda tangan
										basah/digital, dan file teknis
									</p>
								</div>
								<Button
									size="sm"
									onClick={() => {
										setUploadDialogKind("Other");
										setUploadDialogOpen(true);
									}}
									className="h-8 text-xs flex items-center gap-1.5"
								>
									<Plus className="size-3.5" /> Tambah Berkas
								</Button>
							</div>

							<AttachmentList companyId={company.id} />
						</TabsContent>
					</Tabs>
				</div>

				{/* Right: Persistent Linimasa (Audit Trail & Activity Stream) */}
				<aside className="w-full xl:w-80 2xl:w-96 shrink-0 xl:sticky xl:top-4 space-y-4">
					<Card className="border-border/60 shadow-xs">
						<CardHeader className="p-4 pb-3 border-b bg-muted/20">
							<div className="flex items-center justify-between">
								<CardTitle className="text-sm font-semibold">
									Linimasa Berkas
								</CardTitle>
								{timeline && (
									<Badge variant="secondary" className="text-[10px] font-mono">
										{timeline.length} Aktivitas
									</Badge>
								)}
							</div>
							<CardDescription className="text-xs mt-1">
								Jejak audit alur kerja & riwayat transisi status
							</CardDescription>
						</CardHeader>
						<CardContent className="p-4">
							{timeline && timeline.length > 0 ? (
								<div className="space-y-4 max-h-[calc(100vh-14rem)] overflow-y-auto pr-1 relative before:absolute before:inset-0 before:left-2 before:w-0.5 before:bg-border">
									{timeline.map((entry) => (
										<div
											key={entry.id}
											className="flex items-start gap-3 relative pl-6 text-xs"
										>
											<div className="absolute left-0.5 top-1.5 size-3 rounded-full bg-primary border-2 border-background" />
											<div className="space-y-1 bg-muted/30 hover:bg-muted/50 transition-colors p-2.5 rounded-lg border border-border/60 flex-1">
												<div className="flex items-start justify-between gap-1">
													<span className="font-semibold text-foreground leading-tight">
														{entry.action}
													</span>
													<span className="text-[10px] text-muted-foreground font-mono shrink-0 whitespace-nowrap">
														{new Date(entry.occurredAt).toLocaleDateString(
															"id-ID",
															{
																day: "numeric",
																month: "short",
																hour: "2-digit",
																minute: "2-digit",
															},
														)}
													</span>
												</div>
												<div className="text-[11px] text-muted-foreground">
													<span>{entry.actorName}</span>
													{entry.roleLabel && (
														<span className="opacity-80">
															{" "}
															({entry.roleLabel})
														</span>
													)}
												</div>
												<div className="text-[11px] text-muted-foreground pt-0.5 flex items-center gap-1.5">
													<span>Status:</span>
													<Badge
														variant="outline"
														className="text-[10px] py-0 h-4 px-1.5 font-mono bg-background"
													>
														{entry.toStatus}
													</Badge>
												</div>
												{entry.comment && (
													<p className="text-[11px] text-muted-foreground/90 pt-1 italic border-t border-border/50 mt-1">
														"{entry.comment}"
													</p>
												)}
											</div>
										</div>
									))}
								</div>
							) : (
								<div className="text-center py-8 text-xs text-muted-foreground">
									Belum ada riwayat aktivitas pada berkas ini.
								</div>
							)}
						</CardContent>
					</Card>
				</aside>
			</div>

			{/* Contact Modal (Add / Edit) */}
			<Dialog open={contactModalOpen} onOpenChange={setContactModalOpen}>
				<DialogContent className="sm:max-w-[440px]">
					<form
						onSubmit={(e) => {
							e.preventDefault();
							e.stopPropagation();
							contactForm.handleSubmit();
						}}
					>
						<DialogHeader>
							<DialogTitle className="text-base font-semibold">
								{editingContact ? "Ubah Kontak PIC" : "Tambah Kontak PIC"}
							</DialogTitle>
							<DialogDescription className="text-xs">
								{editingContact
									? "Perbarui informasi kontak penanggung jawab perusahaan"
									: "Masukkan informasi kontak penanggung jawab calon pelanggan"}
							</DialogDescription>
						</DialogHeader>
						<div className="space-y-3 py-4 text-xs">
							<div>
								<contactForm.Field name="nama">
									{(field) => {
										const error = field.state.meta.errors[0]?.message;
										return (
											<FormField label="Nama Lengkap" required error={error}>
												<Input
													id={field.name}
													name={field.name}
													placeholder="Contoh: Budi Santoso"
													value={field.state.value}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</contactForm.Field>
							</div>

							<div>
								<contactForm.Field name="jabatan">
									{(field) => {
										const error = field.state.meta.errors[0]?.message;
										return (
											<FormField label="Jabatan / Posisi" error={error}>
												<Input
													id={field.name}
													name={field.name}
													placeholder="Contoh: Manajer Operasional / Purchasing"
													value={field.state.value || ""}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</contactForm.Field>
							</div>

							<div>
								<contactForm.Field name="email">
									{(field) => {
										const error = field.state.meta.errors[0]?.message;
										return (
											<FormField label="Email" error={error}>
												<Input
													id={field.name}
													name={field.name}
													type="email"
													placeholder="budi@perusahaan.co.id"
													value={field.state.value || ""}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</contactForm.Field>
							</div>

							<div>
								<contactForm.Field name="noHp">
									{(field) => {
										const error = field.state.meta.errors[0]?.message;
										return (
											<FormField label="Nomor Telepon / HP" error={error}>
												<Input
													id={field.name}
													name={field.name}
													placeholder="0812-3456-7890"
													value={field.state.value || ""}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													className="text-xs h-9"
												/>
											</FormField>
										);
									}}
								</contactForm.Field>
							</div>

							<div className="pt-2">
								<contactForm.Field name="isPrimary">
									{(field) => (
										<div className="flex items-center gap-2">
											<Checkbox
												id="isPrimary"
												name={field.name}
												checked={Boolean(field.state.value)}
												onCheckedChange={(checked) =>
													field.handleChange(Boolean(checked))
												}
											/>
											<Label
												htmlFor="isPrimary"
												className="text-xs font-medium cursor-pointer"
											>
												Jadikan sebagai Kontak Utama (Primary Contact)
											</Label>
										</div>
									)}
								</contactForm.Field>
							</div>
						</div>
						<DialogFooter>
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => setContactModalOpen(false)}
								className="text-xs h-8"
							>
								Batal
							</Button>
							<contactForm.Subscribe
								selector={(state) => [state.canSubmit, state.isSubmitting]}
							>
								{([canSubmit, isSubmitting]) => (
									<Button
										type="submit"
										size="sm"
										disabled={!canSubmit || isSubmitting}
										className="text-xs h-8"
									>
										{isSubmitting ? (
											<Loader2 className="size-3.5 animate-spin mr-1" />
										) : null}
										Simpan Kontak
									</Button>
								)}
							</contactForm.Subscribe>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>

			{/* Upload Attachment Dialog */}
			<AttachmentUploadDialog
				companyId={company.id}
				isOpen={uploadDialogOpen}
				defaultKind={uploadDialogKind}
				onClose={() => setUploadDialogOpen(false)}
				onSuccess={() => {
					queryClient.invalidateQueries({
						queryKey: [
							"get",
							"/api/companies/{companyId}/attachments",
							{ params: { path: { companyId: company.id } } },
						],
					});
				}}
			/>
		</div>
	);
}
