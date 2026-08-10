namespace Pgn.Dms.Api.Data;

public static class SimandoRoles
{
    public const string SalesArea = "SalesArea";
    public const string AreaHead = "AreaHead";
    public const string RegionSales = "RegionSales";
    public const string Reviewer = "Reviewer";

    // Must stay in sync with Frontend/Data/SimandoRoles.cs — the seeder creates exactly these,
    // and a role missing here can never be assigned.
    public static readonly string[] All = [SalesArea, AreaHead, RegionSales, Reviewer];
}
