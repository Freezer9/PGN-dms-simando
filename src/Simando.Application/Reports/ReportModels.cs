namespace Simando.Application.Reports;

public sealed record FunnelStageRow(
    byte Stage,
    string StageName,
    int RecordCount,
    double ConversionRatePct,
    double AvgTurnaroundDays
);

public sealed record FunnelReportDto(
    IReadOnlyList<FunnelStageRow> Stages,
    int TotalRecords,
    double OverallConversionRatePct
);

public sealed record GasDemandByStageRow(
    byte Stage,
    string StageName,
    int RecordCount,
    decimal TotalDemandMMBtu
);

public sealed record GasDemandByRegionRow(
    string RegionName,
    int RecordCount,
    decimal TotalDemandMMBtu
);

public sealed record GasDemandByIndustryRow(
    string IndustryTypeName,
    int RecordCount,
    decimal TotalDemandMMBtu
);

public sealed record GasDemandReportDto(
    IReadOnlyList<GasDemandByStageRow> ByStage,
    IReadOnlyList<GasDemandByRegionRow> ByRegion,
    IReadOnlyList<GasDemandByIndustryRow> ByIndustry,
    decimal GrandTotalDemandMMBtu
);

public sealed record SurveyProductivityRow(
    Guid UserId,
    string SalesRepName,
    string AreaName,
    int Month,
    int Year,
    int SurveysCompletedCount,
    double AvgDaysPerSurvey
);

public sealed record SurveyProductivityReportDto(
    IReadOnlyList<SurveyProductivityRow> Rows,
    int TotalSurveysCompleted
);

public sealed record NolOutcomeReasonRow(
    string ReasonCategoryName,
    int Count,
    double Percentage
);

public sealed record NolOutcomesReportDto(
    int TotalEvaluated,
    int NolCount,
    int RlCount,
    double NolPercentage,
    double RlPercentage,
    IReadOnlyList<NolOutcomeReasonRow> RejectionReasons
);
