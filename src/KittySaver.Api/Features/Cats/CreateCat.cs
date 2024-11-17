using FluentValidation;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
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
        string MedicalHelpUrgency,
        string AgeCategory,
        string Behavior,
        string HealthStatus,
        string? AdditionalRequirements = null) : ICatSmartEnumsRequest;
    
    public sealed record CreateCatCommand(
        Guid PersonId,
        string Name,
        bool IsCastrated,
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
            
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(CatName.MaxLength);
            
            RuleFor(x => x.AdditionalRequirements).MaximumLength(Description.MaxLength);
            
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
    
    internal sealed class CreateCatCommandHandler(ApplicationDbContext db, ICatPriorityCalculatorService calculator) : IRequestHandler<CreateCatCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCatCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.HealthStatus);
            ArgumentNullException.ThrowIfNull(request.Behavior);
            ArgumentNullException.ThrowIfNull(request.AgeCategory);
            ArgumentNullException.ThrowIfNull(request.MedicalHelpUrgency);
            
            Person person = await db.Persons
                              .Include(x => x.Cats)
                              .FirstOrDefaultAsync(x => x.Id == request.PersonId, cancellationToken)
                          ?? throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);

            CatName catName = CatName.Create(request.Name);
            Description additionalRequirements = Description.Create(request.AdditionalRequirements);
            
            Cat cat = Cat.Create(
                priorityScoreCalculator: calculator,
                person: person,
                name: catName,
                medicalHelpUrgency: request.MedicalHelpUrgency,
                ageCategory: request.AgeCategory,
                behavior: request.Behavior,
                healthStatus: request.HealthStatus,
                isCastrated: request.IsCastrated,
                additionalRequirements: additionalRequirements);
            
            await db.SaveChangesAsync(cancellationToken);
            return cat.Id;
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
    
    [MapperIgnoreSource(nameof(CreateCat.CreateCatRequest.Behavior))]
    [MapperIgnoreSource(nameof(CreateCat.CreateCatRequest.MedicalHelpUrgency))]
    [MapperIgnoreSource(nameof(CreateCat.CreateCatRequest.AgeCategory))]
    [MapperIgnoreSource(nameof(CreateCat.CreateCatRequest.HealthStatus))]
    private static partial CreateCat.CreateCatCommand ToCreateCatCommand(
        this CreateCat.CreateCatRequest request,
        Guid personId,
        MedicalHelpUrgency? medicalHelpUrgency,
        AgeCategory? ageCategory,
        Behavior? behavior,
        HealthStatus? healthStatus);
}
