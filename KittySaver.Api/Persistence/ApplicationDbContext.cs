using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    
}