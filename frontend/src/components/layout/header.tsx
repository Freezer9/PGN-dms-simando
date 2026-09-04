import { Link, useNavigate } from "@tanstack/react-router";
import { KeyRound, LogOut, Search } from "lucide-react";
import { Breadcrumbs } from "@/components/layout/breadcrumbs";
import { NotificationDropdown } from "@/components/notifications/notification-dropdown";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuGroup,
	DropdownMenuItem,
	DropdownMenuLabel,
	DropdownMenuSeparator,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { SidebarTrigger } from "@/components/ui/sidebar";
import { useAuth } from "@/lib/auth";
import { formatRole } from "@/lib/roles";

export function Header() {
	const { user, logout } = useAuth();
	const navigate = useNavigate();

	const handleLogout = async () => {
		try {
			await logout();
		} finally {
			navigate({ to: "/sign-in" });
		}
	};

	const scopeLabel = user
		? user.scope === "All"
			? "Seluruh Region"
			: user.scope === "Region"
				? "Tingkat Region"
				: "Tingkat Area"
		: "";

	const initials = user?.fullName
		? user.fullName
				.split(" ")
				.map((n) => n[0])
				.slice(0, 2)
				.join("")
				.toUpperCase()
		: user?.username?.slice(0, 2).toUpperCase() || "U";

	return (
		<header className="sticky top-0 z-30 flex h-14 shrink-0 items-center justify-between border-b bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60">
			{/* Left: Sidebar Toggle + Breadcrumb Navigation */}
			<div className="flex items-center gap-2 min-w-0">
				<SidebarTrigger className="-ml-1 text-muted-foreground hover:text-foreground shrink-0" />
				<Separator orientation="vertical" className="mr-2 h-4 shrink-0" />
				<Breadcrumbs className="hidden sm:flex min-w-0" />
			</div>

			{/* Right: Search, Notifications, and Quick User Profile */}
			<div className="flex items-center gap-3">
				{/* Global Search Bar */}
				<div className="relative hidden md:block w-48 lg:w-64">
					<Search className="absolute left-2.5 top-2.5 size-3.5 text-muted-foreground" />
					<Input
						type="search"
						placeholder="Cari perusahaan..."
						className="h-8 pl-8 text-xs bg-muted/30 focus-visible:bg-background border-border/80"
						readOnly
						onClick={() => navigate({ to: "/directory" })}
					/>
				</div>

				{/* Notification Dropdown */}
				<NotificationDropdown />

				<Separator orientation="vertical" className="h-4 hidden sm:block" />

				{/* User Avatar & Menu */}
				<DropdownMenu>
					<DropdownMenuTrigger asChild>
						<Button
							variant="ghost"
							size="sm"
							className="relative flex items-center gap-2 p-1 px-2 h-8 rounded-full hover:bg-muted/70"
						>
							<Avatar className="size-6 rounded-full">
								<AvatarFallback className="bg-primary/10 text-primary font-bold text-[10px]">
									{initials}
								</AvatarFallback>
							</Avatar>
							<div className="hidden lg:flex flex-col text-left text-xs leading-none">
								<span className="font-medium text-foreground truncate max-w-[120px]">
									{user?.fullName || user?.username}
								</span>
								<span className="text-[10px] text-muted-foreground">
									{scopeLabel}
								</span>
							</div>
						</Button>
					</DropdownMenuTrigger>
					<DropdownMenuContent align="end" className="w-56 p-1.5 shadow-lg">
						<DropdownMenuLabel className="p-2 font-normal bg-muted/40 rounded-md">
							<div className="flex flex-col space-y-1">
								<p className="text-xs font-semibold leading-none text-foreground">
									{user?.fullName || user?.username}
								</p>
								<p className="text-[11px] leading-none text-muted-foreground">
									{user?.email}
								</p>
								{user?.roles && user.roles.length > 0 && (
									<div className="flex flex-wrap gap-1 mt-1.5">
										{user.roles.map((role) => (
											<Badge
												key={formatRole(role)}
												variant="secondary"
												className="text-[9px] py-0 px-1.5 font-normal h-4"
											>
												{formatRole(role)}
											</Badge>
										))}
									</div>
								)}
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
			</div>
		</header>
	);
}
