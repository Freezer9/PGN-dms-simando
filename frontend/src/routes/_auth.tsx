import { createFileRoute, Outlet } from "@tanstack/react-router";
import { Breadcrumbs } from "@/components/layout/breadcrumbs";
import { Header } from "@/components/layout/header";
import { Sidebar } from "@/components/layout/sidebar";
import { protectedRoute } from "@/lib/auth-middleware";

export const Route = createFileRoute("/_auth")({
	beforeLoad: protectedRoute,
	component: AuthLayout,
});

function AuthLayout() {
	return (
		<div className="flex min-h-screen flex-col bg-background text-foreground">
			<Header />
			<div className="flex flex-1 overflow-hidden">
				<Sidebar />
				<main className="flex-1 overflow-y-auto p-6 container max-w-7xl">
					<Breadcrumbs />
					<Outlet />
				</main>
			</div>
		</div>
	);
}
