import { useQueryClient } from "@tanstack/react-query";
import { createFileRoute, Link } from "@tanstack/react-router";
import {
	AlertCircle,
	ArrowLeft,
	Building2,
	CheckCircle2,
	Clock,
	Edit2,
	Layers,
	Loader2,
	MapPin,
	Plus,
	Save,
	Star,
	Trash2,
	UserCheck,
	Users,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	ContactDetail,
	Kawasan,
	PosisiPelanggan,
	SaveContactRequest,
	SavePlottingRequest,
} from "@/api/types";
import { Map, type MapCoordinates } from "@/components/map";
import { A1RegistrationForm } from "@/components/stages/a1-registration-form";
import { NolEvaluationForm } from "@/components/stages/nol-evaluation-form";
import { NolIssuanceForm } from "@/components/stages/nol-issuance-form";
import { NolRequestForm } from "@/components/stages/nol-request-form";
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
	getStageInfo,
	getStatusLabel,
	STAGE_CONFIG,
} from "@/lib/directory-utils";

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

	// State for Contact Modal (Add / Edit)
	const [contactModalOpen, setContactModalOpen] = React.useState(false);
	const [editingContact, setEditingContact] =
		React.useState<ContactDetail | null>(null);
	const [contactNama, setContactNama] = React.useState("");
	const [contactJabatan, setContactJabatan] = React.useState("");
	const [contactEmail, setContactEmail] = React.useState("");
	const [contactTelp, setContactTelp] = React.useState("");
	const [contactIsPrimary, setContactIsPrimary] = React.useState(false);

	// State for Plotting Form
	const [selectedSalesUserId, setSelectedSalesUserId] = React.useState("");
	const [selectedPosisi, setSelectedPosisi] = React.useState<
		PosisiPelanggan | ""
	>("");
	const [selectedKawasan, setSelectedKawasan] = React.useState<Kawasan | "">(
		"",
	);

	// State for Coordinates Adjuster
	const [currentCoords, setCurrentCoords] =
		React.useState<MapCoordinates | null>(null);
	const [hasChangedCoords, setHasChangedCoords] = React.useState(false);

	// Sync Plotting State when data loaded
	React.useEffect(() => {
		if (plotting) {
			setSelectedSalesUserId(plotting.salesUserId || "");
			setSelectedPosisi(plotting.posisiPelanggan || "");
			setSelectedKawasan(plotting.kawasan || "");
		}
	}, [plotting]);

	// Sync Coordinates when company loaded
	React.useEffect(() => {
		if (company && company.latitude != null && company.longitude != null) {
			setCurrentCoords({
				latitude: company.latitude,
				longitude: company.longitude,
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

	// Handlers
	const handleOpenAddContact = () => {
		setEditingContact(null);
		setContactNama("");
		setContactJabatan("");
		setContactEmail("");
		setContactTelp("");
		setContactIsPrimary(false);
		setContactModalOpen(true);
	};

	const handleOpenEditContact = (c: ContactDetail) => {
		setEditingContact(c);
		setContactNama(c.nama);
		setContactJabatan(c.jabatan || "");
		setContactEmail(c.email || "");
		setContactTelp(c.telp || "");
		setContactIsPrimary(c.isPrimary);
		setContactModalOpen(true);
	};

	const handleSaveContact = (e: React.FormEvent) => {
		e.preventDefault();
		if (!contactNama.trim()) {
			toast.error("Nama kontak wajib diisi");
			return;
		}

		const payload: SaveContactRequest = {
			nama: contactNama.trim(),
			jabatan: contactJabatan.trim() || undefined,
			email: contactEmail.trim() || undefined,
			telp: contactTelp.trim() || undefined,
			isPrimary: contactIsPrimary,
		};

		if (editingContact) {
			updateContactMutation.mutate({
				params: { path: { id: companyId, contactId: editingContact.id } },
				body: payload,
			});
		} else {
			addContactMutation.mutate({
				params: { path: { id: companyId } },
				body: payload,
			});
		}
	};

	const handleSavePlotting = (e: React.FormEvent) => {
		e.preventDefault();
		const payload: SavePlottingRequest = {
			salesUserId: selectedSalesUserId || undefined,
			posisiPelanggan: selectedPosisi ? selectedPosisi : undefined,
			kawasan: selectedKawasan ? selectedKawasan : undefined,
		};

		savePlottingMutation.mutate({
			params: { path: { id: companyId } },
			body: payload,
		});
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
		<div className="max-w-7xl mx-auto space-y-6 pb-16">
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
					{company.currentStage === 2 && (
						<Button
							size="sm"
							onClick={() =>
								promoteToProspekMutation.mutate({
									params: { path: { id: companyId } },
								})
							}
							disabled={promoteToProspekMutation.isPending}
							className="h-8 text-xs bg-emerald-600 hover:bg-emerald-700 text-white flex items-center gap-1.5"
						>
							<UserCheck className="size-3.5" /> Promosikan ke Prospek (Tahap 3)
						</Button>
					)}
				</div>
			</div>

			{/* Main Company Header Card */}
			<Card className="border-border/60 shadow-xs bg-card">
				<CardContent className="p-6">
					<div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6">
						{/* Left: Company Identity */}
						<div className="space-y-2">
							<div className="flex flex-wrap items-center gap-2">
								<Badge
									variant="outline"
									className="font-mono text-xs px-2.5 py-0.5 bg-muted/60"
								>
									{company.nomor}
								</Badge>
								<Badge
									variant="outline"
									className={`text-xs px-2.5 py-0.5 border ${stageInfo.badgeClass}`}
								>
									{stageInfo.name}
								</Badge>
								<Badge
									variant="outline"
									className={`text-xs px-2.5 py-0.5 border ${statusInfo.badgeClass}`}
								>
									{statusInfo.label}
								</Badge>
							</div>

							<h1 className="text-2xl font-bold tracking-tight text-foreground">
								{company.namaPerusahaan}
							</h1>

							<div className="flex flex-wrap items-center gap-y-1 gap-x-4 text-xs text-muted-foreground">
								<div className="flex items-center gap-1">
									<Building2 className="size-3.5 text-muted-foreground" />
									<span>{company.industryTypeName || "Sektor Industri"}</span>
								</div>
								<div className="flex items-center gap-1">
									<MapPin className="size-3.5 text-muted-foreground" />
									<span>{company.locationLabel || "Lokasi"}</span>
								</div>
								<div className="flex items-center gap-1">
									<Layers className="size-3.5 text-muted-foreground" />
									<span>
										{company.areaName} ({company.regionName})
									</span>
								</div>
								<div className="flex items-center gap-1">
									<Users className="size-3.5 text-muted-foreground" />
									<span>
										Sales PIC:{" "}
										<strong className="text-foreground">
											{company.salesUserName || "Belum Ditugaskan"}
										</strong>
									</span>
								</div>
							</div>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* Universal Workflow Action Bar */}
			<WorkflowActionBar company={company} />

			{/* Visual 8-Stage Progress Stepper */}
			<Card className="border-border/60 shadow-xs">
				<CardContent className="p-4">
					<div className="text-xs font-semibold text-muted-foreground mb-3 uppercase tracking-wider">
						Progres Tahapan Berlangganan Gas
					</div>
					<div className="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-8 gap-2">
						{[1, 2, 3, 4, 5, 6, 7, 8].map((s) => {
							const stepInfo = STAGE_CONFIG[s];
							const isCompleted = company.currentStage > s;
							const isCurrent = company.currentStage === s;

							return (
								<div
									key={s}
									className={`flex flex-col items-center p-2.5 rounded-lg border text-center transition-all ${
										isCurrent
											? "border-primary bg-primary/5 shadow-xs font-semibold"
											: isCompleted
												? "border-emerald-200 bg-emerald-50/50 dark:bg-emerald-950/20 text-foreground"
												: "border-border/50 bg-muted/20 text-muted-foreground opacity-60"
									}`}
								>
									<div className="flex items-center justify-center size-6 rounded-full text-xs mb-1.5">
										{isCompleted ? (
											<CheckCircle2 className="size-5 text-emerald-600" />
										) : isCurrent ? (
											<span className="size-5 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold text-[11px]">
												{s}
											</span>
										) : (
											<span className="size-5 rounded-full bg-muted border flex items-center justify-center text-[11px]">
												{s}
											</span>
										)}
									</div>
									<span className="text-[11px] leading-tight line-clamp-2">
										{stepInfo.shortName}
									</span>
								</div>
							);
						})}
					</div>
				</CardContent>
			</Card>

			{/* 9-Tabbed Hub Navigation */}
			<Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
				<TabsList className="grid grid-cols-3 sm:grid-cols-5 lg:grid-cols-9 h-auto p-1 bg-muted/60 border rounded-lg">
					<TabsTrigger value="overview" className="text-xs py-2">
						Ringkasan
					</TabsTrigger>
					<TabsTrigger value="contacts" className="text-xs py-2">
						Kontak ({contacts?.length || 0})
					</TabsTrigger>
					<TabsTrigger value="plotting" className="text-xs py-2">
						Plotting
					</TabsTrigger>
					<TabsTrigger value="survey" className="text-xs py-2">
						Survei KK0
					</TabsTrigger>
					<TabsTrigger value="reg-a1" className="text-xs py-2">
						Registrasi A1
					</TabsTrigger>
					<TabsTrigger value="nol-req" className="text-xs py-2">
						Permohonan
					</TabsTrigger>
					<TabsTrigger value="nol-eval" className="text-xs py-2">
						Evaluasi
					</TabsTrigger>
					<TabsTrigger value="nol-issue" className="text-xs py-2">
						Penerbitan
					</TabsTrigger>
					<TabsTrigger value="timeline" className="text-xs py-2">
						Lini Masa
					</TabsTrigger>
				</TabsList>

				{/* TAB 1: RINGKASAN (OVERVIEW) */}
				<TabsContent value="overview" className="space-y-6 pt-4">
					{/* 4 Executive KPI Cards */}
					<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
						<Card className="border-border/60 shadow-xs">
							<CardContent className="p-4 space-y-1">
								<span className="text-xs text-muted-foreground">
									Sektor Industri
								</span>
								<div className="text-base font-bold text-foreground">
									{company.industryTypeName || "-"}
								</div>
								<span className="text-[11px] text-muted-foreground">
									Klasifikasi Usaha
								</span>
							</CardContent>
						</Card>

						<Card className="border-border/60 shadow-xs">
							<CardContent className="p-4 space-y-1">
								<span className="text-xs text-muted-foreground">
									Wilayah & Area
								</span>
								<div className="text-base font-bold text-foreground">
									{company.areaName}
								</div>
								<span className="text-[11px] text-muted-foreground">
									{company.regionName}
								</span>
							</CardContent>
						</Card>

						<Card className="border-border/60 shadow-xs">
							<CardContent className="p-4 space-y-1">
								<span className="text-xs text-muted-foreground">
									Sales Representative PIC
								</span>
								<div className="text-base font-bold text-foreground">
									{company.salesUserName || "Belum Ditugaskan"}
								</div>
								<span className="text-[11px] text-muted-foreground">
									Petugas Penanggung Jawab
								</span>
							</CardContent>
						</Card>

						<Card className="border-border/60 shadow-xs">
							<CardContent className="p-4 space-y-1">
								<span className="text-xs text-muted-foreground">
									Status Berkas
								</span>
								<div className="text-base font-bold text-foreground flex items-center gap-1.5">
									<Badge
										variant="outline"
										className={`text-[11px] font-normal border ${statusInfo.badgeClass}`}
									>
										{statusInfo.label}
									</Badge>
								</div>
								<span className="text-[11px] text-muted-foreground">
									Tahap {company.currentStage} dari 8
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
									Informasi Profil Perusahaan
								</CardTitle>
							</CardHeader>
							<CardContent className="space-y-3 text-xs">
								<div className="grid grid-cols-3 gap-2 py-1.5 border-b">
									<span className="text-muted-foreground">Nama Perusahaan</span>
									<span className="col-span-2 font-medium">
										{company.namaPerusahaan}
									</span>
								</div>
								<div className="grid grid-cols-3 gap-2 py-1.5 border-b">
									<span className="text-muted-foreground">NPWP</span>
									<span className="col-span-2 font-mono">
										{company.npwp || "-"}
									</span>
								</div>
								<div className="grid grid-cols-3 gap-2 py-1.5 border-b">
									<span className="text-muted-foreground">Alamat Lengkap</span>
									<span className="col-span-2">{company.address}</span>
								</div>
								<div className="grid grid-cols-3 gap-2 py-1.5 border-b">
									<span className="text-muted-foreground">Wilayah BPS</span>
									<span className="col-span-2">{company.locationLabel}</span>
								</div>
								<div className="grid grid-cols-3 gap-2 py-1.5 border-b">
									<span className="text-muted-foreground">Kode Pos</span>
									<span className="col-span-2">{company.kodePos || "-"}</span>
								</div>
								<div className="grid grid-cols-3 gap-2 py-1.5 border-b">
									<span className="text-muted-foreground">Email Kontak</span>
									<span className="col-span-2">{company.email || "-"}</span>
								</div>
								<div className="grid grid-cols-3 gap-2 py-1.5 border-b">
									<span className="text-muted-foreground">No. Telepon</span>
									<span className="col-span-2">{company.telp || "-"}</span>
								</div>
								<div className="grid grid-cols-3 gap-2 py-1.5">
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
							</CardContent>
						</Card>

						{/* Right: Titik Koordinat & Peta Mini */}
						<Card className="border-border/60 shadow-xs">
							<CardHeader className="pb-3">
								<div className="flex items-center justify-between">
									<CardTitle className="text-sm font-semibold">
										Lokasi Spasial & Koordinat
									</CardTitle>
									{company.latitude != null && company.longitude != null && (
										<Badge variant="outline" className="font-mono text-xs">
											{company.latitude.toFixed(6)},{" "}
											{company.longitude.toFixed(6)}
										</Badge>
									)}
								</div>
							</CardHeader>
							<CardContent>
								{company.latitude != null && company.longitude != null ? (
									<div className="h-[280px] w-full rounded-md overflow-hidden border">
										<Map
											center={[company.longitude, company.latitude]}
											zoom={13}
											interactive={false}
											selectedCoordinates={{
												latitude: company.latitude,
												longitude: company.longitude,
											}}
											className="h-full w-full"
										/>
									</div>
								) : (
									<div className="h-[280px] w-full flex items-center justify-center border rounded-md bg-muted/20 text-xs text-muted-foreground">
										Koordinat lokasi belum diatur
									</div>
								)}
							</CardContent>
						</Card>
					</div>
				</TabsContent>

				{/* TAB 2: KONTAK PERUSAHAAN */}
				<TabsContent value="contacts" className="space-y-4 pt-4">
					<Card className="border-border/60 shadow-xs">
						<CardHeader className="p-4 flex flex-row items-center justify-between">
							<div>
								<CardTitle className="text-base font-semibold">
									Daftar Kontak Person (PIC) Perusahaan
								</CardTitle>
								<CardDescription className="text-xs">
									Informasi penanggung jawab teknis, komersial, dan manajemen
									calon pelanggan
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
													{c.telp || "-"}
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
														<Button
															variant="ghost"
															size="icon"
															className="size-7"
															onClick={() => handleOpenEditContact(c)}
														>
															<Edit2 className="size-3.5 text-muted-foreground" />
														</Button>
														<Button
															variant="ghost"
															size="icon"
															className="size-7 text-destructive hover:text-destructive"
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
														</Button>
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
												Belum ada kontak terdaftar. Silakan klik tombol Tambah
												Kontak.
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
								<form onSubmit={handleSavePlotting} className="space-y-4">
									{/* Sales Representative */}
									<div className="space-y-1.5">
										<Label className="text-xs font-medium">
											Sales Representative Penanggung Jawab
										</Label>
										<Select
											value={selectedSalesUserId || "NONE"}
											onValueChange={(val) =>
												setSelectedSalesUserId(val === "NONE" ? "" : val)
											}
										>
											<SelectTrigger className="text-xs h-9">
												<SelectValue placeholder="Pilih Sales Representative" />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="NONE">Belum Ditugaskan</SelectItem>
												{salesUsers?.map((u) => (
													<SelectItem key={u.id} value={u.id}>
														{u.fullName} ({u.role})
													</SelectItem>
												))}
											</SelectContent>
										</Select>
									</div>

									{/* Posisi Pelanggan */}
									<div className="space-y-1.5">
										<Label className="text-xs font-medium">
											Posisi Pelanggan Terhadap Jalur Pipa
										</Label>
										<Select
											value={selectedPosisi || "NONE"}
											onValueChange={(val) =>
												setSelectedPosisi(
													val === "NONE" ? "" : (val as PosisiPelanggan),
												)
											}
										>
											<SelectTrigger className="text-xs h-9">
												<SelectValue placeholder="Pilih Posisi Pelanggan" />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="NONE">Belum Ditetapkan</SelectItem>
												<SelectItem value="JalurExisting">
													Jalur Existing (Dekat Pipa Eksisting)
												</SelectItem>
												<SelectItem value="Pengembangan">
													Pengembangan (Perlu Jaringan Baru)
												</SelectItem>
											</SelectContent>
										</Select>
									</div>

									{/* Kawasan */}
									<div className="space-y-1.5">
										<Label className="text-xs font-medium">
											Klasifikasi Kawasan
										</Label>
										<Select
											value={selectedKawasan || "NONE"}
											onValueChange={(val) =>
												setSelectedKawasan(
													val === "NONE" ? "" : (val as Kawasan),
												)
											}
										>
											<SelectTrigger className="text-xs h-9">
												<SelectValue placeholder="Pilih Klasifikasi Kawasan" />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="NONE">Belum Ditetapkan</SelectItem>
												<SelectItem value="KawasanIndustri">
													Kawasan Industri
												</SelectItem>
												<SelectItem value="NonKawasanIndustri">
													Non Kawasan Industri
												</SelectItem>
											</SelectContent>
										</Select>
									</div>

									<Button
										type="submit"
										disabled={savePlottingMutation.isPending}
										className="text-xs h-9 w-full flex items-center justify-center gap-1.5 mt-2"
									>
										{savePlottingMutation.isPending ? (
											<Loader2 className="size-4 animate-spin" />
										) : (
											<Save className="size-4" />
										)}
										Simpan Konfigurasi Plotting
									</Button>
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
											Geser pin penanda pada peta untuk memperbarui koordinat
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
											center={[currentCoords.longitude, currentCoords.latitude]}
											zoom={14}
											selectedCoordinates={currentCoords}
											onCoordinateSelect={(c) => {
												setCurrentCoords(c);
												setHasChangedCoords(true);
											}}
											className="h-full w-full"
										/>
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
						companyId={companyId}
						initialData={surveyData}
						canEdit={company.userCanEdit}
					/>
				</TabsContent>

				{/* TAB 5: REGISTRASI A1 (STAGE 5) */}
				<TabsContent value="reg-a1" className="pt-4">
					<A1RegistrationForm
						companyId={companyId}
						initialData={registrationData}
						canEdit={company.userCanEdit}
					/>
				</TabsContent>

				{/* TAB 6: PERMOHONAN NOL (STAGE 6) */}
				<TabsContent value="nol-req" className="pt-4">
					<NolRequestForm
						companyId={companyId}
						initialData={nolRequestData}
						canEdit={company.userCanEdit}
						canSubmit={company.userCanSubmit}
					/>
				</TabsContent>

				{/* TAB 7: EVALUASI NOL (STAGE 7) */}
				<TabsContent value="nol-eval" className="pt-4">
					<NolEvaluationForm
						companyId={companyId}
						initialData={nolEvaluationData}
						canEdit={company.userCanEdit}
						canChooseReviewers={company.userCanChooseReviewers}
					/>
				</TabsContent>

				{/* TAB 8: PENERBITAN NOL (STAGE 8) */}
				<TabsContent value="nol-issue" className="pt-4">
					<NolIssuanceForm
						companyId={companyId}
						initialData={nolIssuanceData}
						canEdit={company.userCanEdit}
					/>
				</TabsContent>

				{/* TAB 9: LINI MASA & AUDIT TRAIL */}
				<TabsContent value="timeline" className="pt-4">
					<Card className="border-border/60 shadow-xs">
						<CardHeader>
							<CardTitle className="text-base font-semibold flex items-center gap-2">
								<Clock className="size-4 text-primary" />
								Lini Masa & Riwayat Audit Status
							</CardTitle>
							<CardDescription className="text-xs">
								Catatan jejak audit riwayat perubahan status, transisi alur
								kerja, dan catatan verifikasi
							</CardDescription>
						</CardHeader>
						<CardContent>
							{timeline && timeline.length > 0 ? (
								<div className="space-y-4 relative before:absolute before:inset-0 before:left-3.5 before:w-0.5 before:bg-muted">
									{timeline.map((entry) => (
										<div
											key={`${entry.createdAt}-${entry.action}-${entry.fromStatus}-${entry.toStatus}`}
											className="flex items-start gap-4 relative pl-8"
										>
											<div className="absolute left-2 top-1 size-3 rounded-full bg-primary border-2 border-background" />
											<div className="space-y-1 bg-muted/30 p-3 rounded-lg border flex-1 text-xs">
												<div className="flex items-center justify-between gap-2">
													<div className="flex items-center gap-2">
														<span className="font-semibold text-foreground">
															{entry.action}
														</span>
														<span className="text-muted-foreground">oleh</span>
														<span className="font-medium text-foreground">
															{entry.actorUserName}
														</span>
													</div>
													<span className="text-[11px] text-muted-foreground font-mono">
														{new Date(entry.createdAt).toLocaleString("id-ID")}
													</span>
												</div>
												<div className="flex items-center gap-2 text-muted-foreground pt-1">
													<span>
														Status: <strong>{entry.fromStatus}</strong> →{" "}
														<strong>{entry.toStatus}</strong>
													</span>
												</div>
												{entry.notes && (
													<p className="text-muted-foreground pt-1 italic">
														"{entry.notes}"
													</p>
												)}
											</div>
										</div>
									))}
								</div>
							) : (
								<div className="text-center py-8 text-xs text-muted-foreground">
									Belum ada riwayat perubahan status pada berkas ini.
								</div>
							)}
						</CardContent>
					</Card>
				</TabsContent>
			</Tabs>

			{/* Contact Modal (Add / Edit) */}
			<Dialog open={contactModalOpen} onOpenChange={setContactModalOpen}>
				<DialogContent className="sm:max-w-[440px]">
					<form onSubmit={handleSaveContact}>
						<DialogHeader>
							<DialogTitle className="text-base font-semibold">
								{editingContact ? "Ubah Kontak Person" : "Tambah Kontak Person"}
							</DialogTitle>
							<DialogDescription className="text-xs">
								{editingContact
									? "Perbarui informasi kontak penanggung jawab perusahaan"
									: "Masukkan informasi kontak penanggung jawab calon pelanggan"}
							</DialogDescription>
						</DialogHeader>
						<div className="space-y-3 py-4 text-xs">
							<div className="space-y-1">
								<Label htmlFor="contactNama" className="text-xs font-medium">
									Nama Lengkap <span className="text-destructive">*</span>
								</Label>
								<Input
									id="contactNama"
									placeholder="Contoh: Budi Santoso"
									value={contactNama}
									onChange={(e) => setContactNama(e.target.value)}
									className="text-xs h-9"
									required
								/>
							</div>
							<div className="space-y-1">
								<Label htmlFor="contactJabatan" className="text-xs font-medium">
									Jabatan / Posisi
								</Label>
								<Input
									id="contactJabatan"
									placeholder="Contoh: Plant Manager / Purchasing"
									value={contactJabatan}
									onChange={(e) => setContactJabatan(e.target.value)}
									className="text-xs h-9"
								/>
							</div>
							<div className="space-y-1">
								<Label htmlFor="contactEmail" className="text-xs font-medium">
									Email
								</Label>
								<Input
									id="contactEmail"
									type="email"
									placeholder="budi@perusahaan.co.id"
									value={contactEmail}
									onChange={(e) => setContactEmail(e.target.value)}
									className="text-xs h-9"
								/>
							</div>
							<div className="space-y-1">
								<Label htmlFor="contactTelp" className="text-xs font-medium">
									No. Telepon / HP
								</Label>
								<Input
									id="contactTelp"
									placeholder="0812-3456-7890"
									value={contactTelp}
									onChange={(e) => setContactTelp(e.target.value)}
									className="text-xs h-9"
								/>
							</div>
							<div className="flex items-center gap-2 pt-2">
								<input
									type="checkbox"
									id="isPrimary"
									checked={contactIsPrimary}
									onChange={(e) => setContactIsPrimary(e.target.checked)}
									className="rounded border-gray-300 text-primary focus:ring-primary size-4"
								/>
								<Label
									htmlFor="isPrimary"
									className="text-xs font-medium cursor-pointer"
								>
									Jadikan sebagai Kontak Utama (Primary Contact)
								</Label>
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
							<Button
								type="submit"
								size="sm"
								disabled={
									addContactMutation.isPending ||
									updateContactMutation.isPending
								}
								className="text-xs h-8"
							>
								{addContactMutation.isPending ||
								updateContactMutation.isPending ? (
									<Loader2 className="size-3.5 animate-spin mr-1" />
								) : null}
								Simpan Kontak
							</Button>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>
		</div>
	);
}
