import { createFileRoute } from "@tanstack/react-router";
import { Tags } from "lucide-react";
import { $api } from "@/api/client";
import type { ReasonCategoryDto } from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/reason-categories")({
	component: ReasonCategoriesPage,
});

function ReasonCategoriesPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/admin/master/reason-categories",
	);

	const createMutation = $api.useMutation(
		"post",
		"/api/admin/master/reason-categories",
		{
			onSuccess: () => refetch(),
		},
	);

	const updateMutation = $api.useMutation(
		"put",
		"/api/admin/master/reason-categories/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const deleteMutation = $api.useMutation(
		"delete",
		"/api/admin/master/reason-categories/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const columns: ColumnDef<ReasonCategoryDto>[] = [
		{
			key: "name",
			header: "Kategori Alasan",
			render: (row) => (
				<span className="font-semibold text-foreground">{row.name}</span>
			),
		},
		{
			key: "description",
			header: "Deskripsi",
			render: (row) => (
				<span className="text-muted-foreground">{row.description || "-"}</span>
			),
		},
	];

	const fields: FieldDef[] = [
		{
			name: "name",
			label: "Nama Kategori Alasan",
			type: "text",
			required: true,
			placeholder: "contoh: Kelengkapan Dokumen Legalitas",
		},
		{
			name: "description",
			label: "Deskripsi Kategori",
			type: "textarea",
			placeholder: "contoh: Alasan terkait izin usaha, NIB, atau NPWP",
		},
	];

	const handleSave = async (formData: Record<string, unknown>, id?: string) => {
		if (id) {
			await updateMutation.mutateAsync({
				params: { path: { id } },
				body: {
					name: String(formData.name),
					description: formData.description
						? String(formData.description)
						: null,
				},
			});
		} else {
			await createMutation.mutateAsync({
				body: {
					name: String(formData.name),
					description: formData.description
						? String(formData.description)
						: null,
				},
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
			title="Kategori Alasan Revisi & Penolakan"
			description="Pengelompokan opsi alasan saat reviewer atau approver meminta revisi atau menolak berkas."
			icon={Tags}
			data={data || []}
			isLoading={isLoading}
			columns={columns}
			fields={fields}
			onSave={handleSave}
			onDelete={handleDelete}
			searchKeys={["name", "description"]}
		/>
	);
}
