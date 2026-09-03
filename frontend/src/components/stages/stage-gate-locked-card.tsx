import { AlertCircle, ArrowRight, Lock, Paperclip } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import type { StageGateResult } from "@/lib/stage-gates";

interface StageGateLockedCardProps {
	stageTitle: string;
	gate: StageGateResult;
	onNavigateToTab?: (tabKey: string) => void;
	onOpenUploadDialog?: () => void;
}

export function StageGateLockedCard({
	stageTitle,
	gate,
	onNavigateToTab,
	onOpenUploadDialog,
}: StageGateLockedCardProps) {
	const needsAttachments = gate.missingRequirements.some(
		(r) =>
			r.toLowerCase().includes("unggah") || r.toLowerCase().includes("dokumen"),
	);

	return (
		<Card className="border-amber-500/40 bg-amber-50/50 dark:bg-amber-950/20 shadow-xs">
			<CardHeader className="pb-3">
				<div className="flex items-center gap-2 text-amber-700 dark:text-amber-400">
					<Lock className="size-5 shrink-0" />
					<CardTitle className="text-base font-semibold">
						{stageTitle} Belum Terbuka
					</CardTitle>
				</div>
				<CardDescription className="text-xs text-muted-foreground mt-1">
					{gate.reason ||
						"Langkah alur kerja ini terkunci karena prasyarat tahap sebelumnya belum terpenuhi."}
				</CardDescription>
			</CardHeader>
			<CardContent className="space-y-4">
				{gate.missingRequirements.length > 0 && (
					<div className="rounded-lg border border-amber-500/30 bg-background/80 p-3 space-y-2">
						<span className="text-xs font-semibold text-foreground flex items-center gap-1.5">
							<AlertCircle className="size-3.5 text-amber-600" />
							Prasyarat yang wajib diselesaikan terlebih dahulu:
						</span>
						<ul className="list-disc list-inside text-xs text-muted-foreground space-y-1 pl-1">
							{gate.missingRequirements.map((req) => (
								<li key={req} className="leading-relaxed">
									{req}
								</li>
							))}
						</ul>
					</div>
				)}

				<div className="flex flex-wrap items-center gap-2 pt-1">
					{needsAttachments && onOpenUploadDialog && (
						<Button
							size="sm"
							onClick={onOpenUploadDialog}
							className="h-8 text-xs bg-amber-600 hover:bg-amber-700 text-white flex items-center gap-1.5"
						>
							<Paperclip className="size-3.5" /> Unggah Dokumen Prasyarat
						</Button>
					)}

					{needsAttachments && onNavigateToTab && (
						<Button
							variant="outline"
							size="sm"
							onClick={() => onNavigateToTab("attachments")}
							className="h-8 text-xs flex items-center gap-1.5 border-amber-300 dark:border-amber-700"
						>
							Buka Tab Lampiran <ArrowRight className="size-3" />
						</Button>
					)}

					{gate.stage === 3 && onNavigateToTab && (
						<Button
							size="sm"
							onClick={() => onNavigateToTab("plotting")}
							className="h-8 text-xs flex items-center gap-1.5"
						>
							Buka Konfigurasi Plotting <ArrowRight className="size-3" />
						</Button>
					)}

					{gate.stage === 4 && onNavigateToTab && (
						<Button
							size="sm"
							onClick={() => onNavigateToTab("contacts")}
							className="h-8 text-xs flex items-center gap-1.5"
						>
							Kelola Kontak Prospek <ArrowRight className="size-3" />
						</Button>
					)}
				</div>
			</CardContent>
		</Card>
	);
}
