import type { Kawasan, PosisiPelanggan, RecordStatus } from "@/api/types";

export interface StageInfo {
	stage: number;
	name: string;
	shortName: string;
	badgeClass: string;
}

export const STAGE_CONFIG: Record<number, StageInfo> = {
	1: {
		stage: 1,
		name: "Tahap 1: Calon Pelanggan",
		shortName: "Calon Pelanggan",
		badgeClass:
			"bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300 border-slate-300",
	},
	2: {
		stage: 2,
		name: "Tahap 2: Plotting",
		shortName: "Plotting",
		badgeClass:
			"bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300 border-blue-300",
	},
	3: {
		stage: 3,
		name: "Tahap 3: Prospek",
		shortName: "Prospek",
		badgeClass:
			"bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300 border-sky-300",
	},
	4: {
		stage: 4,
		name: "Tahap 4: Survei KK0",
		shortName: "Survei KK0",
		badgeClass:
			"bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300 border-emerald-300",
	},
	5: {
		stage: 5,
		name: "Tahap 5: Registrasi A1",
		shortName: "Registrasi A1",
		badgeClass:
			"bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300 border-amber-300",
	},
	6: {
		stage: 6,
		name: "Tahap 6: Permohonan NOL",
		shortName: "Permohonan NOL",
		badgeClass:
			"bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-300 border-orange-300",
	},
	7: {
		stage: 7,
		name: "Tahap 7: Evaluasi NOL",
		shortName: "Evaluasi NOL",
		badgeClass:
			"bg-purple-100 text-purple-700 dark:bg-purple-900/40 dark:text-purple-300 border-purple-300",
	},
	8: {
		stage: 8,
		name: "Tahap 8: Penerbitan NOL",
		shortName: "NOL Terbit",
		badgeClass:
			"bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300 border-green-300",
	},
};

export function getStageInfo(stage: number | string): StageInfo {
	const num = typeof stage === "string" ? Number.parseInt(stage, 10) : stage;
	return (
		STAGE_CONFIG[num] || {
			stage: num,
			name: `Tahap ${num}`,
			shortName: `Tahap ${num}`,
			badgeClass: "bg-muted text-muted-foreground",
		}
	);
}

export function getStatusLabel(status: RecordStatus): {
	label: string;
	badgeClass: string;
} {
	switch (status) {
		case "Draft":
			return {
				label: "Draft",
				badgeClass: "bg-slate-100 text-slate-700 border-slate-300",
			};
		case "AreaHead":
			return {
				label: "Review Head of Area",
				badgeClass: "bg-blue-100 text-blue-700 border-blue-300",
			};
		case "RegionalAdmin":
			return {
				label: "Regional Admin",
				badgeClass: "bg-blue-100 text-blue-700 border-blue-300",
			};
		case "Reviewer1":
			return {
				label: "Reviewer 1",
				badgeClass: "bg-indigo-100 text-indigo-700 border-indigo-300",
			};
		case "Reviewer2":
			return {
				label: "Reviewer 2",
				badgeClass: "bg-indigo-100 text-indigo-700 border-indigo-300",
			};
		case "Reviewer3":
			return {
				label: "Reviewer 3",
				badgeClass: "bg-indigo-100 text-indigo-700 border-indigo-300",
			};
		case "Approval":
			return {
				label: "Persetujuan Akhir",
				badgeClass: "bg-purple-100 text-purple-700 border-purple-300",
			};
		case "IssuedNol":
			return {
				label: "NOL Terbit",
				badgeClass: "bg-emerald-100 text-emerald-700 border-emerald-300",
			};
		case "IssuedRl":
			return {
				label: "RL Terbit",
				badgeClass: "bg-emerald-100 text-emerald-700 border-emerald-300",
			};
		case "Rejected":
			return {
				label: "Ditolak",
				badgeClass: "bg-destructive/10 text-destructive border-destructive/20",
			};
		case "Discontinued":
			return {
				label: "Dihentikan",
				badgeClass: "bg-muted text-muted-foreground border-border",
			};
		default:
			return {
				label: status,
				badgeClass: "bg-muted text-muted-foreground border-border",
			};
	}
}

export function getPosisiPelangganLabel(
	posisi?: PosisiPelanggan | null,
): string {
	if (!posisi) return "-";
	if (posisi === "JalurExisting") return "Jalur Existing";
	if (posisi === "Pengembangan") return "Pengembangan";
	return posisi;
}

export function getKawasanLabel(kawasan?: Kawasan | null): string {
	if (!kawasan) return "-";
	if (kawasan === "KawasanIndustri") return "Kawasan Industri";
	if (kawasan === "NonKawasanIndustri") return "Non Kawasan Industri";
	return kawasan;
}
