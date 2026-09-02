import {
	type ColumnDef,
	flexRender,
	getCoreRowModel,
	getPaginationRowModel,
	useReactTable,
} from "@tanstack/react-table";
import {
	AlertCircle,
	AlertTriangle,
	CheckCircle2,
	Info,
	KeyRound,
	MoreVertical,
	Plus,
	Search,
	Shield,
	UserCheck,
	UserCog,
	Users,
	UserX,
	XCircle,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { UserListItemDto } from "@/api/types";
import { CreateUserDialog } from "@/components/admin/create-user-dialog";
import { EditRolesDialog } from "@/components/admin/edit-roles-dialog";
import { ResetPasswordDialog } from "@/components/admin/reset-password-dialog";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuSeparator,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";
import { TableEmptyState } from "@/components/ui/table-empty-state";
import { TablePagination } from "@/components/ui/table-pagination";
import { TableSkeleton } from "@/components/ui/table-skeleton";

export function UsersView() {
	const [searchTerm, setSearchTerm] = React.useState("");
	const [createUserOpen, setCreateUserOpen] = React.useState(false);
	const [selectedUserForRoles, setSelectedUserForRoles] =
		React.useState<UserListItemDto | null>(null);
	const [selectedUserForPassword, setSelectedUserForPassword] =
		React.useState<UserListItemDto | null>(null);

	const {
		data: users,
		isLoading,
		refetch,
	} = $api.useQuery("get", "/api/admin/users");

	const setStatusMutation = $api.useMutation(
		"put",
		"/api/admin/users/{id}/status",
		{
			onSuccess: () => {
				refetch();
			},
		},
	);

	const userList = React.useMemo(() => users || [], [users]);

	// Filter users
	const filteredUsers = React.useMemo(() => {
		if (!searchTerm.trim()) return userList;
		const q = searchTerm.toLowerCase();
		return userList.filter((u) => {
			const nameMatch = u.fullName.toLowerCase().includes(q);
			const userMatch = u.username.toLowerCase().includes(q);
			const emailMatch = (u.email || "").toLowerCase().includes(q);
			const roleMatch = u.roles.some(
				(r) =>
					r.role.toLowerCase().includes(q) ||
					(r.scopeLabel || "").toLowerCase().includes(q),
			);
			return nameMatch || userMatch || emailMatch || roleMatch;
		});
	}, [userList, searchTerm]);

	// Stats for compensating controls
	const neverLoggedInCount = userList.filter((u) => !u.lastLoginAt).length;
	const now = Date.now();
	const dormant90DaysCount = userList.filter((u) => {
		if (!u.lastLoginAt || !u.active) return false;
		const loginTime = new Date(u.lastLoginAt).getTime();
		const diffDays = Math.floor((now - loginTime) / (1000 * 60 * 60 * 24));
		return diffDays > 90;
	}).length;

	const handleToggleStatus = React.useCallback(
		(u: UserListItemDto) => {
			setStatusMutation.mutate({
				params: {
					path: { id: u.id },
				},
				body: {
					active: !u.active,
				},
			});
		},
		[setStatusMutation],
	);

	const formatLastLogin = React.useCallback(
		(lastLoginAt?: string | null) => {
			if (!lastLoginAt) {
				return {
					text: "belum pernah",
					isNew: true,
					isDormant: false,
				};
			}
			const loginTime = new Date(lastLoginAt).getTime();
			const diffHours = Math.floor((now - loginTime) / (1000 * 60 * 60));
			const diffDays = Math.floor(diffHours / 24);

			if (diffHours < 1) {
				return { text: "baru saja", isNew: false, isDormant: false };
			}
			if (diffHours < 24) {
				return { text: `${diffHours} jam`, isNew: false, isDormant: false };
			}
			if (diffDays === 1) {
				return { text: "kemarin", isNew: false, isDormant: false };
			}
			return {
				text: `${diffDays} hari`,
				isNew: false,
				isDormant: diffDays > 90,
			};
		},
		[now],
	);

	// Columns using OpenAPI UserListItemDto
	const columns = React.useMemo<ColumnDef<UserListItemDto>[]>(
		() => [
			{
				accessorKey: "fullName",
				header: "Nama",
				cell: ({ row }) => {
					const u = row.original;
					return (
						<div className="flex flex-col">
							<span className="font-semibold text-foreground text-sm">
								{u.fullName}
							</span>
							<div className="flex items-center gap-2 text-xs text-muted-foreground">
								<span className="font-mono">{u.username}</span>
								{u.email && (
									<>
										<span>·</span>
										<span>{u.email}</span>
									</>
								)}
							</div>
						</div>
					);
				},
			},
			{
				id: "roles",
				header: "Peran & Lingkup",
				cell: ({ row }) => {
					const u = row.original;
					return (
						<div className="flex flex-wrap gap-1.5 max-w-sm">
							{u.roles.map((r, roleIdx) => {
								const assignmentId = u.assignmentIds?.[roleIdx];
								const roleKey =
									assignmentId || `${u.id}-${r.role}-${r.scopeLabel}`;
								return (
									<Badge
										key={roleKey}
										variant="outline"
										className="text-[11px] bg-background/80 py-0.5"
									>
										<Shield className="h-3 w-3 mr-1 text-primary shrink-0" />
										<strong className="text-foreground">{r.role}</strong>
										<span className="text-muted-foreground ml-1">
											({r.scopeLabel || "Nasional"})
										</span>
									</Badge>
								);
							})}
						</div>
					);
				},
			},
			{
				accessorKey: "active",
				header: "Status",
				cell: ({ row }) => {
					const u = row.original;
					const lastLogin = formatLastLogin(u.lastLoginAt);
					if (!u.active) {
						return (
							<Badge
								variant="outline"
								className="bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/40 dark:text-rose-300 dark:border-rose-800/60 inline-flex items-center gap-1 text-xs"
							>
								<XCircle className="h-3 w-3" />
								<span>Nonaktif</span>
							</Badge>
						);
					}
					if (lastLogin.isNew) {
						return (
							<Badge
								variant="outline"
								className="bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800/60 inline-flex items-center gap-1 text-xs"
							>
								<AlertTriangle className="h-3 w-3" />
								<span>Baru</span>
							</Badge>
						);
					}
					return (
						<Badge
							variant="outline"
							className="bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800/60 inline-flex items-center gap-1 text-xs"
						>
							<CheckCircle2 className="h-3 w-3" />
							<span>Aktif</span>
						</Badge>
					);
				},
			},
			{
				accessorKey: "lastLoginAt",
				header: "Login Terakhir",
				cell: ({ row }) => {
					const lastLogin = formatLastLogin(row.original.lastLoginAt);
					return (
						<div className="flex items-center gap-1.5 text-xs">
							{lastLogin.isDormant ? (
								<span className="text-destructive font-semibold flex items-center gap-1">
									<AlertCircle className="h-3.5 w-3.5" />
									{lastLogin.text}
								</span>
							) : lastLogin.isNew ? (
								<span className="text-muted-foreground italic">
									belum pernah
								</span>
							) : (
								<span className="text-muted-foreground">{lastLogin.text}</span>
							)}
						</div>
					);
				},
			},
			{
				id: "actions",
				header: () => <div className="text-right pr-4">Tindakan</div>,
				cell: ({ row }) => {
					const u = row.original;
					return (
						<div className="text-right pr-2">
							<DropdownMenu>
								<DropdownMenuTrigger asChild>
									<Button
										variant="ghost"
										size="icon"
										className="h-8 w-8 text-muted-foreground hover:text-foreground"
									>
										<MoreVertical className="h-4 w-4" />
									</Button>
								</DropdownMenuTrigger>
								<DropdownMenuContent align="end" className="w-48">
									<DropdownMenuItem
										onClick={() => setSelectedUserForRoles(u)}
										className="gap-2 cursor-pointer text-xs"
									>
										<UserCog className="h-4 w-4 text-primary" />
										<span>Ubah Peran & Lingkup</span>
									</DropdownMenuItem>
									<DropdownMenuItem
										onClick={() => setSelectedUserForPassword(u)}
										className="gap-2 cursor-pointer text-xs"
									>
										<KeyRound className="h-4 w-4 text-primary" />
										<span>Atur Ulang Kata Sandi</span>
									</DropdownMenuItem>
									<DropdownMenuSeparator />
									<DropdownMenuItem
										onClick={() => handleToggleStatus(u)}
										className="gap-2 cursor-pointer text-xs"
									>
										{u.active ? (
											<>
												<UserX className="h-4 w-4 text-destructive" />
												<span className="text-destructive">
													Nonaktifkan Akun
												</span>
											</>
										) : (
											<>
												<UserCheck className="h-4 w-4 text-emerald-600" />
												<span className="text-emerald-600">Aktifkan Akun</span>
											</>
										)}
									</DropdownMenuItem>
								</DropdownMenuContent>
							</DropdownMenu>
						</div>
					);
				},
			},
		],
		[formatLastLogin, handleToggleStatus],
	);

	const [pagination, setPagination] = React.useState({
		pageIndex: 0,
		pageSize: 10,
	});

	const table = useReactTable({
		data: filteredUsers,
		columns,
		getCoreRowModel: getCoreRowModel(),
		getPaginationRowModel: getPaginationRowModel(),
		onPaginationChange: setPagination,
		state: {
			pagination,
		},
	});

	return (
		<div className="space-y-4">
			{/* Header */}
			<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
				<div className="space-y-0.5">
					<h2 className="text-lg font-bold tracking-tight text-foreground flex items-center gap-2">
						<Users className="h-5 w-5 text-primary" />
						<span>Pengguna — Manajemen Akun & Hak Akses</span>
					</h2>
					<p className="text-xs text-muted-foreground">
						Daftar pengguna dan penugasan peran dalam lingkup administrasi Anda.
					</p>
				</div>

				<div className="flex items-center gap-2">
					<div className="relative w-64">
						<Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
						<Input
							placeholder="Cari nama, peran, email..."
							value={searchTerm}
							onChange={(e) => {
								setSearchTerm(e.target.value);
								setPagination((prev) => ({ ...prev, pageIndex: 0 }));
							}}
							className="pl-8 h-9 text-xs"
						/>
					</div>
					<Button
						onClick={() => setCreateUserOpen(true)}
						size="sm"
						className="h-9 gap-1.5 text-xs shrink-0"
					>
						<Plus className="h-4 w-4" />
						<span>Tambah Pengguna</span>
					</Button>
				</div>
			</div>

			{/* Info Notice */}
			<Alert className="bg-muted/40 border-muted text-muted-foreground py-2.5">
				<Info className="h-4 w-4 text-primary shrink-0" />
				<AlertDescription className="text-xs">
					Akun dikelola langsung di dalam sistem DMS Simando. Kata sandi
					sementara dibuat saat pembuatan akun atau reset kata sandi dan wajib
					diperbarui oleh pengguna saat login pertama.
				</AlertDescription>
			</Alert>

			{/* Users Table with TanStack Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table>
					<TableHeader className="bg-muted/40">
						{table.getHeaderGroups().map((headerGroup) => (
							<TableRow key={headerGroup.id}>
								{headerGroup.headers.map((header) => (
									<TableHead
										key={header.id}
										className="font-semibold text-xs py-3"
									>
										{header.isPlaceholder
											? null
											: flexRender(
													header.column.columnDef.header,
													header.getContext(),
												)}
									</TableHead>
								))}
							</TableRow>
						))}
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableSkeleton columns={columns.length} rows={5} />
						) : table.getRowModel().rows?.length ? (
							table.getRowModel().rows.map((row) => (
								<TableRow
									key={row.id}
									className="hover:bg-muted/30 transition-colors"
								>
									{row.getVisibleCells().map((cell) => (
										<TableCell key={cell.id} className="py-3">
											{flexRender(
												cell.column.columnDef.cell,
												cell.getContext(),
											)}
										</TableCell>
									))}
								</TableRow>
							))
						) : (
							<TableEmptyState
								colSpan={columns.length}
								icon={searchTerm ? "search" : "empty"}
								title={
									searchTerm
										? "Tidak Ada Pengguna Ditemukan"
										: "Belum Ada Pengguna"
								}
								description={
									searchTerm
										? "Coba ubah kata kunci pencarian Anda."
										: "Belum ada pengguna terdaftar dalam sistem."
								}
								onReset={searchTerm ? () => setSearchTerm("") : undefined}
								resetLabel="Reset Pencarian"
							/>
						)}
					</TableBody>
				</Table>

				{/* Table Pagination */}
				{filteredUsers.length > 0 && (
					<TablePagination
						pageIndex={pagination.pageIndex}
						page={pagination.pageIndex + 1}
						pageSize={pagination.pageSize}
						totalCount={filteredUsers.length}
						totalPages={table.getPageCount()}
						onPageChange={(page) => table.setPageIndex(page - 1)}
						onPageSizeChange={(size) => table.setPageSize(size)}
						pageSizeOptions={[10, 25, 50]}
						className="border-t px-4"
					/>
				)}
			</div>

			{/* Dormant / New Account Warning Banner */}
			{(neverLoggedInCount > 0 || dormant90DaysCount > 0) && (
				<div className="flex items-center gap-4 text-xs text-muted-foreground bg-muted/30 px-4 py-2.5 rounded-lg border">
					{neverLoggedInCount > 0 && (
						<div className="flex items-center gap-1.5 text-amber-600 dark:text-amber-400">
							<AlertTriangle className="h-4 w-4 shrink-0" />
							<span>
								<strong>{neverLoggedInCount}</strong> akun belum pernah login
							</span>
						</div>
					)}
					{dormant90DaysCount > 0 && (
						<div className="flex items-center gap-1.5 text-destructive">
							<AlertCircle className="h-4 w-4 shrink-0" />
							<span>
								<strong>{dormant90DaysCount}</strong> akun aktif tidak login
								&gt;90 hari
							</span>
						</div>
					)}
				</div>
			)}

			{/* Dialogs */}
			<CreateUserDialog
				open={createUserOpen}
				onOpenChange={setCreateUserOpen}
				onSuccess={() => refetch()}
			/>

			{selectedUserForRoles && (
				<EditRolesDialog
					user={selectedUserForRoles}
					open={!!selectedUserForRoles}
					onOpenChange={(open) => {
						if (!open) setSelectedUserForRoles(null);
					}}
					onSuccess={() => refetch()}
				/>
			)}

			{selectedUserForPassword && (
				<ResetPasswordDialog
					user={selectedUserForPassword}
					open={!!selectedUserForPassword}
					onOpenChange={(open) => {
						if (!open) setSelectedUserForPassword(null);
					}}
					onSuccess={() => refetch()}
				/>
			)}
		</div>
	);
}
