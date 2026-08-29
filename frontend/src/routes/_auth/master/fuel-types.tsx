import { createFileRoute } from "@tanstack/react-router";
import { Flame } from "lucide-react";
import { $api } from "@/api/client";
import type { FuelTypeDto } from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/fuel-types")({
	component: FuelTypesPage,
});

export interface FuelTypeFormData {
	name: string;
}

function FuelTypesPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/master/fuel-types",
	);

	const createMutation = $api.useMutation(
		"post",
		"/api/admin/master/fuel-types",
		{
			onSuccess: () => refetch(),
		},
	);

	const updateMutation = $api.useMutation(
		"put",
		"/api/admin/master/fuel-types/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const deleteMutation = $api.useMutation(
		"delete",
		"/api/admin/master/fuel-types/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const columns: ColumnDef<FuelTypeDto>[] = [
		{
			key: "name",
			header: "Jenis Bahan Bakar",
			render: (row) => (
				<span className="font-semibold text-foreground">{row.name}</span>
			),
		},
	];

	const fields: FieldDef<FuelTypeFormData>[] = [
		{
			name: "name",
			label: "Nama Jenis Bahan Bakar",
			type: "text",
			required: true,
			placeholder: "contoh: Solar / HSD",
		},
	];

	const handleSave = async (formData: FuelTypeFormData, id?: string) => {
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
		<MasterDataTable<FuelTypeDto, FuelTypeFormData>
			title="Jenis Bahan Bakar"
			description="Daftar bahan bakar eksisting yang digunakan calon pelanggan sebelum beralih ke gas bumi."
			icon={Flame}
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
