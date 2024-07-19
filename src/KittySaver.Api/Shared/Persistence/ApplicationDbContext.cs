using KittySaver.Api.Shared.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Persistence;

internal sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<Person, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Person> Persons => Set<Person>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
    }
}