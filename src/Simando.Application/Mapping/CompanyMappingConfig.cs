using Mapster;
using Simando.Application.Directory;
using Simando.Domain.Directory;
using Simando.Domain.Survey;

namespace Simando.Application.Mapping;

public sealed class CompanyMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CompanyContact, ContactDetail>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Nama, src => src.Nama)
            .Map(dest => dest.Jabatan, src => src.Jabatan)
            .Map(dest => dest.NoHp, src => src.NoHp)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.LinkedIn, src => src.LinkedIn)
            .Map(dest => dest.Instagram, src => src.Instagram)
            .Map(dest => dest.Facebook, src => src.Facebook)
            .Map(dest => dest.IsPrimary, src => src.IsPrimary)
            .Map(dest => dest.SortOrder, src => src.SortOrder);

        config.NewConfig<SaveContactRequest, CompanyContact>()
            .Map(dest => dest.Nama, src => src.Nama.Trim())
            .Map(dest => dest.Jabatan, src => src.Jabatan.Trim())
            .Map(dest => dest.NoHp, src => src.NoHp != null ? src.NoHp.Trim() : null)
            .Map(dest => dest.Email, src => src.Email != null ? src.Email.Trim() : null)
            .Map(dest => dest.LinkedIn, src => src.LinkedIn != null ? src.LinkedIn.Trim() : null)
            .Map(dest => dest.Instagram, src => src.Instagram != null ? src.Instagram.Trim() : null)
            .Map(dest => dest.Facebook, src => src.Facebook != null ? src.Facebook.Trim() : null)
            .Map(dest => dest.IsPrimary, src => src.IsPrimary);

        config.NewConfig<SurveyProduct, SurveyProductDetail>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Produk, src => src.Produk)
            .Map(dest => dest.Kapasitas, src => src.Kapasitas)
            .Map(dest => dest.HargaProduk, src => src.HargaProduk)
            .Map(dest => dest.Catatan, src => src.Catatan)
            .Map(dest => dest.SortOrder, src => src.SortOrder);

        config.NewConfig<SaveSurveyProductRequest, SurveyProduct>()
            .Map(dest => dest.Produk, src => src.Produk.Trim())
            .Map(dest => dest.Kapasitas, src => src.Kapasitas)
            .Map(dest => dest.HargaProduk, src => src.HargaProduk)
            .Map(dest => dest.Catatan, src => src.Catatan != null ? src.Catatan.Trim() : null);
    }
}
