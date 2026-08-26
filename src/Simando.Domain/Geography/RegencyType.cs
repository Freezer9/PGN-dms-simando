namespace Simando.Domain.Geography;

// Kota and Kabupaten are siblings at the same administrative level, not
// parent/child — see docs/domain/master-data.md §4 "regency means Kabupaten".
public enum RegencyType
{
    Kota,
    Kabupaten,
}
