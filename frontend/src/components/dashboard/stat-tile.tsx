import type { LucideIcon } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface StatTileProps {
	title: string;
	value: string | number;
	description?: string;
	icon: LucideIcon;
	variant?: "default" | "amber" | "rose" | "emerald" | "blue";
	badge?: string;
	className?: string;
	onClick?: () => void;
}

export function StatTile({
	title,
	value,
	description,
	icon: Icon,
	variant = "default",
	badge,
	className,
	onClick,
}: StatTileProps) {
	const variantStyles = {
		default: "border-border bg-card hover:border-border/80",
		amber:
			"border-amber-200 bg-amber-50/50 dark:border-amber-900/50 dark:bg-amber-950/20 text-amber-900 dark:text-amber-200",
		rose: "border-rose-200 bg-rose-50/50 dark:border-rose-900/50 dark:bg-rose-950/20 text-rose-900 dark:text-rose-200",
		emerald:
			"border-emerald-200 bg-emerald-50/50 dark:border-emerald-900/50 dark:bg-emerald-950/20 text-emerald-900 dark:text-emerald-200",
		blue: "border-blue-200 bg-blue-50/50 dark:border-blue-900/50 dark:bg-blue-950/20 text-blue-900 dark:text-blue-200",
	};

	const iconStyles = {
		default: "text-muted-foreground",
		amber: "text-amber-600 dark:text-amber-400",
		rose: "text-rose-600 dark:text-rose-400",
		emerald: "text-emerald-600 dark:text-emerald-400",
		blue: "text-blue-600 dark:text-blue-400",
	};

	return (
		<Card
			onClick={onClick}
			className={cn(
				"transition-all duration-200 shadow-sm",
				variantStyles[variant],
				onClick && "cursor-pointer hover:shadow hover:scale-[1.01]",
				className,
			)}
		>
			<CardContent className="p-5">
				<div className="flex items-center justify-between space-y-0 pb-2">
					<p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
						{title}
					</p>
					<div
						className={cn(
							"size-9 rounded-lg flex items-center justify-center bg-background/80 shadow-xs",
							iconStyles[variant],
						)}
					>
						<Icon className="size-4.5" />
					</div>
				</div>
				<div className="flex items-baseline justify-between mt-1">
					<div className="text-2xl font-bold tracking-tight">{value}</div>
					{badge && (
						<span className="text-[11px] font-semibold px-2 py-0.5 rounded-full bg-background/90 border text-muted-foreground">
							{badge}
						</span>
					)}
				</div>
				{description && (
					<p className="text-xs text-muted-foreground mt-1.5 line-clamp-1">
						{description}
					</p>
				)}
			</CardContent>
		</Card>
	);
}
