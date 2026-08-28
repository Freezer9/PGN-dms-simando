import { describe, expect, it } from "vitest";
import { $api, fetchClient } from "./client";

describe("OpenAPI client ($api)", () => {
	it("initializes fetchClient with base config", () => {
		expect(fetchClient).toBeDefined();
		expect(typeof fetchClient.GET).toBe("function");
		expect(typeof fetchClient.POST).toBe("function");
	});

	it("initializes $api tanstack query integration", () => {
		expect($api).toBeDefined();
		expect(typeof $api.useQuery).toBe("function");
		expect(typeof $api.useMutation).toBe("function");
		expect(typeof $api.queryOptions).toBe("function");
	});

	it("creates valid queryOptions for /api/auth/me", () => {
		const options = $api.queryOptions("get", "/api/auth/me");
		expect(options).toBeDefined();
		expect(options.queryKey).toBeDefined();
		expect(Array.isArray(options.queryKey)).toBe(true);
	});
});
