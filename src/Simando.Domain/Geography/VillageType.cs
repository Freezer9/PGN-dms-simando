namespace Simando.Domain.Geography;

// Kelurahan (urban) and Desa (rural) split the same way Kota/Kabupaten do
// one level up — docs/domain/master-data.md §4 "Same at level 4".
public enum VillageType
{
    Kelurahan,
    Desa,
}
