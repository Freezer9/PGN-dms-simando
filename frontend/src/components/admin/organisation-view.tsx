import { useForm } from "@tanstack/react-form";
import {
	AlertTriangle,
	Building2,
	Edit2,
	FolderTree,
	Loader2,
	MapPin,
	Network,
	Plus,
	Search,
	Trash2,
} from "lucide-react";
import * as React from "react";
import { z } from "zod";
import { $api } from "@/api/client";
import type {
	AreaItemDto,
	CreateAreaRequest,
	CreateRegionRequest,
	RegionWithAreasDto,
	UpdateAreaRequest,
	UpdateRegionRequest,
} from "@/api/types";
import { IconButton, PageHeader, StatCard } from "@/components/common";
import { FormField } from "@/components/form/form-field";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
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
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";

const regionSchema = z.object({
	code: z
		.string()
		.trim()
		.min(2, "Kode wilayah minimal 2 karakter")
		.max(20, "Kode wilayah maksimal 20 karakter"),
	name: z
		.string()
		.trim()
		.min(3, "Nama wilayah minimal 3 karakter")
		.max(100, "Nama wilayah maksimal 100 karakter"),
});

const areaSchema = z.object({
	regionId: z.string().min(1, "Wilayah (Region) wajib dipilih"),
	code: z
		.string()
		.trim()
		.min(2, "Kode sales area minimal 2 karakter")
		.max(20, "Kode sales area maksimal 20 karakter"),
	name: z
		.string()
		.trim()
		.min(3, "Nama sales area minimal 3 karakter")
		.max(100, "Nama sales area maksimal 100 karakter"),
});

