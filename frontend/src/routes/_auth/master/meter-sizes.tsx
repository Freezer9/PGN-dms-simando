import { createFileRoute } from "@tanstack/react-router";
import { Gauge } from "lucide-react";
import { $api } from "@/api/client";
import type {
	CreateMeterSizeRequest,
	MeterSizeDto,
	UpdateMeterSizeRequest,
} from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/meter-sizes")({
	component: MeterSizesPage,
});

export interface MeterSizeFormData {
	gSize: string;
	nominalFlow: number;
	maxFlow: number;
	pressureRating: number;
}

function MeterSizesPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/master/meter-sizes",
	);

	const createMutation = $api.useMutation(
		"post",
		"/api/admin/master/meter-sizes",
		{
			onSuccess: () => refetch(),
		},
	);

	const updateMutation = $api.useMutation(
		"put",
		"/api/admin/master/meter-sizes/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const deleteMutation = $api.useMutation(
		"delete",
		"/api/admin/master/meter-sizes/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const columns: ColumnDef<MeterSizeDto>[] = [
		{
			key: "gSize",
			header: "G-Size",
			width: "w-36",
			render: (row) => (
				<span className="font-mono font-semibold text-foreground">
					{row.gSize}
				</span>
			),
		},
		{
			key: "nominalFlow",
			header: "Aliran Nominal (Qnom m³/jam)",
			width: "w-64",
			render: (row) => (
				<span className="font-mono">
					{row.nominalFlow !== null && row.nominalFlow !== undefined
						? `${row.nominalFlow} m³/h`
						: "-"}
				</span>
			),
		},
		{
			key: "maxFlow",
			header: "Aliran Maksimum (Qmax m³/jam)",
			width: "w-64",
			render: (row) => (
				<span className="font-mono">
					{row.maxFlow !== null && row.maxFlow !== undefined
						? `${row.maxFlow} m³/h`
						: "-"}
				</span>
			),
		},
		{
			key: "pressureRating",
			header: "Pressure Rating (Bar)",
			width: "w-52",
			render: (row) => (
				<span className="text-muted-foreground font-mono">
					{row.pressureRating != null ? `${row.pressureRating} Bar` : "-"}
				</span>
			),
		},
	];

	const fields: FieldDef<MeterSizeFormData>[] = [
		{
			name: "gSize",
			label: "Ukuran G-Size",
			type: "text",
			required: true,
			placeholder: "contoh: G-100",
		},
		{
			name: "nominalFlow",
			label: "Aliran Nominal (m³/jam)",
			type: "number",
			required: true,
			placeholder: "contoh: 100",
		},
		{
			name: "maxFlow",
			label: "Aliran Maksimum (m³/jam)",
			type: "number",
			required: true,
			placeholder: "contoh: 160",
		},
		{
			name: "pressureRating",
			label: "Pressure Rating (Bar)",
			type: "number",
			required: true,
			placeholder: "contoh: 16",
		},
	];

	const handleSave = async (formData: MeterSizeFormData, id?: string) => {
		const payload: CreateMeterSizeRequest | UpdateMeterSizeRequest = {
			gSize: formData.gSize.trim(),
			nominalFlow: Number(formData.nominalFlow) || 0,
			maxFlow: Number(formData.maxFlow) || 0,
			pressureRating: Number(formData.pressureRating) || 0,
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
		<MasterDataTable<MeterSizeDto, MeterSizeFormData>
			title="Ukuran Meter & Laju Alir (Meter Sizes)"
			description="Daftar kapasitas G-Size meter gas untuk penentuan batas laju alir (Qmax / Qnom) pada stasiun pengukur."
			icon={Gauge}
			data={data || []}
			isLoading={isLoading}
			columns={columns}
			fields={fields}
			onSave={handleSave}
			onDelete={handleDelete}
			searchKeys={["gSize"]}
		/>
	);
}
