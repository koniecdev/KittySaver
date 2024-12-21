// using Bogus;
// using FluentAssertions;
// using KittySaver.Domain.Advertisements;
// using KittySaver.Domain.Common.Primitives.Enums;
// using KittySaver.Domain.Persons;
// using KittySaver.Domain.ValueObjects;
// using NSubstitute;
// using Person = KittySaver.Domain.Persons.Person;
//
// namespace KittySaver.Domain.Tests.Unit.Advertisements;
//
// using Person = Person;
//
// public class AdvertisementServiceTests
// {
//     private static readonly DateTimeOffset Date = new(2024, 10, 31, 11, 0, 0, TimeSpan.Zero);
//
//     private static readonly Address Address = new Faker<Address>()
//         .CustomInstantiator(faker =>
//             Address.Create(
//                 country: faker.Address.Country(),
//                 state: faker.Address.State(),
//                 zipCode: faker.Address.ZipCode(),
//                 city: faker.Address.City(),
//                 street: faker.Address.StreetName(),
//                 buildingNumber: faker.Address.BuildingNumber()
//             )).Generate();
//
//     private static readonly Faker<Address> PickupAddressGenerator = new Faker<Address>()
//         .CustomInstantiator(faker =>
//             Address.Create(
//                 country: faker.Address.Country(),
//                 state: faker.Address.State(),
//                 zipCode: faker.Address.ZipCode(),
//                 city: faker.Address.City(),
//                 street: faker.Address.StreetName(),
//                 buildingNumber: faker.Address.BuildingNumber()
//             ));
//
//     private static readonly Faker<Email> ContactInfoEmailGenerator = new Faker<Email>()
//         .CustomInstantiator(faker => Email.Create(faker.Person.Email));
//     
//     private static readonly Faker<PhoneNumber> ContactInfoPhoneNumberGenerator = new Faker<PhoneNumber>()
//         .CustomInstantiator(faker => PhoneNumber.Create(faker.Person.Phone));
//     
//     private static readonly Person Person = new Faker<Person>()
//         .CustomInstantiator(faker =>
//             Person.Create(
//                 userIdentityId: Guid.NewGuid(),
//                 nickname: Nickname.Create(faker.Person.FirstName),
//                 email: Email.Create(faker.Person.Email),
//                 phoneNumber: PhoneNumber.Create(faker.Person.Phone),
//                 defaultAdvertisementPickupAddress: PickupAddressGenerator.Generate(),
//                 defaultAdvertisementContactInfoEmail: ContactInfoEmailGenerator.Generate(),
//                 defaultAdvertisementContactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate()
//             )).Generate();
//
//     private static readonly Faker<Cat> CatGenerator = new Faker<Cat>()
//         .CustomInstantiator(faker =>
//             Cat.Create(
//                 priorityScoreCalculator: new DefaultCatPriorityCalculatorService(),
//                 person: Person,
//                 name: CatName.Create(faker.Person.FirstName),
//                 medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet,
//                     MedicalHelpUrgency.HaveToSeeVet),
//                 ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
//                 behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
//                 healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
//                 isCastrated: faker.PickRandomParam(true, false),
//                 additionalRequirements: Description.Create(faker.Lorem.Lines(2))
//             ));
//
//     [Fact]
//     public async Task ReplaceCatsOfAdvertisement_ShouldReplaceOldCatsWithNewOne_WhenValidDataAreProvided()
//     {
//         //Arrange
//         List<Cat> cats = [CatGenerator.Generate(), CatGenerator.Generate(), CatGenerator.Generate()];
//         Address pickupAddress = PickupAddressGenerator.Generate();
//         Email contactInfoEmail = ContactInfoEmailGenerator.Generate();
//         PhoneNumber contactInfoPhoneNumber = ContactInfoPhoneNumberGenerator.Generate();
//
//         Advertisement advertisement = Advertisement.Create(
//             currentDate: Date,
//             owner: Person,
//             catsIdsToAssign: cats.Select(x=>x.Id),
//             pickupAddress: pickupAddress,
//             contactInfoEmail: contactInfoEmail,
//             contactInfoPhoneNumber: contactInfoPhoneNumber,
//             description: Description.Create("lorem ipsum"));
//         
//         //Act
//         IPersonRepository personRepository = Substitute.For<IPersonRepository>();
//         personRepository.GetPersonByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Person);
//         IAdvertisementRepository advertisementRepository = Substitute.For<IAdvertisementRepository>();
//         advertisementRepository.GetAdvertisementByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(advertisement);
//         
//         Cat catToAssign = CatGenerator.Generate();
//         AdvertisementService service = new(advertisementRepository, personRepository);
//         
//         await service.ReplaceCatsOfAdvertisementAsync(
//             advertisement.Id,
//             catIdsToAssign: [catToAssign.Id],
//             CancellationToken.None);
//
//         //Assert
//         advertisement.Should().NotBeNull();
//         advertisement.Id.Should().NotBeEmpty();
//         Person.Cats
//             .Count(x => x.AdvertisementId == advertisement.Id)
//             .Should()
//             .Be(1);
//     }
//     
//     [Fact]
//     public async Task RecalculatePriorityScore_ShouldRecalculateAdvertisementPriorityScore_WhenCatsChange()
//     {
//         //Arrange
//         ICatPriorityCalculatorService catPriorityCalculatorService = Substitute.For<ICatPriorityCalculatorService>();
//         catPriorityCalculatorService.Calculate(Arg.Any<Cat>()).Returns(420);
//         Cat cat =  new Faker<Cat>()
//             .CustomInstantiator(faker =>
//                 Cat.Create(
//                     priorityScoreCalculator: new DefaultCatPriorityCalculatorService(),
//                     person: Person,
//                     name: CatName.Create(faker.Person.FirstName),
//                     medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet,
//                         MedicalHelpUrgency.HaveToSeeVet),
//                     ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
//                     behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
//                     healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
//                     isCastrated: faker.PickRandomParam(true, false),
//                     additionalRequirements: Description.Create(faker.Lorem.Lines(2))
//                 )).Generate();
//         Address pickupAddress = PickupAddressGenerator.Generate();
//         Email contactInfoEmail = ContactInfoEmailGenerator.Generate();
//         PhoneNumber contactInfoPhoneNumber = ContactInfoPhoneNumberGenerator.Generate();
//
//         Advertisement advertisement = Advertisement.Create(
//             currentDate: Date,
//             owner: Person,
//             catsIdsToAssign: [cat.Id],
//             pickupAddress: pickupAddress,
//             contactInfoEmail: contactInfoEmail,
//             contactInfoPhoneNumber: contactInfoPhoneNumber,
//             description: Description.Create("lorem ipsum"));
//         
//         IPersonRepository personRepository = Substitute.For<IPersonRepository>();
//         personRepository.GetPersonByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Person);
//         IAdvertisementRepository advertisementRepository = Substitute.For<IAdvertisementRepository>();
//         advertisementRepository.GetAdvertisementByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(advertisement);
//         
//         //Act
//         const int updatedPriorityScore = 100;
//         catPriorityCalculatorService.Calculate(Arg.Any<Cat>()).Returns(updatedPriorityScore);
//         Person.UpdateCat(
//             catId: cat.Id,
//             catPriorityCalculator: catPriorityCalculatorService,
//             name: cat.Name,
//             additionalRequirements: cat.AdditionalRequirements,
//             isCastrated: false,
//             healthStatus: HealthStatus.Critical, 
//             ageCategory: cat.AgeCategory,
//             behavior: cat.Behavior,
//             medicalHelpUrgency: cat.MedicalHelpUrgency);
//         
//         AdvertisementService service = new(advertisementRepository, personRepository);
//         await service.RecalculatePriorityScoreAsync(advertisement.Id, CancellationToken.None);
//
//         //Assert
//         advertisement.PriorityScore.Should().Be(updatedPriorityScore);
//     }
// }