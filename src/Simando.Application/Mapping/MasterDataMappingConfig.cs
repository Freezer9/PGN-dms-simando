using Mapster;
using Simando.Application.MasterData;
using Simando.Domain.MasterData;

namespace Simando.Application.Mapping;

public sealed class MasterDataMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // IndustryType
        config.NewConfig<IndustryType, IndustryTypeResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ContohProduk, src => src.ContohProduk);

        config.NewConfig<CreateIndustryTypeRequest, IndustryType>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.ContohProduk, src => src.ContohProduk != null ? src.ContohProduk.Trim() : null);

        // Segment
        config.NewConfig<Segment, SegmentResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.SortOrder, src => src.SortOrder);

        config.NewConfig<CreateSegmentRequest, Segment>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.SortOrder, src => src.SortOrder);

        // FuelType
        config.NewConfig<FuelType, FuelTypeResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<CreateFuelTypeRequest, FuelType>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Name, src => src.Name.Trim());

        // MrsSpec
        config.NewConfig<MrsSpec, MrsSpecResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<CreateMrsSpecRequest, MrsSpec>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Name, src => src.Name.Trim());

        // ReasonCategory
        config.NewConfig<ReasonCategory, ReasonCategoryResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<CreateReasonCategoryRequest, ReasonCategory>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Name, src => src.Name.Trim());

        // UnitOfMeasure
        config.NewConfig<UnitOfMeasure, UnitResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Dimension, src => src.Dimension);

        config.NewConfig<CreateUnitRequest, UnitOfMeasure>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Code, src => src.Code.Trim())
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.Dimension, src => src.Dimension);

        // MeterSize
        config.NewConfig<MeterSize, MeterSizeResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.GSize, src => src.GSize)
            .Map(dest => dest.NominalFlow, src => src.NominalFlow)
            .Map(dest => dest.MaxFlow, src => src.MaxFlow)
            .Map(dest => dest.PressureRating, src => src.PressureRating);

        config.NewConfig<CreateMeterSizeRequest, MeterSize>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.GSize, src => src.GSize.Trim())
            .Map(dest => dest.NominalFlow, src => src.NominalFlow)
            .Map(dest => dest.MaxFlow, src => src.MaxFlow)
            .Map(dest => dest.PressureRating, src => src.PressureRating);

        // ReferenceDocument
        config.NewConfig<ReferenceDocument, ReferenceDocumentResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Version, src => src.Version)
            .Map(dest => dest.EffectiveFrom, src => src.EffectiveFrom)
            .Map(dest => dest.EffectiveTo, src => src.EffectiveTo)
            .Map(dest => dest.BlobKey, src => src.BlobKey);

        config.NewConfig<CreateReferenceDocumentRequest, ReferenceDocument>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.Version, src => src.Version)
            .Map(dest => dest.EffectiveFrom, src => src.EffectiveFrom)
            .Map(dest => dest.EffectiveTo, src => src.EffectiveTo)
            .Map(dest => dest.BlobKey, src => src.BlobKey);
    }
}
