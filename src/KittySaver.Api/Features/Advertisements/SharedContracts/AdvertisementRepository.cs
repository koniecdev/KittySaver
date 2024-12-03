using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements.SharedContracts;

public class AdvertisementRepository(ApplicationWriteDbContext writeDb) : IAdvertisementRepository
{
    public async Task<Advertisement> GetAdvertisementByIdAsync(Guid id, CancellationToken cancellationToken)
        => await writeDb.Advertisements
               .FirstOrDefaultAsync(advertisement => advertisement.Id == id, cancellationToken)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(id);

    public void Insert(Advertisement advertisement) => writeDb.Advertisements.Add(advertisement);
    
    public void Remove(Advertisement advertisement)
    {
        writeDb.Remove(advertisement);
        advertisement.AnnounceDeletion();
    }
}