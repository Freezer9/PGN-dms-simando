import { createFileRoute } from "@tanstack/react-router";
import { FileText } from "lucide-react";
import { $api } from "@/api/client";
import type {
	CreateReferenceDocumentRequest,
	ReferenceDocumentDto,
	UpdateReferenceDocumentRequest,
} from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/reference-documents")({
	component: ReferenceDocumentsPage,
});

export interface ReferenceDocumentFormData {
	name: string;
	version: number;
	effectiveFrom: string;
	effectiveTo?: string;
}

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
			key: "name",
			header: "Judul Ketentuan / Dokumen",
			render: (row) => (
				<span className="font-semibold text-foreground">{row.name}</span>
			),
		},
		{
			key: "version",
			header: "Versi Dokumen",
			width: "w-32",
			render: (row) => (
				<span className="font-mono text-xs">v{row.version}</span>
			),
		},
		{
			key: "effectiveFrom",
			header: "Berlaku Mulai",
			width: "w-40",
			render: (row) => (
				<span className="text-xs">
					{row.effectiveFrom
						? new Date(row.effectiveFrom).toLocaleDateString("id-ID")
						: "-"}
				</span>
			),
		},
		{
			key: "effectiveTo",
			header: "Berlaku Sampai",
			width: "w-40",
			render: (row) => (
				<span className="text-xs text-muted-foreground">
					{row.effectiveTo
						? new Date(row.effectiveTo).toLocaleDateString("id-ID")
						: "Sekarang"}
				</span>
			),
		},
	];

	const fields: FieldDef<ReferenceDocumentFormData>[] = [
		{
			name: "name",
			label: "Nama Dokumen Acuan",
			type: "text",
			required: true,
			placeholder: "contoh: Ketentuan Penyaluran Gas Bumi Non-Pipa",
		},
		{
			name: "version",
			label: "Versi (Angka)",
			type: "number",
			required: true,
			placeholder: "1",
		},
		{
			name: "effectiveFrom",
			label: "Tanggal Berlaku Mulai",
			type: "text",
			required: true,
			placeholder: "YYYY-MM-DD",
		},
		{
			name: "effectiveTo",
			label: "Tanggal Berlaku Sampai (Opsional)",
			type: "text",
			placeholder: "YYYY-MM-DD",
		},
	];

	const handleSave = async (
		formData: ReferenceDocumentFormData,
		id?: string,
	) => {
		const payload:
			| CreateReferenceDocumentRequest
			| UpdateReferenceDocumentRequest = {
			name: formData.name.trim(),
			version: Number(formData.version) || 1,
			effectiveFrom: formData.effectiveFrom.trim(),
			effectiveTo: formData.effectiveTo ? formData.effectiveTo.trim() : null,
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
		<MasterDataTable<ReferenceDocumentDto, ReferenceDocumentFormData>
			title="Dokumen Acuan & Ketentuan Kerja"
			description="Daftar ketentuan operasional, tata cara teknis, dan dokumen legal acuan kerja proses bisnis DMS."
			icon={FileText}
			data={data || []}
			isLoading={isLoading}
			columns={columns}
			fields={fields}
			onSave={handleSave}
			onDelete={handleDelete}
			searchKeys={["name"]}
		/>
	);
}
