import { TanStackDevtools } from "@tanstack/react-devtools";
import type { QueryClient } from "@tanstack/react-query";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import {
	createRootRouteWithContext,
	Link,
	Outlet,
} from "@tanstack/react-router";
import { TanStackRouterDevtoolsPanel } from "@tanstack/react-router-devtools";
import { Home } from "lucide-react";
import { Toaster } from "@/components/ui/sonner";

import "../styles.css";

export interface RouterContext {
	queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<RouterContext>()({
	component: RootComponent,
});

function RootComponent() {
	const { queryClient } = Route.useRouteContext();

	return (
		<QueryClientProvider client={queryClient}>
			<div className="flex min-h-screen flex-col bg-background text-foreground">
				{/* Header */}
				<header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
					<div className="container flex h-14 max-w-screen-2xl items-center px-4">
						<div className="mr-6 flex items-center space-x-2">
							<span className="font-bold text-primary tracking-tight text-lg">
								DMS Simando
							</span>
							<span className="text-xs bg-primary/10 text-primary px-2 py-0.5 rounded font-mono">
								PGN
							</span>
						</div>
						<nav className="flex items-center space-x-4 text-sm font-medium">
							<Link
								to="/"
								className="transition-colors hover:text-foreground/80 text-foreground/60 [&.active]:text-foreground [&.active]:font-semibold flex items-center gap-1.5"
							>
								<Home className="size-4" />
								Dashboard
							</Link>
						</nav>
					</div>
				</header>

				{/* Main Content */}
				<main className="flex-1 container max-w-screen-2xl py-6 px-4">
					<Outlet />
				</main>

				{/* Global Toaster */}
				<Toaster position="top-right" richColors />

				{/* Devtools */}
				<TanStackDevtools
					config={{
						position: "bottom-right",
					}}
					plugins={[
						{
							name: "TanStack Router",
							render: <TanStackRouterDevtoolsPanel />,
						},
					]}
				/>
				<ReactQueryDevtools
					buttonPosition="bottom-left"
					initialIsOpen={false}
				/>
			</div>
		</QueryClientProvider>
	);
}
