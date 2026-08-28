import { createFileRoute } from "@tanstack/react-router";
import { Globe } from "lucide-react";
import { $api } from "@/api/client";
import type { CountryDto } from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/countries")({
	component: CountriesPage,
});

function CountriesPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/master/countries",
	);

	const columns: ColumnDef<CountryDto>[] = [
		{
			key: "code",
			header: "Kode Negara (ISO)",
			render: (row) => (
				<span className="font-mono font-semibold text-foreground">
					{row.code}
				</span>
			),
		},
		{
			key: "name",
			header: "Nama Negara",
			render: (row) => (
				<span className="font-semibold text-foreground">{row.name}</span>
			),
		},
	];

	const fields: FieldDef[] = [
		{
			name: "code",
			label: "Kode Negara (2-3 Karakter)",
			type: "text",
			required: true,
			placeholder: "contoh: ID",
		},
		{
			name: "name",
			label: "Nama Negara",
			type: "text",
			required: true,
			placeholder: "contoh: Indonesia",
		},
	];

	const handleSave = async (_formData: Record<string, unknown>) => {
		// Country list is standard ISO
		refetch();
	};

	return (
		<MasterDataTable
			title="Daftar Negara"
			description="Referensi daftar negara asal dan kepemilikan modal badan usaha."
			icon={Globe}
			data={data || []}
			isLoading={isLoading}
			columns={columns}
			fields={fields}
			onSave={handleSave}
			searchKeys={["code", "name"]}
		/>
	);
}
