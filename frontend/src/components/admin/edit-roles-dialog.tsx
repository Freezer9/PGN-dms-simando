import {
	AlertTriangle,
	Loader2,
	Plus,
	Shield,
	Trash2,
	UserCog,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { AppRole, UserListItemDto } from "@/api/types";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/lib/auth";

interface EditRolesDialogProps {
	user: UserListItemDto | null;
	open: boolean;
	onOpenChange: (open: boolean) => void;
	onSuccess: () => void;
}

const ALL_ROLES: {
	value: AppRole;
	label: string;
	scopeType: "area" | "region" | "none";
}[] = [
	{ value: "SalesArea", label: "Sales Area", scopeType: "area" },
	{ value: "AreaHead", label: "Area Head", scopeType: "area" },
	{ value: "Reviewer", label: "Reviewer", scopeType: "region" },
	{ value: "RegionalAdmin", label: "Regional Admin", scopeType: "region" },
	{ value: "DivisionHead", label: "Division Head", scopeType: "region" },
	{ value: "SystemAdmin", label: "System Admin", scopeType: "none" },
];

export function EditRolesDialog({
	user,
	open,
	onOpenChange,
	onSuccess,
}: EditRolesDialogProps) {
	const { user: currentUser } = useAuth();
	const isSysAdmin = currentUser?.capabilities?.includes("ManageMasterData");

	const [role, setRole] = React.useState<AppRole>("SalesArea");
	const [regionId, setRegionId] = React.useState<string>("");
	const [areaId, setAreaId] = React.useState<string>("");
	const [error, setError] = React.useState<string | null>(null);

	// Fetch organisation hierarchy for region/area dropdowns
	const { data: orgData } = $api.useQuery(
		"get",
		"/api/admin/organisation",
		undefined,
		{
			enabled: open,
		},
	);

	const regions = orgData || [];
	const selectedRegion = regions.find((r) => r.id === regionId);
	const availableAreas = selectedRegion ? selectedRegion.areas : [];

	// Filter allowed roles based on actor
	const allowedRoles = React.useMemo(() => {
		if (isSysAdmin) return ALL_ROLES;
		return ALL_ROLES.filter(
			(r) =>
				r.value === "SalesArea" ||
				r.value === "AreaHead" ||
				r.value === "Reviewer",
		);
	}, [isSysAdmin]);

	React.useEffect(() => {
		if (regions.length > 0 && !regionId) {
			setRegionId(regions[0].id);
		}
	}, [regions, regionId]);

	React.useEffect(() => {
		if (availableAreas.length > 0 && !areaId) {
			setAreaId(availableAreas[0].id);
		}
	}, [availableAreas, areaId]);

	const selectedRoleMeta =
		ALL_ROLES.find((r) => r.value === role) || ALL_ROLES[0];

	const addRoleMutation = $api.useMutation(
		"post",
		"/api/admin/users/{id}/roles",
		{
			onSuccess: () => {
				setError(null);
				onSuccess();
			},
			onError: (error) => {
				const msg =
					error.detail || error.title || "Gagal menambahkan peran pengguna.";
				setError(msg);
			},
		},
	);

	const removeRoleMutation = $api.useMutation(
		"delete",
		"/api/admin/users/{id}/roles/{assignmentId}",
		{
			onSuccess: () => {
				setError(null);
				onSuccess();
			},
			onError: (error) => {
				const msg =
					error.detail || error.title || "Gagal menonaktifkan peran pengguna.";
				setError(msg);
			},
		},
	);

	if (!user) return null;

	const handleAddRole = (e: React.FormEvent) => {
		e.preventDefault();
		setError(null);

		let reqRegionId: string | undefined;
		let reqAreaId: string | undefined;

		if (selectedRoleMeta.scopeType === "area") {
			if (!areaId) {
				setError("Silakan pilih Sales Area untuk peran ini.");
				return;
			}
			reqAreaId = areaId;
			reqRegionId = regionId || undefined;
		} else if (selectedRoleMeta.scopeType === "region") {
			if (!regionId) {
				setError("Silakan pilih Wilayah (Region) untuk peran ini.");
				return;
			}
			reqRegionId = regionId;
		}

		addRoleMutation.mutate({
			params: {
				path: { id: user.id },
			},
			body: {
				role,
				areaId: reqAreaId || null,
				regionId: reqRegionId || null,
			},
		});
	};

	const handleDeactivateAssignment = (assignmentId: string) => {
		if (user.roles.length <= 1) {
			setError("Pengguna minimal harus memiliki satu peran aktif.");
			return;
		}
		removeRoleMutation.mutate({
			params: {
				path: {
					id: user.id,
					assignmentId,
				},
			},
		});
	};

	return (
		<Dialog open={open} onOpenChange={onOpenChange}>
			<DialogContent className="sm:max-w-[540px]">
				<DialogHeader>
					<DialogTitle className="flex items-center gap-2">
						<UserCog className="h-5 w-5 text-primary" />
						<span>Ubah Peran Pengguna</span>
					</DialogTitle>
					<DialogDescription>
						Kelola peran dan lingkup akses untuk{" "}
						<strong className="text-foreground">{user.fullName}</strong> (
						{user.username})
					</DialogDescription>
				</DialogHeader>

				<div className="space-y-4 py-2">
					{error && (
						<Alert variant="destructive">
							<AlertDescription className="text-xs">{error}</AlertDescription>
						</Alert>
					)}

					{/* Active roles list */}
					<div className="space-y-2">
						<Label className="text-xs font-semibold text-foreground flex items-center justify-between">
							<span>Peran & Lingkup Aktif</span>
							<span className="text-muted-foreground font-normal">
								{user.roles.length} peran
							</span>
						</Label>

						<div className="rounded-lg border bg-muted/20 divide-y overflow-hidden">
							{user.roles.length === 0 ? (
								<div className="p-3 text-center text-xs text-muted-foreground">
									Belum ada peran aktif.
								</div>
							) : (
								user.roles.map((r, idx) => {
									const assignmentId = user.assignmentIds?.[idx];
									const roleKey = assignmentId || `${r.role}-${r.scopeLabel}`;
									return (
										<div
											key={roleKey}
											className="flex items-center justify-between p-3 text-xs"
										>
											<div className="flex items-center gap-2">
												<Shield className="h-3.5 w-3.5 text-primary shrink-0" />
												<div>
													<span className="font-semibold text-foreground">
														{r.role}
													</span>
													<span className="text-muted-foreground mx-1.5">
														·
													</span>
													<span className="text-muted-foreground">
														{r.scopeLabel || "Nasional"}
													</span>
												</div>
											</div>

											{assignmentId && user.roles.length > 1 && (
												<Button
													type="button"
													variant="ghost"
													size="sm"
													onClick={() =>
														handleDeactivateAssignment(assignmentId)
													}
													disabled={removeRoleMutation.isPending}
													className="h-7 px-2 text-[11px] text-destructive hover:text-destructive hover:bg-destructive/10"
												>
													<Trash2 className="h-3 w-3 mr-1" />
													Nonaktifkan
												</Button>
											)}
										</div>
									);
								})
							)}
						</div>
					</div>

					{/* Add role form */}
					<form onSubmit={handleAddRole} className="space-y-3 pt-2 border-t">
						<Label className="text-xs font-semibold text-foreground">
							Tambah Peran Baru
						</Label>

						<div className="grid grid-cols-2 gap-2">
							<div className="space-y-1">
								<Label className="text-[11px] text-muted-foreground">
									Pilih Peran
								</Label>
								<select
									value={role}
									onChange={(e) => setRole(e.target.value as AppRole)}
									className="w-full h-8 px-2.5 rounded-md border bg-background text-xs"
								>
									{allowedRoles.map((r) => (
										<option key={r.value} value={r.value}>
											{r.label}
										</option>
									))}
								</select>
							</div>

							{selectedRoleMeta.scopeType !== "none" && (
								<div className="space-y-1">
									<Label className="text-[11px] text-muted-foreground">
										Wilayah (Region)
									</Label>
									<select
										value={regionId}
										onChange={(e) => {
											setRegionId(e.target.value);
											const reg = regions.find((r) => r.id === e.target.value);
											if (reg && reg.areas.length > 0) {
												setAreaId(reg.areas[0].id);
											}
										}}
										className="w-full h-8 px-2.5 rounded-md border bg-background text-xs"
									>
										<option value="">-- Pilih Wilayah --</option>
										{regions.map((r) => (
											<option key={r.id} value={r.id}>
												{r.name}
											</option>
										))}
									</select>
								</div>
							)}
						</div>

						{selectedRoleMeta.scopeType === "area" && (
							<div className="space-y-1">
								<Label className="text-[11px] text-muted-foreground">
									Sales Area
								</Label>
								<select
									value={areaId}
									onChange={(e) => setAreaId(e.target.value)}
									className="w-full h-8 px-2.5 rounded-md border bg-background text-xs"
								>
									<option value="">-- Pilih Area --</option>
									{availableAreas.map((a) => (
										<option key={a.id} value={a.id}>
											{a.name} ({a.code})
										</option>
									))}
								</select>
							</div>
						)}

						<div className="flex justify-end pt-1">
							<Button
								type="submit"
								size="sm"
								disabled={addRoleMutation.isPending}
								className="h-8 text-xs gap-1.5"
							>
								{addRoleMutation.isPending ? (
									<Loader2 className="h-3.5 w-3.5 animate-spin" />
								) : (
									<Plus className="h-3.5 w-3.5" />
								)}
								<span>Tambah Peran</span>
							</Button>
						</div>
					</form>

					{/* Warning alert */}
					<Alert className="border-amber-200 bg-amber-50 dark:border-amber-800/60 dark:bg-amber-950/40 text-amber-900 dark:text-amber-200">
						<AlertTriangle className="h-4 w-4 text-amber-600 dark:text-amber-400" />
						<AlertDescription className="text-xs leading-relaxed">
							<strong>Catatan Workflow:</strong> Menonaktifkan peran pengguna
							yang sedang memegang tugas persetujuan aktif akan menahan langkah
							tersebut hingga dialihkan melalui menu <em>Tugas Tertahan</em>.
						</AlertDescription>
					</Alert>
				</div>

				<DialogFooter className="pt-2">
					<Button
						type="button"
						variant="outline"
						onClick={() => onOpenChange(false)}
					>
						Tutup
					</Button>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	);
}
