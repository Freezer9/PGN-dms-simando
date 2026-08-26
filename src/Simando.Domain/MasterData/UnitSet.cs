namespace Simando.Domain.MasterData;

// The eight unit sets that back every <MeasureInput Set="..."> dropdown in
// the system. docs/domain/master-data.md §7 "Satuan". Fixed enum, not
// master data — it's set *membership* (UnitSetMember) that grows, not the
// eight sets themselves.
public enum UnitSet
{
    Capacity,        // Kapasitas energi — MW, Ton/Jam, Kkal, TR
    Cooling,         // Pendinginan — TR, PK, Kw
    EnergyUsage,     // Pemakaian energi — Kwh, Ton, Kkal, TR
    FuelConsumption, // Konsumsi bahan bakar — Ton, Liter, Kwh
    RawMaterial,     // Bahan baku / pasar — %, Ton, KL, m2
    Diameter,        // Inch, mm
    GasVolume,       // m3, MMBtu
    Pressure,        // Tekanan — barg
}
