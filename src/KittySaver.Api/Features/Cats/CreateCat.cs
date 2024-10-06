using FluentValidation;
using FluentValidation.Results;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.Enums.Common;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Cats;

public class CreateCat : IEndpoint
{
    public sealed record CreateCatRequest(
        Guid PersonId,
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

    public sealed class CreateCatCommandValidator 
        : AbstractValidator<CreateCatCommand>
    {
        public CreateCatCommandValidator()
        {
            RuleFor(x => x.PersonId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name).MaximumLength(Cat.NameMaxLength);
            RuleFor(x => x.AdditionalRequirements).MaximumLength(Cat.AdditionalRequirementsMaxLength);
        }
    }
    
    internal sealed class CreateCatCommandHandler(ApplicationDbContext db) : IRequestHandler<CreateCatCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCatCommand request, CancellationToken cancellationToken)
        {
            Cat entity = request.ToEntity();
            db.Cats.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("cats", async 
            (CreateCatRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            request.ValidateAndRetrieve(
                out MedicalHelpUrgency medicalHelpUrgency,
                out AgeCategory ageCategory,
                out Behavior behavior,
                out HealthStatus healthStatus);

            CreateCatCommand command = request.ToCreateCatCommand(medicalHelpUrgency,
                ageCategory,
                behavior,
                healthStatus);
            Guid personId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/persons/{personId}", new { Id = personId });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class CreateCatMapper
{
    public static partial CreateCat.CreateCatCommand ToCreateCatCommand(
        this CreateCat.CreateCatRequest request,
        MedicalHelpUrgency medicalHelpUrgency,
        AgeCategory ageCategory,
        Behavior behavior,
        HealthStatus healthStatus);
    
    public static partial Cat ToEntity(this CreateCat.CreateCatCommand command);
}
