import {
	AlertOctagon,
	Building2,
	Calculator,
	ChartBar,
	CirclePause,
	Factory,
	FileText,
	Fuel,
	Gauge,
	Globe,
	Home,
	LifeBuoy,
	ListChecks,
	Map as MapIcon,
	MapPin,
	MapPinned,
	Network,
	Ruler,
	Settings2,
	ShieldAlert,
	Tags,
	Users,
} from "lucide-react";
import type * as React from "react";

const ICON_MAP: Record<
	string,
	React.ComponentType<{ className?: string; size?: number }>
> = {
	house: Home,
	"list-checks": ListChecks,
	"octagon-alert": AlertOctagon,
	"building-2": Building2,
	"map-pin": MapPin,
	map: MapIcon,
	calculator: Calculator,
	"bar-chart-3": ChartBar,
	users: Users,
	"shield-alert": ShieldAlert,
	network: Network,
	"map-pinned": MapPinned,
	globe: Globe,
	factory: Factory,
	tags: Tags,
	fuel: Fuel,
	ruler: Ruler,
	gauge: Gauge,
	"settings-2": Settings2,
	"file-text": FileText,
	"life-buoy": LifeBuoy,
	"octagon-pause": CirclePause,
};

export function DynamicIcon({
	name,
	className,
	size = 18,
}: {
	name: string;
	className?: string;
	size?: number;
}) {
	const IconComponent = ICON_MAP[name] ?? Home;
	return <IconComponent className={className} size={size} />;
}
