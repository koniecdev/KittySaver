using FluentValidation;
using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Shared.Domain.Entites;
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
        string? PickupAddressStreet,
        string? PickupAddressBuildingNumber,
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
        string? PickupAddressStreet,
        string? PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber) : ICommand<Guid>;

    public sealed class CreateAdvertisementCommandValidator 
        : AbstractValidator<CreateAdvertisementCommand>
    {
        public CreateAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId).NotEmpty();

            RuleFor(x => x.CatsIdsToAssign).NotEmpty();
            
            RuleFor(x => x.Description).MaximumLength(Advertisement.Constraints.DescriptionMaxLength);
            
            RuleFor(x => x.ContactInfoPhoneNumber)
                .NotEmpty()
                .MaximumLength(IContact.Constraints.PhoneNumberMaxLength);

            RuleFor(x => x.ContactInfoEmail)
                .NotEmpty()
                .MaximumLength(IContact.Constraints.EmailMaxLength)
                .Matches(IContact.Constraints.EmailPattern);
            
            RuleFor(x => x.PickupAddressCountry)
                .NotEmpty()
                .MaximumLength(IAddress.Constraints.CountryMaxLength);
            
            RuleFor(x => x.PickupAddressState)
                .MaximumLength(IAddress.Constraints.StateMaxLength);
            
            RuleFor(x => x.PickupAddressZipCode)
                .NotEmpty()
                .MaximumLength(IAddress.Constraints.ZipCodeMaxLength);
            
            RuleFor(x => x.PickupAddressCity)
                .NotEmpty()
                .MaximumLength(IAddress.Constraints.CityMaxLength);
            
            RuleFor(x => x.PickupAddressStreet)
                .MaximumLength(IAddress.Constraints.StreetMaxLength);
            
            RuleFor(x => x.PickupAddressBuildingNumber)
                .MaximumLength(IAddress.Constraints.BuildingNumberMaxLength);
        }
    }
    
    internal sealed class CreateAdvertisementCommandHandler(ApplicationDbContext db, IDateTimeService dateTimeService)
        : IRequestHandler<CreateAdvertisementCommand, Guid>
    {
        public async Task<Guid> Handle(CreateAdvertisementCommand request, CancellationToken cancellationToken)
        {
            PickupAddress pickupAddress = new()
            {
                Country = request.PickupAddressCountry,
                State = request.PickupAddressState,
                ZipCode = request.PickupAddressZipCode,
                City = request.PickupAddressCity,
                Street = request.PickupAddressStreet,
                BuildingNumber = request.PickupAddressBuildingNumber
            };

            ContactInfo contactInfo = new()
            {
                Email = request.ContactInfoEmail,
                PhoneNumber = request.ContactInfoPhoneNumber
            };

            Person person = await db.Persons
                                .Where(x=>x.Id == request.PersonId)
                                .Include(x => x.Cats)
                                .FirstOrDefaultAsync(cancellationToken)
                            ?? throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);

            Advertisement advertisement = Advertisement.Create(
                currentDate: dateTimeService.Now,
                person: person,
                catsIdsToAssign: request.CatsIdsToAssign,
                pickupAddress: pickupAddress,
                contactInfo: contactInfo,
                description: null);
            
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
    [UserMapping(Default = true)]
    public static CreateAdvertisement.CreateAdvertisementCommand MapToCreateAdvertisementCommand(
        this CreateAdvertisement.CreateAdvertisementRequest request)
    {
        if (request.PickupAddressState is not null && string.IsNullOrWhiteSpace(request.PickupAddressState))
        {
            request = request with { PickupAddressState = null };
        }

        CreateAdvertisement.CreateAdvertisementCommand dto = request.ToCreateAdvertisementCommand();
        return dto;
    }

    private static partial CreateAdvertisement.CreateAdvertisementCommand ToCreateAdvertisementCommand(
        this CreateAdvertisement.CreateAdvertisementRequest request);
    
}
    
