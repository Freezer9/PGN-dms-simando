import type { AppRole } from "@/api/types";

export interface RoleMeta {
	value: AppRole;
	label: string;
	scopeType: "area" | "region" | "none";
}

export const ALL_ROLES: RoleMeta[] = [
	{ value: "SalesArea", label: "Sales Area", scopeType: "area" },
	{ value: "AreaHead", label: "Area Head", scopeType: "area" },
	{ value: "Reviewer", label: "Reviewer", scopeType: "region" },
	{ value: "RegionalAdmin", label: "Regional Admin", scopeType: "region" },
	{ value: "DivisionHead", label: "Division Head", scopeType: "region" },
	{ value: "SystemAdmin", label: "System Admin", scopeType: "none" },
];

export const ROLE_LABELS: Record<string, string> = {
	SalesArea: "Sales Area",
	AreaHead: "Area Head",
	Reviewer: "Reviewer",
	Reviewer1: "Reviewer 1",
	Reviewer2: "Reviewer 2",
	Reviewer3: "Reviewer 3",
	RegionalAdmin: "Regional Admin",
	AdminRegional: "Regional Admin",
	DivisionHead: "Division Head",
	SystemAdmin: "System Admin",
	Approval: "Division Head",
};

/**
 * Formats a role enum value into a readable display string with proper spaces.
 * E.g., "SalesArea" -> "Sales Area", "AreaHead" -> "Area Head", "Reviewer1" -> "Reviewer 1"
 */
export function formatRole(role?: string | null): string {
	if (!role) return "";
	if (ROLE_LABELS[role]) return ROLE_LABELS[role];
	if (role.startsWith("Peran: ")) {
		return `Peran: ${formatRole(role.slice(7).trim())}`;
	}
	return role.replace(/([a-z])([A-Z0-9])/g, "$1 $2");
}

/**
 * Formats a list of role enums into a comma-separated readable string.
 * E.g., ["SalesArea", "AreaHead"] -> "Sales Area, Area Head"
 */
export function formatRoles(
	roles?: (string | null | undefined)[] | null,
): string {
	if (!roles || roles.length === 0) return "";
	return roles
		.filter((r): r is string => Boolean(r))
		.map((r) => formatRole(r))
		.join(", ");
}
