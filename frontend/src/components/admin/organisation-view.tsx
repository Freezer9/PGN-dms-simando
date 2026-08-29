import {
	AlertTriangle,
	Building2,
	Edit2,
	FolderTree,
	Loader2,
	MapPin,
	Network,
	Plus,
	Trash2,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { AreaItemDto, RegionWithAreasDto } from "@/api/types";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
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
			onError: (error) => {
				setError(
					error.detail || error.title || "Gagal menyimpan data wilayah.",
				);
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
			onError: (error) => {
				setError(
					error.detail || error.title || "Gagal memperbarui data wilayah.",
				);
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
			onError: (error) => {
				setError(
					error.detail ||
						error.title ||
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
			onError: (error) => {
				setError(
					error.detail || error.title || "Gagal menyimpan data sales area.",
				);
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
			onError: (error) => {
				setError(
					error.detail || error.title || "Gagal memperbarui data sales area.",
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
			onError: (error) => {
				setError(
					error.detail ||
						error.title ||
						"Gagal menghapus sales area. Pastikan sales area tidak memiliki berkas pelanggan aktif.",
				);
			},
		},
	);

	const regions = orgData || [];

	// Form states
	const [regionCode, setRegionCode] = React.useState("");
	const [regionName, setRegionName] = React.useState("");

	const [areaCode, setAreaCode] = React.useState("");
	const [areaName, setAreaName] = React.useState("");
	const [areaTargetRegionId, setAreaTargetRegionId] =
		React.useState<string>("");

	const handleOpenCreateRegion = () => {
		setEditingRegion(null);
		setRegionCode("");
		setRegionName("");
		setError(null);
		setRegionDialogOpen(true);
	};

	const handleOpenEditRegion = (r: RegionWithAreasDto) => {
		setEditingRegion(r);
		setRegionCode(r.code);
		setRegionName(r.name);
		setError(null);
		setRegionDialogOpen(true);
	};

	const handleSaveRegion = (e: React.FormEvent) => {
		e.preventDefault();
		if (!regionCode.trim() || !regionName.trim()) {
			setError("Kode dan Nama Wilayah wajib diisi.");
			return;
		}

		if (editingRegion) {
			updateRegionMutation.mutate({
				params: { path: { id: editingRegion.id } },
				body: {
					code: regionCode.trim(),
					name: regionName.trim(),
					active: editingRegion.active ?? true,
				},
			});
		} else {
			createRegionMutation.mutate({
				body: { code: regionCode.trim(), name: regionName.trim() },
			});
		}
	};

	const handleOpenCreateArea = (defaultRegionId?: string) => {
		setEditingArea(null);
		setAreaCode("");
		setAreaName("");
		setAreaTargetRegionId(defaultRegionId || regions[0]?.id || "");
		setError(null);
		setAreaDialogOpen(true);
	};

	const handleOpenEditArea = (area: AreaItemDto, currentRegionId: string) => {
		setEditingArea({ area, regionId: currentRegionId });
		setAreaCode(area.code);
		setAreaName(area.name);
		setAreaTargetRegionId(currentRegionId);
		setError(null);
		setAreaDialogOpen(true);
	};

	const handleSaveArea = (e: React.FormEvent) => {
		e.preventDefault();
		if (!areaCode.trim() || !areaName.trim() || !areaTargetRegionId) {
			setError("Kode, Nama Sales Area, dan Wilayah wajib diisi.");
			return;
		}

		if (editingArea?.area) {
			updateAreaMutation.mutate({
				params: { path: { id: editingArea.area.id } },
				body: {
					regionId: areaTargetRegionId,
					code: areaCode.trim(),
					name: areaName.trim(),
					active: editingArea.area.active ?? true,
				},
			});
		} else {
			createAreaMutation.mutate({
				body: {
					regionId: areaTargetRegionId,
					code: areaCode.trim(),
					name: areaName.trim(),
				},
			});
		}
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
			<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
				<div className="space-y-0.5">
					<h2 className="text-lg font-bold tracking-tight text-foreground flex items-center gap-2">
						<Network className="h-5 w-5 text-primary" />
						<span>Organisasi — Struktur Wilayah & Sales Area</span>
					</h2>
					<p className="text-xs text-muted-foreground">
						Definisi batas lingkup teritorial (SOR & Sales Area) untuk perizinan
						dan jalur persetujuan berkas.
					</p>
				</div>

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
			</div>

			{error && (
				<Alert variant="destructive">
					<AlertTriangle className="h-4 w-4" />
					<AlertDescription className="text-xs">{error}</AlertDescription>
				</Alert>
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
			) : (
				<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
					{regions.map((region) => (
						<Card key={region.id} className="overflow-hidden shadow-xs">
							<CardHeader className="bg-muted/40 p-4 border-b flex flex-row items-center justify-between space-y-0">
								<div className="flex items-center gap-2 min-w-0">
									<Building2 className="h-4 w-4 text-primary shrink-0" />
									<div className="min-w-0">
										<CardTitle className="text-sm font-bold truncate">
											{region.name}
										</CardTitle>
										<span className="font-mono text-[11px] text-muted-foreground">
											Kode: {region.code}
										</span>
									</div>
								</div>

								<div className="flex items-center gap-1.5 shrink-0">
									<Badge variant="secondary" className="text-[10px]">
										{region.areas.length} Area
									</Badge>
									<Button
										variant="ghost"
										size="icon"
										onClick={() => handleOpenEditRegion(region)}
										className="h-7 w-7 text-muted-foreground hover:text-foreground"
										title="Ubah Wilayah"
									>
										<Edit2 className="h-3.5 w-3.5" />
									</Button>
									<Button
										variant="ghost"
										size="icon"
										onClick={() =>
											setDeleteConfirm({
												type: "region",
												id: region.id,
												name: region.name,
											})
										}
										className="h-7 w-7 text-muted-foreground hover:text-destructive"
										title="Hapus Wilayah"
									>
										<Trash2 className="h-3.5 w-3.5" />
									</Button>
								</div>
							</CardHeader>

							<CardContent className="p-3 space-y-2">
								{region.areas.length === 0 ? (
									<div className="text-center py-4 text-xs text-muted-foreground border border-dashed rounded-lg">
										Belum ada Sales Area di wilayah ini.
										<div className="mt-1">
											<Button
												variant="link"
												size="sm"
												onClick={() => handleOpenCreateArea(region.id)}
												className="h-auto p-0 text-xs text-primary"
											>
												+ Tambah Sales Area
											</Button>
										</div>
									</div>
								) : (
									<div className="divide-y rounded-lg border bg-muted/20">
										{region.areas.map((area) => (
											<div
												key={area.id}
												className="flex items-center justify-between p-2.5 text-xs hover:bg-muted/40 transition-colors"
											>
												<div className="flex items-center gap-2 min-w-0">
													<MapPin className="h-3.5 w-3.5 text-primary shrink-0" />
													<div className="min-w-0">
														<span className="font-semibold text-foreground">
															{area.name}
														</span>
														<span className="font-mono text-[11px] text-muted-foreground ml-2">
															({area.code})
														</span>
													</div>
												</div>

												<div className="flex items-center gap-1 shrink-0">
													<Button
														variant="ghost"
														size="icon"
														onClick={() => handleOpenEditArea(area, region.id)}
														className="h-6 w-6 text-muted-foreground hover:text-foreground"
														title="Ubah Sales Area"
													>
														<Edit2 className="h-3 w-3" />
													</Button>
													<Button
														variant="ghost"
														size="icon"
														onClick={() =>
															setDeleteConfirm({
																type: "area",
																id: area.id,
																name: area.name,
															})
														}
														className="h-6 w-6 text-muted-foreground hover:text-destructive"
														title="Hapus Sales Area"
													>
														<Trash2 className="h-3 w-3" />
													</Button>
												</div>
											</div>
										))}
									</div>
								)}
							</CardContent>
						</Card>
					))}
				</div>
			)}

			{/* Dialog Add/Edit Region */}
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

					<form onSubmit={handleSaveRegion} className="space-y-3 py-2">
						<div className="space-y-1">
							<Label htmlFor="reg-code" className="text-xs font-medium">
								Kode Wilayah <span className="text-destructive">*</span>
							</Label>
							<Input
								id="reg-code"
								placeholder="contoh: SOR3"
								value={regionCode}
								onChange={(e) => setRegionCode(e.target.value)}
								className="h-8 text-xs font-mono"
								required
							/>
						</div>

						<div className="space-y-1">
							<Label htmlFor="reg-name" className="text-xs font-medium">
								Nama Wilayah <span className="text-destructive">*</span>
							</Label>
							<Input
								id="reg-name"
								placeholder="contoh: Region 3 - Jatim Bali Nusa"
								value={regionName}
								onChange={(e) => setRegionName(e.target.value)}
								className="h-8 text-xs"
								required
							/>
						</div>

						<DialogFooter className="pt-2">
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => setRegionDialogOpen(false)}
							>
								Batal
							</Button>
							<Button
								type="submit"
								size="sm"
								disabled={
									createRegionMutation.isPending ||
									updateRegionMutation.isPending
								}
							>
								Simpan
							</Button>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>

			{/* Dialog Add/Edit Area */}
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

					<form onSubmit={handleSaveArea} className="space-y-3 py-2">
						<div className="space-y-1">
							<Label htmlFor="area-reg" className="text-xs font-medium">
								Pilih Wilayah (Region){" "}
								<span className="text-destructive">*</span>
							</Label>
							<select
								id="area-reg"
								value={areaTargetRegionId}
								onChange={(e) => setAreaTargetRegionId(e.target.value)}
								className="w-full h-8 px-2.5 rounded-md border bg-background text-xs"
								required
							>
								{regions.map((r) => (
									<option key={r.id} value={r.id}>
										{r.name} ({r.code})
									</option>
								))}
							</select>
						</div>

						<div className="space-y-1">
							<Label htmlFor="area-code" className="text-xs font-medium">
								Kode Sales Area <span className="text-destructive">*</span>
							</Label>
							<Input
								id="area-code"
								placeholder="contoh: SBY"
								value={areaCode}
								onChange={(e) => setAreaCode(e.target.value)}
								className="h-8 text-xs font-mono"
								required
							/>
						</div>

						<div className="space-y-1">
							<Label htmlFor="area-name" className="text-xs font-medium">
								Nama Sales Area <span className="text-destructive">*</span>
							</Label>
							<Input
								id="area-name"
								placeholder="contoh: Area Surabaya"
								value={areaName}
								onChange={(e) => setAreaName(e.target.value)}
								className="h-8 text-xs"
								required
							/>
						</div>

						<DialogFooter className="pt-2">
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => setAreaDialogOpen(false)}
							>
								Batal
							</Button>
							<Button
								type="submit"
								size="sm"
								disabled={
									createAreaMutation.isPending || updateAreaMutation.isPending
								}
							>
								Simpan
							</Button>
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
							disabled={
								deleteRegionMutation.isPending || deleteAreaMutation.isPending
							}
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
