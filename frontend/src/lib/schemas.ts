import { z } from "zod";

// --- Stage 1: KK0 Survey Schema ---
export const surveyProductSchema = z.object({
	id: z.string().optional(),
	produk: z.string(),
	kapasitas: z.union([z.number(), z.string(), z.null()]).optional(),
	hargaProduk: z.union([z.number(), z.string(), z.null()]).optional(),
	catatan: z.string().nullable().optional(),
});

export const surveyRawMaterialSchema = z.object({
	id: z.string().optional(),
	bahan: z.string().nullable().optional(),
	asal: z.string().nullable().optional(),
	countryId: z.string().nullable().optional(),
	volume: z.union([z.number(), z.string(), z.null()]).optional(),
	satuanUnitId: z.string().nullable().optional(),
});

export const surveyMarketSchema = z.object({
	id: z.string().optional(),
	bahan: z.string().nullable().optional(),
	asal: z.string().nullable().optional(),
	countryId: z.string().nullable().optional(),
	volume: z.union([z.number(), z.string(), z.null()]).optional(),
	satuanUnitId: z.string().nullable().optional(),
});

export const surveyEquipmentSchema = z.object({
	id: z.string().optional(),
	jenisPeralatan: z.string(),
	kapasitas: z.union([z.number(), z.string(), z.null()]).optional(),
	kapasitasUnitId: z.string().nullable().optional(),
	jamPerHari: z.union([z.number(), z.string(), z.null()]).optional(),
	hariPerMinggu: z.union([z.number(), z.string(), z.null()]).optional(),
	fuelTypeId: z.string().nullable().optional(),
	hargaBahanBakar: z.union([z.number(), z.string(), z.null()]).optional(),
	konsumsiPerBulan: z.union([z.number(), z.string(), z.null()]).optional(),
	konsumsiUnitId: z.string().nullable().optional(),
	konversiKeGas: z.union([z.number(), z.string(), z.null()]).optional(),
});

export const surveyKk0Schema = z.object({
	tanggalSurvey: z.string().optional(),
	surveyorUserId: z.string().optional(),
	jumlahKaryawan: z.string().optional(),
	jumlahShift: z.string().optional(),
	jamKerjaPerHari: z.string().optional(),
	hariPerMinggu: z.string().optional(),
	bebanPuncak1Mulai: z.string().optional(),
	bebanPuncak1Selesai: z.string().optional(),
	bebanPuncak2Mulai: z.string().optional(),
	bebanPuncak2Selesai: z.string().optional(),
	kebutuhanEnergi: z.string().optional(),
	kebutuhanEnergiLainnya: z.string().optional(),
	kapasitasNilai: z.string().optional(),
	kapasitasUnitId: z.string().optional(),
	pemakaianNilai: z.string().optional(),
	pemakaianUnitId: z.string().optional(),
	pipaTerdekatJarakM: z.string().optional(),
	pipaTerdekatDiameter: z.string().optional(),
	pipaTerdekatTekanan: z.string().optional(),
	bahanBakarEksisting: z.string().optional(),
	namaPemasok: z.string().optional(),
	kapasitasListrikKw: z.string().optional(),
	pemakaianListrikKwh: z.string().optional(),
	rencanaPemanfaatanGas: z.string().optional(),
	deskripsiProsesProduksi: z.string().optional(),
	minEfisiensiDiharapkanPct: z.string().optional(),
	willingnessToPayUsdMmbtu: z.string().optional(),
	keteranganLain: z.string().optional(),
	products: z.array(surveyProductSchema).optional(),
	rawMaterials: z.array(surveyRawMaterialSchema).optional(),
	markets: z.array(surveyMarketSchema).optional(),
	equipment: z.array(surveyEquipmentSchema).optional(),
});

export type SurveyKk0FormValues = z.infer<typeof surveyKk0Schema>;

// --- Stage 2: A1 Registration Schema ---
export const a1UsagePeriodSchema = z.object({
	id: z.string().optional(),
	periodeMulai: z.string().optional(),
	periodeSelesai: z.string().optional(),
	rataRata: z.union([z.number(), z.string()]).optional(),
	minimum: z.union([z.number(), z.string()]).optional(),
	maksimum: z.union([z.number(), z.string()]).optional(),
	sortOrder: z.number().optional(),
});

export const a1RegistrationSchema = z.object({
	tanggalRegistrasi: z.string().optional(),
	registrasiSource: z.enum(["Manual", "Online"]).optional(),
	namaPenanggungJawab: z.string().optional(),
	jabatan: z.string().optional(),
	bulanDimulai: z.string().optional(),
	basisKontrak: z.string().optional(),
	skemaHarga: z.string().optional(),
	segmentId: z.string().optional(),
	kodeHarga: z.string().optional(),
	hargaCurrency: z.string().optional(),
	hargaUnit: z.string().optional(),
	hargaNilai: z.string().optional(),
	capexAwal: z.string().optional(),
	momSigasTersedia: z.boolean().optional(),
	statusBangunan: z.string().optional(),
	sektor: z.string().optional(),
	produksiUtama: z.string().optional(),
	jenisPeralatanGas: z.string().optional(),
	tekananOperasiBarg: z.string().optional(),
	signatureMethod: z.string().optional(),
	usagePeriods: z.array(a1UsagePeriodSchema).optional(),
});

