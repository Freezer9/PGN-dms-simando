import { useForm } from "@tanstack/react-form";
import {
	AlertTriangle,
	Edit2,
	FolderKanban,
	Loader2,
	Plus,
	Search,
	Trash2,
} from "lucide-react";
import * as React from "react";
import { PageHeader } from "@/components/common";
import { FormField } from "@/components/form/form-field";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
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
import { TableEmptyState } from "@/components/ui/table-empty-state";
import { TablePagination } from "@/components/ui/table-pagination";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";

export interface ColumnDef<TItem> {
	key: Extract<keyof TItem, string> | string;
	header: string;
	render?: (row: TItem) => React.ReactNode;
	className?: string;
	width?: string;
}

export type FieldInputType =
	| "text"
	| "number"
	| "textarea"
	| "checkbox"
	| "select";

export interface FieldOption<TValue = string | number> {
	value: TValue;
	label: string;
}

export interface FieldDef<
	TFormData extends object = Record<string, unknown>,
	TKey extends Extract<keyof TFormData, string> = Extract<
		keyof TFormData,
		string
	>,
> {
	name: TKey;
	label: string;
	type: FieldInputType;
	required?: boolean;
	placeholder?: string;
	options?: FieldOption<
		TFormData[TKey] extends string | number ? TFormData[TKey] : string | number
	>[];
}

export interface MasterDataTableProps<
	TItem extends { id: string },
	TFormData extends object = Record<string, unknown>,
> {
	title: string;
	description: string;
	icon?: React.ComponentType<{ className?: string }>;
	data: TItem[];
	isLoading: boolean;
	columns: ColumnDef<TItem>[];
	fields: FieldDef<TFormData>[];
	onSave: (formData: TFormData, editingId?: string) => Promise<void>;
	onDelete?: (id: string) => Promise<void>;
	searchKeys?: (keyof TItem)[];
}

interface ApiErrorShape {
	detail?: string;
	error?: string;
	errors?: string[] | Record<string, string[]>;
	message?: string;
	title?: string;
}

function extractErrorMessage(err: unknown, defaultMessage: string): string {
	if (typeof err === "object" && err !== null) {
		const apiErr = err as ApiErrorShape;
		if (apiErr.detail) return apiErr.detail;
		if (apiErr.error) return apiErr.error;
		if (Array.isArray(apiErr.errors) && apiErr.errors.length > 0) {
			return String(apiErr.errors[0]);
		}
		if (
			apiErr.errors &&
			typeof apiErr.errors === "object" &&
			!Array.isArray(apiErr.errors)
		) {
			const errorsObj = apiErr.errors as Record<string, string[]>;
			const firstKey = Object.keys(errorsObj)[0];
			if (firstKey && errorsObj[firstKey]?.length) {
				return errorsObj[firstKey][0];
			}
		}
		if (apiErr.title) return apiErr.title;
		if (apiErr.message) return apiErr.message;
	}
	if (err instanceof Error) {
		return err.message;
	}
	return defaultMessage;
}

export function MasterDataTable<
	TItem extends { id: string },
	TFormData extends object = Record<string, unknown>,
