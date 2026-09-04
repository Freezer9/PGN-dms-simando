import { useForm } from "@tanstack/react-form";
import { useQueryClient } from "@tanstack/react-query";
import {
	AlertTriangle,
	Award,
	CheckCircle,
	Loader2,
	Plus,
	Save,
	Trash2,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	NolIssuanceDetail,
	NolOutcome,
	SaveNolIssuanceRequest,
} from "@/api/types";
import { IconButton } from "@/components/common";
import { DocumentDownloadButton } from "@/components/documents/document-download-buttons";
import { FormField } from "@/components/form/form-field";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
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
import { type NolIssuanceFormValues, nolIssuanceSchema } from "@/lib/schemas";

interface NolIssuanceFormProps {
	companyId: string;
	initialData?: NolIssuanceDetail | null;
	canEdit?: boolean;
	onSaved?: () => void;
}

function getDefaultValues(
	initialData?: NolIssuanceDetail | null,
): NolIssuanceFormValues {
	return {
		outcome: initialData?.outcome || "Nol",
		nomorNotaDinas: initialData?.nomorNotaDinas || "",
		berlakuSejak: initialData?.berlakuSejak || "",
		berlakuSampai: initialData?.berlakuSampai || "",
		kontrakBersyarat: initialData?.kontrakBersyarat || [],
		approvedTerms:
			initialData?.approvedTerms?.map((t, idx) => ({
				id: t.id || crypto.randomUUID(),
				periodeMulai: t.periodeMulai,
				periodeSelesai: t.periodeSelesai,
				rataRata: Number(t.rataRata) || 0,
				kontrakMinimum: Number(t.kontrakMinimum) || 0,
				kontrakMaksimum: Number(t.kontrakMaksimum) || 0,
				sortOrder: idx + 1,
			})) || [],
	};
}

