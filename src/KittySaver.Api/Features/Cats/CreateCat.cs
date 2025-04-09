using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons;
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

public sealed class CreateCat : IEndpoint
{
    public sealed record CreateCatCommand(
        PersonId PersonId,
        string Name,
        bool IsCastrated,
        string MedicalHelpUrgency,
        string AgeCategory,
        string Behavior,
        string HealthStatus,
        string? AdditionalRequirements = null) : ICommand<CatHateoasResponse>, IAuthorizedRequest, ICatRequest;

    public sealed class CreateCatCommandValidator : AbstractValidator<CreateCatCommand>
    {
        public CreateCatCommandValidator()
        {
            RuleFor(x => x.PersonId).NotEmpty();
            
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
    
    internal sealed class CreateCatCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        ICatPriorityCalculatorService calculator)
        : IRequestHandler<CreateCatCommand, CatHateoasResponse>
    {
        public async Task<CatHateoasResponse> Handle(CreateCatCommand request, CancellationToken cancellationToken)
        {
            Person person = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
        
            CatName catName = CatName.Create(request.Name);
            Description additionalRequirements = Description.Create(request.AdditionalRequirements);

            MedicalHelpUrgency medicalHelpUrgency = MedicalHelpUrgency.FromNameOrValue(request.MedicalHelpUrgency, true);
            AgeCategory ageCategory = AgeCategory.FromNameOrValue(request.AgeCategory, true);
            Behavior behavior = Behavior.FromNameOrValue(request.Behavior, true);
            HealthStatus healthStatus = HealthStatus.FromNameOrValue(request.HealthStatus, true);
            
            Cat cat = person.AddCat(
                priorityScoreCalculator: calculator,
                name: catName,
                medicalHelpUrgency: medicalHelpUrgency,
                ageCategory: ageCategory,
                behavior: behavior,
                healthStatus: healthStatus,
                isCastrated: request.IsCastrated,
                additionalRequirements: additionalRequirements);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new CatHateoasResponse(cat.Id, cat.PersonId, cat.AdvertisementId, cat.IsThumbnailUploaded, cat.IsAdopted);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/cats", async (
            Guid personId,
            CreateCatRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            CreateCatCommand command = request.MapToCreateCatCommand(personId);
            CatHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/persons/{personId}/cats/{hateoasResponse.Id}", new
            {
                hateoasResponse.Id,
                hateoasResponse.PersonId,
                hateoasResponse.AdvertisementId,
                hateoasResponse.Links
            });
        }).RequireAuthorization()
        .WithName(EndpointNames.CreateCat.EndpointName)
        .WithTags(EndpointNames.GroupNames.CatGroup);
    }
}

[Mapper]
public static partial class CreateCatMapper
{
    public static partial CreateCat.CreateCatCommand MapToCreateCatCommand(
        this CreateCatRequest request,
        Guid personId);
}
