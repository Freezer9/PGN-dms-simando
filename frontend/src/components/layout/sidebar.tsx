import { Link } from "@tanstack/react-router";
import { ChevronDown } from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import {
	Collapsible,
	CollapsibleContent,
	CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
	Sidebar,
	SidebarContent,
	SidebarGroup,
	SidebarGroupContent,
	SidebarGroupLabel,
	SidebarHeader,
	SidebarMenu,
	SidebarMenuBadge,
	SidebarMenuButton,
	SidebarMenuItem,
	SidebarMenuSub,
	SidebarMenuSubButton,
	SidebarMenuSubItem,
	SidebarRail,
} from "@/components/ui/sidebar";
import { useAuth } from "@/lib/auth";
import {
	buildNavigationMenu,
	type NavGroup,
	type NavItem,
} from "@/lib/navigation";
import { DynamicIcon } from "./icon";

export function AppSidebar({ className }: { className?: string }) {
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
		<Sidebar collapsible="icon" className={className}>
			<SidebarHeader className="border-b h-14 flex items-center justify-between px-4">
				<Link to="/" className="flex items-center gap-2 font-semibold">
					<div className="flex size-7 items-center justify-center rounded-md bg-primary text-primary-foreground font-mono font-bold text-xs shadow-xs">
						PGN
					</div>
					<span className="font-bold text-primary tracking-tight text-base group-data-[collapsible=icon]:hidden">
						DMS Simando
					</span>
				</Link>
			</SidebarHeader>

			<SidebarContent className="py-2">
				{menu.sections.map((section, idx) => (
					<SidebarGroup key={section.title ?? `section-${idx}`}>
						{section.title && (
							<SidebarGroupLabel className="text-[11px] font-semibold tracking-wider text-muted-foreground/80 uppercase">
								{section.title}
							</SidebarGroupLabel>
						)}
						<SidebarGroupContent>
							<SidebarMenu>
								{section.nodes.map((node) => {
									if (node.type === "item") {
										return (
											<SidebarMenuItemComponent key={node.href} item={node} />
										);
									}
									return (
										<SidebarNavGroupComponent key={node.title} group={node} />
									);
								})}
							</SidebarMenu>
						</SidebarGroupContent>
					</SidebarGroup>
				))}
			</SidebarContent>

			<SidebarRail />
		</Sidebar>
	);
}

// Re-export as Sidebar for backward compatibility with existing imports
export { AppSidebar as Sidebar };

function SidebarMenuItemComponent({ item }: { item: NavItem }) {
	return (
		<SidebarMenuItem>
			<SidebarMenuButton asChild tooltip={item.title}>
				<Link
					to={item.href}
					activeOptions={{ exact: item.href === "/" }}
					className="[&.active]:bg-primary [&.active]:text-primary-foreground [&.active]:font-semibold"
				>
					<DynamicIcon name={item.icon} className="size-4 shrink-0" />
					<span className="truncate">{item.title}</span>
				</Link>
			</SidebarMenuButton>
			{item.badge !== undefined && item.badge !== null && (
				<SidebarMenuBadge className="bg-primary/10 text-primary font-mono text-[10px] font-semibold">
					{item.badge}
				</SidebarMenuBadge>
			)}
		</SidebarMenuItem>
	);
}

function SidebarNavGroupComponent({ group }: { group: NavGroup }) {
	return (
		<Collapsible defaultOpen className="group/collapsible">
			<SidebarMenuItem>
				<CollapsibleTrigger asChild>
					<SidebarMenuButton tooltip={group.title}>
						<DynamicIcon name={group.icon} className="size-4 shrink-0" />
						<span className="truncate">{group.title}</span>
						<ChevronDown className="ml-auto size-4 transition-transform duration-200 group-data-[state=open]/collapsible:rotate-180" />
					</SidebarMenuButton>
				</CollapsibleTrigger>
				<CollapsibleContent>
					<SidebarMenuSub>
						{group.items.map((subItem) => (
							<SidebarMenuSubItem key={subItem.href}>
								<SidebarMenuSubButton asChild>
									<Link
										to={subItem.href}
										className="[&.active]:bg-accent [&.active]:text-accent-foreground [&.active]:font-semibold"
									>
										<span>{subItem.title}</span>
									</Link>
								</SidebarMenuSubButton>
							</SidebarMenuSubItem>
						))}
					</SidebarMenuSub>
				</CollapsibleContent>
			</SidebarMenuItem>
		</Collapsible>
	);
}
