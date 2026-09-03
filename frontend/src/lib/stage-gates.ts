import type { PlottingDetail } from "@/api/types";

export interface StageGateContext {
	currentStage: number;
	status: string;
	plotting?:
		| PlottingDetail
		| {
				salesUserId?: string | null;
				posisiPelanggan?: string | null;
				kawasan?: string | null;
		  }
		| null;
	contactsCount?: number;
	attachments?: Array<{ kind: string }>;
	skemaHarga?: string | null;
}

export interface StageGateResult {
	stage: number;
	isUnlocked: boolean;
	isCurrent: boolean;
	isCompleted: boolean;
	reason?: string;
	missingRequirements: string[];
}

/**
 * Evaluates whether each of the 8 canonical stages is unlocked, current, or completed
 * based on the company's progress and the domain stage-gate rules.
 */
export function evaluateStageGates(
	ctx: StageGateContext,
): Record<number, StageGateResult> {
	const currentStage = Number(ctx.currentStage) || 1;
	const status = ctx.status || "Draft";
	const attachments = ctx.attachments || [];
	const attachmentKinds = new Set(attachments.map((a) => a.kind));

	// Stage 1: Calon Pelanggan (Directory)
	const s1Completed = currentStage > 1;
	const s1Current = currentStage === 1;
	const s1: StageGateResult = {
		stage: 1,
		isUnlocked: true,
		isCurrent: s1Current,
		isCompleted: s1Completed,
		missingRequirements: [],
	};

	// Stage 2: Plotting
	const s2Completed = currentStage > 2;
	const s2Current = currentStage === 2;
	const s2: StageGateResult = {
		stage: 2,
		isUnlocked: true,
		isCurrent: s2Current,
		isCompleted: s2Completed,
		missingRequirements: [],
	};

	// Stage 3: Prospek
	const isPlottingFilled =
		Boolean(ctx.plotting?.salesUserId) &&
		Boolean(ctx.plotting?.posisiPelanggan) &&
		Boolean(ctx.plotting?.kawasan);
	const s3Completed = currentStage > 3;
	const s3Current = currentStage === 3;
	const s3Unlocked = currentStage >= 3 || isPlottingFilled;
	const s3Missing: string[] = [];
	if (!s3Unlocked) {
		if (!ctx.plotting?.salesUserId)
			s3Missing.push("Sales Representative belum ditetapkan");
		if (!ctx.plotting?.posisiPelanggan)
			s3Missing.push("Posisi Pelanggan belum dipilih");
		if (!ctx.plotting?.kawasan)
			s3Missing.push("Klasifikasi Kawasan belum dipilih");
	}
	const s3: StageGateResult = {
		stage: 3,
		isUnlocked: s3Unlocked,
		isCurrent: s3Current,
		isCompleted: s3Completed,
		missingRequirements: s3Missing,
		reason: s3Unlocked
			? undefined
			: "Lengkapi konfigurasi Plotting (Sales Representative, Posisi Pelanggan, Kawasan) terlebih dahulu.",
	};

	// Stage 4: Survei KK0
	const hasContacts = (ctx.contactsCount ?? 0) > 0;
	const s4Completed = currentStage > 4;
	const s4Current = currentStage === 4;
	const s4Unlocked =
		currentStage >= 4 ||
		((currentStage >= 3 || isPlottingFilled) && hasContacts);
	const s4Missing: string[] = [];
	if (!s4Unlocked) {
		if (!isPlottingFilled && currentStage < 3) {
			s4Missing.push("Selesaikan Tahap 2 (Plotting) terlebih dahulu");
		}
		if (!hasContacts) {
			s4Missing.push(
				"Tambahkan minimal 1 kontak PIC perusahaan pada Tahap 3 (Prospek)",
			);
		}
	}
	const s4: StageGateResult = {
		stage: 4,
		isUnlocked: s4Unlocked,
		isCurrent: s4Current,
		isCompleted: s4Completed,
		missingRequirements: s4Missing,
		reason: s4Unlocked
			? undefined
			: "Tambahkan minimal satu kontak PIC pada Tahap 3 (Prospek) untuk membuka formulir survei.",
	};

	// Stage 5: Registrasi A1
	const hasKk0Attachment = attachmentKinds.has("Kk0");
	const s5Completed = currentStage > 5;
	const s5Current = currentStage === 5;
	const s5Unlocked =
		currentStage >= 5 || (currentStage >= 4 && hasKk0Attachment);
	const s5Missing: string[] = [];
	if (!s5Unlocked) {
		if (currentStage < 4) {
			s5Missing.push(
				"Selesaikan formulir Survei KK0 (Tahap 4) terlebih dahulu",
			);
		}
		if (!hasKk0Attachment) {
			s5Missing.push(
				"Dokumen KK0 (Lampiran 10) bertanda tangan belum diunggah",
			);
		}
	}
	const s5: StageGateResult = {
		stage: 5,
		isUnlocked: s5Unlocked,
		isCurrent: s5Current,
		isCompleted: s5Completed,
		missingRequirements: s5Missing,
		reason: s5Unlocked
			? undefined
			: "Selesaikan Survei KK0 dan unggah dokumen KK0 (Lampiran 10) bertanda tangan pada tab Lampiran.",
	};

	// Stage 6: Permohonan NOL
	const hasA1Attachment = attachmentKinds.has("A1");
	const hasBuktiKelayakan = attachmentKinds.has("BuktiKelayakan");
	const isSigas = ctx.skemaHarga === "Sigas";
	const hasMomSigas = !isSigas || attachmentKinds.has("MomSigas");
	const s6GatePassed = hasA1Attachment && hasBuktiKelayakan && hasMomSigas;
	const s6Completed = currentStage > 6;
	const s6Current = currentStage === 6;
	const s6Unlocked = currentStage >= 6 || (currentStage >= 5 && s6GatePassed);
	const s6Missing: string[] = [];
	if (!s6Unlocked) {
		if (currentStage < 5) {
			s6Missing.push("Selesaikan Registrasi A1 (Tahap 5) terlebih dahulu");
		}
		if (!hasA1Attachment) {
			s6Missing.push(
				"Dokumen A1 Registrasi (Lampiran 11) bertanda tangan belum diunggah",
			);
		}
		if (!hasBuktiKelayakan) {
			s6Missing.push("Dokumen Bukti Kelayakan belum diunggah");
		}
		if (isSigas && !hasMomSigas) {
			s6Missing.push("MOM SiGas belum diunggah (wajib untuk skema SiGas)");
		}
	}
	const s6: StageGateResult = {
		stage: 6,
		isUnlocked: s6Unlocked,
		isCurrent: s6Current,
		isCompleted: s6Completed,
		missingRequirements: s6Missing,
		reason: s6Unlocked
			? undefined
			: "Unggah dokumen A1 Registrasi dan Bukti Kelayakan pada tab Lampiran.",
	};

	// Stage 7: Evaluasi NOL
	const isPastAreaHead =
		status !== "Draft" && status !== "AreaHead" && status !== "Discontinued";
	const s7Completed =
		currentStage > 7 ||
		status === "Approval" ||
		status === "IssuedNol" ||
		status === "IssuedRl";
	const s7Current =
		currentStage === 7 ||
		status === "RegionalAdmin" ||
		status === "Reviewer1" ||
		status === "Reviewer2" ||
		status === "Reviewer3";
	const s7Unlocked = currentStage >= 7 || isPastAreaHead;
	const s7Missing: string[] = [];
	if (!s7Unlocked) {
		if (status === "Draft") {
			s7Missing.push(
				"Permohonan NOL harus diajukan untuk persetujuan (Submit) terlebih dahulu",
			);
		} else if (status === "AreaHead") {
			s7Missing.push("Menunggu persetujuan evaluasi dari Head of Area");
		} else {
			s7Missing.push("Tahap alur kerja belum mencapai Evaluasi Regional");
		}
	}
	const s7: StageGateResult = {
		stage: 7,
		isUnlocked: s7Unlocked,
		isCurrent: s7Current,
		isCompleted: s7Completed,
		missingRequirements: s7Missing,
		reason: s7Unlocked
			? undefined
			: "Permohonan NOL harus diajukan dan disetujui Head of Area terlebih dahulu.",
	};

	// Stage 8: Penerbitan NOL
	const isAtOrPastApproval =
		status === "Approval" || status === "IssuedNol" || status === "IssuedRl";
	const s8Completed = status === "IssuedNol" || status === "IssuedRl";
	const s8Current = currentStage === 8 || status === "Approval";
	const s8Unlocked = currentStage >= 8 || isAtOrPastApproval;
	const s8Missing: string[] = [];
	if (!s8Unlocked) {
		s8Missing.push(
			"Alur kerja belum mencapai tahap persetujuan akhir / penerbitan Division Head",
		);
	}
	const s8: StageGateResult = {
		stage: 8,
		isUnlocked: s8Unlocked,
		isCurrent: s8Current,
		isCompleted: s8Completed,
		missingRequirements: s8Missing,
		reason: s8Unlocked
			? undefined
			: "Tahap 8 terbuka saat proses alur kerja mencapai persetujuan akhir oleh Division Head.",
	};

	return {
		1: s1,
		2: s2,
		3: s3,
		4: s4,
		5: s5,
		6: s6,
		7: s7,
		8: s8,
	};
}
