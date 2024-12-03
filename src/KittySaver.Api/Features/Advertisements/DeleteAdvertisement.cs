using FluentValidation;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class DeleteAdvertisement : IEndpoint
{
    public sealed record DeleteAdvertisementCommand(Guid Id) : ICommand;

    public sealed class DeleteAdvertisementCommandValidator
        : AbstractValidator<DeleteAdvertisementCommand>
    {
        public DeleteAdvertisementCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class DeleteAdvertisementCommandHandler(
        IAdvertisementRepository advertisementRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteAdvertisementCommand>
    {
        public async Task Handle(DeleteAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement = await advertisementRepository.GetAdvertisementByIdAsync(request.Id, cancellationToken);
            advertisementRepository.Remove(advertisement);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("advertisements/{id:guid}", async (Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteAdvertisementCommand command = new(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}