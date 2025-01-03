using System.Net.Http.Json;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
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
            PagedList<CatResponse>? cats = await httpClient.GetFromJsonAsync<PagedList<CatResponse>>($"api/v1/persons/{person.Id}/cats");
            if (cats is not null)
            {
                foreach (CatResponse cat in cats.Items)
                {
                    var msg2 = await httpClient.DeleteAsync($"api/v1/persons/{person.Id}/cats/{cat.Id}");
                    if (!msg2.IsSuccessStatusCode)
                    {
                        ProblemDetails msg2ProblemDetail = await msg2.Content.ReadFromJsonAsync<ProblemDetails>() ?? throw new Exception();
                    }
                }
            }
            var msg3 = await httpClient.DeleteAsync($"api/v1/persons/{person.Id}");
            if (!msg3.IsSuccessStatusCode)
            {
                ProblemDetails msg3ProblemDetail = await msg3.Content.ReadFromJsonAsync<ProblemDetails>() ?? throw new Exception();
            }
        }
    }
}