using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Persistence.Repositories;

public class AdvertisementRepository(ApplicationDbContext db) : IAdvertisementRepository
{
    public async Task<IEnumerable<Advertisement>> GetAllAdvertisementsAsync()
        => await db.Advertisements
            .ToListAsync();

    public async Task<Advertisement> GetAdvertisementByIdAsync(Guid id)
        => await db.Advertisements
               .FirstOrDefaultAsync(advertisement => advertisement.Id == id)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(id);
}