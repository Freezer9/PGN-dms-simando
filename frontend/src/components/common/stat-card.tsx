import type { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

interface StatCardProps {
	title: string;
	value: string | number;
	description?: string;
	icon: LucideIcon;
	variant?: "default" | "amber" | "rose" | "emerald" | "blue" | "primary";
	badge?: string;
	trend?: {
		value: string | number;
		isPositive?: boolean;
		label?: string;
	};
	className?: string;
	onClick?: () => void;
}

export function StatCard({
	title,
	value,
	description,
	icon: Icon,
	variant = "default",
	badge,
	trend,
	className,
	onClick,
}: StatCardProps) {
	const variantStyles = {
		default: "border-border/60 bg-card hover:border-border",
		amber:
			"border-amber-200/80 bg-amber-50/40 dark:border-amber-900/40 dark:bg-amber-950/20",
		rose: "border-rose-200/80 bg-rose-50/40 dark:border-rose-900/40 dark:bg-rose-950/20",
		emerald:
			"border-emerald-200/80 bg-emerald-50/40 dark:border-emerald-900/40 dark:bg-emerald-950/20",
		blue: "border-blue-200/80 bg-blue-50/40 dark:border-blue-900/40 dark:bg-blue-950/20",
		primary:
			"border-primary/20 bg-primary/[0.03] dark:border-primary/30 dark:bg-primary/10",
	};

	const iconStyles = {
		default: "text-muted-foreground bg-muted/60",
		amber:
			"text-amber-600 dark:text-amber-400 bg-amber-100/70 dark:bg-amber-900/40",
		rose: "text-rose-600 dark:text-rose-400 bg-rose-100/70 dark:bg-rose-900/40",
		emerald:
			"text-emerald-600 dark:text-emerald-400 bg-emerald-100/70 dark:bg-emerald-900/40",
		blue: "text-blue-600 dark:text-blue-400 bg-blue-100/70 dark:bg-blue-900/40",
		primary:
			"text-primary bg-primary/10 dark:text-primary-foreground dark:bg-primary/20",
	};

	const cardContent = (
		<>
			<div className="flex items-center justify-between pb-2">
				<p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
					{title}
				</p>
				<div
					className={cn(
						"size-9 rounded-lg flex items-center justify-center shadow-xs",
						iconStyles[variant],
					)}
				>
					<Icon className="size-4.5" />
				</div>
			</div>
			<div className="flex items-baseline justify-between mt-1">
				<div className="text-2xl font-bold tracking-tight text-foreground">
					{value}
				</div>
				{badge && (
					<span className="text-[11px] font-semibold px-2 py-0.5 rounded-full bg-background/80 border text-muted-foreground">
						{badge}
					</span>
				)}
			</div>
			{(description || trend) && (
				<div className="flex items-center gap-1.5 mt-2 text-xs text-muted-foreground">
					{trend && (
						<span
							className={cn(
								"font-semibold",
								trend.isPositive
									? "text-emerald-600 dark:text-emerald-400"
									: "text-rose-600 dark:text-rose-400",
							)}
						>
							{trend.value}
						</span>
					)}
					{description && <p className="line-clamp-1">{description}</p>}
				</div>
			)}
		</>
	);

	if (onClick) {
		return (
			<button
				type="button"
				onClick={onClick}
				className={cn(
					"rounded-xl border p-5 shadow-xs text-left w-full transition-all duration-200 cursor-pointer hover:shadow hover:-translate-y-0.5 transition-transform",
					variantStyles[variant],
					className,
				)}
			>
				{cardContent}
			</button>
		);
	}

	return (
		<div
			className={cn(
				"rounded-xl border p-5 shadow-xs transition-all duration-200",
				variantStyles[variant],
				className,
			)}
		>
			{cardContent}
		</div>
	);
}
