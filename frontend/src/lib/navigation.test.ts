import { describe, expect, it } from "vitest";
import type { CurrentUserDto } from "@/api/types";
import { buildNavigationMenu } from "./navigation";

describe("Navigation Menu Engine", () => {
	it("returns empty sections for null user", () => {
		const menu = buildNavigationMenu(null);
		expect(menu.sections).toHaveLength(0);
	});

	it("builds SalesArea navigation correctly", () => {
		const salesUser: CurrentUserDto = {
			id: "1",
			username: "sales",
			email: "sales@pgn.co.id",
			fullName: "Sales User",
			scope: "Area",
			roles: ["SalesArea"],
			capabilities: [
				"ViewDashboardFunnel",
				"ViewCompanyRecords",
				"CreateCompany",
			],
			mustChangePassword: false,
		};

		const menu = buildNavigationMenu(salesUser);
		const allItems = menu.sections.flatMap((s) => s.nodes);

		const hrefs = allItems
			.filter((n) => n.type === "item")
			.map((n) => (n.type === "item" ? n.href : ""));

		expect(hrefs).toContain("/");
		expect(hrefs).toContain("/directory");
		expect(hrefs).toContain("/plotting");
		expect(hrefs).toContain("/map");
		expect(hrefs).toContain("/reports");
		expect(hrefs).not.toContain("/tasks"); // SalesArea doesn't have ActOnApprovalStep
		expect(hrefs).not.toContain("/master/organisation");
	});

	it("builds SystemAdmin navigation with master data groupings", () => {
		const adminUser: CurrentUserDto = {
			id: "2",
			username: "admin",
			email: "admin@pgn.co.id",
			fullName: "System Admin",
			scope: "All",
			roles: ["SystemAdmin"],
			capabilities: ["ManageMasterData", "AssignRoles"],
			mustChangePassword: false,
		};

		const menu = buildNavigationMenu(adminUser);
		const adminSection = menu.sections.find(
			(s) => s.title === "Master Data & Sistem",
		);
		expect(adminSection).toBeDefined();

		const groupTitles = adminSection?.nodes
			.filter((n) => n.type === "group")
			.map((n) => (n.type === "group" ? n.title : ""));

		expect(groupTitles).toContain("Referensi");
		expect(groupTitles).toContain("Komersial");
		expect(groupTitles).toContain("Energi & Konversi");
		expect(groupTitles).toContain("Teknis");
		expect(groupTitles).toContain("Dokumen");
		expect(groupTitles).toContain("Pemulihan");
	});

	it("includes badge for pending tasks on Tugas Saya", () => {
		const reviewerUser: CurrentUserDto = {
			id: "3",
			username: "reviewer",
			email: "reviewer@pgn.co.id",
			fullName: "Reviewer User",
			scope: "Region",
			roles: ["Reviewer"],
			capabilities: ["ActOnApprovalStep", "ViewCompanyRecords"],
			mustChangePassword: false,
		};

		const menuWithBadge = buildNavigationMenu(reviewerUser, 5);
		const tasksItem = menuWithBadge.sections
			.flatMap((s) => s.nodes)
			.find((n) => n.type === "item" && n.href === "/tasks");

		expect(tasksItem).toBeDefined();
		if (tasksItem && tasksItem.type === "item") {
			expect(tasksItem.badge).toBe(5);
		}
	});

	it("includes badge for pending tasks and blocked tasks on RegionalAdmin", () => {
		const regionalAdminUser: CurrentUserDto = {
			id: "4",
			username: "regadmin",
			email: "regadmin@pgn.co.id",
			fullName: "Regional Admin User",
			scope: "Region",
			roles: ["RegionalAdmin"],
			capabilities: [
				"ActOnApprovalStep",
				"ReassignWorkflowStep",
				"ViewCompanyRecords",
			],
			mustChangePassword: false,
		};

		const menuWithBadge = buildNavigationMenu(regionalAdminUser, 5, 2);
		const tasksItem = menuWithBadge.sections
			.flatMap((s) => s.nodes)
			.find((n) => n.type === "item" && n.href === "/tasks");
		const blockedItem = menuWithBadge.sections
			.flatMap((s) => s.nodes)
			.find((n) => n.type === "item" && n.href === "/tasks/blocked");

		expect(tasksItem).toBeDefined();
		if (tasksItem && tasksItem.type === "item") {
			expect(tasksItem.badge).toBe(5);
		}

		expect(blockedItem).toBeDefined();
		if (blockedItem && blockedItem.type === "item") {
			expect(blockedItem.badge).toBe(2);
		}
	});
});
