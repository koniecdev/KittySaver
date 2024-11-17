using FluentValidation;
using KittySaver.Api.Shared.Domain.Advertisement;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Advertisements;

public class CreateAdvertisement : IEndpoint
{
    public sealed record CreateAdvertisementRequest(
        Guid PersonId,
        IEnumerable<Guid> CatsIdsToAssign,
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string PickupAddressStreet,
        string PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber
    );

    public sealed record CreateAdvertisementCommand(
        Guid PersonId,
        IEnumerable<Guid> CatsIdsToAssign,
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string PickupAddressStreet,
        string PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber) : ICommand<Guid>;

    public sealed class CreateAdvertisementCommandValidator
        : AbstractValidator<CreateAdvertisementCommand>
    {
        public CreateAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId).NotEmpty();

            RuleFor(x => x.CatsIdsToAssign).NotEmpty();

            RuleFor(x => x.Description).MaximumLength(Description.MaxLength);

            RuleFor(x => x.ContactInfoPhoneNumber)
                .NotEmpty()
                .MaximumLength(PhoneNumber.MaxLength);

            RuleFor(x => x.ContactInfoEmail)
                .NotEmpty()
                .MaximumLength(Email.MaxLength)
                .Matches(Email.RegexPattern);

            RuleFor(x => x.PickupAddressCountry)
                .NotEmpty()
                .MaximumLength(Address.CountryMaxLength);

            RuleFor(x => x.PickupAddressState)
                .MaximumLength(Address.StateMaxLength);

            RuleFor(x => x.PickupAddressZipCode)
                .NotEmpty()
                .MaximumLength(Address.ZipCodeMaxLength);

            RuleFor(x => x.PickupAddressCity)
                .NotEmpty()
                .MaximumLength(Address.CityMaxLength);

            RuleFor(x => x.PickupAddressStreet)
                .NotEmpty()
                .MaximumLength(Address.StreetMaxLength);

            RuleFor(x => x.PickupAddressBuildingNumber)
                .NotEmpty()
                .MaximumLength(Address.BuildingNumberMaxLength);
        }
    }

    internal sealed class CreateAdvertisementCommandHandler(ApplicationDbContext db, IDateTimeService dateTimeService)
        : IRequestHandler<CreateAdvertisementCommand, Guid>
    {
        public async Task<Guid> Handle(CreateAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person person =
                await db.Persons
                    .Where(x => x.Id == request.PersonId)
                    .Include(x => x.Cats)
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);

            Address pickupAddress = new()
            {
                Country = request.PickupAddressCountry,
                State = request.PickupAddressState,
                ZipCode = request.PickupAddressZipCode,
                City = request.PickupAddressCity,
                Street = request.PickupAddressStreet,
                BuildingNumber = request.PickupAddressBuildingNumber
            };
            Email contactInfoEmail = Email.Create(request.ContactInfoEmail);
            PhoneNumber contactInfoPhoneNumber = PhoneNumber.Create(request.ContactInfoPhoneNumber);
            Description description = Description.Create(request.Description);
            
            Advertisement advertisement = Advertisement.Create(
                currentDate: dateTimeService.Now,
                person: person,
                catsIdsToAssign: request.CatsIdsToAssign,
                pickupAddress: pickupAddress,
                contactInfoEmail: contactInfoEmail,
                contactInfoPhoneNumber: contactInfoPhoneNumber,
                description: description);

            db.Advertisements.Add(advertisement);
            await db.SaveChangesAsync(cancellationToken);
            return advertisement.Id;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("advertisements", async
        (CreateAdvertisementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            CreateAdvertisementCommand command = request.MapToCreateAdvertisementCommand();
            Guid advertisementId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/advertisements/{advertisementId}", new { Id = advertisementId });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class CreateAdvertisementMapper
{
    public static partial CreateAdvertisement.CreateAdvertisementCommand MapToCreateAdvertisementCommand(
        this CreateAdvertisement.CreateAdvertisementRequest request);
}