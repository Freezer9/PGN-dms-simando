import { useForm } from "@tanstack/react-form";
import { useQueryClient } from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { AlertCircle, CheckCircle2, KeyRound, Lock } from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { z } from "zod";
import { $api } from "@/api/client";
import { FormField } from "@/components/form/form-field";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardFooter,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { useAuth } from "@/lib/auth";
import { protectedRoute } from "@/lib/auth-middleware";

export const Route = createFileRoute("/change-password")({
	beforeLoad: protectedRoute,
	component: ChangePasswordPage,
});

const changePasswordSchema = z
	.object({
		currentPassword: z.string().min(1, "Kata sandi saat ini wajib diisi"),
		newPassword: z
			.string()
			.min(8, "Kata sandi baru minimal 8 karakter")
			.regex(/[A-Z]/, "Harus mengandung huruf besar")
			.regex(/[a-z]/, "Harus mengandung huruf kecil")
			.regex(/[0-9]/, "Harus mengandung angka"),
		confirmPassword: z.string().min(1, "Konfirmasi kata sandi wajib diisi"),
	})
	.refine((data) => data.newPassword === data.confirmPassword, {
		message: "Konfirmasi kata sandi tidak cocok dengan kata sandi baru",
		path: ["confirmPassword"],
	});

function ChangePasswordPage() {
	const { user } = useAuth();
	const queryClient = useQueryClient();
	const navigate = useNavigate();
	const [errorMessage, setErrorMessage] = React.useState<string | null>(null);

	const isForced = user?.mustChangePassword;

	const changePasswordMutation = $api.useMutation(
		"post",
		"/api/auth/change-password",
		{
			onSuccess: (data) => {
				queryClient.setQueryData(
					$api.queryOptions("get", "/api/auth/me").queryKey,
					data,
				);
				toast.success("Kata sandi berhasil diperbarui!");
				navigate({ to: "/" });
			},
			onError: (error) => {
				if (error.detail) {
					setErrorMessage(error.detail);
				} else {
					setErrorMessage(
						"Gagal memperbarui kata sandi. Periksa kembali input Anda.",
					);
				}
			},
		},
	);

	const form = useForm({
		defaultValues: {
			currentPassword: "",
			newPassword: "",
			confirmPassword: "",
		},
		validators: {
			onSubmit: changePasswordSchema,
		},
		onSubmit: async ({ value }) => {
			setErrorMessage(null);
			await changePasswordMutation.mutateAsync({ body: value });
		},
	});

	return (
		<div className="min-h-screen flex items-center justify-center bg-muted/40 p-4">
			<div className="w-full max-w-md space-y-6">
				<div className="text-center space-y-2">
					<div className="inline-flex items-center justify-center size-12 rounded-xl bg-primary text-primary-foreground shadow-lg shadow-primary/20 mb-2">
						<KeyRound className="size-6" />
					</div>
					<h1 className="text-2xl font-bold tracking-tight text-foreground">
						Ubah Kata Sandi
					</h1>
					<p className="text-xs text-muted-foreground">
						Perbarui kata sandi akun DMS Simando Anda
					</p>
				</div>

				<Card className="border-border/60 shadow-md">
					<CardHeader className="space-y-1 text-center pb-4">
						<CardTitle className="text-lg font-semibold">
							{isForced ? "Pembaruan Kata Sandi Wajib" : "Ganti Kata Sandi"}
						</CardTitle>
						<CardDescription className="text-xs">
							{isForced
								? "Anda harus memperbarui kata sandi sebelum dapat mengakses fitur sistem."
								: "Masukkan kata sandi saat ini dan tentukan kata sandi baru Anda."}
						</CardDescription>
					</CardHeader>

					<form
						onSubmit={(e) => {
							e.preventDefault();
							e.stopPropagation();
							form.handleSubmit();
						}}
					>
						<CardContent className="space-y-4 pt-0">
							{isForced && (
								<div className="p-3 rounded-md bg-amber-500/10 border border-amber-500/20 text-amber-600 text-xs flex items-start gap-2">
									<AlertCircle className="size-4 shrink-0 mt-0.5" />
									<span>
										Akun Anda memerlukan pengaturan ulang kata sandi sebelum
										melanjutkan.
									</span>
								</div>
							)}

							{errorMessage && (
								<div className="p-3 rounded-md bg-destructive/10 border border-destructive/20 text-destructive text-xs flex items-start gap-2">
									<AlertCircle className="size-4 shrink-0 mt-0.5" />
									<span>{errorMessage}</span>
								</div>
							)}

							<form.Field name="currentPassword">
								{(field) => {
									const error = field.state.meta.errors[0]?.message;
									return (
										<FormField
											label="Kata Sandi Saat Ini"
											required
											error={error}
										>
											<div className="relative">
												<Lock className="absolute left-3 top-2.5 size-4 text-muted-foreground" />
												<Input
													type="password"
													id={field.name}
													name={field.name}
													value={field.state.value}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													placeholder="••••••••"
													className="pl-9 h-10 text-sm"
													autoComplete="current-password"
												/>
											</div>
										</FormField>
									);
								}}
							</form.Field>

							<form.Field name="newPassword">
								{(field) => {
									const error = field.state.meta.errors[0]?.message;
									return (
										<FormField
											label="Kata Sandi Baru"
											required
											error={error}
											description="Minimal 8 karakter, kombinasi huruf besar, huruf kecil, dan angka."
										>
											<div className="relative">
												<Lock className="absolute left-3 top-2.5 size-4 text-muted-foreground" />
												<Input
													type="password"
													id={field.name}
													name={field.name}
													value={field.state.value}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													placeholder="••••••••"
													className="pl-9 h-10 text-sm"
													autoComplete="new-password"
												/>
											</div>
										</FormField>
									);
								}}
							</form.Field>

							<form.Field name="confirmPassword">
								{(field) => {
									const error = field.state.meta.errors[0]?.message;
									return (
										<FormField
											label="Konfirmasi Kata Sandi Baru"
											required
											error={error}
										>
											<div className="relative">
												<Lock className="absolute left-3 top-2.5 size-4 text-muted-foreground" />
												<Input
													type="password"
													id={field.name}
													name={field.name}
													value={field.state.value}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													placeholder="••••••••"
													className="pl-9 h-10 text-sm"
													autoComplete="new-password"
												/>
											</div>
										</FormField>
									);
								}}
							</form.Field>
						</CardContent>

						<CardFooter className="flex flex-col space-y-4 pt-2">
							<form.Subscribe
								selector={(state) => [state.canSubmit, state.isSubmitting]}
							>
								{([canSubmit, isSubmitting]) => (
									<Button
										type="submit"
										className="w-full h-10 font-medium"
										disabled={!canSubmit || isSubmitting}
									>
										{isSubmitting ? (
											"Menyimpan..."
										) : (
											<span className="flex items-center gap-1.5">
												<CheckCircle2 className="size-4" /> Simpan Kata Sandi
											</span>
										)}
									</Button>
								)}
							</form.Subscribe>

							{!isForced && (
								<Button
									type="button"
									variant="ghost"
									className="w-full h-9 text-xs"
									onClick={() => navigate({ to: "/" })}
								>
									Batal dan Kembali
								</Button>
							)}
						</CardFooter>
					</form>
				</Card>
			</div>
		</div>
	);
}
