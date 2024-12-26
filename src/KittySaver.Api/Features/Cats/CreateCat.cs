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

public sealed class CreateCat : IEndpoint
{
    public sealed record CreateCatRequest(
        string Name,
        bool IsCastrated,
        string MedicalHelpUrgency,
        string AgeCategory,
        string Behavior,
        string HealthStatus,
        string? AdditionalRequirements = null);
    
    public sealed record CreateCatCommand(
        Guid PersonId,
        string Name,
        bool IsCastrated,
        string MedicalHelpUrgency,
        string AgeCategory,
        string Behavior,
        string HealthStatus,
        string? AdditionalRequirements = null) : ICatCommand<Guid>;

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
        : IRequestHandler<CreateCatCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCatCommand request, CancellationToken cancellationToken)
        {
            Person person = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
        
            CatName catName = CatName.Create(request.Name);
            Description additionalRequirements = Description.Create(request.AdditionalRequirements);

            MedicalHelpUrgency medicalHelpUrgency = MedicalHelpUrgency.FromName(request.MedicalHelpUrgency, true);
            AgeCategory ageCategory = AgeCategory.FromName(request.AgeCategory, true);
            Behavior behavior = Behavior.FromName(request.Behavior, true);
            HealthStatus healthStatus = HealthStatus.FromName(request.HealthStatus, true);
            
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
    public static partial CreateCat.CreateCatCommand MapToCreateCatCommand(
        this CreateCat.CreateCatRequest request,
        Guid personId);
}
