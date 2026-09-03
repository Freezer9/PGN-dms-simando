import { createFileRoute } from "@tanstack/react-router";
import { Loader2 } from "lucide-react";
import { $api } from "@/api/client";
import { ApproverDashboard } from "@/components/dashboard/approver-dashboard";
import { RegionalAdminDashboard } from "@/components/dashboard/regional-admin-dashboard";
import { SalesAreaDashboard } from "@/components/dashboard/sales-area-dashboard";
import { SystemAdminDashboard } from "@/components/dashboard/system-admin-dashboard";
import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "@/lib/auth";

export const Route = createFileRoute("/_auth/")({
	component: DashboardHome,
});

function DashboardHome() {
	const { user } = useAuth();

	const {
		data: statsData,
		isLoading,
		error,
	} = $api.useQuery("get", "/api/dashboard/stats", undefined, {
		refetchInterval: 60000, // Refresh dashboard stats every 60s
	});

	if (isLoading) {
		return (
			<div className="flex flex-col items-center justify-center min-h-[400px] gap-3">
				<Loader2 className="size-8 animate-spin text-primary" />
				<p className="text-sm text-muted-foreground">
					Memuat data beranda pelanggan gas...
				</p>
			</div>
		);
	}

	if (error || !statsData) {
		return (
			<Card className="border-destructive/30 bg-destructive/5">
				<CardContent className="py-8 text-center space-y-2">
					<h3 className="font-semibold text-lg text-destructive">
						Gagal Memuat Data Dashboard
					</h3>
					<p className="text-sm text-muted-foreground">
						Terjadi kendala saat memuat ringkasan data. Silakan muat ulang
						halaman.
					</p>
				</CardContent>
			</Card>
		);
	}

	// 1. System Admin
	if (statsData.role === "SystemAdmin" && statsData.systemAdmin) {
		return <SystemAdminDashboard data={statsData.systemAdmin} />;
	}

	// 2. Regional Admin
	if (statsData.role === "RegionalAdmin" && statsData.regionalAdmin) {
		return (
			<RegionalAdminDashboard
				data={statsData.regionalAdmin}
				regionName={user?.scope === "Region" ? "Wilayah Regional" : "Regional"}
			/>
		);
	}

	// 3. Sales Area
	if (statsData.role === "SalesArea" && statsData.salesArea) {
		return (
			<SalesAreaDashboard
				data={statsData.salesArea}
				areaName={user?.scope === "Area" ? "Sales Area" : "Area"}
			/>
		);
	}

	// 4. Approver / Reviewer / Area Head / Division Head
	if (statsData.approver) {
		const roleTitleMap: Record<string, string> = {
			AreaHead: "Area Head",
			Reviewer: "Reviewer Teknis & Komersial",
			DivisionHead: "Division Head",
		};
		const roleTitle = roleTitleMap[statsData.role] || "Verifikasi & Reviewer";

		return (
			<ApproverDashboard data={statsData.approver} roleTitle={roleTitle} />
		);
	}

	// Fallback if specific payload not attached but salesArea exists
	if (statsData.salesArea) {
		return <SalesAreaDashboard data={statsData.salesArea} />;
	}

	return null;
}
