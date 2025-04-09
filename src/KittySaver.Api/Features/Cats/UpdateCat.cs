using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.DomainServices;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Requests;
using KittySaver.Shared.TypedIds;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Cats;

public sealed class UpdateCat : IEndpoint
{
    public sealed record UpdateCatCommand(
        PersonId PersonId,
        CatId Id,
        string Name,
        bool IsCastrated,
        string MedicalHelpUrgency,
        string AgeCategory,
        string Behavior,
        string HealthStatus,
        string? AdditionalRequirements = null) : ICommand<CatHateoasResponse>, IAuthorizedRequest, ICatRequest;

    public sealed class UpdateCatCommandValidator : AbstractValidator<UpdateCatCommand>
    {
        public UpdateCatCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.PersonId)
                .NotEmpty();
            
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(CatName.MaxLength);
            
            RuleFor(x => x.AdditionalRequirements).MaximumLength(Description.MaxLength);

            RuleFor(x => x.HealthStatus).NotEmpty();

            RuleFor(x => x.AgeCategory).NotEmpty();

            RuleFor(x => x.Behavior).NotEmpty();

            RuleFor(x => x.MedicalHelpUrgency).NotEmpty();
        }
    }
    
    internal sealed class UpdateCatCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        ICatPriorityCalculatorService calculator)
        : IRequestHandler<UpdateCatCommand, CatHateoasResponse>
    {
        public async Task<CatHateoasResponse> Handle(UpdateCatCommand request, CancellationToken cancellationToken)
        {
            Person catOwner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            
            CatName catName = CatName.Create(request.Name);
            Description additionalRequirements = Description.Create(request.AdditionalRequirements);
            
            MedicalHelpUrgency medicalHelpUrgency = MedicalHelpUrgency.FromNameOrValue(request.MedicalHelpUrgency, true);
            AgeCategory ageCategory = AgeCategory.FromNameOrValue(request.AgeCategory, true);
            Behavior behavior = Behavior.FromNameOrValue(request.Behavior, true);
            HealthStatus healthStatus = HealthStatus.FromNameOrValue(request.HealthStatus, true);
            
            catOwner.UpdateCat(
                request.Id,
                calculator,
                catName,
                additionalRequirements,
                request.IsCastrated,
                healthStatus,
                ageCategory,
                behavior,
                medicalHelpUrgency);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            var catProperties = catOwner.Cats
                .Where(cat => cat.Id == request.Id)
                .Select(c => new
                {
                    advertisementId = c.AdvertisementId,
                    isThumbnailUploaded = c.IsThumbnailUploaded,
                    isAdopted = c.IsAdopted
                }).First();
            
            return new CatHateoasResponse(
                request.Id,
                request.PersonId,
                catProperties.advertisementId,
                catProperties.isThumbnailUploaded,
                catProperties.isAdopted);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("/persons/{personId:guid}/cats/{id:guid}", async(
            Guid personId,
            Guid id,
            UpdateCatRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UpdateCatCommand command = request.MapToUpdateCatCommand(personId, id);
            CatHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
            return Results.Ok(hateoasResponse);
        }).RequireAuthorization()
        .WithName(EndpointNames.UpdateCat.EndpointName)
        .WithTags(EndpointNames.GroupNames.CatGroup);
    }
}

[Mapper]
public static partial class UpdateCatMapper
{
    public static partial UpdateCat.UpdateCatCommand MapToUpdateCatCommand(
        this UpdateCatRequest request,
        Guid personId,
        Guid id);
}
