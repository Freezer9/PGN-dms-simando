import { Link, useNavigate } from "@tanstack/react-router";
import { Bell, KeyRound, LogOut, Search, User as UserIcon } from "lucide-react";
import { $api } from "@/api/client";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuLabel,
	DropdownMenuSeparator,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { useAuth } from "@/lib/auth";

export function Header() {
	const { user, logout } = useAuth();
	const navigate = useNavigate();

	// Live task summary query for header notifications
	const { data: taskSummary } = $api.useQuery(
		"get",
		"/api/tasks/summary",
		undefined,
		{
			enabled: !!user,
			refetchInterval: 30000,
		},
	);

	const pendingCount = Number(taskSummary?.myTasksCount ?? 0);

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

	return (
		<header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 h-14">
			<div className="flex h-14 items-center justify-between px-4 gap-4 max-w-full">
				{/* Brand */}
				<div className="flex items-center gap-3 shrink-0">
					<Link to="/" className="flex items-center gap-2">
						<span className="font-bold text-primary tracking-tight text-lg">
							DMS Simando
						</span>
						<span className="text-[11px] bg-primary text-primary-foreground px-2 py-0.5 rounded font-mono font-semibold shadow-xs">
							PGN
						</span>
					</Link>
				</div>

				{/* Global Search Bar */}
				<div className="flex-1 max-w-md hidden md:block">
					<div className="relative">
						<Search className="absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
						<Input
							type="search"
							placeholder="Cari perusahaan / nomor registrasi..."
							className="pl-9 h-9 text-xs bg-muted/40 w-full"
							readOnly
						/>
					</div>
				</div>

				{/* User Info & Actions */}
				<div className="flex items-center gap-3">
					{/* Notification Bell with Pending Count */}
					<Button
						asChild
						variant="ghost"
						size="icon"
						className="relative size-9 text-muted-foreground hover:text-foreground"
						title={
							pendingCount > 0
								? `${pendingCount} tugas menunggu tindakan Anda`
								: "Tugas & Notifikasi"
						}
					>
						<Link to="/tasks">
							<Bell className="size-4" />
							{pendingCount > 0 ? (
								<span className="absolute -top-0.5 -right-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-rose-500 px-1 text-[10px] font-bold text-white">
									{pendingCount > 99 ? "99+" : pendingCount}
								</span>
							) : (
								<span className="absolute top-1.5 right-1.5 size-2 rounded-full bg-primary/40" />
							)}
						</Link>
					</Button>

					{/* Role & Scope Pill */}
					{user && (
						<div className="hidden lg:flex flex-col items-end text-xs leading-tight mr-1">
							<span className="font-semibold text-foreground">
								{user.fullName || user.username}
							</span>
							<div className="flex items-center gap-1.5 text-[11px] text-muted-foreground mt-0.5">
								<span>{user.roles?.[0] || "User"}</span>
								<span>•</span>
								<span className="text-primary font-medium">{scopeLabel}</span>
							</div>
						</div>
					)}

					{/* User Menu Dropdown */}
					<DropdownMenu>
						<DropdownMenuTrigger asChild>
							<Button
								variant="ghost"
								size="icon"
								className="rounded-full size-9 border bg-muted/50"
							>
								<UserIcon className="size-4" />
								<span className="sr-only">Menu pengguna</span>
							</Button>
						</DropdownMenuTrigger>
						<DropdownMenuContent align="end" className="w-56">
							<DropdownMenuLabel className="font-normal">
								<div className="flex flex-col space-y-1">
									<p className="text-sm font-medium leading-none">
										{user?.fullName || user?.username}
									</p>
									<p className="text-xs leading-none text-muted-foreground">
										{user?.email}
									</p>
									{user?.roles && user.roles.length > 0 && (
										<div className="flex flex-wrap gap-1 mt-1.5">
											{user.roles.map((role) => (
												<Badge
													key={role}
													variant="outline"
													className="text-[10px] py-0 px-1.5 font-normal"
												>
													{role}
												</Badge>
											))}
										</div>
									)}
								</div>
							</DropdownMenuLabel>
							<DropdownMenuSeparator />
							<DropdownMenuItem asChild>
								<Link
									to="/change-password"
									className="flex items-center cursor-pointer"
								>
									<KeyRound className="mr-2 size-4 text-muted-foreground" />
									<span>Ubah Kata Sandi</span>
								</Link>
							</DropdownMenuItem>
							<DropdownMenuSeparator />
							<DropdownMenuItem
								onClick={handleLogout}
								className="text-destructive focus:text-destructive cursor-pointer"
							>
								<LogOut className="mr-2 size-4" />
								<span>Keluar (Sign Out)</span>
							</DropdownMenuItem>
						</DropdownMenuContent>
					</DropdownMenu>
				</div>
			</div>
		</header>
	);
}
