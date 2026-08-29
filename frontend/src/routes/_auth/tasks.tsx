import {
	createFileRoute,
	Link,
	Outlet,
	useLocation,
} from "@tanstack/react-router";
import { AlertOctagon, CheckCircle2, Inbox } from "lucide-react";
import { $api } from "@/api/client";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/_auth/tasks")({
	component: TasksLayout,
});

function TasksLayout() {
	const location = useLocation();

	// Fetch live task counts for badges
	const { data: summary } = $api.useQuery("get", "/api/tasks/summary");

	const navItems = [
		{
			title: "Perlu Tindakan Saya",
			to: "/tasks",
			icon: Inbox,
			exact: true,
			count: summary?.myTasksCount != null ? Number(summary.myTasksCount) : 0,
			badgeVariant: "default" as const,
			active: location.pathname === "/tasks",
		},
		{
			title: "Tugas Tertahan",
			to: "/tasks/blocked",
			icon: AlertOctagon,
			exact: false,
			count:
				summary?.blockedTasksCount != null
					? Number(summary.blockedTasksCount)
					: 0,
			badgeVariant: "destructive" as const,
			active: location.pathname.startsWith("/tasks/blocked"),
		},
		{
			title: "Riwayat Persetujuan",
			to: "/tasks/history",
			icon: CheckCircle2,
			exact: false,
			count: null,
			badgeVariant: "secondary" as const,
			active: location.pathname.startsWith("/tasks/history"),
		},
	];

	return (
		<div className="space-y-6">
			{/* Page Header */}
			<div className="flex flex-col gap-1">
				<h1 className="text-2xl font-bold tracking-tight text-foreground sm:text-3xl flex items-center gap-2.5">
					<Inbox className="h-7 w-7 text-primary" />
					<span>Tugas & Persetujuan</span>
				</h1>
				<p className="text-sm text-muted-foreground">
					Kelola antrean verifikasi, tindak lanjut persetujuan berjenjang, dan
					pantau kepatuhan SLA proses bisnis.
				</p>
			</div>

			{/* Navigation Tabs Bar */}
			<div className="flex items-center gap-2 border-b border-border/80 overflow-x-auto pb-px">
				{navItems.map((item) => {
					const Icon = item.icon;
					const isActive = item.active;

					return (
						<Link
							key={item.to}
							to={item.to}
							className={cn(
								"flex items-center gap-2 px-4 py-2.5 text-sm font-medium border-b-2 whitespace-nowrap transition-colors",
								isActive
									? "border-primary text-primary font-semibold"
									: "border-transparent text-muted-foreground hover:text-foreground hover:border-muted-foreground/30",
							)}
						>
							<Icon className="h-4 w-4 shrink-0" />
							<span>{item.title}</span>
							{item.count !== null && item.count > 0 && (
								<Badge
									variant={item.badgeVariant}
									className={cn(
										"h-5 min-w-5 px-1.5 flex items-center justify-center rounded-full text-[11px] font-bold",
										item.badgeVariant === "destructive" && "animate-pulse",
									)}
								>
									{item.count}
								</Badge>
							)}
						</Link>
					);
				})}
			</div>

			{/* Subroute Content */}
			<Outlet />
		</div>
	);
}
