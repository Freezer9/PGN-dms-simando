import { Check } from "lucide-react";
import { STAGE_CONFIG } from "@/lib/directory-utils";
import { cn } from "@/lib/utils";

interface StageStepperProps {
	currentStage: number;
	activeTab?: string;
	onSelectStage?: (stageNumber: number, tabKey: string) => void;
	className?: string;
}

const STAGE_TAB_MAP: Record<number, string> = {
	1: "overview",
	2: "plotting",
	3: "contacts",
	4: "survey",
	5: "registration",
	6: "nol-req",
	7: "nol-eval",
	8: "nol-issue",
};

export function StageStepper({
	currentStage,
	activeTab,
	onSelectStage,
	className,
}: StageStepperProps) {
	const stages = Object.values(STAGE_CONFIG);

	return (
		<div className={cn("w-full py-1", className)} data-slot="stage-stepper">
			<div className="w-full px-1 py-2">
				<ol className="grid grid-cols-8 gap-0 w-full items-start">
					{stages.map((st, index) => {
						const isCompleted = currentStage > st.stage;
						const isCurrent = currentStage === st.stage;
						const isUpcoming = currentStage < st.stage;
						const isTabActive = activeTab === STAGE_TAB_MAP[st.stage];
						const isLast = index === stages.length - 1;

						return (
							<li
								key={st.stage}
								className="relative flex flex-col items-center group w-full"
							>
								{/* Connecting Line to next stage */}
								{!isLast && (
									<div
										className="absolute top-4 left-1/2 w-full h-0.5 -translate-y-1/2 z-0"
										aria-hidden="true"
									>
										<div
											className={cn(
												"h-full w-full",
												isCompleted
													? "bg-emerald-600 dark:bg-emerald-500"
													: isCurrent
														? "bg-gradient-to-r from-primary to-border"
														: "bg-border",
											)}
										/>
									</div>
								)}

								{/* Step Node Button */}
								<button
									type="button"
									onClick={() =>
										onSelectStage?.(st.stage, STAGE_TAB_MAP[st.stage])
									}
									className="relative z-10 flex flex-col items-center focus:outline-hidden group/btn text-center w-full px-1"
									title={`Buka ${st.name}`}
								>
									{/* Circle Indicator */}
									<div
										style={{ borderRadius: "9999px" }}
										className={cn(
											"h-8 w-8 rounded-full shrink-0 flex items-center justify-center text-xs font-semibold transition-all duration-200 cursor-pointer shadow-xs",
											isCompleted &&
												"bg-emerald-600 text-white hover:bg-emerald-700",
											isCurrent &&
												"bg-primary text-primary-foreground ring-4 ring-primary/25 font-bold scale-105",
											isUpcoming &&
												"bg-background text-muted-foreground border-2 border-border/90 hover:border-primary/50 hover:text-foreground",
											isTabActive &&
												!isCurrent &&
												"ring-2 ring-primary/60",
										)}
									>
										{isCompleted ? (
											<Check className="size-4 stroke-[2.5]" />
										) : (
											<span className="font-mono text-xs">{st.stage}</span>
										)}
									</div>

									{/* Label Under Node */}
									<div className="mt-2 text-center w-full max-w-[100px] leading-tight">
										<p
											className={cn(
												"text-[11px] font-medium truncate transition-colors",
												isCurrent
													? "text-primary font-bold"
													: isCompleted
														? "text-foreground font-semibold"
														: "text-muted-foreground group-hover/btn:text-foreground",
											)}
										>
											{st.shortName}
										</p>
									</div>
								</button>
							</li>
						);
					})}
				</ol>
			</div>
		</div>
	);
}
