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

// --- Directory / Company Creation Schema ---
export const createCompanySchema = z.object({
	namaPerusahaan: z.string().min(1, "Nama perusahaan wajib diisi"),
	industryTypeId: z.string().min(1, "Sektor industri wajib dipilih"),
	areaId: z.string().min(1, "Wilayah area kerja PGN wajib dipilih"),
	provinceId: z.string().min(1, "Provinsi wajib dipilih"),
	regencyId: z.string().min(1, "Kota/Kabupaten wajib dipilih"),
	districtId: z.string().min(1, "Kecamatan wajib dipilih"),
	villageId: z
		.string()
		.min(
			1,
			"Hierarki Lokasi Administratif (Kelurahan/Desa) wajib dipilih lengkap",
		),
	alamat: z.string().min(1, "Alamat lengkap wajib diisi"),
	kodePos: z.string().optional(),
	npwp: z.string().optional(),
	email: z
		.string()
		.email("Format email tidak valid")
		.or(z.literal(""))
		.optional(),
	telp: z.string().optional(),
	website: z.string().optional(),
	latitude: z.number(),
	longitude: z.number(),
});

export type CreateCompanyFormValues = z.infer<typeof createCompanySchema>;

// --- Contact Form Schema ---
export const saveContactSchema = z.object({
	nama: z.string().min(1, "Nama kontak wajib diisi"),
	jabatan: z.string().optional(),
	email: z
		.string()
		.email("Format email tidak valid")
		.or(z.literal(""))
		.optional(),
	noHp: z.string().optional(),
	isPrimary: z.boolean().optional(),
});

export type SaveContactFormValues = z.infer<typeof saveContactSchema>;

// --- Plotting Form Schema ---
export const savePlottingSchema = z.object({
	salesUserId: z.string().min(1, "Sales Representative wajib dipilih"),
	posisiPelanggan: z.string().optional(),
	kawasan: z.string().optional(),
});

export type SavePlottingFormValues = z.infer<typeof savePlottingSchema>;

// --- Admin: User Creation Schema ---
export const createUserSchema = z.object({
	fullName: z.string().min(1, "Nama lengkap wajib diisi"),
	username: z.string().min(1, "Nama pengguna (username) wajib diisi"),
	email: z
		.string()
		.email("Format email tidak valid")
		.or(z.literal(""))
		.optional(),
	role: z.string().min(1, "Peran awal wajib dipilih"),
	regionId: z.string().optional(),
	areaId: z.string().optional(),
});

export type CreateUserFormValues = z.infer<typeof createUserSchema>;

// --- Admin: Role Assignment Schema ---
export const assignRoleSchema = z.object({
	role: z.string().min(1, "Peran wajib dipilih"),
	regionId: z.string().optional(),
	areaId: z.string().optional(),
});

export type AssignRoleFormValues = z.infer<typeof assignRoleSchema>;

// --- Admin: Break Glass Request Schema ---
export const breakGlassRequestSchema = z.object({
	companyId: z.string().min(1, "Silakan pilih perusahaan terlebih dahulu"),
	reason: z.string().min(1, "Alasan akses darurat wajib diisi secara spesifik"),
});

export type BreakGlassRequestFormValues = z.infer<
	typeof breakGlassRequestSchema
>;

// --- Admin: Stuck Step Reassign Schema ---
export const reassignStuckStepSchema = z.object({
	targetUserId: z.string().min(1, "Silakan pilih pengguna tujuan pengalihan"),
});

export type ReassignStuckStepFormValues = z.infer<
	typeof reassignStuckStepSchema
>;

// --- Task Action Modal Schema ---
export const taskActionModalSchema = z.object({
	newUserId: z.string().optional(),
	comment: z.string().optional(),
});

export type TaskActionModalFormValues = z.infer<typeof taskActionModalSchema>;

// --- Workflow Action Bar Schema ---
export const workflowActionBarSchema = z.object({
	comment: z.string().optional(),
});

export type WorkflowActionBarFormValues = z.infer<
	typeof workflowActionBarSchema
>;

// --- Attachment Upload Schema ---
export const attachmentUploadSchema = z.object({
	kind: z.string().min(1, "Jenis dokumen wajib dipilih"),
	signatureMethod: z.string().optional(),
});

export type AttachmentUploadFormValues = z.infer<typeof attachmentUploadSchema>;
