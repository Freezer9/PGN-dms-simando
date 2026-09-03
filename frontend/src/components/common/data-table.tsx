import { flexRender, type Table as TanStackTable } from "@tanstack/react-table";
import type * as React from "react";
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
import { cn } from "@/lib/utils";

interface DataTableToolbarProps {
	children: React.ReactNode;
	actions?: React.ReactNode;
	className?: string;
}

export function DataTableToolbar({
	children,
	actions,
	className,
}: DataTableToolbarProps) {
	return (
		<div
			className={cn(
				"flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3",
				className,
			)}
		>
			<div className="flex flex-1 flex-wrap items-center gap-2.5">
				{children}
			</div>
			{actions && (
				<div className="flex items-center gap-2 shrink-0">{actions}</div>
			)}
		</div>
	);
}

interface DataTableContainerProps {
	children: React.ReactNode;
	className?: string;
}

export function DataTableContainer({
	children,
	className,
}: DataTableContainerProps) {
	return (
		<div
			className={cn(
				"rounded-lg border bg-card shadow-xs overflow-hidden",
				className,
			)}
		>
			{children}
		</div>
	);
}

interface DataTableProps<TData> {
	table: TanStackTable<TData>;
	columnsCount: number;
	isLoading?: boolean;
	skeletonRows?: number;
	emptyTitle?: string;
	emptyDescription?: string;
	emptyIcon?: "search" | "empty" | "folder";
	onResetFilters?: () => void;
	resetLabel?: string;
	emptyAction?: React.ReactNode;
	className?: string;
	pagination?: {
		page: number;
		pageSize: number;
		totalCount: number;
		totalPages: number;
		onPageChange: (newPage: number) => void;
		onPageSizeChange?: (newPageSize: number) => void;
		pageSizeOptions?: number[];
	};
}

export function DataTable<TData>({
	table,
	columnsCount,
	isLoading = false,
	skeletonRows = 5,
	emptyTitle = "Tidak Ada Data",
	emptyDescription = "Tidak ada rekaman data yang sesuai dengan kriteria yang dipilih.",
	emptyIcon = "search",
	onResetFilters,
	resetLabel = "Reset Filter",
	emptyAction,
	className,
	pagination,
}: DataTableProps<TData>) {
	return (
		<div className="space-y-4">
			<DataTableContainer className={className}>
				<Table>
					<TableHeader className="bg-muted/40">
						{table.getHeaderGroups().map((headerGroup) => (
							<TableRow key={headerGroup.id}>
								{headerGroup.headers.map((header) => {
									const meta = header.column.columnDef.meta as
										| { headerClassName?: string; className?: string }
										| undefined;
									return (
										<TableHead
											key={header.id}
											className={cn(
												"h-10 px-3 text-xs font-semibold text-muted-foreground uppercase tracking-wider",
												meta?.headerClassName || meta?.className,
											)}
											style={
												header.column.columnDef.size &&
												header.column.columnDef.size !== 150
													? { minWidth: `${header.column.columnDef.size}px` }
													: undefined
											}
										>
											{header.isPlaceholder
												? null
												: flexRender(
														header.column.columnDef.header,
														header.getContext(),
													)}
										</TableHead>
									);
								})}
							</TableRow>
						))}
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableSkeleton columns={columnsCount} rows={skeletonRows} />
						) : table.getRowModel().rows?.length ? (
							table.getRowModel().rows.map((row) => (
								<TableRow
									key={row.id}
									data-state={row.getIsSelected() && "selected"}
									className="hover:bg-muted/30 transition-colors"
								>
									{row.getVisibleCells().map((cell) => {
										const meta = cell.column.columnDef.meta as
											| { cellClassName?: string; className?: string }
											| undefined;
										return (
											<TableCell
												key={cell.id}
												className={cn(
													"py-3 px-3",
													meta?.cellClassName || meta?.className,
												)}
												style={
													cell.column.columnDef.size &&
													cell.column.columnDef.size !== 150
														? { minWidth: `${cell.column.columnDef.size}px` }
														: undefined
												}
											>
												{flexRender(
													cell.column.columnDef.cell,
													cell.getContext(),
												)}
											</TableCell>
										);
									})}
								</TableRow>
							))
						) : (
							<TableEmptyState
								colSpan={columnsCount}
								icon={emptyIcon}
								title={emptyTitle}
								description={emptyDescription}
								onReset={onResetFilters}
								resetLabel={resetLabel}
								action={emptyAction}
							/>
						)}
					</TableBody>
				</Table>
			</DataTableContainer>

			{pagination && pagination.totalCount > 0 && (
				<TablePagination
					pageIndex={pagination.page - 1}
					page={pagination.page}
					pageSize={pagination.pageSize}
					totalCount={pagination.totalCount}
					totalPages={pagination.totalPages}
					onPageChange={pagination.onPageChange}
					onPageSizeChange={pagination.onPageSizeChange}
					pageSizeOptions={pagination.pageSizeOptions}
				/>
			)}
		</div>
	);
}
