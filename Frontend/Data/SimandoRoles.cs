namespace Pgn.Dms.Web.Data;

public static class SimandoRoles
{
    public const string SalesArea = "SalesArea";
    public const string AreaHead = "AreaHead";
    public const string RegionSales = "RegionSales";
    public const string Reviewer = "Reviewer";

    public static readonly string[] All = [SalesArea, AreaHead, RegionSales, Reviewer];

    /// <summary>Every role is operational — SIMANDO has no non-case-handling role.</summary>
    public static readonly string[] Operational = All;
}

public static class SimandoPolicies
{
    // One policy per role tree. The sidebar and every page under a role's folder gate on
    // these, so a role's whole section is granted or denied in one place.
    public const string IsSalesArea = "IsSalesArea";
    public const string IsAreaHead = "IsAreaHead";
    public const string IsRegionSales = "IsRegionSales";
    public const string IsReviewer = "IsReviewer";

    /// <summary>Any signed-in role. Used by the shared per-company record pages.</summary>
    public const string CanViewSubscriptions = "CanViewSubscriptions";

    /// <summary>Roles that may edit stage data: Sales Area owns 1–6, Region Sales owns 7.</summary>
    public const string CanEditRecord = "CanEditRecord";

    /// <summary>Region Sales owns the evaluation stage.</summary>
    public const string CanAccessEvaluation = "CanAccessEvaluation";

    /// <summary>Region Sales and Reviewer share the QA/QC section.</summary>
    public const string CanAccessQaQc = "CanAccessQaQc";

    /// <summary>Roles that act on the approval chain.</summary>
    public const string CanApprove = "CanApprove";
}
