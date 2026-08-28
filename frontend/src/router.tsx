import { QueryClient } from "@tanstack/react-query";
import { createRouter as createTanStackRouter } from "@tanstack/react-router";
import { routeTree } from "./routeTree.gen";

export function getRouter(queryClient?: QueryClient) {
	const qc =
		queryClient ??
		new QueryClient({
			defaultOptions: {
				queries: {
					staleTime: 1000 * 60 * 5, // 5 minutes
					refetchOnWindowFocus: false,
					retry: 1,
				},
			},
		});

	const router = createTanStackRouter({
		routeTree,
		context: {
			queryClient: qc,
		},
		scrollRestoration: true,
		defaultPreload: "intent",
		defaultPreloadStaleTime: 0,
	});

	return router;
}

declare module "@tanstack/react-router" {
	interface Register {
		router: ReturnType<typeof getRouter>;
	}
}
