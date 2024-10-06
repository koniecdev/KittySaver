using FluentValidation;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Enums;
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
            RuleFor(x => x.Name).MaximumLength(Cat.NameMaxLength);
            RuleFor(x => x.AdditionalRequirements).MaximumLength(Cat.AdditionalRequirementsMaxLength);
        }
    }
    
    internal sealed class UpdateCatCommandHandler(ApplicationDbContext db)
        : IRequestHandler<UpdateCatCommand>
    {
        public async Task Handle(UpdateCatCommand request, CancellationToken cancellationToken)
        {
            int numberOfUpdatedCats = await db.Cats
                .Where(x => x.Id == request.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x=>x.Name, request.Name)
                    .SetProperty(x=>x.IsCastrated, request.IsCastrated)
                    .SetProperty(x=>x.IsInNeedOfSeeingVet, request.IsInNeedOfSeeingVet)
                    .SetProperty(x=>x.AdditionalRequirements, request.AdditionalRequirements)
                    .SetProperty(x=>x.MedicalHelpUrgency, request.MedicalHelpUrgency)
                    .SetProperty(x=>x.AgeCategory, request.AgeCategory)
                    .SetProperty(x=>x.Behavior, request.Behavior)
                    .SetProperty(x=>x.HealthStatus, request.HealthStatus),
                    cancellationToken);
            if (numberOfUpdatedCats == 0)
            {
                throw new Cat.CatNotFoundException(request.Id);
            }
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("cats/{id:guid}", async
            (Guid id, 
            UpdateCatRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            request.ValidateAndRetrieve(
                out MedicalHelpUrgency medicalHelpUrgency,
                out AgeCategory ageCategory,
                out Behavior behavior,
                out HealthStatus healthStatus);
            
            UpdateCatCommand command = request.ToUpdateCommand(
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
    public static partial UpdateCat.UpdateCatCommand ToUpdateCommand(
        this UpdateCat.UpdateCatRequest request,
        Guid id,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus);
}
