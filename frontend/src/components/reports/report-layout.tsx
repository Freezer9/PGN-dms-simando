import { FileSpreadsheet, RotateCcw } from "lucide-react";
import * as React from "react";
import { PageHeader } from "@/components/common";
import { Button } from "@/components/ui/button";

interface ReportLayoutProps {
	title: string;
	description: string;
	exportEndpoint: string;
	exportFileName: string;
	children: React.ReactNode;
	filterContent?: React.ReactNode;
	onResetFilters?: () => void;
}

export function ReportLayout({
	title,
	description,
	exportEndpoint,
	exportFileName,
	children,
	filterContent,
	onResetFilters,
}: ReportLayoutProps) {
	const [isExporting, setIsExporting] = React.useState(false);

	const handleExportExcel = async () => {
		try {
			setIsExporting(true);
			const response = await fetch(exportEndpoint, {
				credentials: "include",
			});
			if (!response.ok) {
				throw new Error("Gagal mengunduh file Excel.");
			}
			const blob = await response.blob();
			const url = window.URL.createObjectURL(blob);
			const link = document.createElement("a");
			link.href = url;
			link.download = exportFileName;
			document.body.appendChild(link);
			link.click();
			document.body.removeChild(link);
			window.URL.revokeObjectURL(url);
		} catch (error) {
			console.error("Excel download error:", error);
		} finally {
			setIsExporting(false);
		}
	};

	return (
		<div className="space-y-6">
			{/* Page Header */}
			<PageHeader
				title={title}
				description={description}
				actions={
					<div className="flex items-center gap-2.5 shrink-0">
						{onResetFilters && (
							<Button
								variant="outline"
								size="sm"
								onClick={onResetFilters}
								className="gap-1.5 h-9 text-xs"
							>
								<RotateCcw className="size-3.5" />
								Reset Filter
							</Button>
						)}
						<Button
							onClick={handleExportExcel}
							disabled={isExporting}
							size="sm"
							className="gap-1.5 h-9 bg-emerald-700 hover:bg-emerald-800 text-white shadow-xs text-xs"
						>
							<FileSpreadsheet className="size-4" />
							{isExporting ? "Mengunduh..." : "Unduh Excel (.xlsx)"}
						</Button>
					</div>
				}
			/>

			{/* Filter Toolbar (if any) */}
			{filterContent && (
				<div className="p-4 rounded-lg bg-card border shadow-xs space-y-3">
					{filterContent}
				</div>
			)}

			{/* Report Content */}
			<div>{children}</div>
		</div>
	);
}
