import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { FormField } from "./form-field";

describe("FormField component", () => {
	it("renders label and required indicator", () => {
		render(
			<FormField label="Email" required>
				<input type="email" id="email" />
			</FormField>,
		);
		expect(screen.getByText("Email")).toBeInTheDocument();
		expect(screen.getByText("*")).toBeInTheDocument();
	});

	it("renders error message when present", () => {
		render(
			<FormField label="Email" error="Invalid email address">
				<input type="email" id="email" />
			</FormField>,
		);
		expect(screen.getByText("Invalid email address")).toBeInTheDocument();
	});
});
