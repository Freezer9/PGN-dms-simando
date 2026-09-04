import { useQueryClient } from "@tanstack/react-query";
import { Link, useNavigate } from "@tanstack/react-router";
import { Bell, CheckCheck, ExternalLink, Inbox } from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { NotificationListItem } from "@/api/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Popover,
	PopoverContent,
	PopoverTrigger,
} from "@/components/ui/popover";
import { useAuth } from "@/lib/auth";

function formatRelativeTime(dateString: string): string {
	const date = new Date(dateString);
	const now = new Date();
	const diffMs = now.getTime() - date.getTime();
	const diffSec = Math.floor(diffMs / 1000);
	const diffMin = Math.floor(diffSec / 60);
	const diffHour = Math.floor(diffMin / 60);
	const diffDay = Math.floor(diffHour / 24);

	if (diffMin < 1) return "Baru saja";
	if (diffMin < 60) return `${diffMin} mnt lalu`;
	if (diffHour < 24) return `${diffHour} jam lalu`;
	if (diffDay < 7) return `${diffDay} hr lalu`;
	return date.toLocaleDateString("id-ID", {
		day: "numeric",
		month: "short",
	});
}

export function NotificationDropdown() {
	const { user } = useAuth();
	const navigate = useNavigate();
	const queryClient = useQueryClient();
	const [isOpen, setIsOpen] = React.useState(false);

	// Poll unread notification count every 30 seconds
	const { data: unreadData } = $api.useQuery(
		"get",
		"/api/notifications/unread-count",
		undefined,
		{
			enabled: !!user,
			refetchInterval: 30000,
		},
	);

	// Fetch notifications list when dropdown is open
	const { data: notifications = [], isLoading } = $api.useQuery(
		"get",
		"/api/notifications",
		{
			params: {
				query: { limit: 15 },
			},
		},
		{
			enabled: !!user && isOpen,
		},
	);

	const markAsReadMutation = $api.useMutation(
		"post",
		"/api/notifications/{id}/read",
		{
			onSuccess: () => {
				queryClient.invalidateQueries({
					queryKey: ["get", "/api/notifications/unread-count"],
				});
				queryClient.invalidateQueries({
					queryKey: ["get", "/api/notifications"],
				});
			},
		},
	);

	const markAllAsReadMutation = $api.useMutation(
		"post",
		"/api/notifications/read-all",
		{
			onSuccess: () => {
				queryClient.invalidateQueries({
					queryKey: ["get", "/api/notifications/unread-count"],
				});
				queryClient.invalidateQueries({
					queryKey: ["get", "/api/notifications"],
				});
			},
		},
	);

	const unreadCount = Number(unreadData?.unreadCount ?? 0);

	const handleNotificationClick = (item: NotificationListItem) => {
		if (!item.readAt) {
			markAsReadMutation.mutate({
				params: { path: { id: item.id } },
			});
		}
		setIsOpen(false);
		navigate({
			to: "/directory/$companyId",
			params: { companyId: item.companyId },
		});
	};

	const handleMarkAllRead = (e: React.MouseEvent) => {
		e.stopPropagation();
		markAllAsReadMutation.mutate({});
	};

	return (
		<Popover open={isOpen} onOpenChange={setIsOpen}>
			<PopoverTrigger asChild>
				<Button
					variant="ghost"
					size="icon"
					className="relative size-8 text-muted-foreground hover:text-foreground"
					title={
						unreadCount > 0
							? `${unreadCount} notifikasi belum dibaca`
							: "Notifikasi"
					}
				>
					<Bell className="size-4" />
					{unreadCount > 0 ? (
						<span className="absolute -top-0.5 -right-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-rose-500 px-1 text-[9px] font-bold text-white shadow-xs">
							{unreadCount > 99 ? "99+" : unreadCount}
						</span>
					) : null}
				</Button>
			</PopoverTrigger>
			<PopoverContent
				align="end"
				className="w-80 sm:w-96 p-0 shadow-lg border border-border"
				sideOffset={8}
			>
				{/* Popover Header */}
				<div className="flex items-center justify-between px-3.5 py-2.5 border-b border-border/80 bg-muted/30">
					<div className="flex items-center gap-2">
						<span className="text-xs font-semibold text-foreground">
							Notifikasi
						</span>
						{unreadCount > 0 && (
							<Badge
								variant="secondary"
								className="h-4 px-1.5 text-[10px] bg-rose-100 text-rose-700 dark:bg-rose-950/50 dark:text-rose-300 font-mono"
							>
								{unreadCount} baru
							</Badge>
						)}
					</div>
					{unreadCount > 0 && (
						<Button
							variant="ghost"
							size="sm"
							onClick={handleMarkAllRead}
							disabled={markAllAsReadMutation.isPending}
							className="h-6 px-2 text-[11px] text-muted-foreground hover:text-foreground flex items-center gap-1 font-normal"
						>
							<CheckCheck className="size-3" />
							<span>Tandai sudah dibaca</span>
						</Button>
					)}
				</div>

				{/* Notifications List */}
				<div className="max-h-[360px] overflow-y-auto divide-y divide-border/40">
					{isLoading ? (
						<div className="p-6 text-center text-xs text-muted-foreground">
							Memuat notifikasi...
						</div>
					) : notifications.length === 0 ? (
						<div className="p-8 text-center flex flex-col items-center justify-center gap-2">
							<div className="size-10 rounded-full bg-muted/60 flex items-center justify-center text-muted-foreground">
								<Inbox className="size-5" />
							</div>
							<p className="text-xs font-medium text-foreground">
								Belum ada notifikasi
							</p>
							<p className="text-[11px] text-muted-foreground max-w-[220px]">
								Pemberitahuan aktivitas alur kerja dan persetujuan akan muncul
								di sini.
							</p>
						</div>
					) : (
						notifications.map((item) => {
							const isUnread = !item.readAt;
							return (
								<button
									key={item.id}
									type="button"
									onClick={() => handleNotificationClick(item)}
									className={`w-full p-3 text-left transition-colors cursor-pointer flex items-start gap-2.5 border-0 ${
										isUnread
											? "bg-primary/5 hover:bg-muted/70"
											: "hover:bg-muted/40"
									}`}
								>
									<div
										className={`size-2 rounded-full mt-1.5 shrink-0 ${
											isUnread ? "bg-primary" : "bg-transparent"
										}`}
									/>
									<div className="flex-1 min-w-0 space-y-1">
										<div className="flex items-center justify-between gap-1.5">
											<div className="flex items-center gap-1.5 min-w-0">
												<span className="font-mono text-[10px] text-muted-foreground bg-muted px-1.5 py-0.5 rounded border border-border/60 shrink-0">
													{item.companyNomor}
												</span>
												<span className="text-[11px] font-medium text-foreground truncate">
													{item.companyName}
												</span>
											</div>
											<span className="text-[10px] text-muted-foreground shrink-0 whitespace-nowrap">
												{formatRelativeTime(item.createdAt)}
											</span>
										</div>
										<p className="text-xs text-foreground/90 leading-snug break-words">
											{item.message}
										</p>
									</div>
								</button>
							);
						})
					)}
				</div>

				{/* Popover Footer: Link to Tasks */}
				<div className="p-2 border-t border-border/80 bg-muted/20 flex items-center justify-between">
					<Button
						asChild
						variant="ghost"
						size="sm"
						className="w-full h-7 text-xs text-muted-foreground hover:text-foreground justify-between font-normal"
						onClick={() => setIsOpen(false)}
					>
						<Link to="/tasks">
							<span>Lihat Semua Tugas & Inbox</span>
							<ExternalLink className="size-3 text-muted-foreground" />
						</Link>
					</Button>
				</div>
			</PopoverContent>
		</Popover>
	);
}
