import type { components, paths } from "./schema";

export type CurrentUserDto = components["schemas"]["CurrentUserDto"];
export type AppRole = components["schemas"]["Role"];
export type AppCapability = components["schemas"]["Capability"];
export type AccessScope = components["schemas"]["AccessScope"];
export type LoginRequest = components["schemas"]["LoginRequest"];
export type ChangePasswordRequest =
	components["schemas"]["ChangePasswordRequest"];
export type ProblemDetails = components["schemas"]["ProblemDetails"];
export type Paths = paths;
