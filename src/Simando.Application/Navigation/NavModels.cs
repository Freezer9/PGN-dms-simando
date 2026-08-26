namespace Simando.Application.Navigation;

// The sidebar's rendered shape — docs/design/frontend/01-shell-and-navigation.md
// "Sidebar per role". A NavItem is a plain route; a NavGroup renders as a
// collapsible submenu (System Admin's "Wilayah & Referensi ▾" etc.). A
// NavSection is one visually distinct block — the doc's "─────────────"
// divider separates sections; there is no divider *within* a section.
public abstract record NavNode;

public sealed record NavItem(string Label, string Route, string Icon, string? Badge = null) : NavNode;

public sealed record NavGroup(string Label, string Icon, IReadOnlyList<NavItem> Children) : NavNode;

public sealed record NavSection(IReadOnlyList<NavNode> Nodes);

public sealed record NavMenu(IReadOnlyList<NavSection> Sections);