export function NolIssuanceForm({
	companyId,
	initialData,
	canEdit = true,
	onSaved,
}: NolIssuanceFormProps) {
	const queryClient = useQueryClient();

	// Save Mutation
	const saveMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/nol-issuance",
		{
			onSuccess: () => {
				toast.success("Penerbitan Surat NOL/RL berhasil disimpan!");
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
						"/api/companies/{id}/nol-issuance",
						{ params: { path: { id: companyId } } },
					],
				});
				onSaved?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error
						? error.message
						: "Gagal menyimpan Penerbitan NOL",
				);
			},
		},
	);

	const form = useForm({
		defaultValues: getDefaultValues(initialData),
		validators: {
			onSubmit: nolIssuanceSchema,
		},
		onSubmit: async ({ value }) => {
			const request: SaveNolIssuanceRequest = {
				outcome: (value.outcome as NolOutcome) || "Nol",
				nomorNotaDinas: value.nomorNotaDinas || null,
				kontrakBersyarat: value.kontrakBersyarat || [],
				berlakuSejak: value.berlakuSejak || null,
				berlakuSampai: value.berlakuSampai || null,
				documentId: initialData?.documentId || null,
				approvedTerms: (value.approvedTerms || []).map((t, idx) => ({
					id: t.id || crypto.randomUUID(),
					periodeMulai: t.periodeMulai || "",
					periodeSelesai: t.periodeSelesai || "",
					rataRata: Number(t.rataRata) || 0,
					kontrakMinimum: Number(t.kontrakMinimum) || 0,
					kontrakMaksimum: Number(t.kontrakMaksimum) || 0,
					sortOrder: idx + 1,
				})),
			};

			await saveMutation.mutateAsync({
				params: { path: { id: companyId } },
				body: request,
			});
		},
	});

	// Synchronize when initialData changes
	React.useEffect(() => {
		if (initialData) {
			form.reset(getDefaultValues(initialData));
		}
	}, [initialData, form]);

	return (
		<form
			onSubmit={(e) => {
				e.preventDefault();
				e.stopPropagation();
				form.handleSubmit();
			}}
			className="space-y-6"
		>
			{/* Top Bar Summary / Save */}
			<div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 bg-muted/40 rounded-lg border">
				<div className="flex items-center gap-3">
					<div className="size-10 rounded-full bg-emerald-100 dark:bg-emerald-900/40 text-emerald-600 flex items-center justify-center">
						<Award className="size-5" />
					</div>
					<div>
						<h3 className="text-sm font-semibold">
							Penerbitan Surat Kesiapan Pasokan Gas (Surat NOL / RL)
						</h3>
						<p className="text-xs text-muted-foreground">
							Surat resmi persetujuan pasokan gas bumi (NOL) atau surat
							tanggapan (RL) beserta ketentuan akhir
						</p>
					</div>
				</div>

				<div className="flex items-center gap-2">
					<DocumentDownloadButton
						companyId={companyId}
						documentType="nol-issuance"
						label="Unduh Surat Penerbitan (.docx)"
					/>
					{canEdit && (
						<form.Subscribe
							selector={(state) => [state.canSubmit, state.isSubmitting]}
						>
							{([canSubmit, isSubmitting]) => (
								<Button
									type="submit"
									size="sm"
									disabled={!canSubmit || isSubmitting}
									className="h-9 text-xs flex items-center gap-1.5 bg-emerald-600 hover:bg-emerald-700 text-white"
								>
									{isSubmitting ? (
										<Loader2 className="size-3.5 animate-spin" />
									) : (
										<Save className="size-3.5" />
									)}
									Simpan Penerbitan
								</Button>
							)}
						</form.Subscribe>
					)}
				</div>
			</div>

			{/* SECTION 1: KEPUTUSAN & NOMOR NOTA DINAS */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold">
						1. Keputusan Akhir & Administrasi Surat Resmi
					</CardTitle>
					<CardDescription className="text-xs">
						Status penerbitan surat NOL (No Objection Letter) / RL (Refusal
						Letter) dan nota dinas divisi
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Hasil / Outcome */}
						<form.Field name="outcome">
							{(field) => (
								<FormField label="Keputusan Akhir">
									<Select
										value={field.state.value || "Nol"}
										onValueChange={(val) =>
											field.handleChange(val as NolOutcome)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9 font-semibold">
											<SelectValue />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="Nol">
												<div className="flex items-center gap-2 text-emerald-600">
													<CheckCircle className="size-3.5" /> Surat NOL
													(Disetujui)
												</div>
											</SelectItem>
											<SelectItem value="Rl">
												<div className="flex items-center gap-2 text-amber-600">
													<AlertTriangle className="size-3.5" /> Surat RL
													(Ditolak)
												</div>
											</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Nomor Nota Dinas */}
						<form.Field name="nomorNotaDinas">
							{(field) => (
								<FormField label="Nomor Nota Dinas Divisi">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: ND-108/PGN/COM/2026"
										disabled={!canEdit}
										className="text-xs h-9 font-mono"
									/>
								</FormField>
							)}
						</form.Field>
					</div>

					{/* Masa Berlaku Surat */}
					<div className="pt-2 border-t">
						<Label className="text-xs font-semibold mb-2 block text-muted-foreground">
							Masa Berlaku Surat NOL
						</Label>
						<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
							<form.Field name="berlakuSejak">
								{(field) => (
									<FormField label="Berlaku Sejak">
										<Input
											type="date"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>

							<form.Field name="berlakuSampai">
								{(field) => (
									<FormField label="Berlaku Sampai (Maks 6 Bulan)">
										<Input
											type="date"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 2: APPROVED TERMS TABLE */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold">
							2. Ketentuan Volume Gas yang Disetujui
						</CardTitle>
						<CardDescription className="text-xs">
							Volume pasokan terjamin per periode kontrak (MMBTUD)
						</CardDescription>
					</div>
					{canEdit && (
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={() => {
								const current = form.getFieldValue("approvedTerms") || [];
								form.setFieldValue("approvedTerms", [
									...current,
									{
										id: crypto.randomUUID(),
										periodeMulai: "",
										periodeSelesai: "",
										rataRata: 0,
										kontrakMinimum: 0,
										kontrakMaksimum: 0,
										sortOrder: current.length + 1,
									},
								]);
							}}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" /> Tambah Ketentuan
						</Button>
					)}
				</CardHeader>
				<CardContent>
					<form.Field name="approvedTerms">
						{(field) => {
							const terms = field.state.value || [];
							return (
								<div className="border rounded-lg overflow-x-auto">
									<Table>
										<TableHeader className="bg-muted/50">
											<TableRow>
												<TableHead className="text-xs font-semibold min-w-[130px]">
													Periode Mulai
												</TableHead>
												<TableHead className="text-xs font-semibold min-w-[130px]">
													Periode Selesai
												</TableHead>
												<TableHead className="text-xs font-semibold min-w-[130px]">
													Vol Rata-rata
												</TableHead>
												<TableHead className="text-xs font-semibold min-w-[120px]">
													Vol Minimum
												</TableHead>
												<TableHead className="text-xs font-semibold min-w-[120px]">
													Vol Maksimum
												</TableHead>
												{canEdit && (
													<TableHead className="text-xs font-semibold w-12 text-center">
														Aksi
													</TableHead>
												)}
											</TableRow>
										</TableHeader>
										<TableBody>
											{terms.length === 0 ? (
												<TableRow>
													<TableCell
														colSpan={canEdit ? 6 : 5}
														className="h-20 text-center text-xs text-muted-foreground"
													>
														Belum ada ketentuan volume. Klik "+ Tambah
														Ketentuan" untuk menambahkan.
													</TableCell>
												</TableRow>
											) : (
												terms.map((row, idx) => (
													<TableRow key={row.id || `term-${idx}`}>
														<TableCell>
															<Input
																type="date"
																value={row.periodeMulai ?? ""}
																onChange={(e) => {
																	const next = [...terms];
																	next[idx] = {
																		...row,
																		periodeMulai: e.target.value,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8"
															/>
														</TableCell>
														<TableCell>
															<Input
																type="date"
																value={row.periodeSelesai ?? ""}
																onChange={(e) => {
																	const next = [...terms];
																	next[idx] = {
																		...row,
																		periodeSelesai: e.target.value,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8"
															/>
														</TableCell>
														<TableCell>
															<Input
																type="number"
																step="0.01"
																value={
																	row.rataRata != null
																		? String(row.rataRata)
																		: ""
																}
																onChange={(e) => {
																	const next = [...terms];
																	next[idx] = {
																		...row,
																		rataRata: e.target.value
																			? Number(e.target.value)
																			: 0,
																	};
																	field.handleChange(next);
																}}
																placeholder="1000"
																disabled={!canEdit}
																className="text-xs h-8 font-mono font-medium"
															/>
														</TableCell>
														<TableCell>
															<Input
																type="number"
																step="0.01"
																value={
																	row.kontrakMinimum != null
																		? String(row.kontrakMinimum)
																		: ""
																}
																onChange={(e) => {
																	const next = [...terms];
																	next[idx] = {
																		...row,
																		kontrakMinimum: e.target.value
																			? Number(e.target.value)
																			: 0,
																	};
																	field.handleChange(next);
																}}
																placeholder="800"
																disabled={!canEdit}
																className="text-xs h-8 font-mono"
															/>
														</TableCell>
														<TableCell>
															<Input
																type="number"
																step="0.01"
																value={
																	row.kontrakMaksimum != null
																		? String(row.kontrakMaksimum)
																		: ""
																}
																onChange={(e) => {
																	const next = [...terms];
																	next[idx] = {
																		...row,
																		kontrakMaksimum: e.target.value
																			? Number(e.target.value)
																			: 0,
																	};
																	field.handleChange(next);
																}}
																placeholder="1200"
																disabled={!canEdit}
																className="text-xs h-8 font-mono"
															/>
														</TableCell>
														{canEdit && (
															<TableCell className="text-center">
																<IconButton
																	type="button"
																	tooltip="Hapus Ketentuan"
																	danger
																	className="size-7"
																	onClick={() => {
																		field.handleChange(
																			terms.filter((_, i) => i !== idx),
																		);
																	}}
																	aria-label="Hapus Ketentuan"
																>
																	<Trash2 className="size-3.5" />
																</IconButton>
															</TableCell>
														)}
													</TableRow>
												))
											)}
										</TableBody>
									</Table>
								</div>
							);
						}}
					</form.Field>
				</CardContent>
			</Card>

			{/* SECTION 3: SYARAT & KETENTUAN TAMBAHAN (KONTRAK BERSYARAT) */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold">
							3. Syarat & Ketentuan Tambahan (Kontrak Bersyarat)
						</CardTitle>
						<CardDescription className="text-xs">
							Klausul khusus yang wajib dipenuhi pelanggan sebelum gas in /
							pengaliran
						</CardDescription>
					</div>
					{canEdit && (
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={() => {
								const current = form.getFieldValue("kontrakBersyarat") || [];
								form.setFieldValue("kontrakBersyarat", [...current, ""]);
							}}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" /> Tambah Klausul Syarat
						</Button>
					)}
				</CardHeader>
				<CardContent className="space-y-3">
					<form.Field name="kontrakBersyarat">
						{(field) => {
							const list = field.state.value || [];
							if (list.length === 0) {
								return (
									<p className="text-xs text-muted-foreground text-center py-4">
										Tidak ada klausul syarat tambahan khusus.
									</p>
								);
							}
							return (
								<div className="space-y-2">
									{list.map((item, idx) => (
										// biome-ignore lint/suspicious/noArrayIndexKey: string list
										<div key={idx} className="flex items-center gap-2">
											<span className="text-xs font-mono text-muted-foreground w-6 text-right">
												{idx + 1}.
											</span>
											<Input
												value={item}
												onChange={(e) => {
													const next = [...list];
													next[idx] = e.target.value;
													field.handleChange(next);
												}}
												placeholder="contoh: Pelanggan wajib menyerahkan Jaminan Pembayaran 14 hari sebelum gas in"
												disabled={!canEdit}
												className="text-xs h-8 flex-1"
											/>
											{canEdit && (
												<Button
													type="button"
													variant="ghost"
													size="icon"
													onClick={() => {
														field.handleChange(
															list.filter((_, i) => i !== idx),
														);
													}}
													className="size-7 text-destructive hover:text-destructive"
												>
													<Trash2 className="size-3.5" />
												</Button>
											)}
										</div>
									))}
								</div>
							);
						}}
					</form.Field>
				</CardContent>
			</Card>

			{canEdit && (
				<div className="flex justify-end pt-2">
					<form.Subscribe
						selector={(state) => [state.canSubmit, state.isSubmitting]}
					>
						{([canSubmit, isSubmitting]) => (
							<Button
								type="submit"
								disabled={!canSubmit || isSubmitting}
								className="h-9 text-xs flex items-center gap-1.5 bg-emerald-600 hover:bg-emerald-700 text-white"
							>
								{isSubmitting ? (
									<Loader2 className="size-3.5 animate-spin" />
								) : (
									<Save className="size-3.5" />
								)}
								Simpan Penerbitan
							</Button>
						)}
					</form.Subscribe>
				</div>
			)}
		</form>
	);
}
