import { useForm, useStore } from "@tanstack/react-form";
import {
	AlertTriangle,
	Check,
	Copy,
	KeyRound,
	Loader2,
	UserPlus,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { AppRole, CreateUserResponse } from "@/api/types";
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
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import { useAuth } from "@/lib/auth";
import { type CreateUserFormValues, createUserSchema } from "@/lib/schemas";

interface CreateUserDialogProps {
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

export function CreateUserDialog({
	open,
	onOpenChange,
	onSuccess,
}: CreateUserDialogProps) {
	const { user: currentUser } = useAuth();
	const isSysAdmin = currentUser?.capabilities?.includes("ManageMasterData");

	const [createdResult, setCreatedResult] =
		React.useState<CreateUserResponse | null>(null);
	const [copied, setCopied] = React.useState(false);
	const [serverError, setServerError] = React.useState<string | null>(null);

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

	// Filter available roles based on current user scope/privileges
	const allowedRoles = React.useMemo(() => {
		if (isSysAdmin) return ALL_ROLES;
		return ALL_ROLES.filter(
			(r) =>
				r.value === "SalesArea" ||
				r.value === "AreaHead" ||
				r.value === "Reviewer",
		);
	}, [isSysAdmin]);

	const createMutation = $api.useMutation("post", "/api/admin/users", {
		onSuccess: (data) => {
			setCreatedResult(data);
			setServerError(null);
			onSuccess();
		},
		onError: (error) => {
			const msg =
				error.detail ||
				error.title ||
				"Gagal membuat pengguna. Pastikan nama pengguna belum digunakan.";
			setServerError(msg);
		},
	});

	const form = useForm({
		defaultValues: {
			fullName: "",
			username: "",
			email: "",
			role: "SalesArea",
			regionId: "",
			areaId: "",
		} as CreateUserFormValues,
		validators: {
			onChange: createUserSchema,
		},
		onSubmit: async ({ value }) => {
			setServerError(null);
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

			await createMutation.mutateAsync({
				body: {
					fullName: value.fullName.trim(),
					username: value.username.trim().toLowerCase(),
					email: value.email?.trim() || null,
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

	// Auto-select region if only 1 is available and none selected
	React.useEffect(() => {
		if (regions.length > 0 && !selectedRegionId) {
			form.setFieldValue("regionId", regions[0].id);
			if (regions[0].areas.length > 0) {
				form.setFieldValue("areaId", regions[0].areas[0].id);
			}
		}
	}, [regions, selectedRegionId, form]);

	const handleReset = () => {
		form.reset({
			fullName: "",
			username: "",
			email: "",
			role: "SalesArea",
			regionId: regions.length > 0 ? regions[0].id : "",
			areaId:
				regions.length > 0 && regions[0].areas.length > 0
					? regions[0].areas[0].id
					: "",
		});
		setServerError(null);
		setCreatedResult(null);
		setCopied(false);
	};

	const handleClose = () => {
		handleReset();
		onOpenChange(false);
	};

	const handleCopy = async () => {
		if (!createdResult?.temporaryPassword) return;
		try {
			await navigator.clipboard.writeText(createdResult.temporaryPassword);
			setCopied(true);
			setTimeout(() => setCopied(false), 2000);
		} catch {
			// Fallback copy
		}
	};

	return (
		<Dialog open={open} onOpenChange={handleClose}>
			<DialogContent className="sm:max-w-[500px]">
				<DialogHeader>
					<DialogTitle className="flex items-center gap-2">
						<UserPlus className="h-5 w-5 text-primary" />
						<span>
							{createdResult ? "Pengguna Dibuat" : "Tambah Pengguna Baru"}
						</span>
					</DialogTitle>
					<DialogDescription>
						{createdResult
							? "Pengguna telah berhasil didaftarkan ke dalam sistem."
							: "Daftarkan akun pengguna baru dengan peran dan lingkup akses awal."}
					</DialogDescription>
				</DialogHeader>

				{createdResult ? (
					<div className="space-y-4 py-2">
						<div className="rounded-lg border bg-muted/30 p-4 space-y-3">
							<div className="text-xs text-muted-foreground">
								Nama Pengguna:{" "}
								<strong className="text-foreground">
									{createdResult.username}
								</strong>
							</div>

							<div className="space-y-1.5">
								<Label className="text-xs font-semibold text-foreground">
									Kata Sandi Sementara:
								</Label>
								<div className="flex items-center gap-2">
									<div className="flex-1 font-mono text-sm bg-background border px-3 py-2 rounded-md font-semibold text-foreground select-all tracking-wider">
										{createdResult.temporaryPassword}
									</div>
									<Button
										type="button"
										variant="outline"
										size="sm"
										onClick={handleCopy}
										className="shrink-0 gap-1.5"
									>
										{copied ? (
											<>
												<Check className="h-4 w-4 text-emerald-600" />
												<span>Tersalin</span>
											</>
										) : (
											<>
												<Copy className="h-4 w-4" />
												<span>Salin</span>
											</>
										)}
									</Button>
								</div>
							</div>
						</div>

						<Alert className="border-amber-200 bg-amber-50 dark:border-amber-800/60 dark:bg-amber-950/40 text-amber-900 dark:text-amber-200">
							<AlertTriangle className="h-4 w-4 text-amber-600 dark:text-amber-400" />
							<AlertDescription className="text-xs leading-relaxed">
								<strong>Penting:</strong> Sampaikan kata sandi ini langsung
								kepada pengguna. Kata sandi hanya ditampilkan sekali dan tidak
								dikirimkan melalui email. Pengguna wajib menggantinya saat login
								pertama.
							</AlertDescription>
						</Alert>

						<DialogFooter className="pt-2">
							<Button onClick={handleClose} className="w-full">
								Selesai & Tutup
							</Button>
						</DialogFooter>
					</div>
				) : (
					<form
						onSubmit={(e) => {
							e.preventDefault();
							e.stopPropagation();
							form.handleSubmit();
						}}
						className="space-y-4 py-2"
					>
						{serverError && (
							<Alert variant="destructive">
								<AlertDescription className="text-xs">
									{serverError}
								</AlertDescription>
							</Alert>
						)}

						<form.Field name="fullName">
							{(field) => {
								const error = field.state.meta.errors[0]?.message;
								return (
									<FormField
										label="Nama Lengkap"
										htmlFor={field.name}
										required
										error={error}
									>
										<Input
											id={field.name}
											name={field.name}
											placeholder="contoh: Sinta Maharani"
											value={field.state.value}
											onBlur={field.handleBlur}
											onChange={(e) => field.handleChange(e.target.value)}
											className="h-9 text-sm"
										/>
									</FormField>
								);
							}}
						</form.Field>

						<div className="grid grid-cols-2 gap-3">
							<form.Field name="username">
								{(field) => {
									const error = field.state.meta.errors[0]?.message;
									return (
										<FormField
											label="Nama Pengguna"
											htmlFor={field.name}
											required
											error={error}
										>
											<Input
												id={field.name}
												name={field.name}
												placeholder="contoh: sinta.maharani"
												value={field.state.value}
												onBlur={field.handleBlur}
												onChange={(e) => field.handleChange(e.target.value)}
												className="h-9 text-sm font-mono"
											/>
										</FormField>
									);
								}}
							</form.Field>

							<form.Field name="email">
								{(field) => {
									const error = field.state.meta.errors[0]?.message;
									return (
										<FormField
											label="Email (Opsional)"
											htmlFor={field.name}
											error={error}
										>
											<Input
												id={field.name}
												name={field.name}
												type="email"
												placeholder="sinta@pgn.co.id"
												value={field.state.value || ""}
												onBlur={field.handleBlur}
												onChange={(e) => field.handleChange(e.target.value)}
												className="h-9 text-sm"
											/>
										</FormField>
									);
								}}
							</form.Field>
						</div>

						<form.Field name="role">
							{(field) => {
								const error = field.state.meta.errors[0]?.message;
								return (
									<FormField
										label="Peran Awal"
										htmlFor={field.name}
										required
										error={error}
									>
										<Select
											value={field.state.value}
											onValueChange={(val) =>
												field.handleChange(val as AppRole)
											}
										>
											<SelectTrigger
												id={field.name}
												className="w-full h-9 text-sm"
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

						{/* Scope selectors */}
						{selectedRoleMeta.scopeType !== "none" && (
							<div className="grid grid-cols-2 gap-3 p-3 bg-muted/40 rounded-lg border">
								<form.Field name="regionId">
									{(field) => {
										const error = field.state.meta.errors[0]?.message;
										return (
											<FormField label="Wilayah (SOR)" required error={error}>
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
								</form.Field>

								{selectedRoleMeta.scopeType === "area" && (
									<form.Field name="areaId">
										{(field) => {
											const error = field.state.meta.errors[0]?.message;
											return (
												<FormField label="Sales Area" required error={error}>
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
							</div>
						)}

						<div className="text-xs text-muted-foreground flex items-start gap-2 bg-muted/20 p-2.5 rounded-md border">
							<KeyRound className="h-4 w-4 text-primary shrink-0 mt-0.5" />
							<span>
								Kata sandi sementara akan dibuatkan otomatis oleh sistem dan
								ditampilkan setelah Anda menekan tombol Buat Pengguna.
							</span>
						</div>

						<DialogFooter className="pt-2">
							<Button
								type="button"
								variant="outline"
								onClick={handleClose}
								disabled={createMutation.isPending}
							>
								Batal
							</Button>
							<form.Subscribe
								selector={(state) => [state.canSubmit, state.isSubmitting]}
							>
								{([canSubmit, isSubmitting]) => (
									<Button type="submit" disabled={!canSubmit || isSubmitting}>
										{isSubmitting ? (
											<>
												<Loader2 className="h-4 w-4 animate-spin mr-2" />
												Menyimpan...
											</>
										) : (
											"Buat Pengguna"
										)}
									</Button>
								)}
							</form.Subscribe>
						</DialogFooter>
					</form>
				)}
			</DialogContent>
		</Dialog>
	);
}
