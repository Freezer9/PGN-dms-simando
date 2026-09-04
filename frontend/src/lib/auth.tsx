import { useQueryClient } from "@tanstack/react-query";
import * as React from "react";
import { $api } from "@/api/client";
import type {
	AppCapability,
	AppRole,
	ChangePasswordRequest,
	CurrentUserDto,
	LoginRequest,
} from "@/api/types";

export interface AuthContextValue {
	user: CurrentUserDto | null;
	isLoading: boolean;
	isAuthenticated: boolean;
	login: (credentials: LoginRequest) => Promise<CurrentUserDto>;
	logout: () => Promise<void>;
	changePassword: (req: ChangePasswordRequest) => Promise<CurrentUserDto>;
	hasRole: (role: AppRole) => boolean;
	hasAnyRole: (roles: AppRole[]) => boolean;
	hasCapability: (cap: AppCapability) => boolean;
	hasAnyCapability: (caps: AppCapability[]) => boolean;
}

const AuthContext = React.createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
	const queryClient = useQueryClient();

	const { data: user, isLoading } = $api.useQuery(
		"get",
		"/api/auth/me",
		undefined,
		{
			retry: false,
			staleTime: 1000 * 60 * 5,
		},
	);

	const loginMutation = $api.useMutation("post", "/api/auth/login", {
		onSuccess: (data) => {
			queryClient.setQueryData(
				$api.queryOptions("get", "/api/auth/me").queryKey,
				data,
			);
		},
	});

	const logoutMutation = $api.useMutation("post", "/api/auth/logout", {
		onSuccess: () => {
			queryClient.setQueryData(
				$api.queryOptions("get", "/api/auth/me").queryKey,
				null,
			);
			queryClient.clear();
		},
	});

	const changePasswordMutation = $api.useMutation(
		"post",
		"/api/auth/change-password",
		{
			onSuccess: (data) => {
				queryClient.setQueryData(
					$api.queryOptions("get", "/api/auth/me").queryKey,
					data,
				);
			},
		},
	);

	const login = React.useCallback(
		async (credentials: LoginRequest): Promise<CurrentUserDto> => {
			const res = await loginMutation.mutateAsync({
				body: credentials,
			});
			return res;
		},
		[loginMutation],
	);

	const logout = React.useCallback(async (): Promise<void> => {
		await logoutMutation.mutateAsync({});
	}, [logoutMutation]);

	const changePassword = React.useCallback(
		async (req: ChangePasswordRequest): Promise<CurrentUserDto> => {
			const res = await changePasswordMutation.mutateAsync({
				body: req,
			});
			return res;
		},
		[changePasswordMutation],
	);

	const hasRole = React.useCallback(
		(role: AppRole): boolean => {
			return Boolean(user?.roles?.includes(role));
		},
		[user],
	);

	const hasAnyRole = React.useCallback(
		(roles: AppRole[]): boolean => {
			return Boolean(user?.roles?.some((r) => roles.includes(r)));
		},
		[user],
	);

	const hasCapability = React.useCallback(
		(cap: AppCapability): boolean => {
			return Boolean(user?.capabilities?.includes(cap));
		},
		[user],
	);

	const hasAnyCapability = React.useCallback(
		(caps: AppCapability[]): boolean => {
			return Boolean(user?.capabilities?.some((c) => caps.includes(c)));
		},
		[user],
	);

	const value = React.useMemo<AuthContextValue>(
		() => ({
			user: user ?? null,
			isLoading,
			isAuthenticated: Boolean(user),
			login,
			logout,
			changePassword,
			hasRole,
			hasAnyRole,
			hasCapability,
			hasAnyCapability,
		}),
		[
			user,
			isLoading,
			login,
			logout,
			changePassword,
			hasRole,
			hasAnyRole,
			hasCapability,
			hasAnyCapability,
		],
	);

	return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
	const context = React.useContext(AuthContext);
	if (!context) {
		throw new Error("useAuth must be used within an AuthProvider");
	}
	return context;
}

export function useOptionalAuth(): AuthContextValue | null {
	return React.useContext(AuthContext);
}
