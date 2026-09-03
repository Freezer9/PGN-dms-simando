import * as React from "react";
import { Button } from "@/components/ui/button";
import {
	Tooltip,
	TooltipContent,
	TooltipProvider,
	TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

export interface IconButtonProps
	extends React.ComponentProps<typeof Button> {
	tooltip?: React.ReactNode;
	tooltipSide?: "top" | "bottom" | "left" | "right";
	danger?: boolean;
}

export const IconButton = React.forwardRef<HTMLButtonElement, IconButtonProps>(
	(
		{
			children,
			className,
			variant = "outline",
			size = "icon-sm",
			tooltip,
			tooltipSide = "top",
			danger = false,
			"aria-label": ariaLabel,
			...props
		},
		ref,
	) => {
		const label =
			ariaLabel || (typeof tooltip === "string" ? tooltip : undefined);

		const dangerClasses = danger
			? "text-destructive border-destructive/30 hover:bg-destructive/10 hover:text-destructive hover:border-destructive dark:border-destructive/40 dark:hover:bg-destructive/20"
			: undefined;

		const button = (
			<Button
				ref={ref}
				variant={variant}
				size={size}
				aria-label={label}
				className={cn(dangerClasses, className)}
				{...props}
			>
				{children}
			</Button>
		);

		if (!tooltip) {
			return button;
		}

		return (
			<TooltipProvider>
				<Tooltip>
					<TooltipTrigger asChild>{button}</TooltipTrigger>
					<TooltipContent side={tooltipSide}>{tooltip}</TooltipContent>
				</Tooltip>
			</TooltipProvider>
		);
	},
);

IconButton.displayName = "IconButton";
