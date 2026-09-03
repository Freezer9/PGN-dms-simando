import type { CurrentUserDto } from "@/api/types";

export interface NavItem {
	type: "item";
	title: string;
	href: string;
	to?: string;
	search?: Record<string, unknown>;
	activeOptions?: {
		exact?: boolean;
		explicitUndefined?: boolean;
		includeSearch?: boolean;
	};
	icon: string;
	badge?: string | number | null;
}

export interface NavGroup {
	type: "group";
	title: string;
	icon: string;
	items: NavItem[];
}

export type NavNode = NavItem | NavGroup;

export interface NavSection {
	title?: string;
	nodes: NavNode[];
}

export interface NavMenu {
	sections: NavSection[];
}

const BROWSE_PIPELINE_ROLES = ["SalesArea", "AreaHead", "RegionalAdmin"];

export function buildNavigationMenu(
	user: CurrentUserDto | null,
	pendingTaskCount?: number | string | null,
	blockedTaskCount?: number | string | null,
): NavMenu {
	if (!user) {
		return { sections: [] };
	}

	const capabilities = new Set(user.capabilities || []);
	const roles = new Set(user.roles || []);
	const isSysAdmin = capabilities.has("ManageMasterData");

	const sections: NavSection[] = [];

	// 1. Case Work Section (Non-System Admin or general workflow)
	const caseWorkNodes: NavNode[] = [];

	if (capabilities.has("ViewDashboardFunnel")) {
		caseWorkNodes.push({
			type: "item",
			title: "Beranda",
			href: "/",
			icon: "house",
		});
	}

	if (capabilities.has("ActOnApprovalStep")) {
		const pCount =
			pendingTaskCount !== null && pendingTaskCount !== undefined
				? Number(pendingTaskCount)
				: null;
		const badge = pCount && pCount > 0 ? pCount : null;
		caseWorkNodes.push({
			type: "item",
			title: "Tugas Saya",
			href: "/tasks",
			icon: "list-checks",
			badge,
		});
	}

	if (capabilities.has("ReassignWorkflowStep") && user.scope === "Region") {
		const bCount =
			blockedTaskCount !== null && blockedTaskCount !== undefined
				? Number(blockedTaskCount)
				: null;
		const blockedBadge = bCount && bCount > 0 ? bCount : null;
		caseWorkNodes.push({
			type: "item",
			title: "Tugas Tertahan",
			href: "/tasks/blocked",
			icon: "octagon-alert",
			badge: blockedBadge,
		});
	}

	// Direktori / Plotting: Role-gated
	const hasBrowseRole = Array.from(roles).some((r) =>
		BROWSE_PIPELINE_ROLES.includes(r),
	);
	if (hasBrowseRole) {
		caseWorkNodes.push({
			type: "item",
			title: "Direktori",
			href: "/directory",
			to: "/directory",
			search: { stage: undefined },
			activeOptions: { explicitUndefined: true },
			icon: "building-2",
		});
		caseWorkNodes.push({
			type: "item",
			title: "Plotting",
			href: "/directory?stage=2",
			to: "/directory",
			search: { stage: 2 },
			icon: "map-pin",
		});
	}

	if (capabilities.has("ViewCompanyRecords")) {
		caseWorkNodes.push({
			type: "item",
			title: "Peta",
			href: "/map",
			icon: "map",
		});
	}

	if (capabilities.has("EditEvaluation")) {
		caseWorkNodes.push({
			type: "item",
			title: "Evaluasi",
			href: "/directory?stage=7",
			to: "/directory",
			search: { stage: 7 },
			icon: "calculator",
		});
	}

	if (capabilities.has("ViewDashboardFunnel")) {
		caseWorkNodes.push({
			type: "item",
			title: "Laporan",
			href: "/reports",
			icon: "bar-chart-3",
		});
	}

	if (caseWorkNodes.length > 0) {
		sections.push({ nodes: caseWorkNodes });
	}

	// 2. Extras Section for Regional Admin / Division Head
	if (!isSysAdmin) {
		const extraNodes: NavNode[] = [];

		if (capabilities.has("AssignRoles")) {
			extraNodes.push({
				type: "item",
				title: "Pengguna",
				href: "/master/users",
				icon: "users",
			});
		}

		if (
			capabilities.has("ViewBreakGlassActivity") ||
			capabilities.has("BreakGlassRecordRead")
		) {
			extraNodes.push({
				type: "item",
				title: "Akses Darurat",
				href: "/admin/break-glass",
				icon: "shield-alert",
			});
		}

		if (extraNodes.length > 0) {
			sections.push({ title: "Administrasi Wilayah", nodes: extraNodes });
		}
	}

	// 3. Admin Section for System Admin
	if (isSysAdmin) {
		const adminNodes: NavNode[] = [
			{
				type: "item",
				title: "Organisasi",
				href: "/master/organisation",
				icon: "network",
			},
			{
				type: "item",
				title: "Pengguna",
				href: "/master/users",
				icon: "users",
			},
			{
				type: "group",
				title: "Referensi",
				icon: "map-pinned",
				items: [
					{
						type: "item",
						title: "Negara",
						href: "/master/countries",
						icon: "globe",
					},
					{
						type: "item",
						title: "Jenis Industri",
						href: "/master/industry-types",
						icon: "factory",
					},
				],
			},
			{
				type: "group",
				title: "Komersial",
				icon: "tags",
				items: [
					{
						type: "item",
						title: "Segmen",
						href: "/master/segments",
						icon: "tags",
					},
				],
			},
			{
				type: "group",
				title: "Energi & Konversi",
				icon: "fuel",
				items: [
					{
						type: "item",
						title: "Jenis Bahan Bakar",
						href: "/master/fuel-types",
						icon: "fuel",
					},
					{
						type: "item",
						title: "Satuan",
						href: "/master/units",
						icon: "ruler",
					},
				],
			},
			{
				type: "group",
				title: "Teknis",
				icon: "wrench",
				items: [
					{
						type: "item",
						title: "G-Size / Meter",
						href: "/master/meter-sizes",
						icon: "gauge",
					},
					{
						type: "item",
						title: "Spesifikasi MRS",
						href: "/master/mrs-specs",
						icon: "settings-2",
					},
				],
			},
			{
				type: "group",
				title: "Dokumen",
				icon: "folder",
				items: [
					{
						type: "item",
						title: "Dokumen Acuan Kerja",
						href: "/master/reference-documents",
						icon: "file-text",
					},
					{
						type: "item",
						title: "Kategori Alasan",
						href: "/master/reason-categories",
						icon: "tags",
					},
				],
			},
			{
				type: "group",
				title: "Pemulihan",
				icon: "life-buoy",
				items: [
					{
						type: "item",
						title: "Langkah Tertahan",
						href: "/admin/stuck-steps",
						icon: "octagon-pause",
					},
					{
						type: "item",
						title: "Akses Darurat (break-glass)",
						href: "/admin/break-glass",
						icon: "shield-alert",
					},
				],
			},
		];

		sections.push({ title: "Master Data & Sistem", nodes: adminNodes });
	}

	return { sections };
}
