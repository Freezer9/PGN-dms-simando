namespace Simando.Domain.Survey;

// Shared by survey_raw_material and survey_market — same shape, different
// meaning (sourced from vs. destined to). docs/design/data-model.md#survey--stage-4-kk0-header.
public enum Asal
{
    Impor,
    Lokal,
}
