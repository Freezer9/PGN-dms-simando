import { useForm } from "@tanstack/react-form";
import { useQueryClient } from "@tanstack/react-query";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { AlertCircle, Lock, ShieldCheck, User } from "lucide-react";
import * as React from "react";
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
import { guestOnly } from "@/lib/auth-middleware";

const searchSchema = z.object({
	redirect: z.string().optional(),
});

export const Route = createFileRoute("/sign-in")({
	validateSearch: searchSchema,
	beforeLoad: guestOnly,
	component: SignInPage,
});

const loginSchema = z.object({
	username: z.string().min(1, "Nama pengguna atau email wajib diisi"),
	password: z.string().min(1, "Kata sandi wajib diisi"),
});

function SignInPage() {
	const queryClient = useQueryClient();
	const navigate = useNavigate();
	const search = Route.useSearch();
	const [errorMessage, setErrorMessage] = React.useState<string | null>(null);

	const loginMutation = $api.useMutation("post", "/api/auth/login", {
		onSuccess: (data) => {
			queryClient.setQueryData(
				$api.queryOptions("get", "/api/auth/me").queryKey,
				data,
			);
			if (data.mustChangePassword) {
				navigate({ to: "/change-password" });
			} else {
				navigate({ to: search.redirect || "/" });
			}
		},
		onError: (error) => {
			if (error.status === 423) {
				setErrorMessage(
					"Akun terkunci karena terlalu banyak percobaan masuk yang gagal. Silakan coba lagi beberapa saat lagi.",
				);
			} else if (error.status === 401) {
				setErrorMessage("Nama pengguna atau kata sandi tidak valid.");
			} else if (error.detail) {
				setErrorMessage(error.detail);
			} else {
				setErrorMessage(
					"Gagal masuk ke sistem. Periksa kembali kredensial Anda.",
				);
			}
		},
	});

	const form = useForm({
		defaultValues: {
			username: "",
			password: "",
		},
		validators: {
			onSubmit: loginSchema,
		},
		onSubmit: async ({ value }) => {
			setErrorMessage(null);
			await loginMutation.mutateAsync({ body: value });
		},
	});

	return (
		<div className="min-h-screen flex items-center justify-center bg-muted/40 p-4">
			<div className="w-full max-w-md space-y-6">
				{/* Logo & Header */}
				<div className="text-center space-y-2">
					<div className="inline-flex items-center justify-center size-12 rounded-xl bg-primary text-primary-foreground shadow-lg shadow-primary/20 mb-2">
						<ShieldCheck className="size-6" />
					</div>
					<h1 className="text-2xl font-bold tracking-tight text-foreground">
						DMS Simando
					</h1>
					<p className="text-xs text-muted-foreground">
						PT Perusahaan Gas Negara Tbk • Single Sign-On Portal
					</p>
				</div>

				<Card className="border-border/60 shadow-md">
					<CardHeader className="space-y-1 text-center pb-4">
						<CardTitle className="text-lg font-semibold">
							Masuk ke Akun
						</CardTitle>
						<CardDescription className="text-xs">
							Masukkan nama pengguna dan kata sandi Anda untuk melanjutkan
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
							{errorMessage && (
								<div className="p-3 rounded-md bg-destructive/10 border border-destructive/20 text-destructive text-xs flex items-start gap-2">
									<AlertCircle className="size-4 shrink-0 mt-0.5" />
									<span>{errorMessage}</span>
								</div>
							)}

							<form.Field name="username">
								{(field) => {
									const error = field.state.meta.errors[0]?.message;
									return (
										<FormField
											label="Nama Pengguna / Email"
											required
											error={error}
										>
											<div className="relative">
												<User className="absolute left-3 top-2.5 size-4 text-muted-foreground" />
												<Input
													id={field.name}
													name={field.name}
													value={field.state.value}
													onBlur={field.handleBlur}
													onChange={(e) => field.handleChange(e.target.value)}
													placeholder="admin@pgn.co.id"
													className="pl-9 h-10 text-sm"
													autoComplete="username"
												/>
											</div>
										</FormField>
									);
								}}
							</form.Field>

							<form.Field name="password">
								{(field) => {
									const error = field.state.meta.errors[0]?.message;
									return (
										<FormField label="Kata Sandi" required error={error}>
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
										{isSubmitting ? "Memproses Masuk..." : "Masuk"}
									</Button>
								)}
							</form.Subscribe>

							<p className="text-[11px] text-center text-muted-foreground">
								Lupa kata sandi? Hubungi administrator sistem Anda untuk
								pengaturan ulang.
							</p>
						</CardFooter>
					</form>
				</Card>
			</div>
		</div>
	);
}
