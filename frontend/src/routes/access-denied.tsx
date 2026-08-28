import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { ArrowLeft, ShieldAlert } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardFooter,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { useAuth } from "@/lib/auth";

export const Route = createFileRoute("/access-denied")({
	component: AccessDeniedPage,
});

function AccessDeniedPage() {
	const { user } = useAuth();
	const navigate = useNavigate();

	return (
		<div className="min-h-screen flex items-center justify-center bg-muted/30 p-4">
			<Card className="w-full max-w-md text-center shadow-lg border-border/80">
				<CardHeader className="space-y-3 pb-4">
					<div className="mx-auto flex size-14 items-center justify-center rounded-full bg-destructive/10 text-destructive">
						<ShieldAlert className="size-8" />
					</div>
					<CardTitle className="text-xl font-bold">
						Akses Tidak Diizinkan
					</CardTitle>
					<CardDescription className="text-xs text-muted-foreground">
						Anda tidak memiliki hak akses atau scope otoritas yang cukup untuk
						membuka halaman ini.
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-3 text-xs text-left bg-muted/40 p-4 rounded-md mx-6 mb-2">
					<div className="flex justify-between">
						<span className="text-muted-foreground">Pengguna:</span>
						<span className="font-medium text-foreground">
							{user?.fullName || user?.username || "Tamu"}
						</span>
					</div>
					<div className="flex justify-between">
						<span className="text-muted-foreground">Peran (Role):</span>
						<span className="font-medium text-foreground">
							{user?.roles?.join(", ") || "Tidak ada"}
						</span>
					</div>
					<div className="flex justify-between">
						<span className="text-muted-foreground">Scope Wilayah:</span>
						<span className="font-medium text-foreground">
							{user?.scope || "-"}
						</span>
					</div>
				</CardContent>
				<CardFooter className="flex flex-col gap-2 pt-2">
					<Button
						className="w-full gap-2"
						onClick={() => navigate({ to: "/" })}
					>
						<ArrowLeft className="size-4" />
						Kembali ke Beranda
					</Button>
				</CardFooter>
			</Card>
		</div>
	);
}
