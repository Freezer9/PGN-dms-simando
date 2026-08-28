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
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";

export interface ColumnDef<T> {
	key: string;
	header: string;
	render?: (row: T) => React.ReactNode;
	className?: string;
}

export interface FieldDef {
	name: string;
	label: string;
	type: "text" | "number" | "textarea" | "checkbox" | "select";
	required?: boolean;
	placeholder?: string;
	options?: { value: string | number; label: string }[];
}

export interface MasterDataTableProps<T extends { id: string }> {
	title: string;
	description: string;
	icon?: React.ComponentType<{ className?: string }>;
	data: T[];
	isLoading: boolean;
	columns: ColumnDef<T>[];
	fields: FieldDef[];
	onSave: (
		formData: Record<string, unknown>,
		editingId?: string,
	) => Promise<void>;
	onDelete?: (id: string) => Promise<void>;
	searchKeys?: (keyof T)[];
}

export function MasterDataTable<T extends { id: string }>({
	title,
	description,
	icon: Icon = FolderKanban,
	data,
	isLoading,
	columns,
	fields,
	onSave,
	onDelete,
	searchKeys = ["name" as keyof T],
}: MasterDataTableProps<T>) {
	const [searchTerm, setSearchTerm] = React.useState("");
	const [dialogOpen, setDialogOpen] = React.useState(false);
	const [editingItem, setEditingItem] = React.useState<T | null>(null);
	const [deleteConfirm, setDeleteConfirm] = React.useState<T | null>(null);
	const [formData, setFormData] = React.useState<Record<string, unknown>>({});
	const [error, setError] = React.useState<string | null>(null);
	const [isSubmitting, setIsSubmitting] = React.useState(false);

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

	const handleOpenCreate = () => {
		setEditingItem(null);
		const initial: Record<string, unknown> = {};
		for (const f of fields) {
			if (f.type === "checkbox") initial[f.name] = true;
			else if (f.type === "number") initial[f.name] = 0;
			else initial[f.name] = "";
		}
		setFormData(initial);
		setError(null);
		setDialogOpen(true);
	};

	const handleOpenEdit = (item: T) => {
		setEditingItem(item);
		const current: Record<string, unknown> = {};
		for (const f of fields) {
			current[f.name] = (item as Record<string, unknown>)[f.name] ?? "";
		}
		setFormData(current);
		setError(null);
		setDialogOpen(true);
	};

	const handleSubmit = async (e: React.FormEvent) => {
		e.preventDefault();
		setError(null);
		setIsSubmitting(true);

		try {
			await onSave(formData, editingItem?.id);
			setDialogOpen(false);
			setEditingItem(null);
		} catch (err: unknown) {
			const errorObj = err as {
				error?: string;
				errors?: string[];
				message?: string;
			};
			setError(
				errorObj?.error ||
					errorObj?.errors?.[0] ||
					errorObj?.message ||
					"Gagal menyimpan data master.",
			);
		} finally {
			setIsSubmitting(false);
		}
	};

	const handleConfirmDelete = async () => {
		if (!deleteConfirm || !onDelete) return;
		setIsSubmitting(true);
		setError(null);
		try {
			await onDelete(deleteConfirm.id);
			setDeleteConfirm(null);
		} catch (err: unknown) {
			const errorObj = err as {
				error?: string;
				errors?: string[];
				message?: string;
			};
			setError(
				errorObj?.error ||
					errorObj?.errors?.[0] ||
					errorObj?.message ||
					"Gagal menghapus data master.",
			);
		} finally {
			setIsSubmitting(false);
		}
	};

	return (
		<div className="space-y-4">
			{/* Top Header */}
			<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
				<div className="space-y-0.5">
					<h2 className="text-lg font-bold tracking-tight text-foreground flex items-center gap-2">
						<Icon className="h-5 w-5 text-primary" />
						<span>{title}</span>
					</h2>
					<p className="text-xs text-muted-foreground">{description}</p>
				</div>

				<div className="flex items-center gap-2">
					<div className="relative w-64">
						<Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
						<Input
							placeholder="Cari data..."
							value={searchTerm}
							onChange={(e) => setSearchTerm(e.target.value)}
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
			</div>

			{error && (
				<Alert variant="destructive">
					<AlertTriangle className="h-4 w-4" />
					<AlertDescription className="text-xs">{error}</AlertDescription>
				</Alert>
			)}

			{/* Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table>
					<TableHeader className="bg-muted/40">
						<TableRow>
							{columns.map((col) => (
								<TableHead
									key={col.key}
									className={`font-semibold text-xs py-3 ${col.className || ""}`}
								>
									{col.header}
								</TableHead>
							))}
							<TableHead className="text-right font-semibold text-xs py-3 pr-4">
								Tindakan
							</TableHead>
						</TableRow>
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableRow>
								<TableCell
									colSpan={columns.length + 1}
									className="text-center py-12"
								>
									<div className="flex flex-col items-center justify-center gap-2 text-muted-foreground">
										<Loader2 className="h-6 w-6 animate-spin text-primary" />
										<span className="text-sm font-medium">Memuat data...</span>
									</div>
								</TableCell>
							</TableRow>
						) : filteredData.length === 0 ? (
							<TableRow>
								<TableCell
									colSpan={columns.length + 1}
									className="text-center py-12 text-muted-foreground text-xs"
								>
									{searchTerm
										? "Tidak ada data yang cocok dengan pencarian."
										: "Belum ada data referensi terdaftar."}
								</TableCell>
							</TableRow>
						) : (
							filteredData.map((row) => (
								<TableRow
									key={row.id}
									className="hover:bg-muted/30 transition-colors"
								>
									{columns.map((col) => (
										<TableCell
											key={`${row.id}-${col.key}`}
											className={`py-3 text-xs ${col.className || ""}`}
										>
											{col.render
												? col.render(row)
												: String(
														(row as Record<string, unknown>)[col.key] ?? "-",
													)}
										</TableCell>
									))}
									<TableCell className="py-3 text-right pr-4">
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
							))
						)}
					</TableBody>
				</Table>
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

					<form onSubmit={handleSubmit} className="space-y-3 py-2">
						{error && (
							<Alert variant="destructive">
								<AlertDescription className="text-xs">{error}</AlertDescription>
							</Alert>
						)}

						{fields.map((f) => (
							<div key={f.name} className="space-y-1">
								<Label htmlFor={f.name} className="text-xs font-medium">
									{f.label}{" "}
									{f.required && <span className="text-destructive">*</span>}
								</Label>

								{f.type === "textarea" ? (
									<Textarea
										id={f.name}
										placeholder={f.placeholder}
										value={String(formData[f.name] ?? "")}
										onChange={(e) =>
											setFormData((prev) => ({
												...prev,
												[f.name]: e.target.value,
											}))
										}
										className="text-xs min-h-[70px]"
										required={f.required}
									/>
								) : f.type === "select" ? (
									<select
										id={f.name}
										value={String(formData[f.name] ?? "")}
										onChange={(e) =>
											setFormData((prev) => ({
												...prev,
												[f.name]: e.target.value,
											}))
										}
										className="w-full h-8 px-2.5 rounded-md border bg-background text-xs"
										required={f.required}
									>
										<option value="">-- Pilih --</option>
										{f.options?.map((opt) => (
											<option key={opt.value} value={opt.value}>
												{opt.label}
											</option>
										))}
									</select>
								) : f.type === "checkbox" ? (
									<div className="flex items-center gap-2 pt-1">
										<input
											type="checkbox"
											id={f.name}
											checked={Boolean(formData[f.name])}
											onChange={(e) =>
												setFormData((prev) => ({
													...prev,
													[f.name]: e.target.checked,
												}))
											}
											className="rounded border-gray-300 size-4 text-primary"
										/>
										<label
											htmlFor={f.name}
											className="text-xs text-foreground cursor-pointer"
										>
											Aktif
										</label>
									</div>
								) : (
									<Input
										id={f.name}
										type={f.type === "number" ? "number" : "text"}
										placeholder={f.placeholder}
										value={String(formData[f.name] ?? "")}
										onChange={(e) =>
											setFormData((prev) => ({
												...prev,
												[f.name]:
													f.type === "number"
														? Number(e.target.value)
														: e.target.value,
											}))
										}
										className="h-8 text-xs"
										required={f.required}
									/>
								)}
							</div>
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
							<Button type="submit" size="sm" disabled={isSubmitting}>
								{isSubmitting ? (
									<Loader2 className="h-3.5 w-3.5 animate-spin mr-1" />
								) : null}
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
							<span>Hapus Data</span>
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
							disabled={isSubmitting}
						>
							Batal
						</Button>
						<Button
							type="button"
							variant="destructive"
							size="sm"
							onClick={handleConfirmDelete}
							disabled={isSubmitting}
						>
							{isSubmitting ? (
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
