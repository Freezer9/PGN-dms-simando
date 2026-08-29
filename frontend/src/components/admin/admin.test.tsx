import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { Tag } from "lucide-react";
import type * as React from "react";
import { describe, expect, it, vi } from "vitest";
import type { UserListItemDto } from "@/api/types";
import { CreateUserDialog } from "./create-user-dialog";
import { EditRolesDialog } from "./edit-roles-dialog";
import {
	type ColumnDef,
	type FieldDef,
	MasterDataTable,
} from "./master-data-table";
import { OrganisationView } from "./organisation-view";
import { ResetPasswordDialog } from "./reset-password-dialog";
import { UsersView } from "./users-view";

vi.mock("@tanstack/react-router", () => ({
	Link: ({
		children,
		to,
		...props
	}: {
		children: React.ReactNode;
		to?: string;
		[key: string]: unknown;
	}) => (
		<a href={typeof to === "string" ? to : "#"} {...props}>
			{children}
		</a>
	),
}));

vi.mock("@/lib/auth", () => ({
	useAuth: () => ({
		user: {
			id: "00000000-0000-0000-0000-000000000001",
			username: "admin.sys",
			fullName: "System Admin",
			roles: ["SystemAdmin"],
			capabilities: ["ManageMasterData", "BreakGlassEmergencyAccess"],
		},
	}),
}));

function createTestQueryClient() {
	return new QueryClient({
		defaultOptions: {
			queries: {
				retry: false,
			},
		},
	});
}

function renderWithClient(ui: React.ReactElement) {
	const queryClient = createTestQueryClient();
	return render(
		<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
	);
}

interface TestItem {
	id: string;
	name: string;
	sortOrder: number;
}

interface TestItemFormData {
	name: string;
	sortOrder: number;
}

const mockTestItems: TestItem[] = [
	{ id: "1", name: "Gold", sortOrder: 1 },
	{ id: "2", name: "Platinum", sortOrder: 2 },
];

const mockColumns: ColumnDef<TestItem>[] = [
	{ key: "name", header: "Nama Segmen" },
	{ key: "sortOrder", header: "Urutan" },
];

const mockFields: FieldDef<TestItemFormData>[] = [
	{ name: "name", label: "Nama Segmen", type: "text", required: true },
	{ name: "sortOrder", label: "Urutan", type: "number", required: true },
];

const mockUser: UserListItemDto = {
	id: "00000000-0000-0000-0000-000000000010",
	fullName: "Budi Sales",
	username: "budi.sales",
	email: "budi@pgn.co.id",
	active: true,
	lastLoginAt: new Date().toISOString(),
	roles: [
		{
			role: "SalesArea",
			scopeLabel: "Surabaya",
		},
	],
	assignmentIds: ["assign-1"],
};

describe("Admin and Master Data Frontend Components", () => {
	describe("MasterDataTable", () => {
		it("renders master data title, description, and columns", () => {
			renderWithClient(
				<MasterDataTable<TestItem, TestItemFormData>
					title="Segmen Pelanggan"
					description="Daftar hierarki segmen komersial."
					icon={Tag}
					data={mockTestItems}
					isLoading={false}
					columns={mockColumns}
					fields={mockFields}
					onSave={vi.fn()}
					onDelete={vi.fn()}
				/>,
			);

			expect(screen.getByText("Segmen Pelanggan")).toBeInTheDocument();
			expect(
				screen.getByText("Daftar hierarki segmen komersial."),
			).toBeInTheDocument();
			expect(screen.getByText("Nama Segmen")).toBeInTheDocument();
			expect(screen.getByText("Urutan")).toBeInTheDocument();
			expect(screen.getByText("Gold")).toBeInTheDocument();
			expect(screen.getByText("Platinum")).toBeInTheDocument();
			expect(screen.getByText("Tambah Data")).toBeInTheDocument();
		});

		it("renders empty state when data array is empty", () => {
			renderWithClient(
				<MasterDataTable<TestItem, TestItemFormData>
					title="Segmen Pelanggan"
					description="Daftar hierarki segmen komersial."
					icon={Tag}
					data={[]}
					isLoading={false}
					columns={mockColumns}
					fields={mockFields}
					onSave={vi.fn()}
				/>,
			);

			expect(
				screen.getByText("Belum ada data referensi terdaftar."),
			).toBeInTheDocument();
		});
	});

	describe("CreateUserDialog", () => {
		it("renders form inputs when open", () => {
			renderWithClient(
				<CreateUserDialog
					open={true}
					onOpenChange={vi.fn()}
					onSuccess={vi.fn()}
				/>,
			);

			expect(screen.getByText("Tambah Pengguna Baru")).toBeInTheDocument();
			expect(screen.getByLabelText(/Nama Lengkap/i)).toBeInTheDocument();
			expect(screen.getByLabelText(/Nama Pengguna/i)).toBeInTheDocument();
			expect(screen.getByLabelText(/Email/i)).toBeInTheDocument();
			expect(screen.getByText("Buat Pengguna")).toBeInTheDocument();
		});
	});

	describe("EditRolesDialog", () => {
		it("renders active roles and add role controls for user", () => {
			renderWithClient(
				<EditRolesDialog
					user={mockUser}
					open={true}
					onOpenChange={vi.fn()}
					onSuccess={vi.fn()}
				/>,
			);

			expect(screen.getByText("Ubah Peran Pengguna")).toBeInTheDocument();
			expect(screen.getByText("Peran & Lingkup Aktif")).toBeInTheDocument();
			expect(screen.getByText("Tambah Peran Baru")).toBeInTheDocument();
		});
	});

	describe("ResetPasswordDialog", () => {
		it("renders password reset warning and confirmation button", () => {
			renderWithClient(
				<ResetPasswordDialog
					user={mockUser}
					open={true}
					onOpenChange={vi.fn()}
					onSuccess={vi.fn()}
				/>,
			);

			expect(screen.getByText("Atur Ulang Kata Sandi")).toBeInTheDocument();
			expect(screen.getByText("Buat Kata Sandi Baru")).toBeInTheDocument();
			expect(screen.getByText("Batal")).toBeInTheDocument();
		});
	});

	describe("UsersView", () => {
		it("renders user table headers and search input", () => {
			renderWithClient(<UsersView />);

			expect(
				screen.getByText("Pengguna — Manajemen Akun & Hak Akses"),
			).toBeInTheDocument();
			expect(
				screen.getByPlaceholderText("Cari nama, peran, email..."),
			).toBeInTheDocument();
			expect(screen.getByText("Tambah Pengguna")).toBeInTheDocument();
		});
	});

	describe("OrganisationView", () => {
		it("renders organisation header and action buttons", () => {
			renderWithClient(<OrganisationView />);

			expect(
				screen.getByText("Organisasi — Struktur Wilayah & Sales Area"),
			).toBeInTheDocument();
			expect(screen.getByText("Tambah Wilayah")).toBeInTheDocument();
			expect(screen.getByText("Tambah Sales Area")).toBeInTheDocument();
		});
	});
});
