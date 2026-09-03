import { describe, expect, it } from "vitest";
import { evaluateStageGates } from "./stage-gates";

describe("evaluateStageGates", () => {
	it("unlocks only Stage 1 and 2 for a newly created company", () => {
		const gates = evaluateStageGates({
			currentStage: 1,
			status: "Draft",
			plotting: null,
			contactsCount: 0,
			attachments: [],
		});

		expect(gates[1].isUnlocked).toBe(true);
		expect(gates[2].isUnlocked).toBe(true);
		expect(gates[3].isUnlocked).toBe(false);
		expect(gates[4].isUnlocked).toBe(false);
		expect(gates[5].isUnlocked).toBe(false);
		expect(gates[6].isUnlocked).toBe(false);
		expect(gates[7].isUnlocked).toBe(false);
		expect(gates[8].isUnlocked).toBe(false);
	});

	it("unlocks Stage 3 when plotting is completely configured", () => {
		const gates = evaluateStageGates({
			currentStage: 2,
			status: "Draft",
			plotting: {
				salesUserId: "user-1",
				posisiPelanggan: "JalurExisting",
				kawasan: "KawasanIndustri",
			},
			contactsCount: 0,
			attachments: [],
		});

		expect(gates[1].isUnlocked).toBe(true);
		expect(gates[2].isUnlocked).toBe(true);
		expect(gates[3].isUnlocked).toBe(true);
		expect(gates[4].isUnlocked).toBe(false);
	});

	it("unlocks Stage 4 when at Stage 3 and at least 1 contact is added", () => {
		const gates = evaluateStageGates({
			currentStage: 3,
			status: "Draft",
			plotting: {
				salesUserId: "user-1",
				posisiPelanggan: "JalurExisting",
				kawasan: "KawasanIndustri",
			},
			contactsCount: 1,
			attachments: [],
		});

		expect(gates[4].isUnlocked).toBe(true);
		expect(gates[5].isUnlocked).toBe(false);
	});

	it("unlocks Stage 5 when at Stage 4 and KK0 attachment is uploaded", () => {
		const gates = evaluateStageGates({
			currentStage: 4,
			status: "Draft",
			contactsCount: 1,
			attachments: [{ kind: "Kk0" }],
		});

		expect(gates[5].isUnlocked).toBe(true);
		expect(gates[6].isUnlocked).toBe(false);
	});

	it("unlocks Stage 6 when A1, BuktiKelayakan attachments are uploaded", () => {
		const gates = evaluateStageGates({
			currentStage: 5,
			status: "Draft",
			attachments: [
				{ kind: "Kk0" },
				{ kind: "A1" },
				{ kind: "BuktiKelayakan" },
			],
		});

		expect(gates[6].isUnlocked).toBe(true);
		expect(gates[7].isUnlocked).toBe(false);
	});

	it("requires MOM SiGas for Stage 6 if pricing scheme is Sigas", () => {
		const withoutMom = evaluateStageGates({
			currentStage: 5,
			status: "Draft",
			skemaHarga: "Sigas",
			attachments: [
				{ kind: "Kk0" },
				{ kind: "A1" },
				{ kind: "BuktiKelayakan" },
			],
		});
		expect(withoutMom[6].isUnlocked).toBe(false);

		const withMom = evaluateStageGates({
			currentStage: 5,
			status: "Draft",
			skemaHarga: "Sigas",
			attachments: [
				{ kind: "Kk0" },
				{ kind: "A1" },
				{ kind: "BuktiKelayakan" },
				{ kind: "MomSigas" },
			],
		});
		expect(withMom[6].isUnlocked).toBe(true);
	});

	it("unlocks Stage 7 when workflow is at RegionalAdmin status", () => {
		const gates = evaluateStageGates({
			currentStage: 6,
			status: "RegionalAdmin",
			attachments: [
				{ kind: "Kk0" },
				{ kind: "A1" },
				{ kind: "BuktiKelayakan" },
				{ kind: "CapexPreGr3" },
			],
		});

		expect(gates[7].isUnlocked).toBe(true);
		expect(gates[8].isUnlocked).toBe(false);
	});

	it("unlocks Stage 8 when workflow reaches Approval or final issuance", () => {
		const gates = evaluateStageGates({
			currentStage: 7,
			status: "Approval",
		});

		expect(gates[7].isUnlocked).toBe(true);
		expect(gates[8].isUnlocked).toBe(true);
	});

	it("keeps all past stages unlocked when company is at higher stage", () => {
		const gates = evaluateStageGates({
			currentStage: 6,
			status: "Draft",
		});

		for (let s = 1; s <= 6; s++) {
			expect(gates[s].isUnlocked).toBe(true);
		}
		expect(gates[7].isUnlocked).toBe(false);
		expect(gates[8].isUnlocked).toBe(false);
	});
});
