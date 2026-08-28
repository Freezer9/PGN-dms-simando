import * as React from "react";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

export interface FormFieldProps extends React.HTMLAttributes<HTMLDivElement> {
	label?: string;
	required?: boolean;
	error?: string | null;
	description?: string;
}

export const FormField = React.forwardRef<HTMLDivElement, FormFieldProps>(
	(
		{ label, required, error, description, className, children, ...props },
		ref,
	) => {
		return (
			<div ref={ref} className={cn("space-y-1.5", className)} {...props}>
				{label && (
					<Label
						className={cn(
							error && "text-destructive",
							"flex items-center gap-1",
						)}
					>
						{label}
						{required && <span className="text-destructive">*</span>}
					</Label>
				)}
				{children}
				{description && !error && (
					<p className="text-xs text-muted-foreground">{description}</p>
				)}
				{error && (
					<p className="text-xs font-medium text-destructive">{error}</p>
				)}
			</div>
		);
	},
);

FormField.displayName = "FormField";
