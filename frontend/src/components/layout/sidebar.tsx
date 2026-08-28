import { Link } from "@tanstack/react-router";
import { ChevronDown } from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import { Badge } from "@/components/ui/badge";
import { useAuth } from "@/lib/auth";
import {
	buildNavigationMenu,
	type NavGroup,
	type NavItem,
} from "@/lib/navigation";
import { cn } from "@/lib/utils";
import { DynamicIcon } from "./icon";

export function Sidebar({ className }: { className?: string }) {
	const { user } = useAuth();

	// Live task summary query for badge counts
	const { data: taskSummary } = $api.useQuery(
		"get",
		"/api/tasks/summary",
		undefined,
		{
			enabled: !!user,
			refetchInterval: 30000, // Refresh counts every 30s
		},
	);

	const menu = React.useMemo(
		() =>
			buildNavigationMenu(
				user,
				taskSummary?.myTasksCount,
				taskSummary?.blockedTasksCount,
			),
		[user, taskSummary?.myTasksCount, taskSummary?.blockedTasksCount],
	);

	return (
		<aside
			className={cn(
				"w-64 border-r bg-card/50 backdrop-blur-sm flex flex-col shrink-0 min-h-[calc(100vh-3.5rem)]",
				className,
			)}
		>
			<div className="flex-1 py-4 px-3 space-y-6 overflow-y-auto">
				{menu.sections.map((section, idx) => (
					<div key={section.title ?? `section-${idx}`} className="space-y-1">
						{section.title && (
							<h4 className="px-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground/80 mb-2">
								{section.title}
							</h4>
						)}
						<nav className="space-y-1">
							{section.nodes.map((node) => {
								if (node.type === "item") {
									return <SidebarNavItem key={node.href} item={node} />;
								}
								return <SidebarNavGroup key={node.title} group={node} />;
							})}
						</nav>
					</div>
				))}
			</div>
		</aside>
	);
}

function SidebarNavItem({ item }: { item: NavItem }) {
	return (
		<Link
			to={item.href}
			activeOptions={{ exact: item.href === "/" }}
			className="group flex items-center justify-between px-3 py-2 text-sm font-medium rounded-md text-muted-foreground hover:bg-accent hover:text-accent-foreground transition-colors [&.active]:bg-primary/10 [&.active]:text-primary [&.active]:font-semibold"
		>
			<div className="flex items-center gap-2.5 min-w-0">
				<DynamicIcon
					name={item.icon}
					className="size-4 shrink-0 transition-colors group-[.active]:text-primary"
				/>
				<span className="truncate">{item.title}</span>
			</div>
			{item.badge !== undefined && item.badge !== null && (
				<Badge
					variant="secondary"
					className="ml-auto px-1.5 py-0.5 text-[10px] font-mono h-5 min-w-5 flex items-center justify-center bg-primary text-primary-foreground"
				>
					{item.badge}
				</Badge>
			)}
		</Link>
	);
}

function SidebarNavGroup({ group }: { group: NavGroup }) {
	const [isOpen, setIsOpen] = React.useState(true);

	return (
		<div className="space-y-1">
			<button
				type="button"
				onClick={() => setIsOpen(!isOpen)}
				className="w-full flex items-center justify-between px-3 py-2 text-sm font-medium rounded-md text-muted-foreground hover:bg-accent hover:text-accent-foreground transition-colors"
			>
				<div className="flex items-center gap-2.5 min-w-0">
					<DynamicIcon name={group.icon} className="size-4 shrink-0" />
					<span className="truncate">{group.title}</span>
				</div>
				<ChevronDown
					className={cn(
						"size-4 shrink-0 transition-transform duration-200",
						isOpen && "rotate-180",
					)}
				/>
			</button>
			{isOpen && (
				<div className="pl-6 space-y-1">
					{group.items.map((subItem) => (
						<SidebarNavItem key={subItem.href} item={subItem} />
					))}
				</div>
			)}
		</div>
	);
}
