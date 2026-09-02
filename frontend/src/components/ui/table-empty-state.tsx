import { FileQuestion, FolderOpen, RotateCcw, SearchX } from "lucide-react";
import type * as React from "react";
import { Button } from "@/components/ui/button";
import { TableCell, TableRow } from "@/components/ui/table";
import { cn } from "@/lib/utils";

interface TableEmptyStateProps {
	colSpan?: number;
	icon?: "search" | "empty" | "folder";
	title?: string;
	description?: string;
	onReset?: () => void;
	resetLabel?: string;
	action?: React.ReactNode;
	className?: string;
}

export function TableEmptyState({
	colSpan,
	icon = "search",
	title = "Tidak Ada Data Ditemukan",
	description = "Tidak ada rekaman yang sesuai dengan filter atau kriteria pencarian Anda.",
	onReset,
	resetLabel = "Reset Filter",
	action,
	className,
}: TableEmptyStateProps) {
	const content = (
		<div
			className={cn(
				"flex flex-col items-center justify-center py-12 px-4 text-center max-w-sm mx-auto space-y-3",
				className,
			)}
		>
			<div className="flex size-12 items-center justify-center rounded-full bg-muted/60 text-muted-foreground ring-8 ring-muted/20">
				{icon === "search" && <SearchX className="size-6" />}
				{icon === "empty" && <FileQuestion className="size-6" />}
				{icon === "folder" && <FolderOpen className="size-6" />}
			</div>

			<div className="space-y-1">
				<h4 className="text-sm font-semibold tracking-tight text-foreground">
					{title}
				</h4>
				<p className="text-xs text-muted-foreground leading-relaxed">
					{description}
				</p>
			</div>

			{onReset && (
				<Button
					variant="outline"
					size="sm"
					onClick={onReset}
					className="h-8 text-xs gap-1.5 mt-2"
				>
					<RotateCcw className="size-3" />
					<span>{resetLabel}</span>
				</Button>
			)}

			{action && <div className="mt-2">{action}</div>}
		</div>
	);

	if (colSpan !== undefined) {
		return (
			<TableRow>
				<TableCell colSpan={colSpan} className="h-48 p-0">
					{content}
				</TableCell>
			</TableRow>
		);
	}

	return content;
}
