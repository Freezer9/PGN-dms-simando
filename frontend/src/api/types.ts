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

export type Paths = paths;
