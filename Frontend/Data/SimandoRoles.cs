namespace Pgn.Dms.Web.Data;

public static class SimandoRoles
{
    public const string SalesArea = "SalesArea";
    public const string AreaHead = "AreaHead";
    public const string AdminRegional = "AdminRegional";
    public const string Reviewer = "Reviewer";
    public const string DivisionHead = "DivisionHead";
    public const string SystemAdmin = "SystemAdmin";

    public static readonly string[] All = [SalesArea, AreaHead, AdminRegional, Reviewer, DivisionHead, SystemAdmin];
    public static readonly string[] Operational = [SalesArea, AreaHead, AdminRegional, Reviewer, DivisionHead];
}

public static class SimandoPolicies
{
    public const string CanViewDashboard = "CanViewDashboard";
    public const string CanViewSubscriptions = "CanViewSubscriptions";

    public const string CanCreateSubscription = "CanCreateSubscription";
    public const string CanReview = "CanReview";
    public const string CanApprove = "CanApprove";
    public const string CanAccessEvaluation = "CanAccessEvaluation";
    public const string CanViewRegion = "CanViewRegion";
    public const string CanAdminister = "CanAdminister";

    public const string CanIssueNOL = "CanIssueNOL";
    public const string CanManageTasksBlocked = "CanManageTasksBlocked";
    public const string CanAccessBreakGlass = "CanAccessBreakGlass";
}
