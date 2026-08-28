import { createFileRoute } from "@tanstack/react-router";
import {
	Activity,
	Building2,
	CheckCircle2,
	Clock,
	FileCheck,
	Sparkles,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { useAuth } from "@/lib/auth";

export const Route = createFileRoute("/_auth/")({
	component: DashboardHome,
});

function DashboardHome() {
	const { user } = useAuth();

	return (
		<div className="space-y-6">
			<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
				<div>
					<h1 className="text-2xl sm:text-3xl font-bold tracking-tight">
						Dashboard Pipeline Gas
					</h1>
					<p className="text-muted-foreground mt-1 text-sm">
						Sistem Informasi Manajemen Dokumen Berlangganan Gas (DMS Simando)
					</p>
				</div>
				<div className="flex items-center gap-2">
					<Badge variant="outline" className="gap-1.5 py-1 px-3">
						<span className="size-2 rounded-full bg-emerald-500 animate-pulse" />
						Sesi Aktif: {user?.roles?.[0] || "User"}
					</Badge>
				</div>
			</div>

			<div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
				<Card>
					<CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
						<CardTitle className="text-sm font-medium">Total Prospek</CardTitle>
						<Building2 className="h-4 w-4 text-muted-foreground" />
					</CardHeader>
					<CardContent>
						<div className="text-2xl font-bold">128</div>
						<p className="text-xs text-muted-foreground mt-1">
							Industri terdaftar di Area
						</p>
					</CardContent>
				</Card>

				<Card>
					<CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
						<CardTitle className="text-sm font-medium">
							Dalam Survei (KK0)
						</CardTitle>
						<Clock className="h-4 w-4 text-amber-500" />
					</CardHeader>
					<CardContent>
						<div className="text-2xl font-bold">34</div>
						<p className="text-xs text-muted-foreground mt-1">
							Verifikasi teknis & plotting
						</p>
					</CardContent>
				</Card>

				<Card>
					<CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
						<CardTitle className="text-sm font-medium">Review NOL</CardTitle>
						<Activity className="h-4 w-4 text-blue-500" />
					</CardHeader>
					<CardContent>
						<div className="text-2xl font-bold">12</div>
						<p className="text-xs text-muted-foreground mt-1">
							Menunggu persetujuan reviewer
						</p>
					</CardContent>
				</Card>

				<Card>
					<CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
						<CardTitle className="text-sm font-medium">
							NOL Terbit (RL)
						</CardTitle>
						<FileCheck className="h-4 w-4 text-emerald-500" />
					</CardHeader>
					<CardContent>
						<div className="text-2xl font-bold">82</div>
						<p className="text-xs text-muted-foreground mt-1">
							Siap berkontrak gas
						</p>
					</CardContent>
				</Card>
			</div>

			<Card>
				<CardHeader>
					<CardTitle className="text-lg">Status Pipeline Penjualan</CardTitle>
					<CardDescription>
						Monitoring tahapan berlangganan gas dari Prospek hingga Penerbitan
						Surat NOL
					</CardDescription>
				</CardHeader>
				<CardContent>
					<div className="flex items-center justify-between py-3 border-b">
						<div className="flex items-center gap-3">
							<CheckCircle2 className="h-5 w-5 text-emerald-600 shrink-0" />
							<div>
								<p className="font-medium text-sm">
									Autentikasi & RBAC Shell Aktif
								</p>
								<p className="text-xs text-muted-foreground">
									Sesi Cookie ASP.NET Identity, Dynamic Navigation Menu & Route
									Guards
								</p>
							</div>
						</div>
						<Badge variant="secondary">Active</Badge>
					</div>

					<div className="flex items-center justify-between py-3">
						<div className="flex items-center gap-3">
							<Sparkles className="h-5 w-5 text-primary shrink-0" />
							<div>
								<p className="font-medium text-sm">
									Pengguna Terhubung: {user?.fullName} ({user?.email})
								</p>
								<p className="text-xs text-muted-foreground">
									Scope: {user?.scope} • Hak Akses:{" "}
									{user?.capabilities?.length || 0} capabilities
								</p>
							</div>
						</div>
						<Badge variant="outline">{user?.scope}</Badge>
					</div>
				</CardContent>
			</Card>
		</div>
	);
}
