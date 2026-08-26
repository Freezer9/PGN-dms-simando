namespace Simando.Domain.Registration;

public enum RegistrasiSource
{
    Online,
    Manual,
}

public enum BasisKontrak
{
    Harian,
    Bulanan,
    Tahunan,
}

public enum SkemaHarga
{
    Reguler,
    Sigas,
    Bersyarat,
}

public enum HargaCurrency
{
    USD,
    IDR,
}

public enum HargaUnit
{
    MMBtu,
    M3,
}

public enum StatusBangunan
{
    DalamRencana,
    DalamPembangunan,
    Eksisting,
    ProsesEkspansi,
}

public enum Sektor
{
    Komersial,
    Industri,
    Transportasi,
}

public enum SignatureMethod
{
    Wet,
    Digital,
}
