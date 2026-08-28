import { createFileRoute } from "@tanstack/react-router";
import { UsersView } from "@/components/admin/users-view";

export const Route = createFileRoute("/_auth/master/users")({
	component: UsersPage,
});

function UsersPage() {
	return <UsersView />;
}
