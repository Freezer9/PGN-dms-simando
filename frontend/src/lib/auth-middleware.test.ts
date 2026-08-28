import type { QueryClient } from "@tanstack/react-query";
import { isRedirect } from "@tanstack/react-router";
import { describe, expect, it, vi } from "vitest";
import type { CurrentUserDto } from "@/api/types";
import {
	guestOnly,
	protectedRoute,
	requireCapabilities,
	requireRoles,
} from "./auth-middleware";

describe("Auth Middleware", () => {
	const mockUser: CurrentUserDto = {
		id: "123e4567-e89b-12d3-a456-426614174000",
		username: "sales.user",
		email: "sales@pgn.co.id",
		fullName: "Budi Sales",
		scope: "Area",
		roles: ["SalesArea"],
		capabilities: ["ViewCompanyRecords", "CreateCompany"],
		mustChangePassword: false,
	};

	it("protectedRoute allows authenticated user", async () => {
		const queryClient = {
			ensureQueryData: vi.fn().mockResolvedValue(mockUser),
		} as unknown as QueryClient;

		const user = await protectedRoute({
			context: { queryClient },
			location: { href: "/directory", pathname: "/directory" },
		});

		expect(user).toEqual(mockUser);
	});

	it("protectedRoute redirects unauthenticated user to /sign-in", async () => {
		const queryClient = {
			ensureQueryData: vi.fn().mockRejectedValue(new Error("Unauthorized")),
		} as unknown as QueryClient;

		try {
			await protectedRoute({
				context: { queryClient },
				location: { href: "/directory", pathname: "/directory" },
			});
			expect.unreachable("Should have thrown redirect");
		} catch (error: unknown) {
			expect(isRedirect(error)).toBe(true);
			const err = error as { options?: { to?: string } };
			expect(err.options?.to).toBe("/sign-in");
		}
	});

	it("protectedRoute redirects user with mustChangePassword to /change-password", async () => {
		const queryClient = {
			ensureQueryData: vi
				.fn()
				.mockResolvedValue({ ...mockUser, mustChangePassword: true }),
		} as unknown as QueryClient;

		try {
			await protectedRoute({
				context: { queryClient },
				location: { href: "/directory", pathname: "/directory" },
			});
			expect.unreachable("Should have thrown redirect");
		} catch (error: unknown) {
			expect(isRedirect(error)).toBe(true);
			const err = error as { options?: { to?: string } };
			expect(err.options?.to).toBe("/change-password");
		}
	});

	it("guestOnly redirects authenticated user to /", async () => {
		const queryClient = {
			ensureQueryData: vi.fn().mockResolvedValue(mockUser),
		} as unknown as QueryClient;

		try {
			await guestOnly({
				context: { queryClient },
				location: { href: "/sign-in", pathname: "/sign-in" },
			});
			expect.unreachable("Should have thrown redirect");
		} catch (error: unknown) {
			expect(isRedirect(error)).toBe(true);
			const err = error as { options?: { to?: string } };
			expect(err.options?.to).toBe("/");
		}
	});

	it("requireRoles allows authorized role and rejects unauthorized role", async () => {
		const queryClient = {
			ensureQueryData: vi.fn().mockResolvedValue(mockUser),
		} as unknown as QueryClient;

		const allowMiddleware = requireRoles(["SalesArea", "AreaHead"]);
		const user = await allowMiddleware({
			context: { queryClient },
			location: { href: "/directory", pathname: "/directory" },
		});
		expect(user).toEqual(mockUser);

		const rejectMiddleware = requireRoles(["SystemAdmin"]);
		try {
			await rejectMiddleware({
				context: { queryClient },
				location: {
					href: "/master/organisation",
					pathname: "/master/organisation",
				},
			});
			expect.unreachable("Should have thrown redirect");
		} catch (error: unknown) {
			expect(isRedirect(error)).toBe(true);
			const err = error as { options?: { to?: string } };
			expect(err.options?.to).toBe("/access-denied");
		}
	});

	it("requireCapabilities allows authorized capability and rejects missing capability", async () => {
		const queryClient = {
			ensureQueryData: vi.fn().mockResolvedValue(mockUser),
		} as unknown as QueryClient;

		const allowMiddleware = requireCapabilities(["ViewCompanyRecords"]);
		const user = await allowMiddleware({
			context: { queryClient },
			location: { href: "/directory", pathname: "/directory" },
		});
		expect(user).toEqual(mockUser);

		const rejectMiddleware = requireCapabilities(["ManageMasterData"]);
		try {
			await rejectMiddleware({
				context: { queryClient },
				location: {
					href: "/master/organisation",
					pathname: "/master/organisation",
				},
			});
			expect.unreachable("Should have thrown redirect");
		} catch (error: unknown) {
			expect(isRedirect(error)).toBe(true);
			const err = error as { options?: { to?: string } };
			expect(err.options?.to).toBe("/access-denied");
		}
	});
});
