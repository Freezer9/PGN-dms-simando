import { createFileRoute } from "@tanstack/react-router";
import {
	Activity,
	Building2,
	CheckCircle2,
	Clock,
	FileCheck,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";

export const Route = createFileRoute("/")({
	component: DashboardHome,
});

function DashboardHome() {
	return (
		<div className="space-y-6">
			<div className="flex items-center justify-between">
				<div>
					<h1 className="text-3xl font-bold tracking-tight">
						Dashboard Pipeline Gas
					</h1>
					<p className="text-muted-foreground mt-1">
						Sistem Informasi Manajemen Dokumen Berlangganan Gas (DMS Simando)
					</p>
				</div>
				<div className="flex items-center gap-2">
					<Badge variant="outline" className="gap-1.5 py-1 px-3">
						<span className="size-2 rounded-full bg-emerald-500 animate-pulse" />
						Live System
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
					<CardTitle>Status Pipeline Penjualan</CardTitle>
					<CardDescription>
						Monitoring tahapan berlangganan gas dari Prospek hingga Penerbitan
						Surat NOL
					</CardDescription>
				</CardHeader>
				<CardContent>
					<div className="flex items-center justify-between py-4 border-b">
						<div className="flex items-center gap-3">
							<CheckCircle2 className="h-5 w-5 text-emerald-600" />
							<div>
								<p className="font-medium text-sm">
									Arsitektur Terpisah: React SPA + ASP.NET Core 10 Web API
								</p>
								<p className="text-xs text-muted-foreground">
									OpenAPI 3.1, TanStack Query & Biome Linter/Formatter
								</p>
							</div>
						</div>
						<Badge variant="secondary">Active</Badge>
					</div>
				</CardContent>
			</Card>
		</div>
	);
}
