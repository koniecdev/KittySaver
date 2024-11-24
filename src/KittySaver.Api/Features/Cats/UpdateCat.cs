using FluentValidation;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Cats;

public sealed class UpdateCat : IEndpoint
{
    public sealed record UpdateCatRequest(
        string Name,
        bool IsCastrated,
        string MedicalHelpUrgency,
        string AgeCategory,
        string Behavior,
        string HealthStatus,
        string? AdditionalRequirements = null) : ICatSmartEnumsRequest;
    
    public sealed record UpdateCatCommand(
        Guid PersonId,
        Guid Id,
        string Name,
        bool IsCastrated,
        MedicalHelpUrgency? MedicalHelpUrgency,
        AgeCategory? AgeCategory,
        Behavior? Behavior,
        HealthStatus? HealthStatus,
        string? AdditionalRequirements = null) : ICommand;

    public sealed class UpdateCatCommandValidator
        : AbstractValidator<UpdateCatCommand>
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
    
    internal sealed class UpdateCatCommandHandler(ApplicationDbContext db, ICatPriorityCalculatorService calculator)
        : IRequestHandler<UpdateCatCommand>
    {
        public async Task Handle(UpdateCatCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.HealthStatus);
            ArgumentNullException.ThrowIfNull(request.Behavior);
            ArgumentNullException.ThrowIfNull(request.AgeCategory);
            ArgumentNullException.ThrowIfNull(request.MedicalHelpUrgency);
            
            Person catOwner = await db.Persons
                                  .Where(x => x.Id == request.PersonId)
                                  .Include(x => x.Cats)
                                  .FirstOrDefaultAsync(cancellationToken)
                              ?? throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);
            Cat catToUpdate = catOwner.Cats.FirstOrDefault(x => x.Id == request.Id)
                              ?? throw new NotFoundExceptions.CatNotFoundException(request.Id);
            
            CatName catName = CatName.Create(request.Name);
            Description additionalRequirements = Description.Create(request.AdditionalRequirements);

            catToUpdate.Name = catName;
            catToUpdate.AdditionalRequirements = additionalRequirements;
            catToUpdate.IsCastrated = request.IsCastrated;
            catOwner.ReplaceCatPriorityCompounds(
                calculator,
                request.Id,
                request.HealthStatus,
                request.AgeCategory,
                request.Behavior,
                request.MedicalHelpUrgency);
            
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
            UpdateCatCommand command = request.MapToUpdateCatCommand(personId, id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        });
    }
}

[Mapper]
public static partial class UpdateCatMapper
{
    public static UpdateCat.UpdateCatCommand MapToUpdateCatCommand(this UpdateCat.UpdateCatRequest request, Guid personId, Guid id)
    {
        request.RetrieveSmartEnumsFromNames(
            out (bool mappedSuccessfully, MedicalHelpUrgency value) medicalHelpUrgencyResults,
            out (bool mappedSuccessfully, AgeCategory value) ageCategoryResults,
            out (bool mappedSuccessfully, Behavior value) behaviorResults,
            out (bool mappedSuccessfully, HealthStatus value) healthStatusResults);

        UpdateCat.UpdateCatCommand dto = request.ToUpdateCatCommand(
            personId,
            id,
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
    private static partial UpdateCat.UpdateCatCommand ToUpdateCatCommand(
        this UpdateCat.UpdateCatRequest request,
        Guid personId,
        Guid id,
        MedicalHelpUrgency? medicalHelpUrgency,
        AgeCategory? ageCategory,
        Behavior? behavior,
        HealthStatus? healthStatus);
}
