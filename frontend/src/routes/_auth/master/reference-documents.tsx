import { createFileRoute } from "@tanstack/react-router";
import { FileText } from "lucide-react";
import { $api } from "@/api/client";
import type { ReferenceDocumentDto } from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/reference-documents")({
	component: ReferenceDocumentsPage,
});

function ReferenceDocumentsPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/master/reference-documents",
	);

	const createMutation = $api.useMutation(
		"post",
		"/api/admin/master/reference-documents",
		{
			onSuccess: () => refetch(),
		},
	);

	const updateMutation = $api.useMutation(
		"put",
		"/api/admin/master/reference-documents/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const deleteMutation = $api.useMutation(
		"delete",
		"/api/admin/master/reference-documents/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const columns: ColumnDef<ReferenceDocumentDto>[] = [
		{
			key: "title",
			header: "Judul Ketentuan / Dokumen",
			render: (row) => (
				<span className="font-semibold text-foreground">{row.title}</span>
			),
		},
		{
			key: "documentNumber",
			header: "Nomor Dokumen",
			render: (row) => (
				<span className="font-mono text-xs">{row.documentNumber || "-"}</span>
			),
		},
		{
			key: "description",
			header: "Deskripsi",
			render: (row) => (
				<span className="text-muted-foreground">{row.description || "-"}</span>
			),
		},
		{
			key: "effectiveFrom",
			header: "Berlaku Mulai",
			render: (row) => (
				<span className="text-xs">
					{row.effectiveFrom
						? new Date(row.effectiveFrom).toLocaleDateString("id-ID")
						: "-"}
				</span>
			),
		},
	];

	const fields: FieldDef[] = [
		{
			name: "title",
			label: "Judul Dokumen Acuan",
			type: "text",
			required: true,
			placeholder: "contoh: Ketentuan Penyaluran Gas Bumi Non-Pipa",
		},
		{
			name: "documentNumber",
			label: "Nomor Ketentuan / Keputusan",
			type: "text",
			placeholder: "contoh: SK-DIR/012/PGN/2024",
		},
		{
			name: "description",
			label: "Ringkasan / Catatan Dokumen",
			type: "textarea",
			placeholder: "contoh: Ketentuan teknis dan komersial baku",
		},
	];

	const handleSave = async (formData: Record<string, unknown>, id?: string) => {
		const payload = {
			title: String(formData.title),
			documentNumber: formData.documentNumber
				? String(formData.documentNumber)
				: null,
			description: formData.description ? String(formData.description) : null,
			effectiveFrom: null,
			effectiveTo: null,
			fileUrl: null,
		};

		if (id) {
			await updateMutation.mutateAsync({
				params: { path: { id } },
				body: payload,
			});
		} else {
			await createMutation.mutateAsync({
				body: payload,
			});
		}
	};

	const handleDelete = async (id: string) => {
		await deleteMutation.mutateAsync({
			params: { path: { id } },
		});
	};

	return (
		<MasterDataTable
			title="Dokumen Acuan & Ketentuan Kerja"
			description="Daftar ketentuan operasional, tata cara teknis, dan dokumen legal acuan kerja proses bisnis DMS."
			icon={FileText}
			data={data || []}
			isLoading={isLoading}
			columns={columns}
			fields={fields}
			onSave={handleSave}
			onDelete={handleDelete}
			searchKeys={["title", "documentNumber", "description"]}
		/>
	);
}
