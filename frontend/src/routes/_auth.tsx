import { createFileRoute, Outlet } from "@tanstack/react-router";
import { Breadcrumbs } from "@/components/layout/breadcrumbs";
import { Header } from "@/components/layout/header";
import { AppSidebar } from "@/components/layout/sidebar";
import { SidebarInset, SidebarProvider } from "@/components/ui/sidebar";
import { protectedRoute } from "@/lib/auth-middleware";

export const Route = createFileRoute("/_auth")({
	beforeLoad: protectedRoute,
	component: AuthLayout,
});

function AuthLayout() {
	return (
		<SidebarProvider>
			<AppSidebar />
			<SidebarInset className="flex flex-col min-h-screen bg-background text-foreground">
				<Header />
				<main className="flex-1 overflow-y-auto p-6 container max-w-7xl">
					<Breadcrumbs />
					<Outlet />
				</main>
			</SidebarInset>
		</SidebarProvider>
	);
}
