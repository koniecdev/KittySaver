using KittySaver.Domain.Common;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Domain.Persons.DomainRepositories;

public interface IPersonRepository : IRepository<Person, PersonId>;