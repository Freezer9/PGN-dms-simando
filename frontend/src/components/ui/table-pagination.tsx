import {
	ChevronLeft,
	ChevronRight,
	ChevronsLeft,
	ChevronsRight,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";

interface TablePaginationProps {
	pageIndex: number; // 1-based or 0-based, we'll expose 1-based page
	page: number;
	pageSize: number;
	totalCount: number;
	totalPages: number;
	onPageChange: (newPage: number) => void;
	onPageSizeChange?: (newPageSize: number) => void;
	pageSizeOptions?: number[];
	className?: string;
}

export function TablePagination({
	page,
	pageSize,
	totalCount,
	totalPages,
	onPageChange,
	onPageSizeChange,
	pageSizeOptions = [10, 25, 50, 100],
	className,
}: TablePaginationProps) {
	const startItem = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
	const endItem = Math.min(page * pageSize, totalCount);

	return (
		<div
			className={`flex flex-col sm:flex-row items-center justify-between gap-4 py-4 px-2 text-xs text-muted-foreground ${className || ""}`}
		>
			<div className="flex items-center gap-4">
				<span>
					Menampilkan{" "}
					<strong className="text-foreground font-semibold">
						{startItem} - {endItem}
					</strong>{" "}
					dari{" "}
					<strong className="text-foreground font-semibold">
						{totalCount}
					</strong>{" "}
					data
				</span>

				{onPageSizeChange && (
					<div className="flex items-center gap-2">
						<span className="hidden sm:inline">Per halaman:</span>
						<Select
							value={pageSize.toString()}
							onValueChange={(val) => onPageSizeChange(Number(val))}
						>
							<SelectTrigger className="h-8 w-16 text-xs bg-background">
								<SelectValue placeholder={pageSize.toString()} />
							</SelectTrigger>
							<SelectContent side="top">
								{pageSizeOptions.map((opt) => (
									<SelectItem key={opt} value={opt.toString()}>
										{opt}
									</SelectItem>
								))}
							</SelectContent>
						</Select>
					</div>
				)}
			</div>

			<div className="flex items-center gap-2">
				<span className="text-xs font-medium mr-2">
					Halaman {page} dari {Math.max(1, totalPages)}
				</span>

				<div className="flex items-center gap-1">
					<Button
						variant="outline"
						size="icon"
						className="size-8"
						onClick={() => onPageChange(1)}
						disabled={page <= 1}
						title="Halaman Pertama"
					>
						<ChevronsLeft className="size-4" />
					</Button>
					<Button
						variant="outline"
						size="icon"
						className="size-8"
						onClick={() => onPageChange(page - 1)}
						disabled={page <= 1}
						title="Halaman Sebelumnya"
					>
						<ChevronLeft className="size-4" />
					</Button>
					<Button
						variant="outline"
						size="icon"
						className="size-8"
						onClick={() => onPageChange(page + 1)}
						disabled={page >= totalPages || totalPages === 0}
						title="Halaman Selanjutnya"
					>
						<ChevronRight className="size-4" />
					</Button>
					<Button
						variant="outline"
						size="icon"
						className="size-8"
						onClick={() => onPageChange(totalPages)}
						disabled={page >= totalPages || totalPages === 0}
						title="Halaman Terakhir"
					>
						<ChevronsRight className="size-4" />
					</Button>
				</div>
			</div>
		</div>
	);
}
