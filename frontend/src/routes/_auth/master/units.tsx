import { createFileRoute } from "@tanstack/react-router";
import { Ruler } from "lucide-react";
import { $api } from "@/api/client";
import type { UnitOfMeasureDto } from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/units")({
	component: UnitsPage,
});

function UnitsPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/master/units",
	);

	const createMutation = $api.useMutation("post", "/api/admin/master/units", {
		onSuccess: () => refetch(),
	});

	const updateMutation = $api.useMutation(
		"put",
		"/api/admin/master/units/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const deleteMutation = $api.useMutation(
		"delete",
		"/api/admin/master/units/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const columns: ColumnDef<UnitOfMeasureDto>[] = [
		{
			key: "code",
			header: "Kode Satuan",
			render: (row) => (
				<span className="font-mono font-semibold text-foreground">
					{row.code}
				</span>
			),
		},
		{
			key: "name",
			header: "Nama Satuan",
			render: (row) => <span>{row.name}</span>,
		},
		{
			key: "dimension",
			header: "Dimensi / Kategori",
			render: (row) => (
				<span className="text-muted-foreground">{row.dimension || "-"}</span>
			),
		},
	];

	const fields: FieldDef[] = [
		{
			name: "code",
			label: "Kode Satuan",
			type: "text",
			required: true,
			placeholder: "contoh: MMBTU",
		},
		{
			name: "name",
			label: "Nama Satuan",
			type: "text",
			required: true,
			placeholder: "contoh: Million British Thermal Units",
		},
		{
			name: "dimension",
			label: "Dimensi / Kategori",
			type: "text",
			placeholder: "contoh: Energy, Volume, Pressure, Mass",
		},
	];

	const handleSave = async (formData: Record<string, unknown>, id?: string) => {
		const dimensionVal = (
			formData.dimension ? String(formData.dimension) : "Energy"
		) as "Energy" | "Volume" | "Pressure" | "Mass";

		if (id) {
			await updateMutation.mutateAsync({
				params: { path: { id } },
				body: {
					code: String(formData.code),
					name: String(formData.name),
					dimension: dimensionVal,
				},
			});
		} else {
			await createMutation.mutateAsync({
				body: {
					code: String(formData.code),
					name: String(formData.name),
					dimension: dimensionVal,
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
			title="Satuan Pengukuran (Units)"
			description="Daftar satuan ukuran untuk energi, volume, tekanan, massa, dan laju alir gas."
			icon={Ruler}
			data={data || []}
			isLoading={isLoading}
			columns={columns}
			fields={fields}
			onSave={handleSave}
			onDelete={handleDelete}
			searchKeys={["code", "name", "dimension"]}
		/>
	);
}