export function OrganisationView() {
	const [regionDialogOpen, setRegionDialogOpen] = React.useState(false);
	const [editingRegion, setEditingRegion] =
		React.useState<RegionWithAreasDto | null>(null);

	const [areaDialogOpen, setAreaDialogOpen] = React.useState(false);
	const [editingArea, setEditingArea] = React.useState<{
		area: AreaItemDto | null;
		regionId: string;
	} | null>(null);

	const [deleteConfirm, setDeleteConfirm] = React.useState<{
		type: "region" | "area";
		id: string;
		name: string;
	} | null>(null);

	const [searchTerm, setSearchTerm] = React.useState("");
	const [error, setError] = React.useState<string | null>(null);

	// Fetch organisation hierarchy
	const {
		data: orgData,
		isLoading,
		refetch,
	} = $api.useQuery("get", "/api/admin/organisation");

	// Mutations
	const createRegionMutation = $api.useMutation(
		"post",
		"/api/admin/organisation/regions",
		{
			onSuccess: () => {
				setRegionDialogOpen(false);
				setEditingRegion(null);
				setError(null);
				refetch();
			},
			onError: (err) => {
				setError(err.detail || err.title || "Gagal menyimpan data wilayah.");
			},
		},
	);

	const updateRegionMutation = $api.useMutation(
		"put",
		"/api/admin/organisation/regions/{id}",
		{
			onSuccess: () => {
				setRegionDialogOpen(false);
				setEditingRegion(null);
				setError(null);
				refetch();
			},
			onError: (err) => {
				setError(err.detail || err.title || "Gagal memperbarui data wilayah.");
			},
		},
	);

	const deleteRegionMutation = $api.useMutation(
		"delete",
		"/api/admin/organisation/regions/{id}",
		{
			onSuccess: () => {
				setDeleteConfirm(null);
				setError(null);
				refetch();
			},
			onError: (err) => {
				setError(
					err.detail ||
						err.title ||
						"Gagal menghapus wilayah. Pastikan wilayah tidak memiliki sales area atau data terkait.",
				);
			},
		},
	);

	const createAreaMutation = $api.useMutation(
		"post",
		"/api/admin/organisation/areas",
		{
			onSuccess: () => {
				setAreaDialogOpen(false);
				setEditingArea(null);
				setError(null);
				refetch();
			},
			onError: (err) => {
				setError(err.detail || err.title || "Gagal menyimpan data sales area.");
			},
		},
	);

	const updateAreaMutation = $api.useMutation(
		"put",
		"/api/admin/organisation/areas/{id}",
		{
			onSuccess: () => {
				setAreaDialogOpen(false);
				setEditingArea(null);
				setError(null);
				refetch();
			},
			onError: (err) => {
				setError(
					err.detail || err.title || "Gagal memperbarui data sales area.",
				);
			},
		},
	);

	const deleteAreaMutation = $api.useMutation(
		"delete",
		"/api/admin/organisation/areas/{id}",
		{
			onSuccess: () => {
				setDeleteConfirm(null);
				setError(null);
				refetch();
			},
			onError: (err) => {
				setError(
					err.detail ||
						err.title ||
						"Gagal menghapus sales area. Pastikan sales area tidak memiliki berkas pelanggan aktif.",
				);
			},
		},
	);

	const regions = React.useMemo(() => orgData || [], [orgData]);

	const totalAreas = React.useMemo(
		() => regions.reduce((acc, r) => acc + (r.areas?.length || 0), 0),
		[regions],
	);

	const activeAreas = React.useMemo(
		() =>
			regions.reduce(
				(acc, r) =>
					acc + (r.areas?.filter((a) => a.active ?? true).length || 0),
				0,
			),
		[regions],
	);

	const filteredRegions = React.useMemo(() => {
		if (!searchTerm.trim()) return regions;
		const q = searchTerm.toLowerCase();
		return regions
			.map((r) => {
				const matchesRegion =
					r.name.toLowerCase().includes(q) || r.code.toLowerCase().includes(q);
				const matchingAreas = (r.areas || []).filter(
					(a) =>
						a.name.toLowerCase().includes(q) ||
						a.code.toLowerCase().includes(q),
				);
				if (matchesRegion) return r;
				if (matchingAreas.length > 0) {
					return { ...r, areas: matchingAreas };
				}
				return null;
			})
			.filter(Boolean) as RegionWithAreasDto[];
	}, [regions, searchTerm]);

	// TanStack Form for Region
	const regionForm = useForm({
		defaultValues: {
			code: "",
			name: "",
		} as CreateRegionRequest,
		validators: {
			onSubmit: regionSchema,
		},
		onSubmit: async ({ value }) => {
			if (editingRegion) {
				const updatePayload: UpdateRegionRequest = {
					code: value.code.trim(),
					name: value.name.trim(),
					active: editingRegion.active ?? true,
				};
				updateRegionMutation.mutate({
					params: { path: { id: editingRegion.id } },
					body: updatePayload,
				});
			} else {
				createRegionMutation.mutate({
					body: {
						code: value.code.trim(),
						name: value.name.trim(),
					},
				});
			}
		},
	});

	// TanStack Form for Area
	const areaForm = useForm({
		defaultValues: {
			regionId: "",
			code: "",
			name: "",
		} as CreateAreaRequest,
		validators: {
			onSubmit: areaSchema,
		},
		onSubmit: async ({ value }) => {
			if (editingArea?.area) {
				const updatePayload: UpdateAreaRequest = {
					regionId: value.regionId,
					code: value.code.trim(),
					name: value.name.trim(),
					active: editingArea.area.active ?? true,
				};
				updateAreaMutation.mutate({
					params: { path: { id: editingArea.area.id } },
					body: updatePayload,
				});
			} else {
				createAreaMutation.mutate({
					body: {
						regionId: value.regionId,
						code: value.code.trim(),
						name: value.name.trim(),
					},
				});
			}
		},
	});

	const handleOpenCreateRegion = () => {
		setEditingRegion(null);
		regionForm.reset({ code: "", name: "" });
		setError(null);
		setRegionDialogOpen(true);
	};

	const handleOpenEditRegion = (r: RegionWithAreasDto) => {
		setEditingRegion(r);
		regionForm.reset({ code: r.code, name: r.name });
		setError(null);
		setRegionDialogOpen(true);
	};

	const handleOpenCreateArea = (defaultRegionId?: string) => {
		setEditingArea(null);
		areaForm.reset({
			regionId: defaultRegionId || regions[0]?.id || "",
			code: "",
			name: "",
		});
		setError(null);
		setAreaDialogOpen(true);
	};

	const handleOpenEditArea = (area: AreaItemDto, currentRegionId: string) => {
		setEditingArea({ area, regionId: currentRegionId });
		areaForm.reset({
			regionId: currentRegionId,
			code: area.code,
			name: area.name,
		});
		setError(null);
		setAreaDialogOpen(true);
	};

	const handleConfirmDelete = () => {
		if (!deleteConfirm) return;
		if (deleteConfirm.type === "region") {
			deleteRegionMutation.mutate({
				params: { path: { id: deleteConfirm.id } },
			});
		} else {
			deleteAreaMutation.mutate({
				params: { path: { id: deleteConfirm.id } },
			});
		}
	};

	return (
		<div className="space-y-4">
			{/* Top Header */}
			<PageHeader
				title="Organisasi — Struktur Wilayah & Sales Area"
				description="Definisi batas lingkup teritorial (SOR & Sales Area) untuk perizinan dan jalur persetujuan berkas."
				badge={
					<span className="p-1 rounded-md bg-primary/10 text-primary">
						<Network className="h-4 w-4" />
					</span>
				}
				actions={
					<div className="flex items-center gap-2">
						<Button
							variant="outline"
							size="sm"
							onClick={handleOpenCreateRegion}
							className="h-9 gap-1.5 text-xs"
						>
							<Plus className="h-4 w-4" />
							<span>Tambah Wilayah</span>
						</Button>
						<Button
							size="sm"
							onClick={() => handleOpenCreateArea()}
							disabled={regions.length === 0}
							className="h-9 gap-1.5 text-xs"
						>
							<Plus className="h-4 w-4" />
							<span>Tambah Sales Area</span>
						</Button>
					</div>
				}
			/>

			{error && (
				<Alert variant="destructive">
					<AlertTriangle className="h-4 w-4" />
					<AlertDescription className="text-xs">{error}</AlertDescription>
				</Alert>
			)}

			{/* Executive Stat Summary */}
			{!isLoading && regions.length > 0 && (
				<div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
					<StatCard
						title="Total Wilayah (SOR)"
						value={regions.length}
						description="Wilayah operasional teritorial"
						icon={Building2}
						variant="primary"
					/>
					<StatCard
						title="Total Sales Area"
						value={totalAreas}
						description="Unit pelayanan penjualan gas"
						icon={MapPin}
						variant="blue"
					/>
					<StatCard
						title="Sales Area Aktif"
						value={`${activeAreas} / ${totalAreas}`}
						description="Unit operasional aktif"
						icon={Network}
						variant="emerald"
					/>
				</div>
			)}

			{/* Search & Filter Toolbar */}
			{!isLoading && regions.length > 0 && (
				<div className="flex items-center justify-between gap-3">
					<div className="relative w-full max-w-sm">
						<Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
						<Input
							placeholder="Cari wilayah atau sales area..."
							value={searchTerm}
							onChange={(e) => setSearchTerm(e.target.value)}
							className="pl-8 h-9 text-xs bg-background"
						/>
					</div>
				</div>
			)}

			{/* Loading State */}
			{isLoading ? (
				<div className="rounded-xl border bg-card p-12 text-center flex flex-col items-center justify-center gap-2 text-muted-foreground">
					<Loader2 className="h-6 w-6 animate-spin text-primary" />
					<span className="text-sm font-medium">
						Memuat hierarki organisasi...
					</span>
				</div>
			) : regions.length === 0 ? (
				<div className="rounded-xl border bg-card p-12 text-center flex flex-col items-center justify-center gap-2 text-muted-foreground">
					<FolderTree className="h-8 w-8 text-muted-foreground/60" />
					<h3 className="font-semibold text-foreground text-sm">
						Belum Ada Wilayah Terdaftar
					</h3>
					<p className="text-xs max-w-sm">
						Silakan buat wilayah (Region SOR) terlebih dahulu sebelum
						menambahkan Sales Area dan pengguna.
					</p>
					<Button
						size="sm"
						onClick={handleOpenCreateRegion}
						className="mt-2 text-xs"
					>
						Buat Wilayah Pertama
					</Button>
				</div>
			) : filteredRegions.length === 0 ? (
				<div className="rounded-xl border bg-card p-8 text-center flex flex-col items-center justify-center gap-2 text-muted-foreground">
					<Search className="h-6 w-6 text-muted-foreground/60" />
					<h3 className="font-semibold text-foreground text-sm">
						Data Tidak Ditemukan
					</h3>
					<p className="text-xs max-w-sm">
						Tidak ada wilayah atau sales area yang cocok dengan kata kunci "
						{searchTerm}".
					</p>
					<Button
						variant="outline"
						size="sm"
						onClick={() => setSearchTerm("")}
						className="mt-1 text-xs"
					>
						Reset Pencarian
					</Button>
				</div>
			) : (
				<div className="space-y-4">
					{filteredRegions.map((region) => (
						<Card
							key={region.id}
							className="overflow-hidden shadow-xs border-border/70"
						>
							<CardHeader className="bg-muted/30 p-4 border-b flex flex-row items-center justify-between space-y-0">
								<div className="flex items-center gap-3 min-w-0">
									<div className="size-9 rounded-lg bg-primary/10 text-primary flex items-center justify-center shrink-0">
										<Building2 className="size-4.5" />
									</div>
									<div className="min-w-0">
										<div className="flex items-center gap-2">
											<CardTitle className="text-sm sm:text-base font-bold truncate">
												{region.name}
											</CardTitle>
											<Badge
												variant="outline"
												className="font-mono text-[10px] bg-background"
											>
												{region.code}
											</Badge>
											<Badge variant="secondary" className="text-[10px]">
												{region.areas.length} Sales Area
											</Badge>
										</div>
										<p className="text-xs text-muted-foreground mt-0.5">
											Wilayah Operasional Teritorial PGN
										</p>
									</div>
								</div>

								<div className="flex items-center gap-1.5 shrink-0">
									<Button
										variant="outline"
										size="sm"
										onClick={() => handleOpenCreateArea(region.id)}
										className="h-8 text-xs gap-1.5"
									>
										<Plus className="size-3.5" />
										<span className="hidden sm:inline">Tambah Sales Area</span>
									</Button>
									<IconButton
										tooltip="Ubah Wilayah"
										onClick={() => handleOpenEditRegion(region)}
										className="size-8"
										aria-label="Ubah Wilayah"
									>
										<Edit2 className="size-3.5" />
									</IconButton>
									<IconButton
										tooltip="Hapus Wilayah"
										danger
										onClick={() =>
											setDeleteConfirm({
												type: "region",
												id: region.id,
												name: region.name,
											})
										}
										className="size-8"
										aria-label="Hapus Wilayah"
									>
										<Trash2 className="size-3.5" />
									</IconButton>
								</div>
							</CardHeader>

							<CardContent className="p-0">
								{region.areas.length === 0 ? (
									<div className="text-center py-8 px-4 text-xs text-muted-foreground">
										<p>
											Belum ada Sales Area yang terdaftar pada wilayah{" "}
											{region.name}.
										</p>
										<Button
											variant="link"
											size="sm"
											onClick={() => handleOpenCreateArea(region.id)}
											className="mt-1 text-xs text-primary"
										>
											+ Tambah Sales Area Pertama
										</Button>
									</div>
								) : (
									<Table>
										<TableHeader className="bg-muted/15">
											<TableRow>
												<TableHead className="w-48 font-semibold text-xs py-2.5 pl-4">
													Kode Area
												</TableHead>
												<TableHead className="font-semibold text-xs py-2.5">
													Nama Sales Area
												</TableHead>
												<TableHead className="w-32 font-semibold text-xs py-2.5">
													Status
												</TableHead>
												<TableHead className="w-28 text-right font-semibold text-xs py-2.5 pr-4">
													Aksi
												</TableHead>
											</TableRow>
										</TableHeader>
										<TableBody>
											{region.areas.map((area) => (
												<TableRow
													key={area.id}
													className="hover:bg-muted/30 transition-colors"
												>
													<TableCell className="font-mono text-xs font-semibold pl-4">
														{area.code}
													</TableCell>
													<TableCell className="text-xs font-medium text-foreground">
														<div className="flex items-center gap-2">
															<MapPin className="size-3.5 text-primary shrink-0" />
															<span>{area.name}</span>
														</div>
													</TableCell>
													<TableCell className="text-xs">
														<Badge
															variant="outline"
															className={
																area.active !== false
																	? "text-[10px] bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800"
																	: "text-[10px] bg-muted text-muted-foreground"
															}
														>
															{area.active !== false ? "Aktif" : "Nonaktif"}
														</Badge>
													</TableCell>
													<TableCell className="text-right pr-4 py-2">
														<div className="flex items-center justify-end gap-1">
															<IconButton
																tooltip="Ubah Sales Area"
																onClick={() =>
																	handleOpenEditArea(area, region.id)
																}
																className="size-7"
																aria-label="Ubah Sales Area"
															>
																<Edit2 className="size-3.5" />
															</IconButton>
															<IconButton
																tooltip="Hapus Sales Area"
																danger
																onClick={() =>
																	setDeleteConfirm({
																		type: "area",
																		id: area.id,
																		name: area.name,
																	})
																}
																className="size-7"
																aria-label="Hapus Sales Area"
															>
																<Trash2 className="size-3.5" />
															</IconButton>
														</div>
													</TableCell>
												</TableRow>
											))}
										</TableBody>
									</Table>
								)}
							</CardContent>
						</Card>
					))}
				</div>
			)}

			{/* Dialog Add/Edit Region with TanStack Form */}
			<Dialog open={regionDialogOpen} onOpenChange={setRegionDialogOpen}>
				<DialogContent className="sm:max-w-[420px]">
					<DialogHeader>
						<DialogTitle className="flex items-center gap-2 text-sm font-semibold">
							<Building2 className="h-4 w-4 text-primary" />
							<span>
								{editingRegion
									? "Ubah Wilayah (Region)"
									: "Tambah Wilayah (Region)"}
							</span>
						</DialogTitle>
						<DialogDescription className="text-xs">
							Wilayah mewakili unit Sales & Operations Region (SOR).
						</DialogDescription>
					</DialogHeader>

					<form
						onSubmit={(e) => {
							e.preventDefault();
							e.stopPropagation();
							regionForm.handleSubmit();
						}}
						className="space-y-3 py-2"
					>
						<regionForm.Field name="code">
							{(field) => {
								const fieldError = field.state.meta.errors.length
									? String(field.state.meta.errors[0])
									: undefined;
								return (
									<FormField
										label="Kode Wilayah"
										htmlFor="reg-code"
										required
										error={fieldError}
									>
										<Input
											id="reg-code"
											placeholder="contoh: SOR3"
											value={field.state.value}
											onBlur={field.handleBlur}
											onChange={(e) => field.handleChange(e.target.value)}
											className="h-8 text-xs font-mono"
											required
										/>
									</FormField>
								);
							}}
						</regionForm.Field>

						<regionForm.Field name="name">
							{(field) => {
								const fieldError = field.state.meta.errors.length
									? String(field.state.meta.errors[0])
									: undefined;
								return (
									<FormField
										label="Nama Wilayah"
										htmlFor="reg-name"
										required
										error={fieldError}
									>
										<Input
											id="reg-name"
											placeholder="contoh: Region 3 - Jatim Bali Nusa"
											value={field.state.value}
											onBlur={field.handleBlur}
											onChange={(e) => field.handleChange(e.target.value)}
											className="h-8 text-xs"
											required
										/>
									</FormField>
								);
							}}
						</regionForm.Field>

						<DialogFooter className="pt-2">
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => setRegionDialogOpen(false)}
							>
								Batal
							</Button>
							<regionForm.Subscribe
								selector={(state) => [state.canSubmit, state.isSubmitting]}
							>
								{([canSubmit, isSubmitting]) => (
									<Button
										type="submit"
										size="sm"
										disabled={
											!canSubmit ||
											isSubmitting ||
											createRegionMutation.isPending ||
											updateRegionMutation.isPending
										}
									>
										{isSubmitting ||
										createRegionMutation.isPending ||
										updateRegionMutation.isPending ? (
											<Loader2 className="h-3.5 w-3.5 animate-spin mr-1" />
										) : null}
										Simpan
									</Button>
								)}
							</regionForm.Subscribe>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>

			{/* Dialog Add/Edit Area with TanStack Form & shadcn Select */}
			<Dialog open={areaDialogOpen} onOpenChange={setAreaDialogOpen}>
				<DialogContent className="sm:max-w-[440px]">
					<DialogHeader>
						<DialogTitle className="flex items-center gap-2 text-sm font-semibold">
							<MapPin className="h-4 w-4 text-primary" />
							<span>
								{editingArea ? "Ubah Sales Area" : "Tambah Sales Area Baru"}
							</span>
						</DialogTitle>
						<DialogDescription className="text-xs">
							Sales Area berada di bawah naungan suatu Wilayah SOR.
						</DialogDescription>
					</DialogHeader>

					<form
						onSubmit={(e) => {
							e.preventDefault();
							e.stopPropagation();
							areaForm.handleSubmit();
						}}
						className="space-y-3 py-2"
					>
						<areaForm.Field name="regionId">
							{(field) => {
								const fieldError = field.state.meta.errors.length
									? String(field.state.meta.errors[0])
									: undefined;
								return (
									<FormField
										label="Pilih Wilayah (Region)"
										htmlFor="area-reg"
										required
										error={fieldError}
									>
										<Combobox
											id="area-reg"
											value={field.state.value}
											onValueChange={(val) => field.handleChange(val)}
											options={regions.map((r) => ({
												value: r.id,
												label: `${r.name} (${r.code})`,
											}))}
											placeholder="-- Pilih Wilayah --"
											searchPlaceholder="Cari wilayah..."
											emptyText="Wilayah tidak ditemukan."
											className="h-8 text-xs"
										/>
									</FormField>
								);
							}}
						</areaForm.Field>

						<areaForm.Field name="code">
							{(field) => {
								const fieldError = field.state.meta.errors.length
									? String(field.state.meta.errors[0])
									: undefined;
								return (
									<FormField
										label="Kode Sales Area"
										htmlFor="area-code"
										required
										error={fieldError}
									>
										<Input
											id="area-code"
											placeholder="contoh: SBY"
											value={field.state.value}
											onBlur={field.handleBlur}
											onChange={(e) => field.handleChange(e.target.value)}
											className="h-8 text-xs font-mono"
											required
										/>
									</FormField>
								);
							}}
						</areaForm.Field>

						<areaForm.Field name="name">
							{(field) => {
								const fieldError = field.state.meta.errors.length
									? String(field.state.meta.errors[0])
									: undefined;
								return (
									<FormField
										label="Nama Sales Area"
										htmlFor="area-name"
										required
										error={fieldError}
									>
										<Input
											id="area-name"
											placeholder="contoh: Area Surabaya"
											value={field.state.value}
											onBlur={field.handleBlur}
											onChange={(e) => field.handleChange(e.target.value)}
											className="h-8 text-xs"
											required
										/>
									</FormField>
								);
							}}
						</areaForm.Field>

						<DialogFooter className="pt-2">
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => setAreaDialogOpen(false)}
							>
								Batal
							</Button>
							<areaForm.Subscribe
								selector={(state) => [state.canSubmit, state.isSubmitting]}
							>
								{([canSubmit, isSubmitting]) => (
									<Button
										type="submit"
										size="sm"
										disabled={
											!canSubmit ||
											isSubmitting ||
											createAreaMutation.isPending ||
											updateAreaMutation.isPending
										}
									>
										{isSubmitting ||
										createAreaMutation.isPending ||
										updateAreaMutation.isPending ? (
											<Loader2 className="h-3.5 w-3.5 animate-spin mr-1" />
										) : null}
										Simpan
									</Button>
								)}
							</areaForm.Subscribe>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>

			{/* Delete Confirmation Dialog */}
			<Dialog
				open={Boolean(deleteConfirm)}
				onOpenChange={(open) => {
					if (!open) setDeleteConfirm(null);
				}}
			>
				<DialogContent className="sm:max-w-[400px]">
					<DialogHeader>
						<DialogTitle className="text-sm font-semibold flex items-center gap-2 text-destructive">
							<AlertTriangle className="h-4 w-4" />
							<span>
								Hapus{" "}
								{deleteConfirm?.type === "region" ? "Wilayah" : "Sales Area"}
							</span>
						</DialogTitle>
						<DialogDescription className="text-xs">
							Apakah Anda yakin ingin menghapus{" "}
							<strong className="text-foreground">{deleteConfirm?.name}</strong>
							? Tindakan ini tidak dapat dibatalkan.
						</DialogDescription>
					</DialogHeader>

					<DialogFooter className="pt-2">
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={() => setDeleteConfirm(null)}
						>
							Batal
						</Button>
						<Button
							type="button"
							variant="destructive"
							size="sm"
							onClick={handleConfirmDelete}
							disabled={
								deleteRegionMutation.isPending || deleteAreaMutation.isPending
							}
						>
							{deleteRegionMutation.isPending ||
							deleteAreaMutation.isPending ? (
								<Loader2 className="h-3.5 w-3.5 animate-spin mr-1" />
							) : null}
							Hapus
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</div>
	);
}
