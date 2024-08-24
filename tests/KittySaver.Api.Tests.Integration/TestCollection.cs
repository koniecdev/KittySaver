using Microsoft.AspNetCore.Mvc.Testing;

namespace KittySaver.Api.Tests.Integration;

[CollectionDefinition("Api")]
public class TestCollection : ICollectionFixture<KittySaverApiFactory> { }