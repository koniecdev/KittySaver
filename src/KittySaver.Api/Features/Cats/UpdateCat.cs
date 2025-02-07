using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Requests;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Cats;

public sealed class UpdateCat : IEndpoint
{
    public sealed record UpdateCatCommand(
        Guid PersonId,
        Guid Id,
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
                .NotEmpty()
                .NotEqual(x => x.PersonId);
            
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.Id);
            
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
            
            MedicalHelpUrgency medicalHelpUrgency = MedicalHelpUrgency.FromName(request.MedicalHelpUrgency, true);
            AgeCategory ageCategory = AgeCategory.FromName(request.AgeCategory, true);
            Behavior behavior = Behavior.FromName(request.Behavior, true);
            HealthStatus healthStatus = HealthStatus.FromName(request.HealthStatus, true);
            
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
            
            var catAdvertisementIdAndIsThumbnailUploaded = catOwner.Cats
                .Where(cat => cat.Id == request.Id)
                .Select(c => new
                {
                    advertisementId = c.AdvertisementId,
                    isThumbnailUploaded = c.IsThumbnailUploaded
                }).First();
            
            return new CatHateoasResponse(
                request.Id, 
                request.PersonId,
                catAdvertisementIdAndIsThumbnailUploaded.advertisementId,
                catAdvertisementIdAndIsThumbnailUploaded.isThumbnailUploaded);
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
