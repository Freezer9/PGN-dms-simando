import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Button } from "./button";

describe("Button component", () => {
	it("renders correctly with default props", () => {
		render(<Button>Click me</Button>);
		const button = screen.getByRole("button", { name: /click me/i });
		expect(button).toBeInTheDocument();
	});

	it("applies variant and size classes", () => {
		render(
			<Button variant="destructive" size="sm">
				Delete
			</Button>,
		);
		const button = screen.getByRole("button", { name: /delete/i });
		expect(button).toBeInTheDocument();
		expect(button.className).toContain("bg-destructive");
	});
});
