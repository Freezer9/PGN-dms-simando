import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen } from "@testing-library/react";
import type * as React from "react";
import { describe, expect, it, vi } from "vitest";
import { NotificationDropdown } from "./notification-dropdown";

vi.mock("@tanstack/react-router", () => ({
	Link: ({
		children,
		to,
		...props
	}: {
		children: React.ReactNode;
		to?: string;
		[key: string]: unknown;
	}) => (
		<a href={typeof to === "string" ? to : "#"} {...props}>
			{children}
		</a>
	),
	useNavigate: () => vi.fn(),
}));

vi.mock("@/lib/auth", () => ({
	useAuth: () => ({
		user: {
			id: "user-1",
			username: "sales.user",
			fullName: "Sales Representative",
		},
	}),
}));

const mockUnreadQuery = vi.fn();
const mockNotificationsQuery = vi.fn();

vi.mock("@/api/client", () => ({
	$api: {
		useQuery: (_method: string, path: string) => {
			if (path === "/api/notifications/unread-count") {
				return mockUnreadQuery();
			}
			if (path === "/api/notifications") {
				return mockNotificationsQuery();
			}
			return { data: undefined, isLoading: false };
		},
		useMutation: () => ({
			mutate: vi.fn(),
			mutateAsync: vi.fn(),
			isPending: false,
		}),
	},
}));

function renderWithClient(ui: React.ReactElement) {
	const queryClient = new QueryClient({
		defaultOptions: {
			queries: { retry: false },
		},
	});
	return render(
		<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
	);
}

describe("NotificationDropdown", () => {
	it("renders bell button without badge when unread count is 0", () => {
		mockUnreadQuery.mockReturnValue({
			data: { unreadCount: 0 },
			isLoading: false,
		});
		mockNotificationsQuery.mockReturnValue({
			data: [],
			isLoading: false,
		});

		renderWithClient(<NotificationDropdown />);

		const bellBtn = screen.getByRole("button", { name: /notifikasi/i });
		expect(bellBtn).toBeInTheDocument();
		expect(screen.queryByText("0")).not.toBeInTheDocument();
	});

	it("renders unread badge when unread count > 0", () => {
		mockUnreadQuery.mockReturnValue({
			data: { unreadCount: 3 },
			isLoading: false,
		});
		mockNotificationsQuery.mockReturnValue({
			data: [],
			isLoading: false,
		});

		renderWithClient(<NotificationDropdown />);

		expect(screen.getByText("3")).toBeInTheDocument();
	});

	it("opens popover with notification items on click", () => {
		mockUnreadQuery.mockReturnValue({
			data: { unreadCount: 1 },
			isLoading: false,
		});
		mockNotificationsQuery.mockReturnValue({
			data: [
				{
					id: "notif-1",
					companyId: "comp-1",
					companyNomor: "1-35-001",
					companyName: "PT Keramik Sejahtera",
					message: "Permohonan NOL menunggu review Anda",
					createdAt: new Date().toISOString(),
					readAt: null,
				},
			],
			isLoading: false,
		});

		renderWithClient(<NotificationDropdown />);

		const bellBtn = screen.getByRole("button", { name: /1/i });
		fireEvent.click(bellBtn);

		expect(screen.getByText("PT Keramik Sejahtera")).toBeInTheDocument();
		expect(
			screen.getByText("Permohonan NOL menunggu review Anda"),
		).toBeInTheDocument();
		expect(screen.getByText("1-35-001")).toBeInTheDocument();
	});
});
