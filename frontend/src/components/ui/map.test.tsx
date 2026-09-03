import { act, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { getActiveTheme, useResolvedTheme } from "./map";

describe("Map Theme Resolution", () => {
	const originalMatchMedia = window.matchMedia;

	beforeEach(() => {
		document.documentElement.className = "";
		delete document.documentElement.dataset.theme;
	});

	afterEach(() => {
		document.documentElement.className = "";
		delete document.documentElement.dataset.theme;
		window.matchMedia = originalMatchMedia;
		vi.restoreAllMocks();
	});

	describe("getActiveTheme", () => {
		it("defaults to light theme when document has no class or light class", () => {
			expect(getActiveTheme()).toBe("light");

			document.documentElement.className = "light";
			expect(getActiveTheme()).toBe("light");
		});

		it("returns dark theme when document has dark class", () => {
			document.documentElement.className = "dark";
			expect(getActiveTheme()).toBe("dark");
		});

		it("returns dark theme when document has data-theme='dark'", () => {
			document.documentElement.dataset.theme = "dark";
			expect(getActiveTheme()).toBe("dark");
		});

		it("returns dark theme when element is inside a .dark container", () => {
			const container = document.createElement("div");
			container.className = "dark";
			const child = document.createElement("div");
			container.appendChild(child);
			document.body.appendChild(container);

			expect(getActiveTheme(child)).toBe("dark");

			document.body.removeChild(container);
		});

		it("does not turn dark solely based on system dark mode preference when app is in light mode", () => {
			window.matchMedia = vi.fn().mockImplementation((query: string) => ({
				matches: query.includes("prefers-color-scheme: dark"),
				media: query,
				onchange: null,
				addListener: vi.fn(),
				removeListener: vi.fn(),
				addEventListener: vi.fn(),
				removeEventListener: vi.fn(),
				dispatchEvent: vi.fn(),
			}));

			// Even with OS dark mode active, light mode app should keep map light
			expect(getActiveTheme()).toBe("light");
		});
	});

	describe("useResolvedTheme", () => {
		it("respects explicit theme prop over document theme", () => {
			document.documentElement.className = "dark";
			const { result: lightResult } = renderHook(() => useResolvedTheme("light"));
			expect(lightResult.current).toBe("light");

			document.documentElement.className = "light";
			const { result: darkResult } = renderHook(() => useResolvedTheme("dark"));
			expect(darkResult.current).toBe("dark");
		});

		it("defaults to light theme without explicit prop", () => {
			const { result } = renderHook(() => useResolvedTheme());
			expect(result.current).toBe("light");
		});

		it("updates dynamically when document class changes", async () => {
			const { result } = renderHook(() => useResolvedTheme());
			expect(result.current).toBe("light");

			act(() => {
				document.documentElement.className = "dark";
			});

			await waitFor(() => {
				expect(result.current).toBe("dark");
			});

			act(() => {
				document.documentElement.className = "light";
			});

			await waitFor(() => {
				expect(result.current).toBe("light");
			});
		});
	});
});
