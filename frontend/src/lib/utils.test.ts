import { describe, expect, it } from "vitest";
import { cn } from "./utils";

describe("cn utility function", () => {
	it("merges class names correctly", () => {
		expect(cn("px-2 py-1", "bg-red-500")).toBe("px-2 py-1 bg-red-500");
	});

	it("resolves conflicting Tailwind class names", () => {
		expect(cn("p-4", "p-2")).toBe("p-2");
		expect(cn("text-red-500", "text-blue-500")).toBe("text-blue-500");
	});

	it("handles falsy and conditional values", () => {
		const isHidden = false;
		const isVisible = true;
		expect(cn("base-class", isHidden && "hidden", isVisible && "visible")).toBe(
			"base-class visible",
		);
	});
});
