import { createFileRoute, Outlet } from "@tanstack/react-router";
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
			<SidebarInset className="flex flex-col min-h-screen bg-background text-foreground min-w-0">
				<Header />
				<main className="flex-1 overflow-y-auto p-4 md:p-6 w-full min-w-0 flex flex-col">
					<Outlet />
				</main>
			</SidebarInset>
		</SidebarProvider>
	);
}
