import { createFileRoute } from "@tanstack/react-router";
import { Settings2 } from "lucide-react";
import { $api } from "@/api/client";
import type { MrsSpecDto } from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/mrs-specs")({
	component: MrsSpecsPage,
});

export interface MrsSpecFormData {
	name: string;
}

function MrsSpecsPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/master/mrs-specs",
	);

	const createMutation = $api.useMutation(
		"post",
		"/api/admin/master/mrs-specs",
		{
			onSuccess: () => refetch(),
		},
	);

	const updateMutation = $api.useMutation(
		"put",
		"/api/admin/master/mrs-specs/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const deleteMutation = $api.useMutation(
		"delete",
		"/api/admin/master/mrs-specs/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const columns: ColumnDef<MrsSpecDto>[] = [
		{
			key: "name",
			header: "Spesifikasi MRS",
			render: (row) => (
				<span className="font-semibold text-foreground">{row.name}</span>
			),
		},
	];

	const fields: FieldDef<MrsSpecFormData>[] = [
		{
			name: "name",
			label: "Nama Spesifikasi MRS",
			type: "text",
			required: true,
			placeholder: "contoh: MRS Standar 2 Stream (1W + 1S)",
		},
	];

	const handleSave = async (formData: MrsSpecFormData, id?: string) => {
		if (id) {
			await updateMutation.mutateAsync({
				params: { path: { id } },
				body: { name: formData.name.trim() },
			});
		} else {
			await createMutation.mutateAsync({
				body: { name: formData.name.trim() },
			});
		}
	};

	const handleDelete = async (id: string) => {
		await deleteMutation.mutateAsync({
			params: { path: { id } },
		});
	};

	return (
		<MasterDataTable<MrsSpecDto, MrsSpecFormData>
			title="Spesifikasi Metering & Regulating Station (MRS)"
			description="Daftar konfigurasi baku stasiun pengukur dan pengatur tekanan gas pelanggan."
			icon={Settings2}
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
