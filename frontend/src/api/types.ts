import type { components, paths } from "./schema";

export type CurrentUserDto = components["schemas"]["CurrentUserDto"];
export type AppRole = components["schemas"]["Role"];
export type AppCapability = components["schemas"]["Capability"];
export type AccessScope = components["schemas"]["AccessScope"];
export type LoginRequest = components["schemas"]["LoginRequest"];
export type ChangePasswordRequest =
	components["schemas"]["ChangePasswordRequest"];
export type ProblemDetails = components["schemas"]["ProblemDetails"];

// Epic 4: Directory, Company Hub, Plotting, Geography, Master Data & Map
export type CompanyListItem = components["schemas"]["CompanyListItem"];
export type PagedResultOfCompanyListItem =
	components["schemas"]["PagedResultOfCompanyListItem"];
export type CompanyRecordDto = components["schemas"]["CompanyRecordDto"];
export type CompanyMapPinDto = components["schemas"]["CompanyMapPinDto"];
export type CreateCompanyRequest =
	components["schemas"]["CreateCompanyRequest"];
export type CreateCompanyResult = components["schemas"]["CreateCompanyResult"];
export type UpdateCompanyRequest =
	components["schemas"]["UpdateCompanyRequest"];
export type UpdateLocationRequest =
	components["schemas"]["UpdateLocationRequest"];

export type ContactDetail = components["schemas"]["ContactDetail"];
export type SaveContactRequest = components["schemas"]["SaveContactRequest"];

export type PlottingDetail = components["schemas"]["PlottingDetail"];
export type SavePlottingRequest = components["schemas"]["SavePlottingRequest"];
export type PosisiPelanggan = components["schemas"]["PosisiPelanggan"];
export type Kawasan = components["schemas"]["Kawasan"];
export type RecordStatus = components["schemas"]["RecordStatus"];
export type WorkflowStepKind = components["schemas"]["WorkflowStepKind"];
export type StatusEventAction = components["schemas"]["StatusEventAction"];
export type TimelineEntry = components["schemas"]["TimelineEntry"];

export type GeographyOption = components["schemas"]["GeographyOption"];
export type IndustryTypeDto = components["schemas"]["IndustryTypeDto"];
export type AreaDto = components["schemas"]["AreaDto"];
export type SalesUserDto = components["schemas"]["SalesUserDto"];

// Epic 5: Stage 4 - KK0 Survey Form
export type SurveyDetail = components["schemas"]["SurveyDetail"];
export type SaveSurveyRequest = components["schemas"]["SaveSurveyRequest"];
export type SaveSurveyFullPayload =
	components["schemas"]["SaveSurveyFullPayload"];
export type SurveyProductDetail = components["schemas"]["SurveyProductDetail"];
export type SaveSurveyProductRequest =
	components["schemas"]["SaveSurveyProductRequest"];
export type SurveyRawMaterialDetail =
	components["schemas"]["SurveyRawMaterialDetail"];
export type SaveSurveyRawMaterialRequest =
	components["schemas"]["SaveSurveyRawMaterialRequest"];
export type SurveyMarketDetail = components["schemas"]["SurveyMarketDetail"];
export type SaveSurveyMarketRequest =
	components["schemas"]["SaveSurveyMarketRequest"];
export type SurveyEquipmentDetail =
	components["schemas"]["SurveyEquipmentDetail"];
export type SaveSurveyEquipmentRequest =
	components["schemas"]["SaveSurveyEquipmentRequest"];
export type KebutuhanEnergiJenis =
	components["schemas"]["KebutuhanEnergiJenis"];
export type BahanBakarEksisting = components["schemas"]["BahanBakarEksisting"];
export type RencanaPemanfaatanGas =
	components["schemas"]["RencanaPemanfaatanGas"];
export type Asal = components["schemas"]["Asal"];

// Epic 5: Stage 5 - A1 Registration Form
export type A1RegistrationDetail =
	components["schemas"]["A1RegistrationDetail"];
export type SaveA1RegistrationRequest =
	components["schemas"]["SaveA1RegistrationRequest"];
export type A1UsagePeriodDetail = components["schemas"]["A1UsagePeriodDetail"];
export type RegistrasiSource = components["schemas"]["RegistrasiSource"];
export type BasisKontrak = components["schemas"]["BasisKontrak"];
export type SkemaHarga = components["schemas"]["SkemaHarga"];
export type StatusBangunan = components["schemas"]["StatusBangunan"];
export type Sektor = components["schemas"]["Sektor"];
export type HargaCurrency = components["schemas"]["HargaCurrency"];
export type HargaUnit = components["schemas"]["HargaUnit"];
export type SignatureMethod = components["schemas"]["SignatureMethod"];

// Epic 5: Stage 6 - Permohonan NOL Form
export type NolRequestDetail = components["schemas"]["NolRequestDetail"];
export type SaveNolRequestRequest =
	components["schemas"]["SaveNolRequestRequest"];
export type NolRequestPeriodDetail =
	components["schemas"]["NolRequestPeriodDetail"];
export type NolRequestDailyDetail =
	components["schemas"]["NolRequestDailyDetail"];
export type RegistrationType = components["schemas"]["RegistrationType"];

// Epic 5: Stage 7 - Evaluasi NOL Form
export type NolEvaluationDetail = components["schemas"]["NolEvaluationDetail"];
export type SaveNolEvaluationRequest =
	components["schemas"]["SaveNolEvaluationRequest"];
export type NolEvaluationScenarioDetail =
	components["schemas"]["NolEvaluationScenarioDetail"];
export type FeedStatus = components["schemas"]["FeedStatus"];
export type DiameterUnit = components["schemas"]["DiameterUnit"];
export type StatusRkap = components["schemas"]["StatusRkap"];
export type SkemaPembayaran = components["schemas"]["SkemaPembayaran"];

// Epic 5: Stage 8 - Penerbitan Surat NOL Form
export type NolIssuanceDetail = components["schemas"]["NolIssuanceDetail"];
export type SaveNolIssuanceRequest =
	components["schemas"]["SaveNolIssuanceRequest"];
export type NolIssuanceApprovedTermDetail =
	components["schemas"]["NolIssuanceApprovedTermDetail"];
export type NolOutcome = components["schemas"]["NolOutcome"];

// Epic 5: Workflow Actions & Submissions
export type SubmitResult = components["schemas"]["SubmitResult"];
export type ChooseReviewersRequest =
	components["schemas"]["ChooseReviewersRequest"];
export type ActOnStepRequest = components["schemas"]["ActOnStepRequest"];
export type ReworkRequest = components["schemas"]["ReworkRequest"];
export type DiscontinueRequest = components["schemas"]["DiscontinueRequest"];
export type WorkflowAction = components["schemas"]["WorkflowAction"];

// Epic 5: Master Data Lookups
export type FuelTypeDto = components["schemas"]["FuelTypeDto"];
export type UnitOfMeasureDto = components["schemas"]["UnitOfMeasureDto"];
export type CountryDto = components["schemas"]["CountryDto"];
export type SegmentDto = components["schemas"]["SegmentDto"];
export type ReferenceDocumentDto =
	components["schemas"]["ReferenceDocumentDto"];
export type MrsSpecDto = components["schemas"]["MrsSpecDto"];
export type MeterSizeDto = components["schemas"]["MeterSizeDto"];
export type ReviewerOptionDto = components["schemas"]["ReviewerOptionDto"];

export type Paths = paths;
