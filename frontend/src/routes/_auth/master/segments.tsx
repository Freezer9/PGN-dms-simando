import { createFileRoute } from "@tanstack/react-router";
import { Tag } from "lucide-react";
import { $api } from "@/api/client";
import type { SegmentDto } from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/segments")({
	component: SegmentsPage,
});

export interface SegmentFormData {
	name: string;
	sortOrder: number;
}

function SegmentsPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/master/segments",
	);

	const createMutation = $api.useMutation(
		"post",
		"/api/admin/master/segments",
		{
			onSuccess: () => refetch(),
		},
	);

	const updateMutation = $api.useMutation(
		"put",
		"/api/admin/master/segments/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const deleteMutation = $api.useMutation(
		"delete",
		"/api/admin/master/segments/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const columns: ColumnDef<SegmentDto>[] = [
		{
			key: "name",
			header: "Nama Segmen",
			render: (row) => (
				<span className="font-semibold text-foreground">{row.name}</span>
			),
		},
		{
			key: "sortOrder",
			header: "Urutan (Sort Order)",
			width: "w-48",
			render: (row) => (
				<span className="font-mono text-muted-foreground">{row.sortOrder}</span>
			),
		},
	];

	const fields: FieldDef<SegmentFormData>[] = [
		{
			name: "name",
			label: "Nama Segmen",
			type: "text",
			required: true,
			placeholder: "contoh: Gold",
		},
		{
			name: "sortOrder",
			label: "Urutan Tampilan",
			type: "number",
			required: true,
			placeholder: "contoh: 1",
		},
	];

	const handleSave = async (formData: SegmentFormData, id?: string) => {
		if (id) {
			await updateMutation.mutateAsync({
				params: { path: { id } },
				body: {
					name: formData.name.trim(),
					sortOrder: Number(formData.sortOrder) || 0,
				},
			});
		} else {
			await createMutation.mutateAsync({
				body: {
					name: formData.name.trim(),
					sortOrder: Number(formData.sortOrder) || 0,
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
		<MasterDataTable<SegmentDto, SegmentFormData>
			title="Segmen Pelanggan"
			description="Daftar hierarki segmen komersial pelanggan gas (Bronze, Silver, Gold, Platinum)."
			icon={Tag}
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
