export interface MockCompanyData {
	namaPerusahaan: string;
	npwp: string;
	email: string;
	telp: string;
	website: string;
	alamat: string;
	kodePos: string;
	latitude: number;
	longitude: number;
}

export function generateMockCompany(
	suffix: string = Date.now().toString().slice(-4),
): MockCompanyData {
	return {
		namaPerusahaan: `PT Maju Energi Bersama ${suffix}`,
		npwp: `01.234.567.${suffix.slice(-1)}-890.000`,
		email: `contact@energi${suffix}.co.id`,
		telp: "021-5551234",
		website: `https://energi${suffix}.co.id`,
		alamat: `Jl. Industri Raya Blok D No. ${suffix.slice(-2)}, Kawasan Industri SIER`,
		kodePos: "61256",
		latitude: -7.31956,
		longitude: 112.7582,
	};
}

export function generateMockContact() {
	return {
		name: "Budi Santoso",
		role: "Plant Operations Manager",
		phone: "081234567890",
		email: "budi.santoso@energi.co.id",
		isPrimary: true,
	};
}

export function generateMockSurveyData() {
	return {
		products: "Keramik dan Granit Tile",
		rawMaterials: "Tanah Liat, Feldspar, Pasir Silika",
		marketOrientation: "Domestik 70%, Ekspor 30%",
		operationHoursPerDay: 24,
		operationDaysPerMonth: 30,
		equipmentName: "Tunnel Kiln 01",
		equipmentBrand: "Sacmi",
		equipmentCount: 2,
		fuelUsage: "1500 Liter Solar / Jam",
		gasDemand: 1200,
	};
}

export function generateMockA1Data() {
	return {
		basisKontrak: "Bulanan",
		skema: "Reguler",
		segment: "Industri Menengah (Gold)",
		kodeHarga: "IND-GOLD-01",
		harga: 6.5,
		volumeMin: 10000,
		volumeMax: 50000,
	};
}

export function generateMockEvaluationData() {
	return {
		feedStatus: "OK - Jalur Tersedia",
		capexPreGR3: 1500000000,
		biayaPenyambungan: 250000000,
		pipaIndukDiameter: '4 Inch Steel API 5L',
		pipaServiceDiameter: '2 Inch PE 100',
		mrsSpec: 'MRS 1000 G-65 PN 16',
		gSize: 'G-65',
		tekananOperasi: '4 Barg',
		flowrate: '1000 Nm3/h',
		irr: 18.5,
		npv: 850000000,
		paybackYears: 3.2,
	};
}

export const MOCK_PDF_BUFFER = {
	name: "signed_document.pdf",
	mimeType: "application/pdf",
	buffer: Buffer.from("%PDF-1.4 mock pdf binary content for e2e testing"),
};

export const MOCK_DOCX_BUFFER = {
	name: "survey_lampiran.docx",
	mimeType:
		"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
	buffer: Buffer.from("PK mock docx binary content for e2e testing"),
};
