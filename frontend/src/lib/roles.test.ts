import { describe, expect, it } from "vitest";
import { formatRole, formatRoles } from "./roles";

describe("roles utilities", () => {
	it("formats known roles with spaces correctly", () => {
		expect(formatRole("SalesArea")).toBe("Sales Area");
		expect(formatRole("AreaHead")).toBe("Area Head");
		expect(formatRole("RegionalAdmin")).toBe("Regional Admin");
		expect(formatRole("Reviewer")).toBe("Reviewer");
		expect(formatRole("Reviewer1")).toBe("Reviewer 1");
		expect(formatRole("Reviewer2")).toBe("Reviewer 2");
		expect(formatRole("Reviewer3")).toBe("Reviewer 3");
		expect(formatRole("DivisionHead")).toBe("Division Head");
		expect(formatRole("SystemAdmin")).toBe("System Admin");
		expect(formatRole("Approval")).toBe("Division Head");
		expect(formatRole("AdminRegional")).toBe("Regional Admin");
	});

	it("formats prefixed roles like 'Peran: RegionalAdmin' correctly", () => {
		expect(formatRole("Peran: RegionalAdmin")).toBe("Peran: Regional Admin");
		expect(formatRole("Peran: AreaHead")).toBe("Peran: Area Head");
	});

	it("falls back to PascalCase splitting for unknown roles", () => {
		expect(formatRole("CustomRole")).toBe("Custom Role");
		expect(formatRole("SuperAdminUser")).toBe("Super Admin User");
	});

	it("handles empty or null values gracefully", () => {
		expect(formatRole(null)).toBe("");
		expect(formatRole(undefined)).toBe("");
		expect(formatRole("")).toBe("");
	});

	it("formats list of roles with commas", () => {
		expect(formatRoles(["SalesArea", "AreaHead"])).toBe(
			"Sales Area, Area Head",
		);
		expect(formatRoles([])).toBe("");
		expect(formatRoles(null)).toBe("");
	});
});
