import { Skeleton } from "@/components/ui/skeleton";
import { TableCell, TableRow } from "@/components/ui/table";

interface TableSkeletonProps {
	columns: number;
	rows?: number;
	className?: string;
}

export function TableSkeleton({
	columns,
	rows = 5,
	className,
}: TableSkeletonProps) {
	return (
		<>
			{Array.from({ length: rows }).map((_, rowIndex) => (
				<TableRow
					// biome-ignore lint/suspicious/noArrayIndexKey: Static skeleton array
					key={`skeleton-row-${rowIndex}`}
					className={className}
				>
					{Array.from({ length: columns }).map((_, colIndex) => (
						<TableCell
							// biome-ignore lint/suspicious/noArrayIndexKey: Static skeleton array
							key={`skeleton-cell-${rowIndex}-${colIndex}`}
							className="py-3"
						>
							<Skeleton
								className={`h-4 w-full rounded ${
									colIndex === 0
										? "max-w-[120px]"
										: colIndex === columns - 1
											? "max-w-[80px] ml-auto"
											: "max-w-[200px]"
								}`}
							/>
						</TableCell>
					))}
				</TableRow>
			))}
		</>
	);
}
