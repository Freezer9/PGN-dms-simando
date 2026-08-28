import { describe, expect, it } from "vitest";
import {
	getKawasanLabel,
	getPosisiPelangganLabel,
	getStageInfo,
	getStatusLabel,
	STAGE_CONFIG,
} from "./directory-utils";

describe("directory-utils", () => {
	it("returns correct stage information for stages 1 to 8", () => {
		for (let s = 1; s <= 8; s++) {
			const info = getStageInfo(s);
			expect(info.stage).toBe(s);
			expect(info.name).toBe(STAGE_CONFIG[s].name);
			expect(info.shortName).toBe(STAGE_CONFIG[s].shortName);
			expect(info.badgeClass).toBeDefined();
		}
	});

	it("handles string stage parameter properly", () => {
		const info = getStageInfo("3");
		expect(info.stage).toBe(3);
		expect(info.shortName).toBe("Prospek");
	});

	it("returns fallback for unknown stage number", () => {
		const info = getStageInfo(99);
		expect(info.stage).toBe(99);
		expect(info.shortName).toBe("Tahap 99");
	});

	it("maps RecordStatus to appropriate label and badge style", () => {
		expect(getStatusLabel("Draft").label).toBe("Draft");
		expect(getStatusLabel("AreaHead").label).toBe("Review Head of Area");
		expect(getStatusLabel("RegionalAdmin").label).toBe("Regional Admin");
		expect(getStatusLabel("Reviewer1").label).toBe("Reviewer 1");
		expect(getStatusLabel("Approval").label).toBe("Persetujuan Akhir");
		expect(getStatusLabel("IssuedNol").label).toBe("NOL Terbit");
		expect(getStatusLabel("Rejected").label).toBe("Ditolak");
		expect(getStatusLabel("Discontinued").label).toBe("Dihentikan");
	});

	it("formats PosisiPelanggan correctly", () => {
		expect(getPosisiPelangganLabel("JalurExisting")).toBe("Jalur Existing");
		expect(getPosisiPelangganLabel("Pengembangan")).toBe("Pengembangan");
		expect(getPosisiPelangganLabel(null)).toBe("-");
		expect(getPosisiPelangganLabel(undefined)).toBe("-");
	});

	it("formats Kawasan correctly", () => {
		expect(getKawasanLabel("KawasanIndustri")).toBe("Kawasan Industri");
		expect(getKawasanLabel("NonKawasanIndustri")).toBe("Non Kawasan Industri");
		expect(getKawasanLabel(null)).toBe("-");
		expect(getKawasanLabel(undefined)).toBe("-");
	});
});
