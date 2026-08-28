import { AlertCircle, AlertTriangle, Clock } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

export interface SlaClockBadgeProps {
	waitingSince: string | Date;
	showIcon?: boolean;
	className?: string;
}

export function formatWaitingDuration(waitingSince: string | Date): {
	days: number;
	hours: number;
	label: string;
	status: "normal" | "warning" | "urgent";
} {
	const now = Date.now();
	const since = new Date(waitingSince).getTime();
	const diffMs = Math.max(0, now - since);

	const diffMinutes = Math.floor(diffMs / (1000 * 60));
	const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
	const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

	let label = "";
	if (diffDays >= 1) {
		const remainingHours = diffHours % 24;
		label =
			remainingHours > 0
				? `${diffDays} hari ${remainingHours} jam`
				: `${diffDays} hari`;
	} else if (diffHours >= 1) {
		label = `${diffHours} jam`;
	} else {
		label = `${Math.max(1, diffMinutes)} mnt`;
	}

	let status: "normal" | "warning" | "urgent" = "normal";
	if (diffDays >= 7) {
		status = "urgent";
	} else if (diffDays >= 3) {
		status = "warning";
	} else {
		status = "normal";
	}

	return {
		days: diffDays,
		hours: diffHours,
		label,
		status,
	};
}

export function SlaClockBadge({
	waitingSince,
	showIcon = true,
	className,
}: SlaClockBadgeProps) {
	const { label, status } = formatWaitingDuration(waitingSince);

	return (
		<Badge
			variant="outline"
			className={cn(
				"inline-flex items-center gap-1.5 px-2.5 py-0.5 text-xs font-medium rounded-full transition-colors",
				status === "normal" &&
					"bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800/60",
				status === "warning" &&
					"bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800/60",
				status === "urgent" &&
					"bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/40 dark:text-rose-300 dark:border-rose-800/60 animate-pulse font-semibold shadow-xs",
				className,
			)}
			title={`Menunggu sejak ${new Date(waitingSince).toLocaleString("id-ID")}`}
		>
			{showIcon &&
				(status === "urgent" ? (
					<AlertCircle className="h-3 w-3 text-rose-600 dark:text-rose-400 shrink-0" />
				) : status === "warning" ? (
					<AlertTriangle className="h-3 w-3 text-amber-600 dark:text-amber-400 shrink-0" />
				) : (
					<Clock className="h-3 w-3 text-emerald-600 dark:text-emerald-400 shrink-0" />
				))}
			<span>{label}</span>
			{status === "urgent" && (
				<span className="text-[10px] uppercase font-bold tracking-wider">
					(Tertahan)
				</span>
			)}
		</Badge>
	);
}
