import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook } from "@testing-library/react";
import type * as React from "react";
import { describe, expect, it, vi } from "vitest";
import type { CurrentUserDto } from "@/api/types";
import { AuthProvider, useAuth } from "./auth";

const mockUser: CurrentUserDto = {
	id: "00000000-0000-0000-0000-000000000001",
	username: "sales.sby",
	fullName: "Sales Surabaya",
	roles: ["SalesArea"],
	capabilities: ["CreateCompany", "EditDraft"],
};

vi.mock("@/api/client", () => ({
	$api: {
		useQuery: vi.fn(() => ({
			data: mockUser,
			isLoading: false,
		})),
		useMutation: vi.fn(() => ({
			mutateAsync: vi.fn(),
		})),
		queryOptions: vi.fn(() => ({
			queryKey: ["auth", "me"],
		})),
	},
}));

function createWrapper() {
	const queryClient = new QueryClient({
		defaultOptions: {
			queries: {
				retry: false,
			},
		},
	});

	return ({ children }: { children: React.ReactNode }) => (
		<QueryClientProvider client={queryClient}>
			<AuthProvider>{children}</AuthProvider>
		</QueryClientProvider>
	);
}

describe("AuthProvider and useAuth", () => {
	it("throws error if useAuth is used outside AuthProvider", () => {
		expect(() => renderHook(() => useAuth())).toThrow(
			"useAuth must be used within an AuthProvider",
		);
	});

	it("returns authenticated user state and role helpers", () => {
		const { result } = renderHook(() => useAuth(), {
			wrapper: createWrapper(),
		});

		expect(result.current.isAuthenticated).toBe(true);
		expect(result.current.user).toEqual(mockUser);
		expect(result.current.hasRole("SalesArea")).toBe(true);
		expect(result.current.hasRole("SystemAdmin")).toBe(false);
		expect(result.current.hasAnyRole(["SalesArea", "DivisionHead"])).toBe(true);
		expect(result.current.hasCapability("CreateCompany")).toBe(true);
		expect(result.current.hasCapability("BreakGlassEmergencyAccess")).toBe(
			false,
		);
		expect(
			result.current.hasAnyCapability([
				"CreateCompany",
				"BreakGlassEmergencyAccess",
			]),
		).toBe(true);
	});
});
