import type { Kawasan, PosisiPelanggan, RecordStatus } from "@/api/types";
import { formatRole } from "./roles";

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
				label: formatRole(status) || status,
				badgeClass: "bg-muted text-muted-foreground border-border",
			};
	}
}

export interface TimelineActionInfo {
	label: string;
	badgeClass: string;
	dotClass: string;
	nodeClass: string;
	iconName:
		| "Plus"
		| "ArrowRight"
		| "Send"
		| "Check"
		| "RotateCcw"
		| "X"
		| "Wrench"
		| "Ban"
		| "Award"
		| "UserCheck"
		| "ShieldAlert"
		| "Activity";
}

export function getTimelineActionInfo(action: string): TimelineActionInfo {
	switch (action) {
		case "Create":
			return {
				label: "Pendaftaran Berkas",
				badgeClass:
					"bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-950/40 dark:text-blue-300 dark:border-blue-800",
				dotClass: "bg-blue-500",
				nodeClass:
					"bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-950/60 dark:text-blue-300 dark:border-blue-800",
				iconName: "Plus",
			};
		case "Save":
			return {
				label: "Kemajuan Tahap",
				badgeClass:
					"bg-sky-50 text-sky-700 border-sky-200 dark:bg-sky-950/40 dark:text-sky-300 dark:border-sky-800",
				dotClass: "bg-sky-500",
				nodeClass:
					"bg-sky-50 text-sky-700 border-sky-200 dark:bg-sky-950/60 dark:text-sky-300 dark:border-sky-800",
				iconName: "ArrowRight",
			};
		case "Submit":
			return {
				label: "Pengajuan Persetujuan",
				badgeClass:
					"bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800",
				dotClass: "bg-amber-500",
				nodeClass:
					"bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800",
				iconName: "Send",
			};
		case "Setuju":
			return {
				label: "Disetujui",
				badgeClass:
					"bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800",
				dotClass: "bg-emerald-500",
				nodeClass:
					"bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800",
				iconName: "Check",
			};
		case "Revisi":
			return {
				label: "Diminta Revisi",
				badgeClass:
					"bg-orange-50 text-orange-700 border-orange-200 dark:bg-orange-950/40 dark:text-orange-300 dark:border-orange-800",
				dotClass: "bg-orange-500",
				nodeClass:
					"bg-orange-50 text-orange-700 border-orange-200 dark:bg-orange-950/60 dark:text-orange-300 dark:border-orange-800",
				iconName: "RotateCcw",
			};
		case "Tolak":
			return {
				label: "Ditolak",
				badgeClass:
					"bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/40 dark:text-rose-300 dark:border-rose-800",
				dotClass: "bg-rose-500",
				nodeClass:
					"bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/60 dark:text-rose-300 dark:border-rose-800",
				iconName: "X",
			};
		case "Rework":
			return {
				label: "Rework (Perbaikan)",
				badgeClass:
					"bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800",
				dotClass: "bg-amber-500",
				nodeClass:
					"bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800",
				iconName: "Wrench",
			};
		case "Discontinue":
			return {
				label: "Berkas Dihentikan",
				badgeClass:
					"bg-slate-100 text-slate-700 border-slate-300 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700",
				dotClass: "bg-slate-500",
				nodeClass:
					"bg-slate-100 text-slate-700 border-slate-300 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700",
				iconName: "Ban",
			};
		case "Issue":
			return {
				label: "Surat Diterbitkan",
				badgeClass:
					"bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800",
				dotClass: "bg-emerald-500",
				nodeClass:
					"bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800",
				iconName: "Award",
			};
		case "Reassign":
			return {
				label: "Pengalihan PIC",
				badgeClass:
					"bg-purple-50 text-purple-700 border-purple-200 dark:bg-purple-950/40 dark:text-purple-300 dark:border-purple-800",
				dotClass: "bg-purple-500",
				nodeClass:
					"bg-purple-50 text-purple-700 border-purple-200 dark:bg-purple-950/60 dark:text-purple-300 dark:border-purple-800",
				iconName: "UserCheck",
			};
		case "BreakGlass":
			return {
				label: "Akses Darurat",
				badgeClass:
					"bg-red-50 text-red-700 border-red-200 dark:bg-red-950/40 dark:text-red-300 dark:border-red-800",
				dotClass: "bg-red-500",
				nodeClass:
					"bg-red-50 text-red-700 border-red-200 dark:bg-red-950/60 dark:text-red-300 dark:border-red-800",
				iconName: "ShieldAlert",
			};
		default:
			return {
				label: action,
				badgeClass: "bg-muted text-muted-foreground border-border",
				dotClass: "bg-muted-foreground",
				nodeClass: "bg-muted text-muted-foreground border-border",
				iconName: "Activity",
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
