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
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/lib/auth";

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

	const [fullName, setFullName] = React.useState("");
	const [username, setUsername] = React.useState("");
	const [email, setEmail] = React.useState("");
	const [role, setRole] = React.useState<AppRole>("SalesArea");
	const [regionId, setRegionId] = React.useState<string>("");
	const [areaId, setAreaId] = React.useState<string>("");
	const [error, setError] = React.useState<string | null>(null);
	const [createdResult, setCreatedResult] =
		React.useState<CreateUserResponse | null>(null);
	const [copied, setCopied] = React.useState(false);

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

	// Auto-select region if only 1 is available (e.g. for regional admin)
	React.useEffect(() => {
		if (regions.length > 0 && !regionId) {
			setRegionId(regions[0].id);
		}
	}, [regions, regionId]);

	// Auto-select first area when region changes
	React.useEffect(() => {
		if (availableAreas.length > 0 && !areaId) {
			setAreaId(availableAreas[0].id);
		}
	}, [availableAreas, areaId]);

	const selectedRoleMeta =
		ALL_ROLES.find((r) => r.value === role) || ALL_ROLES[0];

	const createMutation = $api.useMutation("post", "/api/admin/users", {
		onSuccess: (data) => {
			setCreatedResult(data);
			onSuccess();
		},
		onError: (error) => {
			const msg =
				error.detail ||
				error.title ||
				"Gagal membuat pengguna. Pastikan nama pengguna belum digunakan.";
			setError(msg);
		},
	});

	const handleReset = () => {
		setFullName("");
		setUsername("");
		setEmail("");
		setRole("SalesArea");
		setError(null);
		setCreatedResult(null);
		setCopied(false);
	};

	const handleClose = () => {
		handleReset();
		onOpenChange(false);
	};

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();
		setError(null);

		if (!fullName.trim()) {
			setError("Nama lengkap wajib diisi.");
			return;
		}
		if (!username.trim()) {
			setError("Nama pengguna (username) wajib diisi.");
			return;
		}

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

		createMutation.mutate({
			body: {
				fullName: fullName.trim(),
				username: username.trim().toLowerCase(),
				email: email.trim() || null,
				role: role,
				areaId: reqAreaId || null,
				regionId: reqRegionId || null,
			},
		});
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
					<form onSubmit={handleSubmit} className="space-y-4 py-2">
						{error && (
							<Alert variant="destructive">
								<AlertDescription className="text-xs">{error}</AlertDescription>
							</Alert>
						)}

						<div className="space-y-1.5">
							<Label htmlFor="fullname" className="text-xs font-medium">
								Nama Lengkap <span className="text-destructive">*</span>
							</Label>
							<Input
								id="fullname"
								placeholder="contoh: Sinta Maharani"
								value={fullName}
								onChange={(e) => setFullName(e.target.value)}
								className="h-9 text-sm"
								required
							/>
						</div>

						<div className="grid grid-cols-2 gap-3">
							<div className="space-y-1.5">
								<Label htmlFor="username" className="text-xs font-medium">
									Nama Pengguna <span className="text-destructive">*</span>
								</Label>
								<Input
									id="username"
									placeholder="contoh: sinta.maharani"
									value={username}
									onChange={(e) => setUsername(e.target.value)}
									className="h-9 text-sm font-mono"
									required
								/>
							</div>

							<div className="space-y-1.5">
								<Label htmlFor="email" className="text-xs font-medium">
									Email (Opsional)
								</Label>
								<Input
									id="email"
									type="email"
									placeholder="sinta@pgn.co.id"
									value={email}
									onChange={(e) => setEmail(e.target.value)}
									className="h-9 text-sm"
								/>
							</div>
						</div>

						<div className="space-y-1.5">
							<Label htmlFor="role" className="text-xs font-medium">
								Peran Awal <span className="text-destructive">*</span>
							</Label>
							<select
								id="role"
								value={role}
								onChange={(e) => setRole(e.target.value as AppRole)}
								className="w-full h-9 px-3 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-ring"
							>
								{allowedRoles.map((r) => (
									<option key={r.value} value={r.value}>
										{r.label}
									</option>
								))}
							</select>
						</div>

						{/* Scope selectors */}
						{selectedRoleMeta.scopeType !== "none" && (
							<div className="grid grid-cols-2 gap-3 p-3 bg-muted/40 rounded-lg border">
								<div className="space-y-1.5">
									<Label htmlFor="region" className="text-xs font-medium">
										Wilayah (SOR) <span className="text-destructive">*</span>
									</Label>
									<select
										id="region"
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
												{r.name} ({r.code})
											</option>
										))}
									</select>
								</div>

								{selectedRoleMeta.scopeType === "area" && (
									<div className="space-y-1.5">
										<Label htmlFor="area" className="text-xs font-medium">
											Sales Area <span className="text-destructive">*</span>
										</Label>
										<select
											id="area"
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
							<Button type="submit" disabled={createMutation.isPending}>
								{createMutation.isPending ? (
									<>
										<Loader2 className="h-4 w-4 animate-spin mr-2" />
										Menyimpan...
									</>
								) : (
									"Buat Pengguna"
								)}
							</Button>
						</DialogFooter>
					</form>
				)}
			</DialogContent>
		</Dialog>
	);
}
