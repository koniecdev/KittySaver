using FluentValidation;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.Services;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Cats;

public sealed class UpdateCat : IEndpoint
{
    public sealed record UpdateCatRequest(
        string Name,
        bool IsCastrated,
        bool IsInNeedOfSeeingVet,
        string MedicalHelpUrgencyName,
        string AgeCategoryName,
        string BehaviorName,
        string HealthStatusName,
        string? AdditionalRequirements = null) : ICatSmartEnumsRequest;
    
    public sealed record UpdateCatCommand(
        Guid PersonId,
        Guid Id,
        string Name,
        bool IsCastrated,
        bool IsInNeedOfSeeingVet,
        MedicalHelpUrgency MedicalHelpUrgency,
        AgeCategory AgeCategory,
        Behavior Behavior,
        HealthStatus HealthStatus,
        string? AdditionalRequirements = null) : ICommand;

    public sealed class UpdateCatCommandValidator
        : AbstractValidator<UpdateCatCommand>
    {
        public UpdateCatCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name).MaximumLength(Cat.Constraints.NameMaxLength);
            RuleFor(x => x.AdditionalRequirements).MaximumLength(Cat.Constraints.AdditionalRequirementsMaxLength);
        }
    }
    
    internal sealed class UpdateCatCommandHandler(ApplicationDbContext db, ICatPriorityCalculator calculator)
        : IRequestHandler<UpdateCatCommand>
    {
        public async Task Handle(UpdateCatCommand request, CancellationToken cancellationToken)
        {
            Person catOwner = await db.Persons
                                  .Where(x => x.Id == request.PersonId)
                                  .Include(x => x.Cats)
                                  .FirstOrDefaultAsync(cancellationToken)
                              ?? throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);
            Cat catToUpdate = catOwner.Cats.FirstOrDefault(x => x.Id == request.Id)
                              ?? throw new NotFoundExceptions.CatNotFoundException(request.Id);
            
            UpdateCatMapper.MapUpdateCatCommandPropertiesToExistingCat(request, catToUpdate, calculator);
            await db.SaveChangesAsync(cancellationToken);
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
            request.ValidateAndRetrieve(
                out MedicalHelpUrgency medicalHelpUrgency,
                out AgeCategory ageCategory,
                out Behavior behavior,
                out HealthStatus healthStatus);
            
            UpdateCatCommand command = request.MapToUpdateCatCommand(
                personId,
                id,
                medicalHelpUrgency,
                ageCategory,
                behavior,
                healthStatus);
            
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        });
    }
}

[Mapper]
public static partial class UpdateCatMapper
{
    public static UpdateCat.UpdateCatCommand MapToUpdateCatCommand(this UpdateCat.UpdateCatRequest request,
        Guid personId,
        Guid id,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus)
    {
        if (request.AdditionalRequirements is not null && string.IsNullOrWhiteSpace(request.AdditionalRequirements))
        {
            request = request with { AdditionalRequirements = null };
        }
        UpdateCat.UpdateCatCommand dto = request.ToUpdateCatCommand(personId, id, medicalHelpUrgency, ageCategory, behavior, healthStatus);
        return dto;
    }
    
    private static partial UpdateCat.UpdateCatCommand ToUpdateCatCommand(
        this UpdateCat.UpdateCatRequest request,
        Guid personId,
        Guid id,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus);

    public static void MapUpdateCatCommandPropertiesToExistingCat(UpdateCat.UpdateCatCommand command,
        Cat entity, ICatPriorityCalculator calculator)
    {
        UpdateCatCommandPropertiesToExistingCat(command, entity);
        entity.ReCalculatePriorityScore(calculator);
    }
    
    private static partial void UpdateCatCommandPropertiesToExistingCat(
        UpdateCat.UpdateCatCommand command,
        Cat entity);
}
