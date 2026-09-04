import { Link, useNavigate } from "@tanstack/react-router";
import { ChevronsUpDown, KeyRound, LogOut } from "lucide-react";
import type { CurrentUserDto } from "@/api/types";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuGroup,
	DropdownMenuItem,
	DropdownMenuLabel,
	DropdownMenuSeparator,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
	SidebarMenu,
	SidebarMenuButton,
	SidebarMenuItem,
	useSidebar,
} from "@/components/ui/sidebar";
import { useAuth } from "@/lib/auth";
import { formatRole } from "@/lib/roles";

export function NavUser({ user }: { user: CurrentUserDto | null }) {
	const { isMobile } = useSidebar();
	const { logout } = useAuth();
	const navigate = useNavigate();

	if (!user) {
		return null;
	}

	const handleLogout = async () => {
		try {
			await logout();
		} finally {
			navigate({ to: "/sign-in" });
		}
	};

	const initials = user.fullName
		? user.fullName
				.split(" ")
				.map((n) => n[0])
				.slice(0, 2)
				.join("")
				.toUpperCase()
		: user.username?.slice(0, 2).toUpperCase() || "U";

	const scopeLabel =
		user.scope === "All"
			? "Seluruh Wilayah"
			: user.scope === "Region"
				? "Tingkat Region"
				: "Tingkat Area";

	return (
		<SidebarMenu>
			<SidebarMenuItem>
				<DropdownMenu>
					<DropdownMenuTrigger asChild>
						<SidebarMenuButton
							size="lg"
							className="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground hover:bg-sidebar-accent/80 transition-colors"
						>
							<Avatar className="h-8 w-8 rounded-lg">
								<AvatarFallback className="rounded-lg bg-primary/10 text-primary font-semibold text-xs">
									{initials}
								</AvatarFallback>
							</Avatar>
							<div className="grid flex-1 text-left text-sm leading-tight group-data-[collapsible=icon]:hidden">
								<span className="truncate font-semibold text-xs">
									{user.fullName || user.username}
								</span>
								<span className="truncate text-[11px] text-muted-foreground">
									{user.email || formatRole(user.roles?.[0]) || "Pengguna"}
								</span>
							</div>
							<ChevronsUpDown className="ml-auto size-4 group-data-[collapsible=icon]:hidden opacity-60" />
						</SidebarMenuButton>
					</DropdownMenuTrigger>
					<DropdownMenuContent
						className="w-(--radix-dropdown-menu-trigger-width) min-w-56 rounded-lg p-1.5 shadow-lg"
						side={isMobile ? "bottom" : "right"}
						align="end"
						sideOffset={8}
					>
						<DropdownMenuLabel className="p-0 font-normal">
							<div className="flex items-center gap-2.5 px-2 py-2 text-left text-sm bg-muted/40 rounded-md">
								<Avatar className="h-9 w-9 rounded-lg">
									<AvatarFallback className="rounded-lg bg-primary text-primary-foreground font-semibold text-xs">
										{initials}
									</AvatarFallback>
								</Avatar>
								<div className="grid flex-1 text-left text-xs leading-tight">
									<span className="truncate font-semibold text-foreground">
										{user.fullName || user.username}
									</span>
									<span className="truncate text-[11px] text-muted-foreground">
										{user.email}
									</span>
									<div className="flex items-center gap-1.5 mt-1">
										<Badge
											variant="secondary"
											className="text-[9px] px-1.5 py-0 h-4 font-normal"
										>
											{formatRole(user.roles?.[0]) || "Pengguna"}
										</Badge>
										<span className="text-[10px] text-muted-foreground">
											• {scopeLabel}
										</span>
									</div>
								</div>
							</div>
						</DropdownMenuLabel>
						<DropdownMenuSeparator className="my-1" />
						<DropdownMenuGroup>
							<DropdownMenuItem asChild>
								<Link
									to="/change-password"
									className="flex items-center gap-2 px-2 py-1.5 text-xs cursor-pointer rounded-sm"
								>
									<KeyRound className="size-3.5 text-muted-foreground" />
									<span>Ubah Kata Sandi</span>
								</Link>
							</DropdownMenuItem>
						</DropdownMenuGroup>
						<DropdownMenuSeparator className="my-1" />
						<DropdownMenuItem
							onClick={handleLogout}
							className="flex items-center gap-2 px-2 py-1.5 text-xs cursor-pointer text-destructive focus:text-destructive focus:bg-destructive/10 rounded-sm"
						>
							<LogOut className="size-3.5" />
							<span>Keluar</span>
						</DropdownMenuItem>
					</DropdownMenuContent>
				</DropdownMenu>
			</SidebarMenuItem>
		</SidebarMenu>
	);
}
