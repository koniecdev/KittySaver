global using CreateCat = KittySaver.Api.Features.Cats.CreateCat;

namespace KittySaver.Api.Tests.Integration;

[CollectionDefinition("Api")]
public class TestCollection : ICollectionFixture<KittySaverApiFactory> { }