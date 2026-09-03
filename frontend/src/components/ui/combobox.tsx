import { Check, ChevronsUpDown } from "lucide-react";
import * as React from "react";
import { Button } from "@/components/ui/button";
import {
	Command,
	CommandEmpty,
	CommandGroup,
	CommandInput,
	CommandItem,
	CommandList,
} from "@/components/ui/command";
import {
	Popover,
	PopoverContent,
	PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";

export interface ComboboxOption {
	value: string;
	label: string;
}

export interface ComboboxProps {
	options: ComboboxOption[];
	value?: string;
	onValueChange?: (value: string) => void;
	placeholder?: string;
	searchPlaceholder?: string;
	emptyText?: string;
	disabled?: boolean;
	className?: string;
	id?: string;
	"aria-label"?: string;
}

export function Combobox({
	options,
	value,
	onValueChange,
	placeholder = "Pilih...",
	searchPlaceholder = "Cari...",
	emptyText = "Tidak ada hasil ditemukan.",
	disabled = false,
	className,
	id,
	"aria-label": ariaLabel,
}: ComboboxProps) {
	const [open, setOpen] = React.useState(false);

	const selectedOption = React.useMemo(
		() => options.find((opt) => opt.value === value),
		[options, value],
	);

	return (
		<Popover open={open} onOpenChange={setOpen}>
			<PopoverTrigger asChild>
				<Button
					id={id}
					type="button"
					variant="outline"
					role="combobox"
					aria-expanded={open}
					aria-label={ariaLabel || placeholder}
					disabled={disabled}
					className={cn(
						"w-full h-9 justify-between font-normal text-xs bg-transparent dark:bg-input/30",
						!selectedOption && "text-muted-foreground",
						className,
					)}
				>
					<span className="truncate">
						{selectedOption ? selectedOption.label : placeholder}
					</span>
					<ChevronsUpDown className="ml-2 size-3.5 shrink-0 opacity-50" />
				</Button>
			</PopoverTrigger>
			<PopoverContent
				className="w-[var(--radix-popper-anchor-width,var(--radix-popover-trigger-width))] min-w-[200px] p-0 shadow-md"
				align="start"
			>
				<Command>
					<CommandInput placeholder={searchPlaceholder} className="h-8 text-xs" />
					<CommandList className="max-h-60 overflow-y-auto">
						<CommandEmpty className="py-4 text-center text-xs text-muted-foreground">
							{emptyText}
						</CommandEmpty>
						<CommandGroup>
							{options.map((option) => (
								<CommandItem
									key={option.value}
									value={`${option.label} ${option.value}`}
									onSelect={() => {
										onValueChange?.(option.value === value ? "" : option.value);
										setOpen(false);
									}}
									className="text-xs cursor-pointer"
								>
									<span className="truncate flex-1">{option.label}</span>
									<Check
										className={cn(
											"ml-auto size-3.5",
											value === option.value ? "opacity-100" : "opacity-0",
										)}
									/>
								</CommandItem>
							))}
						</CommandGroup>
					</CommandList>
				</Command>
			</PopoverContent>
		</Popover>
	);
}
