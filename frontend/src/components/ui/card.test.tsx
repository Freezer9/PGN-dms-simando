import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import {
	Card,
	CardContent,
	CardDescription,
	CardFooter,
	CardHeader,
	CardTitle,
} from "./card";

describe("Card component", () => {
	it("renders Card container without hardcoded py-6 or gap-6", () => {
		render(<Card data-testid="card">Card Body</Card>);
		const card = screen.getByTestId("card");
		expect(card).toBeInTheDocument();
		expect(card.className).toContain("rounded-xl");
		expect(card.className).toContain("border");
		expect(card.className).toContain("bg-card");
		// Ensure hardcoded vertical padding and gap from modern experimental variant are absent
		expect(card.className).not.toContain("py-6");
		expect(card.className).not.toContain("gap-6");
	});

	it("allows custom padding on Card without vertical gap interference", () => {
		render(
			<Card data-testid="card-compact" className="p-4">
				Compact Content
			</Card>,
		);
		const card = screen.getByTestId("card-compact");
		expect(card.className).toContain("p-4");
	});

	it("renders CardHeader with standard p-6 and space-y-1.5", () => {
		render(
			<CardHeader data-testid="card-header">
				<CardTitle>Title</CardTitle>
				<CardDescription>Description</CardDescription>
			</CardHeader>,
		);
		const header = screen.getByTestId("card-header");
		expect(header.className).toContain("p-6");
		expect(header.className).toContain("space-y-1.5");
	});

	it("renders CardContent with p-6 pt-0", () => {
		render(<CardContent data-testid="card-content">Content body</CardContent>);
		const content = screen.getByTestId("card-content");
		expect(content.className).toContain("p-6");
		expect(content.className).toContain("pt-0");
	});

	it("renders CardFooter with flex items-center p-6 pt-0", () => {
		render(<CardFooter data-testid="card-footer">Footer action</CardFooter>);
		const footer = screen.getByTestId("card-footer");
		expect(footer.className).toContain("p-6");
		expect(footer.className).toContain("pt-0");
		expect(footer.className).toContain("flex");
		expect(footer.className).toContain("items-center");
	});
});
