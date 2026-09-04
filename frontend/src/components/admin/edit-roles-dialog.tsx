import { useForm, useStore } from "@tanstack/react-form";
import { Loader2, Plus, Shield, Trash2, UserCog } from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { AppRole, UserListItemDto } from "@/api/types";
import { FormField } from "@/components/form/form-field";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import { useAuth } from "@/lib/auth";
import { ALL_ROLES, formatRole } from "@/lib/roles";
import { type AssignRoleFormValues, assignRoleSchema } from "@/lib/schemas";

interface EditRolesDialogProps {
	user: UserListItemDto | null;
	open: boolean;
	onOpenChange: (open: boolean) => void;
	onSuccess: () => void;
}

export function EditRolesDialog({
	user,
	open,
	onOpenChange,
	onSuccess,
}: EditRolesDialogProps) {
	const { user: currentUser } = useAuth();
	const isSysAdmin = currentUser?.capabilities?.includes("ManageMasterData");

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

	const addRoleMutation = $api.useMutation(
		"post",
		"/api/admin/users/{id}/roles",
		{
			onSuccess: () => {
				setError(null);
				onSuccess();
			},
			onError: (err) => {
				const msg =
					err.detail || err.title || "Gagal menambahkan peran pengguna.";
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
			onError: (err) => {
				const msg =
					err.detail || err.title || "Gagal menonaktifkan peran pengguna.";
				setError(msg);
			},
		},
	);

	const form = useForm({
		defaultValues: {
			role: "SalesArea",
			regionId: "",
			areaId: "",
		} as AssignRoleFormValues,
		validators: {
			onSubmit: assignRoleSchema,
		},
		onSubmit: async ({ value }) => {
			if (!user) return;
			setError(null);

			const selectedRole =
				ALL_ROLES.find((r) => r.value === value.role) || ALL_ROLES[0];

			let reqRegionId: string | undefined;
			let reqAreaId: string | undefined;

			if (selectedRole.scopeType === "area") {
				reqAreaId = value.areaId || undefined;
				reqRegionId = value.regionId || undefined;
			} else if (selectedRole.scopeType === "region") {
				reqRegionId = value.regionId || undefined;
			}

			await addRoleMutation.mutateAsync({
				params: {
					path: { id: user.id },
				},
				body: {
					role: value.role as AppRole,
					areaId: reqAreaId || null,
					regionId: reqRegionId || null,
				},
			});
		},
	});

	const selectedRoleValue = useStore(form.store, (state) => state.values.role);
	const selectedRegionId = useStore(
		form.store,
		(state) => state.values.regionId,
	);

	const selectedRoleMeta =
		ALL_ROLES.find((r) => r.value === selectedRoleValue) || ALL_ROLES[0];
	const selectedRegion = regions.find((r) => r.id === selectedRegionId);
	const availableAreas = selectedRegion ? selectedRegion.areas : [];

	React.useEffect(() => {
		if (regions.length > 0 && !selectedRegionId) {
			form.setFieldValue("regionId", regions[0].id);
			if (regions[0].areas.length > 0) {
				form.setFieldValue("areaId", regions[0].areas[0].id);
			}
		}
	}, [regions, selectedRegionId, form]);

	if (!user) return null;

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
									const roleKey =
										assignmentId || `${formatRole(r.role)}-${r.scopeLabel}`;
									return (
										<div
											key={roleKey}
											className="flex items-center justify-between p-3 text-xs"
										>
											<div className="flex items-center gap-2">
												<Shield className="h-3.5 w-3.5 text-primary shrink-0" />
												<div>
													<span className="font-semibold text-foreground">
														{formatRole(r.role)}
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
					<form
						onSubmit={(e) => {
							e.preventDefault();
							e.stopPropagation();
							form.handleSubmit();
						}}
						className="space-y-3 pt-2 border-t"
					>
						<Label className="text-xs font-semibold text-foreground">
							Tambah Peran Baru
						</Label>

						<div className="grid grid-cols-2 gap-2">
							<form.Field name="role">
								{(field) => {
									const fieldError = field.state.meta.errors[0]?.message;
									return (
										<FormField label="Pilih Peran" error={fieldError}>
											<Select
												value={field.state.value}
												onValueChange={(val) =>
													field.handleChange(val as AppRole)
												}
											>
												<SelectTrigger
													id={field.name}
													className="w-full h-8 text-xs"
												>
													<SelectValue placeholder="Pilih Peran" />
												</SelectTrigger>
												<SelectContent side="bottom">
													{allowedRoles.map((r) => (
														<SelectItem key={r.value} value={r.value}>
															{r.label}
														</SelectItem>
													))}
												</SelectContent>
											</Select>
										</FormField>
									);
								}}
							</form.Field>

							{selectedRoleMeta.scopeType !== "none" && (
								<form.Field name="regionId">
									{(field) => {
										const fieldError = field.state.meta.errors[0]?.message;
										return (
											<FormField label="Wilayah (SOR)" error={fieldError}>
												<Combobox
													id={field.name}
													value={field.state.value || ""}
													onValueChange={(regId) => {
														field.handleChange(regId);
														const reg = regions.find((r) => r.id === regId);
														if (reg && reg.areas.length > 0) {
															form.setFieldValue("areaId", reg.areas[0].id);
														} else {
															form.setFieldValue("areaId", "");
														}
													}}
													options={regions.map((r) => ({
														value: r.id,
														label: r.name,
													}))}
													placeholder="-- Pilih Wilayah --"
													searchPlaceholder="Cari wilayah..."
													emptyText="Wilayah tidak ditemukan."
													className="h-8 text-xs"
												/>
											</FormField>
										);
									}}
								</form.Field>
							)}
						</div>

						{selectedRoleMeta.scopeType === "area" && (
							<form.Field name="areaId">
								{(field) => {
									const fieldError = field.state.meta.errors[0]?.message;
									return (
										<FormField label="Sales Area" error={fieldError}>
											<Combobox
												id={field.name}
												value={field.state.value || ""}
												onValueChange={(val) => field.handleChange(val)}
												options={availableAreas.map((a) => ({
													value: a.id,
													label: `${a.name} (${a.code})`,
												}))}
												placeholder="-- Pilih Area --"
												searchPlaceholder="Cari sales area..."
												emptyText="Sales area tidak ditemukan."
												className="h-8 text-xs"
											/>
										</FormField>
									);
								}}
							</form.Field>
						)}

						<div className="flex justify-end pt-1">
							<form.Subscribe
								selector={(state) => [state.canSubmit, state.isSubmitting]}
							>
								{([canSubmit, isSubmitting]) => (
									<Button
										type="submit"
										size="sm"
										disabled={!canSubmit || isSubmitting}
										className="h-8 text-xs gap-1.5"
									>
										{isSubmitting ? (
											<Loader2 className="h-3.5 w-3.5 animate-spin" />
										) : (
											<Plus className="h-3.5 w-3.5" />
										)}
										<span>Tambahkan Peran</span>
									</Button>
								)}
							</form.Subscribe>
						</div>
					</form>
				</div>

				<DialogFooter className="pt-2 border-t">
					<Button
						type="button"
						variant="outline"
						size="sm"
						onClick={() => onOpenChange(false)}
					>
						Tutup
					</Button>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	);
}
