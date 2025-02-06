using System.Net.Http.Json;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Pagination;
using KittySaver.Shared.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Tests.Integration.Helpers;

public class CleanupHelper(HttpClient httpClient)
{
    public async Task Cleanup()
    {
        await CleanupPersons();
    }
    
    private async Task CleanupPersons()
    {
        PagedList<PersonResponse>? persons = await httpClient.GetFromJsonAsync<PagedList<PersonResponse>>("api/v1/persons");
        if (persons is null)
        {
            return;
        }
        foreach (PersonResponse person in persons.Items)
        {
            HttpResponseMessage msg3 = await httpClient.DeleteAsync($"api/v1/persons/{person.Id}");
            if (!msg3.IsSuccessStatusCode)
            {
                ProblemDetails problemDetails = await msg3.Content.ReadFromJsonAsync<ProblemDetails>() ?? throw new Exception();
            }
        }
    }
}