>({
	title,
	description,
	icon: Icon = FolderKanban,
	data,
	isLoading,
	columns,
	fields,
	onSave,
	onDelete,
	searchKeys = ["name" as keyof TItem],
}: MasterDataTableProps<TItem, TFormData>) {
	const [searchTerm, setSearchTerm] = React.useState("");
	const [dialogOpen, setDialogOpen] = React.useState(false);
	const [editingItem, setEditingItem] = React.useState<TItem | null>(null);
	const [deleteConfirm, setDeleteConfirm] = React.useState<TItem | null>(null);
	const [error, setError] = React.useState<string | null>(null);
	const [isDeleting, setIsDeleting] = React.useState(false);

	const [page, setPage] = React.useState(1);
	const [pageSize, setPageSize] = React.useState(10);

	const initialValues = React.useMemo(() => {
		const init: Record<string, unknown> = {};
		for (const f of fields) {
			if (f.type === "checkbox") init[f.name] = true;
			else if (f.type === "number") init[f.name] = 0;
			else init[f.name] = "";
		}
		return init as unknown as TFormData;
	}, [fields]);

	const form = useForm({
		defaultValues: initialValues,
		onSubmit: async ({ value }) => {
			setError(null);
			try {
				await onSave(value, editingItem?.id);
				setDialogOpen(false);
				setEditingItem(null);
			} catch (err: unknown) {
				setError(extractErrorMessage(err, "Gagal menyimpan data master."));
			}
		},
	});

	// Filter data
	const filteredData = React.useMemo(() => {
		if (!searchTerm.trim()) return data;
		const q = searchTerm.toLowerCase();
		return data.filter((item) => {
			return searchKeys.some((k) => {
				const val = item[k];
				if (typeof val === "string") return val.toLowerCase().includes(q);
				if (typeof val === "number") return String(val).includes(q);
				return false;
			});
		});
	}, [data, searchTerm, searchKeys]);

	// Paginate data
	const totalCount = filteredData.length;
	const totalPages = Math.ceil(totalCount / pageSize) || 1;
	const paginatedData = React.useMemo(() => {
		const start = (page - 1) * pageSize;
		return filteredData.slice(start, start + pageSize);
	}, [filteredData, page, pageSize]);

	const handleOpenCreate = () => {
		setEditingItem(null);
		form.reset(initialValues);
		setError(null);
		setDialogOpen(true);
	};

	const handleOpenEdit = (item: TItem) => {
		setEditingItem(item);
		const current: Record<string, unknown> = {};
		const itemRecord = item as unknown as Record<string, unknown>;
		for (const f of fields) {
			const val = itemRecord[f.name];
			if (f.type === "checkbox") {
				current[f.name] = Boolean(val ?? true);
			} else if (f.type === "number") {
				current[f.name] = typeof val === "number" ? val : Number(val) || 0;
			} else {
				current[f.name] = val != null ? String(val) : "";
			}
		}
		form.reset(current as unknown as TFormData);
		setError(null);
		setDialogOpen(true);
	};

	const handleConfirmDelete = async () => {
		if (!deleteConfirm || !onDelete) return;
		setIsDeleting(true);
		setError(null);
		try {
			await onDelete(deleteConfirm.id);
			setDeleteConfirm(null);
		} catch (err: unknown) {
			setError(extractErrorMessage(err, "Gagal menghapus data master."));
		} finally {
			setIsDeleting(false);
		}
	};

	return (
		<div className="space-y-4">
			{/* Top Header */}
			<PageHeader
				title={title}
				description={description}
				badge={
					<span className="p-1 rounded-md bg-primary/10 text-primary">
						<Icon className="h-4 w-4" />
					</span>
				}
				actions={
					<div className="flex items-center gap-2">
						<div className="relative w-64">
							<Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
							<Input
								placeholder="Cari data..."
								value={searchTerm}
								onChange={(e) => {
									setSearchTerm(e.target.value);
									setPage(1);
								}}
								className="pl-8 h-9 text-xs"
							/>
						</div>
						<Button
							onClick={handleOpenCreate}
							size="sm"
							className="h-9 gap-1.5 text-xs shrink-0"
						>
							<Plus className="h-4 w-4" />
							<span>Tambah Data</span>
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

			{/* Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table className="w-full">
					<TableHeader className="bg-muted/40">
						<TableRow>
							{columns.map((col) => (
								<TableHead
									key={col.key}
									style={
										col.width && !col.width.startsWith("w-")
											? { width: col.width }
											: undefined
									}
									className={cn(
										"font-semibold text-xs py-3",
										col.width?.startsWith("w-") && col.width,
										col.className,
									)}
								>
									{col.header}
								</TableHead>
							))}
							<TableHead className="w-24 sm:w-28 text-right font-semibold text-xs py-3 pr-4">
								Tindakan
							</TableHead>
						</TableRow>
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableSkeleton columns={columns.length + 1} rows={5} />
						) : filteredData.length === 0 ? (
							<TableEmptyState
								colSpan={columns.length + 1}
								icon={searchTerm ? "search" : "folder"}
								title={
									searchTerm ? "Data Tidak Ditemukan" : "Belum Ada Data Master"
								}
								description={
									searchTerm
										? "Tidak ada data yang cocok dengan kriteria pencarian Anda."
										: "Belum ada data referensi yang tersimpan dalam sistem."
								}
								onReset={searchTerm ? () => setSearchTerm("") : undefined}
								resetLabel="Reset Pencarian"
							/>
						) : (
							paginatedData.map((row) => {
								const rowRecord = row as unknown as Record<string, unknown>;
								return (
									<TableRow
										key={row.id}
										className="hover:bg-muted/30 transition-colors"
									>
										{columns.map((col) => {
											const cellVal = rowRecord[col.key];
											return (
												<TableCell
													key={`${row.id}-${col.key}`}
													className={cn(
														"py-3 text-xs truncate",
														col.className,
													)}
													title={
														typeof cellVal === "string" ? cellVal : undefined
													}
												>
													{col.render
														? col.render(row)
														: cellVal != null
															? String(cellVal)
															: "-"}
												</TableCell>
											);
										})}
										<TableCell className="w-24 sm:w-28 py-3 text-right pr-4">
											<div className="flex items-center justify-end gap-1">
												<Button
													variant="ghost"
													size="icon"
													onClick={() => handleOpenEdit(row)}
													className="h-7 w-7 text-muted-foreground hover:text-foreground"
													title="Ubah Data"
												>
													<Edit2 className="h-3.5 w-3.5" />
												</Button>
												{onDelete && (
													<Button
														variant="ghost"
														size="icon"
														onClick={() => setDeleteConfirm(row)}
														className="h-7 w-7 text-muted-foreground hover:text-destructive"
														title="Hapus Data"
													>
														<Trash2 className="h-3.5 w-3.5" />
													</Button>
												)}
											</div>
										</TableCell>
									</TableRow>
								);
							})
						)}
					</TableBody>
				</Table>

				{/* Standardized Pagination */}
				{filteredData.length > 0 && (
					<TablePagination
						pageIndex={page - 1}
						page={page}
						pageSize={pageSize}
						totalCount={totalCount}
						totalPages={totalPages}
						onPageChange={(newPage) => setPage(newPage)}
						onPageSizeChange={(newSize) => {
							setPageSize(newSize);
							setPage(1);
						}}
						pageSizeOptions={[10, 25, 50]}
						className="border-t px-4"
					/>
				)}
			</div>

			{/* Form Dialog */}
			<Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
				<DialogContent className="sm:max-w-[480px]">
					<DialogHeader>
						<DialogTitle className="flex items-center gap-2 text-sm font-semibold">
							<Icon className="h-4 w-4 text-primary" />
							<span>{editingItem ? `Ubah ${title}` : `Tambah ${title}`}</span>
						</DialogTitle>
						<DialogDescription className="text-xs">
							Lengkapi atribut data master di bawah ini.
						</DialogDescription>
					</DialogHeader>

					<form
						onSubmit={(e) => {
							e.preventDefault();
							e.stopPropagation();
							form.handleSubmit();
						}}
						className="space-y-3 py-2"
					>
						{error && (
							<Alert variant="destructive">
								<AlertDescription className="text-xs">{error}</AlertDescription>
							</Alert>
						)}

						{fields.map((f) => (
							<form.Field key={f.name} name={f.name as never}>
								{(field) => {
									const fieldError = field.state.meta.errors.length
										? String(field.state.meta.errors[0])
										: undefined;
									return (
										<FormField
											label={f.label}
											htmlFor={f.name}
											required={f.required}
											error={fieldError}
										>
											{f.type === "textarea" ? (
												<Textarea
													id={f.name}
													placeholder={f.placeholder}
													value={String(field.state.value ?? "")}
													onBlur={field.handleBlur}
													onChange={(e) =>
														field.handleChange(e.target.value as never)
													}
													className="text-xs min-h-[70px]"
													required={f.required}
												/>
											) : f.type === "select" ? (
												<Select
													value={String(field.state.value ?? "")}
													onValueChange={(val) =>
														field.handleChange(val as never)
													}
												>
													<SelectTrigger id={f.name} className="h-8 text-xs">
														<SelectValue
															placeholder={f.placeholder || "-- Pilih --"}
														/>
													</SelectTrigger>
													<SelectContent side="bottom">
														{f.options?.map((opt) => (
															<SelectItem
																key={String(opt.value)}
																value={String(opt.value)}
															>
																{opt.label}
															</SelectItem>
														))}
													</SelectContent>
												</Select>
											) : f.type === "checkbox" ? (
												<div className="flex items-center gap-2 pt-1">
													<Checkbox
														id={f.name}
														checked={Boolean(field.state.value)}
														onCheckedChange={(checked) =>
															field.handleChange(Boolean(checked) as never)
														}
													/>
													<label
														htmlFor={f.name}
														className="text-xs text-foreground cursor-pointer select-none font-medium"
													>
														Aktif
													</label>
												</div>
											) : (
												<Input
													id={f.name}
													type={f.type === "number" ? "number" : "text"}
													placeholder={f.placeholder}
													value={
														f.type === "number"
															? ((field.state.value as unknown as number) ?? 0)
															: String(field.state.value ?? "")
													}
													onBlur={field.handleBlur}
													onChange={(e) =>
														field.handleChange(
															(f.type === "number"
																? e.target.value === ""
																	? 0
																	: Number(e.target.value)
																: e.target.value) as never,
														)
													}
													className="h-8 text-xs"
													required={f.required}
												/>
											)}
										</FormField>
									);
								}}
							</form.Field>
						))}

						<DialogFooter className="pt-2">
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => setDialogOpen(false)}
							>
								Batal
							</Button>
							<form.Subscribe
								selector={(state) => [state.canSubmit, state.isSubmitting]}
							>
								{([canSubmit, isSubmitting]) => (
									<Button
										type="submit"
										size="sm"
										disabled={!canSubmit || isSubmitting}
									>
										{isSubmitting ? (
											<Loader2 className="h-3.5 w-3.5 animate-spin mr-1" />
										) : null}
										Simpan
									</Button>
								)}
							</form.Subscribe>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>

			{/* Delete Confirmation Dialog */}
			<Dialog
				open={!!deleteConfirm}
				onOpenChange={(open) => !open && setDeleteConfirm(null)}
			>
				<DialogContent className="sm:max-w-[400px]">
					<DialogHeader>
						<DialogTitle className="flex items-center gap-2 text-sm font-semibold text-destructive">
							<Trash2 className="h-4 w-4" />
							<span>Hapus {title}</span>
						</DialogTitle>
						<DialogDescription className="text-xs">
							Apakah Anda yakin ingin menghapus data ini? Tindakan ini tidak
							dapat dibatalkan.
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
							disabled={isDeleting}
						>
							{isDeleting ? (
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
