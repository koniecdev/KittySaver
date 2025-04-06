using KittySaver.Shared.TypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KittySaver.Api.Persistence;

public static class StronglyTypedIdsConverters
{
    public static ModelConfigurationBuilder RegisterAllStronglyTypedIdConverters(
        this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<PersonId>()
            .HaveConversion<PersonIdConverter>();

        configurationBuilder
            .Properties<CatId>()
            .HaveConversion<CatIdConverter>();

        configurationBuilder
            .Properties<AdvertisementId>()
            .HaveConversion<AdvertisementIdConverter>();

        return configurationBuilder;
    }
    
    private class PersonIdConverter() : ValueConverter<PersonId, Guid>(
        id => id.Value,
        value => new PersonId(value));

    private class CatIdConverter() : ValueConverter<CatId, Guid>(
        id => id.Value,
        value => new CatId(value));

    private class AdvertisementIdConverter() : ValueConverter<AdvertisementId, Guid>(
        id => id.Value,
        value => new AdvertisementId(value));
}
