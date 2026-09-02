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
					retry: (failureCount, error: unknown) => {
						const status = (error as { status?: number })?.status;
						if (status === 401 || status === 403 || status === 404) {
							return false;
						}
						return failureCount < 1;
					},
				},
			},
		});

	const router = createTanStackRouter({
		routeTree,
		context: {
			queryClient: qc,
		},
		defaultErrorComponent: ({ error }) => (
			<div className="p-4 text-red-500 bg-red-50 m-4 rounded border border-red-300">
				<h2 className="font-bold text-lg">Error: {error?.message}</h2>
				<pre className="text-xs mt-2 overflow-auto whitespace-pre-wrap">
					{error?.stack}
				</pre>
			</div>
		),
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
