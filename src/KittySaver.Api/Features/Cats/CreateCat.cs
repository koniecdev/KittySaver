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

public class CreateCat : IEndpoint
{
    public sealed record CreateCatRequest(
        string Name,
        bool IsCastrated,
        bool IsInNeedOfSeeingVet,
        string MedicalHelpUrgencyName,
        string AgeCategoryName,
        string BehaviorName,
        string HealthStatusName,
        string? AdditionalRequirements = null) : ICatSmartEnumsRequest;
    
    public sealed record CreateCatCommand(
        Guid PersonId,
        string Name,
        bool IsCastrated,
        bool IsInNeedOfSeeingVet,
        MedicalHelpUrgency MedicalHelpUrgency,
        AgeCategory AgeCategory,
        Behavior Behavior,
        HealthStatus HealthStatus,
        string? AdditionalRequirements = null) : ICommand<Guid>;

    public sealed class CreateCatCommandValidator : AbstractValidator<CreateCatCommand>
    {
        public CreateCatCommandValidator()
        {
            RuleFor(x => x.PersonId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name).MaximumLength(Cat.Constraints.NameMaxLength);
            RuleFor(x => x.AdditionalRequirements).MaximumLength(Cat.Constraints.AdditionalRequirementsMaxLength);
        }
    }
    
    internal sealed class CreateCatCommandHandler(ApplicationDbContext db, ICatPriorityCalculator calculator) : IRequestHandler<CreateCatCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCatCommand request, CancellationToken cancellationToken)
        {
            Person root = await db.Persons
                              .Include(x => x.Cats)
                              .FirstOrDefaultAsync(x => x.Id == request.PersonId, cancellationToken)
                          ?? throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);
            
            Cat entity = Cat.Create(
                calculator: calculator,
                personId: request.PersonId,
                name: request.Name,
                medicalHelpUrgency: request.MedicalHelpUrgency,
                ageCategory: request.AgeCategory,
                behavior: request.Behavior,
                healthStatus: request.HealthStatus,
                isCastrated: request.IsCastrated,
                isInNeedOfSeeingVet: request.IsInNeedOfSeeingVet,
                additionalRequirements: request.AdditionalRequirements);
            
            root.AddCat(entity);
            await db.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/cats", async 
            (Guid personId,
            CreateCatRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            request.ValidateAndRetrieve(
                out MedicalHelpUrgency medicalHelpUrgency,
                out AgeCategory ageCategory,
                out Behavior behavior,
                out HealthStatus healthStatus);

            CreateCatCommand command = request.MapToCreateCatCommand(
                personId,
                medicalHelpUrgency,
                ageCategory,
                behavior,
                healthStatus);
            
            Guid catId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/persons/{personId}/cats/{catId}", new { Id = catId });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class CreateCatMapper
{
    public static CreateCat.CreateCatCommand MapToCreateCatCommand(this CreateCat.CreateCatRequest request,
        Guid personId,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus)
    {
        if (request.AdditionalRequirements is not null && string.IsNullOrWhiteSpace(request.AdditionalRequirements))
        {
            request = request with { AdditionalRequirements = null };
        }
        CreateCat.CreateCatCommand dto = request.ToCreateCatCommand(personId, medicalHelpUrgency, ageCategory, behavior, healthStatus);
        return dto;
    }
    
    private static partial CreateCat.CreateCatCommand ToCreateCatCommand(
        this CreateCat.CreateCatRequest request,
        Guid personId,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus);
}
