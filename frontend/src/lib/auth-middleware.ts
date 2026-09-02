import type { QueryClient } from "@tanstack/react-query";
import { isRedirect, redirect } from "@tanstack/react-router";
import { $api } from "@/api/client";
import type { AppCapability, AppRole, CurrentUserDto } from "@/api/types";

export interface RouteMiddlewareContext {
	context: {
		queryClient: QueryClient;
	};
	location: {
		href: string;
		pathname: string;
	};
}

/**
 * Ensures the user is NOT authenticated. If they are already signed in,
 * redirects them to dashboard (or `/change-password` if mustChangePassword is set).
 */
export async function guestOnly({
	context,
}: RouteMiddlewareContext): Promise<void> {
	let user: CurrentUserDto | null = null;
	try {
		user = await context.queryClient.ensureQueryData(
			$api.queryOptions("get", "/api/auth/me", undefined, { retry: false }),
		);
	} catch (error) {
		if (isRedirect(error)) {
			throw error;
		}
		return;
	}

	if (user) {
		if (user.mustChangePassword) {
			throw redirect({ to: "/change-password" });
		}
		throw redirect({ to: "/" });
	}
}

/**
 * Ensures the user IS authenticated.
 * If unauthenticated, redirects to `/sign-in` with return url.
 * If mustChangePassword is true and target is not `/change-password`, redirects to `/change-password`.
 */
export async function protectedRoute({
	context,
	location,
}: RouteMiddlewareContext): Promise<CurrentUserDto> {
	let user: CurrentUserDto | null = null;
	try {
		user = await context.queryClient.ensureQueryData(
			$api.queryOptions("get", "/api/auth/me", undefined, { retry: false }),
		);
	} catch (error) {
		if (isRedirect(error)) {
			throw error;
		}
		throw redirect({
			to: "/sign-in",
			search: { redirect: location.href },
		});
	}

	if (!user) {
		throw redirect({
			to: "/sign-in",
			search: { redirect: location.href },
		});
	}

	if (user.mustChangePassword && location.pathname !== "/change-password") {
		throw redirect({
			to: "/change-password",
		});
	}

	return user;
}

/**
 * Reusable middleware requiring at least one of the specified OpenAPI AppRoles.
 */
export function requireRoles(roles: AppRole[]) {
	return async (ctx: RouteMiddlewareContext): Promise<CurrentUserDto> => {
		const user = await protectedRoute(ctx);

		const hasRole = user.roles.some((r) => roles.includes(r));
		if (!hasRole) {
			throw redirect({ to: "/access-denied" });
		}

		return user;
	};
}

/**
 * Reusable middleware requiring at least one of the specified OpenAPI AppCapabilities.
 */
export function requireCapabilities(capabilities: AppCapability[]) {
	return async (ctx: RouteMiddlewareContext): Promise<CurrentUserDto> => {
		const user = await protectedRoute(ctx);

		const hasCap = user.capabilities.some((c) => capabilities.includes(c));
		if (!hasCap) {
			throw redirect({ to: "/access-denied" });
		}

		return user;
	};
}
