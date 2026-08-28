import { createFileRoute } from "@tanstack/react-router";
import { OrganisationView } from "@/components/admin/organisation-view";

export const Route = createFileRoute("/_auth/master/organisation")({
	component: OrganisationPage,
});

function OrganisationPage() {
	return <OrganisationView />;
}
