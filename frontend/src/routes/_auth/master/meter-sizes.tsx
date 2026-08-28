import { createFileRoute } from "@tanstack/react-router";
import { Gauge } from "lucide-react";
import { $api } from "@/api/client";
import type { MeterSizeDto } from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/meter-sizes")({
	component: MeterSizesPage,
});

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
			key: "gsize",
			header: "G-Size",
			render: (row) => (
				<span className="font-mono font-semibold text-foreground">
					{row.gsize}
				</span>
			),
		},
		{
			key: "nominalFlowM3h",
			header: "Aliran Nominal (Qnom m³/jam)",
			render: (row) => (
				<span className="font-mono">
					{row.nominalFlowM3h !== null && row.nominalFlowM3h !== undefined
						? `${row.nominalFlowM3h} m³/h`
						: "-"}
				</span>
			),
		},
		{
			key: "maxFlowM3h",
			header: "Aliran Maksimum (Qmax m³/jam)",
			render: (row) => (
				<span className="font-mono">
					{row.maxFlowM3h !== null && row.maxFlowM3h !== undefined
						? `${row.maxFlowM3h} m³/h`
						: "-"}
				</span>
			),
		},
		{
			key: "pressureRating",
			header: "Pressure Rating",
			render: (row) => (
				<span className="text-muted-foreground">
					{row.pressureRating || "-"}
				</span>
			),
		},
	];

	const fields: FieldDef[] = [
		{
			name: "gsize",
			label: "Ukuran G-Size",
			type: "text",
			required: true,
			placeholder: "contoh: G-100",
		},
		{
			name: "nominalFlowM3h",
			label: "Aliran Nominal (m³/jam)",
			type: "number",
			placeholder: "contoh: 100",
		},
		{
			name: "maxFlowM3h",
			label: "Aliran Maksimum (m³/jam)",
			type: "number",
			placeholder: "contoh: 160",
		},
		{
			name: "pressureRating",
			label: "Pressure Rating (ANSI / Bar)",
			type: "text",
			placeholder: "contoh: ANSI 150 / PN 16",
		},
	];

	const handleSave = async (formData: Record<string, unknown>, id?: string) => {
		const payload = {
			gsize: String(formData.gsize),
			nominalFlowM3h:
				formData.nominalFlowM3h !== "" && formData.nominalFlowM3h !== undefined
					? Number(formData.nominalFlowM3h)
					: null,
			maxFlowM3h:
				formData.maxFlowM3h !== "" && formData.maxFlowM3h !== undefined
					? Number(formData.maxFlowM3h)
					: null,
			pressureRating: formData.pressureRating
				? String(formData.pressureRating)
				: null,
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
			title="Ukuran Meter & Laju Alir (Meter Sizes)"
			description="Daftar kapasitas G-Size meter gas untuk penentuan batas laju alir (Qmax / Qnom) pada stasiun pengukur."
			icon={Gauge}
			data={data || []}
			isLoading={isLoading}
			columns={columns}
			fields={fields}
			onSave={handleSave}
			onDelete={handleDelete}
			searchKeys={["gsize", "pressureRating"]}
		/>
	);
}
