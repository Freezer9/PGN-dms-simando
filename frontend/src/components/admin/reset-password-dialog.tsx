import {
	AlertTriangle,
	Check,
	Copy,
	KeyRound,
	Loader2,
	RotateCcw,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { UserListItemDto } from "@/api/types";
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

interface ResetPasswordDialogProps {
	user: UserListItemDto | null;
	open: boolean;
	onOpenChange: (open: boolean) => void;
	onSuccess: () => void;
}

export function ResetPasswordDialog({
	user,
	open,
	onOpenChange,
	onSuccess,
}: ResetPasswordDialogProps) {
	const [tempPassword, setTempPassword] = React.useState<string | null>(null);
	const [error, setError] = React.useState<string | null>(null);
	const [copied, setCopied] = React.useState(false);

	const resetMutation = $api.useMutation(
		"post",
		"/api/admin/users/{id}/reset-password",
		{
			onSuccess: (data) => {
				setTempPassword(data.temporaryPassword);
				setError(null);
				onSuccess();
			},
			onError: (err: unknown) => {
				const errorObj = err as { error?: string; errors?: string[] };
				const msg =
					errorObj?.error ||
					errorObj?.errors?.[0] ||
					"Gagal mengatur ulang kata sandi.";
				setError(msg);
			},
		},
	);

	const handleClose = () => {
		setTempPassword(null);
		setError(null);
		setCopied(false);
		onOpenChange(false);
	};

	const handleConfirmReset = () => {
		if (!user) return;
		resetMutation.mutate({
			params: {
				path: { id: user.id },
			},
		});
	};

	const handleCopy = async () => {
		if (!tempPassword) return;
		try {
			await navigator.clipboard.writeText(tempPassword);
			setCopied(true);
			setTimeout(() => setCopied(false), 2000);
		} catch {
			// Fallback copy
		}
	};

	if (!user) return null;

	return (
		<Dialog open={open} onOpenChange={handleClose}>
			<DialogContent className="sm:max-w-[460px]">
				<DialogHeader>
					<DialogTitle className="flex items-center gap-2">
						<KeyRound className="h-5 w-5 text-primary" />
						<span>Atur Ulang Kata Sandi</span>
					</DialogTitle>
					<DialogDescription>
						{tempPassword
							? "Kata sandi baru berhasil dibuat."
							: `Apakah Anda yakin ingin mengatur ulang kata sandi untuk ${user.fullName}?`}
					</DialogDescription>
				</DialogHeader>

				{tempPassword ? (
					<div className="space-y-4 py-2">
						<div className="rounded-lg border bg-muted/30 p-4 space-y-3">
							<div className="text-xs text-muted-foreground">
								Pengguna:{" "}
								<strong className="text-foreground">{user.fullName}</strong> (
								{user.username})
							</div>

							<div className="space-y-1.5">
								<Label className="text-xs font-semibold text-foreground">
									Kata Sandi Sementara Baru:
								</Label>
								<div className="flex items-center gap-2">
									<div className="flex-1 font-mono text-sm bg-background border px-3 py-2 rounded-md font-semibold text-foreground select-all tracking-wider">
										{tempPassword}
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
								Sampaikan kata sandi ini langsung kepada pengguna. Pengguna
								diwajibkan untuk mengganti kata sandi saat masuk kembali.
							</AlertDescription>
						</Alert>

						<DialogFooter className="pt-2">
							<Button onClick={handleClose} className="w-full">
								Selesai & Tutup
							</Button>
						</DialogFooter>
					</div>
				) : (
					<div className="space-y-4 py-2">
						{error && (
							<Alert variant="destructive">
								<AlertDescription className="text-xs">{error}</AlertDescription>
							</Alert>
						)}

						<p className="text-xs text-muted-foreground leading-relaxed">
							Tindakan ini akan membatalkan kata sandi pengguna saat ini dan
							menghasilkan kata sandi acak sementara. Pengguna akan diminta
							membuat kata sandi baru pada saat sesi login berikutnya.
						</p>

						<DialogFooter className="pt-2">
							<Button
								type="button"
								variant="outline"
								onClick={handleClose}
								disabled={resetMutation.isPending}
							>
								Batal
							</Button>
							<Button
								type="button"
								variant="destructive"
								onClick={handleConfirmReset}
								disabled={resetMutation.isPending}
								className="gap-1.5"
							>
								{resetMutation.isPending ? (
									<>
										<Loader2 className="h-4 w-4 animate-spin" />
										<span>Memproses...</span>
									</>
								) : (
									<>
										<RotateCcw className="h-4 w-4" />
										<span>Buat Kata Sandi Baru</span>
									</>
								)}
							</Button>
						</DialogFooter>
					</div>
				)}
			</DialogContent>
		</Dialog>
	);
}
