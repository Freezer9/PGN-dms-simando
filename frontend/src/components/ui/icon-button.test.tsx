import { render, screen } from "@testing-library/react";
import { Trash2 } from "lucide-react";
import { describe, expect, it } from "vitest";
import { IconButton } from "./icon-button";
import { TooltipProvider } from "./tooltip";

describe("IconButton component", () => {
	it("renders button with default outline variant and tooltip label as aria-label", () => {
		render(
			<TooltipProvider>
				<IconButton tooltip="Edit item">
					<Trash2 data-testid="icon" />
				</IconButton>
			</TooltipProvider>,
		);

		const button = screen.getByRole("button", { name: "Edit item" });
		expect(button).toBeInTheDocument();
		expect(button.getAttribute("data-variant")).toBe("outline");
		expect(button.getAttribute("data-size")).toBe("icon-sm");
	});

	it("applies danger styling when danger prop is true", () => {
		render(
			<TooltipProvider>
				<IconButton tooltip="Hapus data" danger>
					<Trash2 />
				</IconButton>
			</TooltipProvider>,
		);

		const button = screen.getByRole("button", { name: "Hapus data" });
		expect(button.className).toContain("text-destructive");
		expect(button.className).toContain("hover:bg-destructive/10");
	});

	it("renders without tooltip when tooltip is omitted", () => {
		render(
			<IconButton aria-label="Standalone">
				<Trash2 />
			</IconButton>,
		);

		const button = screen.getByRole("button", { name: "Standalone" });
		expect(button).toBeInTheDocument();
	});

	it("supports custom sizes and variants", () => {
		render(
			<TooltipProvider>
				<IconButton
					tooltip="Custom"
					size="xs"
					variant="ghost"
					className="custom-class"
				>
					<Trash2 />
				</IconButton>
			</TooltipProvider>,
		);

		const button = screen.getByRole("button", { name: "Custom" });
		expect(button.getAttribute("data-variant")).toBe("ghost");
		expect(button.getAttribute("data-size")).toBe("xs");
		expect(button.className).toContain("custom-class");
	});

	it("supports asChild delegation", () => {
		render(
			<TooltipProvider>
				<IconButton tooltip="Buka Link" asChild>
					<a href="/test">Link Icon</a>
				</IconButton>
			</TooltipProvider>,
		);

		const link = screen.getByRole("link", { name: "Buka Link" });
		expect(link).toBeInTheDocument();
		expect(link.getAttribute("href")).toBe("/test");
	});
});