export type A1RegistrationFormValues = z.infer<typeof a1RegistrationSchema>;

// --- Stage 3: Permohonan NOL Schema ---
export const nolRequestPeriodSchema = z.object({
	id: z.string().optional(),
	periodeMulai: z.string().optional(),
	periodeSelesai: z.string().optional(),
	rataRata: z.union([z.number(), z.string()]).optional(),
	kontrakMinimum: z.union([z.number(), z.string()]).optional(),
	kontrakMaksimum: z.union([z.number(), z.string()]).optional(),
	sortOrder: z.number().optional(),
});

export const nolRequestDailySchema = z.object({
	id: z.string().optional(),
	hari: z.enum([
		"Monday",
		"Tuesday",
		"Wednesday",
		"Thursday",
		"Friday",
		"Saturday",
		"Sunday",
	]),
	min: z.union([z.number(), z.string()]).optional(),
	max: z.union([z.number(), z.string()]).optional(),
});

export const nolRequestSchema = z.object({
	nomorNotaDinas: z.string().optional(),
	registrationType: z
		.enum(["RegistrasiBaru", "Amendemen", "Perpanjangan"])
		.optional(),
	samaDenganA1: z.boolean().optional(),
	bulanDimulai: z.string().optional(),
	skemaHarga: z.string().optional(),
	hargaNilai: z.string().optional(),
	alasanKontrakBersyarat: z.string().optional(),
	namaPimpinanPerusahaan: z.string().optional(),
	jangkaWaktuKontrak: z.string().optional(),
	capexPreGr3: z.string().optional(),
	biayaPenyambunganReguler: z.string().optional(),
	biayaPenyambunganExtra: z.string().optional(),
	referenceDocumentIds: z.array(z.string()).optional(),
	periods: z.array(nolRequestPeriodSchema).optional(),
	dailyBasisRows: z.array(nolRequestDailySchema).optional(),
});

export type NolRequestFormValues = z.infer<typeof nolRequestSchema>;

// --- Stage 4: Evaluasi NOL Schema ---
export const nolEvaluationScenarioSchema = z.object({
	id: z.string().optional(),
	label: z.string(),
	irrPct: z.union([z.number(), z.string(), z.null()]).optional(),
	npv: z.union([z.number(), z.string(), z.null()]).optional(),
	paybackYears: z.union([z.number(), z.string(), z.null()]).optional(),
	hasilAnalisis: z.string().nullable().optional(),
});

export const nolEvaluationSchema = z.object({
	feedStatus: z.string().optional(),
	feedCompletedAt: z.string().optional(),
	statusRkap: z.string().optional(),
	pipaIndukPanjangM: z.string().optional(),
	pipaIndukDiameter: z.string().optional(),
	pipaIndukDiameterUnit: z.string().optional(),
	pipaServicePanjangM: z.string().optional(),
	pipaServiceDiameter: z.string().optional(),
	pipaServiceDiameterUnit: z.string().optional(),
	spesifikasiMrs: z.string().optional(),
	gSize: z.string().optional(),
	maksKapasitasMeterM3Jam: z.string().optional(),
	tekanan: z.string().optional(),
	maksFlowrate: z.string().optional(),
	skemaPembayaran: z.string().optional(),
	jaminanStatus: z.string().optional(),
	jaminanJenis: z.string().optional(),
	jaminanMasaBerlaku: z.string().optional(),
	jaminanPenerbit: z.string().optional(),
	ketersediaanPasokanBbtud: z.string().optional(),
	capexFinal: z.string().optional(),
	durasiPelaksanaanBulan: z.string().optional(),
	analisisKomersial: z.string().optional(),
	analisisKompetitor: z.string().optional(),
	radiusKompetitorKm: z.string().optional(),
	kesimpulan: z.string().optional(),
	scenarios: z.array(nolEvaluationScenarioSchema).optional(),
});

export type NolEvaluationFormValues = z.infer<typeof nolEvaluationSchema>;

// --- Stage 7: Penerbitan NOL Schema ---
export const nolIssuanceApprovedTermSchema = z.object({
	id: z.string().optional(),
	periodeMulai: z.string().optional(),
	periodeSelesai: z.string().optional(),
	rataRata: z.union([z.number(), z.string()]).optional(),
	kontrakMinimum: z.union([z.number(), z.string()]).optional(),
	kontrakMaksimum: z.union([z.number(), z.string()]).optional(),
	sortOrder: z.number().optional(),
});

export const nolIssuanceSchema = z.object({
	outcome: z.enum(["Nol", "Rl"]).optional(),
	nomorNotaDinas: z.string().optional(),
	berlakuSejak: z.string().optional(),
	berlakuSampai: z.string().optional(),
	kontrakBersyarat: z.array(z.string()).optional(),
	approvedTerms: z.array(nolIssuanceApprovedTermSchema).optional(),
});

export type NolIssuanceFormValues = z.infer<typeof nolIssuanceSchema>;
