﻿using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services.FileServices;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Features.Advertisements;

public sealed class DeleteAdvertisement : IEndpoint
{
    public sealed record DeleteAdvertisementCommand(PersonId PersonId, AdvertisementId Id)
        : ICommand, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class DeleteAdvertisementCommandValidator : AbstractValidator<DeleteAdvertisementCommand>
    {
        public DeleteAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                // .WithMessage("'Person Id' cannot be empty.");
                .WithMessage("'Id osoby' nie może być puste.");
            RuleFor(x => x.Id)
                .NotEmpty()
                // .WithMessage("'Id' cannot be empty.");
                .WithMessage("'Id' nie może być puste.");
        }
    }


    internal sealed class DeleteAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IAdvertisementFileStorageService advertisementFileStorageService)
        : IRequestHandler<DeleteAdvertisementCommand>
    {
        public async Task Handle(DeleteAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetByIdAsync(request.PersonId, cancellationToken);
            owner.RemoveAdvertisement(request.Id);
            
            advertisementFileStorageService.DeleteThumbnail(request.Id);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("persons/{personId:guid}/advertisements/{id:guid}", async (
            Guid personId,
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteAdvertisementCommand command = new(new PersonId(personId), new AdvertisementId(id));
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization()
        .WithName(EndpointNames.Advertisements.Delete.EndpointName)
        .WithTags(EndpointNames.Advertisements.Group);
    }
}