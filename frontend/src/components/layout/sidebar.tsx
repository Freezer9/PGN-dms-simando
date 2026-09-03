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
	SidebarFooter,
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
import { NavUser } from "./nav-user";

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
			<SidebarHeader className="border-b h-14 flex items-center justify-between px-3.5">
				<Link
					to="/"
					className="flex items-center gap-2.5 font-semibold group-data-[collapsible=icon]:justify-center w-full"
				>
					<div className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground font-mono font-bold text-xs shadow-xs tracking-wider">
						PGN
					</div>
					<div className="grid flex-1 text-left leading-tight group-data-[collapsible=icon]:hidden">
						<span className="font-bold text-foreground text-sm tracking-tight">
							DMS Simando
						</span>
						<span className="text-[10px] text-muted-foreground font-medium truncate">
							Enterprise Delivery System
						</span>
					</div>
				</Link>
			</SidebarHeader>

			<SidebarContent className="py-2 gap-1">
				{menu.sections.map((section, idx) => (
					<SidebarGroup
						key={section.title ?? `section-${idx}`}
						className="py-1"
					>
						{section.title && (
							<SidebarGroupLabel className="text-[10px] font-semibold tracking-wider text-muted-foreground/70 uppercase px-3 py-1">
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

			<SidebarFooter className="border-t p-2">
				<NavUser user={user} />
			</SidebarFooter>

			<SidebarRail />
		</Sidebar>
	);
}

// Re-export as Sidebar for backward compatibility with existing imports
export { AppSidebar as Sidebar };

function SidebarMenuItemComponent({ item }: { item: NavItem }) {
	return (
		<SidebarMenuItem>
			<SidebarMenuButton asChild tooltip={item.title} size="sm">
				<Link
					to={item.href}
					activeOptions={{ exact: item.href === "/" }}
					activeProps={{
						className:
							"bg-primary text-primary-foreground font-medium shadow-xs hover:bg-primary/95 hover:text-primary-foreground",
					}}
					inactiveProps={{
						className:
							"text-muted-foreground hover:bg-muted/80 hover:text-foreground",
					}}
				>
					<DynamicIcon name={item.icon} className="size-4 shrink-0" />
					<span className="truncate text-xs">{item.title}</span>
				</Link>
			</SidebarMenuButton>
			{item.badge !== undefined && item.badge !== null && (
				<SidebarMenuBadge className="bg-primary/10 text-primary font-mono text-[10px] font-bold px-1.5 py-0.5 rounded-full">
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
					<SidebarMenuButton tooltip={group.title} size="sm">
						<DynamicIcon name={group.icon} className="size-4 shrink-0" />
						<span className="truncate text-xs">{group.title}</span>
						<ChevronDown className="ml-auto size-3.5 transition-transform duration-200 group-data-[state=open]/collapsible:rotate-180 opacity-60" />
					</SidebarMenuButton>
				</CollapsibleTrigger>
				<CollapsibleContent>
					<SidebarMenuSub className="my-0.5 ml-3 border-l pl-2">
						{group.items.map((subItem) => (
							<SidebarMenuSubItem key={subItem.href}>
								<SidebarMenuSubButton asChild size="sm">
									<Link
										to={subItem.href}
										activeProps={{
											className:
												"bg-accent text-accent-foreground font-semibold",
										}}
										inactiveProps={{
											className:
												"text-muted-foreground hover:bg-muted/60 hover:text-foreground",
										}}
									>
										<span className="text-xs">{subItem.title}</span>
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
