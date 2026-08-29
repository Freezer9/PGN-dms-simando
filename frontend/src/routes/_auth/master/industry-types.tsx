import { createFileRoute } from "@tanstack/react-router";
import { Factory } from "lucide-react";
import { $api } from "@/api/client";
import type {
	CreateIndustryTypeRequest,
	IndustryTypeDto,
	UpdateIndustryTypeRequest,
} from "@/api/types";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "@/components/admin/master-data-table";

export const Route = createFileRoute("/_auth/master/industry-types")({
	component: IndustryTypesPage,
});

export interface IndustryTypeFormData {
	name: string;
	contohProduk: string;
}

function IndustryTypesPage() {
	const { data, isLoading, refetch } = $api.useQuery(
		"get",
		"/api/master/industry-types",
	);

	const createMutation = $api.useMutation(
		"post",
		"/api/admin/master/industry-types",
		{
			onSuccess: () => refetch(),
		},
	);

	const updateMutation = $api.useMutation(
		"put",
		"/api/admin/master/industry-types/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const deleteMutation = $api.useMutation(
		"delete",
		"/api/admin/master/industry-types/{id}",
		{
			onSuccess: () => refetch(),
		},
	);

	const columns: ColumnDef<IndustryTypeDto>[] = [
		{
			key: "name",
			header: "Jenis Industri",
			render: (row) => (
				<span className="font-semibold text-foreground">{row.name}</span>
			),
		},
		{
			key: "contohProduk",
			header: "Contoh Produk",
			render: (row) => (
				<span className="text-muted-foreground">{row.contohProduk || "-"}</span>
			),
		},
	];

	const fields: FieldDef<IndustryTypeFormData>[] = [
		{
			name: "name",
			label: "Nama Jenis Industri",
			type: "text",
			required: true,
			placeholder: "contoh: Industri Makanan & Minuman",
		},
		{
			name: "contohProduk",
			label: "Contoh Produk",
			type: "textarea",
			placeholder: "contoh: Mie instan, biskuit, sirup",
		},
	];

	const handleSave = async (formData: IndustryTypeFormData, id?: string) => {
		const payload: CreateIndustryTypeRequest | UpdateIndustryTypeRequest = {
			name: formData.name.trim(),
			contohProduk: formData.contohProduk ? formData.contohProduk.trim() : null,
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
		<MasterDataTable<IndustryTypeDto, IndustryTypeFormData>
			title="Jenis Industri"
			description="Daftar klasifikasi industri pelanggan gas bumi PGN."
			icon={Factory}
			data={data || []}
			isLoading={isLoading}
			columns={columns}
			fields={fields}
			onSave={handleSave}
			onDelete={handleDelete}
			searchKeys={["name", "contohProduk"]}
		/>
	);
}
