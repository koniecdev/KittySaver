global using CreateCat = KittySaver.Api.Features.Cats.CreateCat;
global using KittySaver.Shared.Requests;


namespace KittySaver.Api.Tests.Integration;

[CollectionDefinition("Api")]
public class TestCollection : ICollectionFixture<KittySaverApiFactory>;