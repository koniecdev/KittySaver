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

public sealed class CreateCat : IEndpoint
{
    public sealed record CreateCatRequest(
        string Name,
        bool IsCastrated,
        bool IsInNeedOfSeeingVet,
        string MedicalHelpUrgency,
        string AgeCategory,
        string Behavior,
        string HealthStatus,
        string? AdditionalRequirements = null) : ICatSmartEnumsRequest;
    
    public sealed record CreateCatCommand(
        Guid PersonId,
        string Name,
        bool IsCastrated,
        bool IsInNeedOfSeeingVet,
        MedicalHelpUrgency? MedicalHelpUrgency,
        AgeCategory? AgeCategory,
        Behavior? Behavior,
        HealthStatus? HealthStatus,
        string? AdditionalRequirements = null) : ICommand<Guid>;

    public sealed class CreateCatCommandValidator : AbstractValidator<CreateCatCommand>
    {
        public CreateCatCommandValidator()
        {
            RuleFor(x => x.PersonId).NotEmpty();
            
            RuleFor(x => x.Name).NotEmpty();
            
            RuleFor(x => x.Name).MaximumLength(Cat.Constraints.NameMaxLength);
            
            RuleFor(x => x.AdditionalRequirements).MaximumLength(Cat.Constraints.AdditionalRequirementsMaxLength);
            
            RuleFor(x => x.HealthStatus)
                .NotNull()
                .WithMessage("Provided empty or invalid Health Status.");
            
            RuleFor(x => x.AgeCategory)
                .NotNull()
                .WithMessage("Provided empty or invalid Age Category.");
            
            RuleFor(x => x.Behavior)
                .NotNull()
                .WithMessage("Provided empty or invalid Behavior.");
            
            RuleFor(x => x.MedicalHelpUrgency)
                .NotNull()
                .WithMessage("Provided empty or invalid Medical Help Urgency.");
        }
    }
    
    internal sealed class CreateCatCommandHandler(ApplicationDbContext db, ICatPriorityCalculator calculator) : IRequestHandler<CreateCatCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCatCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.HealthStatus);
            ArgumentNullException.ThrowIfNull(request.Behavior);
            ArgumentNullException.ThrowIfNull(request.AgeCategory);
            ArgumentNullException.ThrowIfNull(request.MedicalHelpUrgency);
            
            Person root = await db.Persons
                              .Include(x => x.Cats)
                              .FirstOrDefaultAsync(x => x.Id == request.PersonId, cancellationToken)
                          ?? throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);
            
            Cat entity = Cat.Create(
                calculator: calculator,
                person: root,
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
            CreateCatCommand command = request.MapToCreateCatCommand(personId);
            Guid catId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/persons/{personId}/cats/{catId}", new { Id = catId });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class CreateCatMapper
{
    public static CreateCat.CreateCatCommand MapToCreateCatCommand(this CreateCat.CreateCatRequest request,
        Guid personId)
    {
        if (request.AdditionalRequirements is not null && string.IsNullOrWhiteSpace(request.AdditionalRequirements))
        {
            request = request with { AdditionalRequirements = null };
        }
        
        request.RetrieveSmartEnumsFromNames(
            out (bool mappedSuccessfully, MedicalHelpUrgency value) medicalHelpUrgencyResults,
            out (bool mappedSuccessfully, AgeCategory value) ageCategoryResults,
            out (bool mappedSuccessfully, Behavior value) behaviorResults,
            out (bool mappedSuccessfully, HealthStatus value) healthStatusResults);
        
        CreateCat.CreateCatCommand dto = request.ToCreateCatCommand(personId,
            medicalHelpUrgencyResults.mappedSuccessfully ? medicalHelpUrgencyResults.value : null,
            ageCategoryResults.mappedSuccessfully ? ageCategoryResults.value : null,
            behaviorResults.mappedSuccessfully ? behaviorResults.value : null,
            healthStatusResults.mappedSuccessfully ? healthStatusResults.value : null);
        return dto;
    }
    
    private static partial CreateCat.CreateCatCommand ToCreateCatCommand(
        this CreateCat.CreateCatRequest request,
        Guid personId,
        MedicalHelpUrgency? medicalHelpUrgency,
        AgeCategory? ageCategory,
        Behavior? behavior,
        HealthStatus? healthStatus);
}
