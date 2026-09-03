import type * as React from "react";
import { cn } from "@/lib/utils";

interface PageHeaderProps {
	title: string;
	description?: string;
	badge?: React.ReactNode;
	actions?: React.ReactNode;
	children?: React.ReactNode;
	className?: string;
}

export function PageHeader({
	title,
	description,
	badge,
	actions,
	children,
	className,
}: PageHeaderProps) {
	return (
		<div
			className={cn(
				"flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between",
				className,
			)}
		>
			<div className="space-y-1">
				<div className="flex items-center gap-2.5">
					<h1 className="text-2xl font-bold tracking-tight text-foreground">
						{title}
					</h1>
					{badge}
				</div>
				{description && (
					<p className="text-sm text-muted-foreground">{description}</p>
				)}
				{children}
			</div>
			{actions && (
				<div className="flex flex-wrap items-center gap-2.5 shrink-0">
					{actions}
				</div>
			)}
		</div>
	);
}
