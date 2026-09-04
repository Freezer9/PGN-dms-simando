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

export interface CountryFormData {
	isoCode: string;
	name: string;
}

function CountriesPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/master/countries",
	);

	const columns: ColumnDef<CountryDto>[] = [
		{
			key: "isoCode",
			header: "Kode ISO",
			width: "w-52",
			render: (row) => (
				<span className="font-mono font-semibold text-foreground">
					{row.isoCode}
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

	const fields: FieldDef<CountryFormData>[] = [
		{
			name: "isoCode",
			label: "Kode ISO Negara",
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

	const handleSave = async (_formData: CountryFormData) => {
		// Country list is standard ISO
		refetch();
	};

	return (
		<MasterDataTable<CountryDto, CountryFormData>
			title="Daftar Negara"
			description="Referensi daftar negara asal dan kepemilikan modal badan usaha."
			icon={Globe}
			data={data || []}
			isLoading={isLoading}
			columns={columns}
			fields={fields}
			onSave={handleSave}
			searchKeys={["isoCode", "name"]}
		/>
	);
}
