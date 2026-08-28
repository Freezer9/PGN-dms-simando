import { createFileRoute } from "@tanstack/react-router";
import { UsersView } from "@/components/admin/users-view";

export const Route = createFileRoute("/_auth/admin/users")({
	component: UsersPage,
});

function UsersPage() {
	return <UsersView />;
}
