namespace KittySaver.Api.Shared.Endpoints;

public static class EndpointNames
{
    public const string SelfRel = "self";
    
    public static readonly EndpointInfo GetApiDiscoveryV1 = 
        new(ApiDiscovery.GetApiDiscoveryV1Name, SelfRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo CreatePerson = 
        new(Person.CreatePersonName, Person.CreatePersonRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo UpdatePerson = 
        new (Person.UpdatePersonName, Person.UpdatePersonRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo DeletePerson = 
        new (Person.DeletePersonName, Person.DeletePersonRel, HttpVerbs.Delete);
    
    public static readonly EndpointInfo GetPerson = 
        new (Person.GetPersonName, Person.GetPersonRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetPersons = 
        new (Person.GetPersonsName, Person.GetPersonsRel, HttpVerbs.Get);
        
    public static readonly EndpointInfo CreateCat = 
        new (Cat.CreateCatName, Cat.CreateCatRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo UpdateCat = 
        new (Cat.UpdateCatName, Cat.UpdateCatRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo DeleteCat = 
        new (Cat.DeleteCatName, Cat.DeleteCatRel, HttpVerbs.Delete);
    
    public static readonly EndpointInfo GetCat = 
        new (Cat.GetCatName, Cat.GetCatRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetCats = 
        new (Cat.GetCatsName, Cat.GetCatsRel, HttpVerbs.Get);
        
    public static readonly EndpointInfo CreateAdvertisement = 
        new (Advertisement.CreateAdvertisementName, Advertisement.CreateAdvertisementRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo UpdateAdvertisement = 
        new (Advertisement.UpdateAdvertisementName, Advertisement.UpdateAdvertisementRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo DeleteAdvertisement = 
        new (Advertisement.DeleteAdvertisementName, Advertisement.DeleteAdvertisementRel, HttpVerbs.Delete);
    
    public static readonly EndpointInfo GetAdvertisement = 
        new (Advertisement.GetAdvertisementName, Advertisement.GetAdvertisementRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetAdvertisements = 
        new (Advertisement.GetAdvertisementsName, Advertisement.GetAdvertisementsRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetPersonAdvertisements = 
        new (Advertisement.GetAdvertisementsName, Advertisement.GetPersonAdvertisementsRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo CloseAdvertisement = 
        new (Advertisement.CloseAdvertisementName, Advertisement.CloseAdvertisementRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo ExpireAdvertisement = 
        new (Advertisement.ExpireAdvertisementName, Advertisement.ExpireAdvertisementsRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo RefreshAdvertisement = 
        new (Advertisement.RefreshAdvertisementName, Advertisement.RefreshAdvertisementRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo ReassignCatsToAdvertisement = 
        new (Advertisement.ReassignCatsToAdvertisementName, Advertisement.ReassignCatsToAdvertisementRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo UpdateAdvertisementThumbnail = 
        new (Advertisement.UpdateAdvertisementThumbnailName, Advertisement.UpdateAdvertisementThumbnailRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo GetAdvertisementThumbnail = 
        new (Advertisement.GetAdvertisementThumbnailName, Advertisement.GetAdvertisementThumbnailRel, HttpVerbs.Get);

    public static class GroupNames
    {
        public const string Discovery = "Discovery";
        public const string PersonGroup = "Persons";
        public const string CatGroup = "Cats";
        public const string AdvertisementGroup = "Advertisements";
    }
    
    private static class HttpVerbs
    {
        public const string Get = "GET";
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Delete = "DELETE";
    }

    private static class ApiDiscovery
    {
        public const string GetApiDiscoveryV1Name = "GetApiDiscoveryV1";
    }
    private static class Person
    {
        public const string CreatePersonName = "CreatePerson";
        public const string CreatePersonRel = "create-person";
        
        public const string UpdatePersonName = "UpdatePersonRel";
        public const string UpdatePersonRel = "update-person";
        
        public const string DeletePersonName = "DeletePerson";
        public const string DeletePersonRel = "delete-person";
        
        public const string GetPersonName = "GetPerson";
        public const string GetPersonRel = "get-person";
        
        public const string GetPersonsName = "GetPersons";
        public const string GetPersonsRel = "get-persons";
    }
    private static class Cat
    {
        public const string CreateCatName = "CreateCat";
        public const string CreateCatRel = "create-cat";
        
        public const string UpdateCatName = "UpdateCat";
        public const string UpdateCatRel = "update-cat";
        
        public const string DeleteCatName = "DeleteCat";
        public const string DeleteCatRel = "delete-cat";
        
        public const string GetCatName = "GetCat";
        public const string GetCatRel = "get-cat";
        
        public const string GetCatsName = "GetCats";
        public const string GetCatsRel = "get-cats";
    }
    private static class Advertisement
    {
        public const string CreateAdvertisementName = "CreateAdvertisement";
        public const string CreateAdvertisementRel = "create-advertisement";
        
        public const string UpdateAdvertisementName = "UpdateAdvertisementRel";
        public const string UpdateAdvertisementRel = "update-advertisement";
        
        public const string DeleteAdvertisementName = "DeleteAdvertisement";
        public const string DeleteAdvertisementRel = "delete-advertisement";
        
        public const string GetAdvertisementName = "GetAdvertisement";
        public const string GetAdvertisementRel = "get-advertisement";
        
        public const string GetAdvertisementsName = "GetAdvertisements";
        public const string GetAdvertisementsRel = "get-advertisements";
        public const string GetPersonAdvertisementsRel = "get-advertisements-by-personid";
        
        public const string CloseAdvertisementName = "CloseAdvertisement";
        public const string CloseAdvertisementRel = "close-advertisement";
        
        public const string ExpireAdvertisementName = "ExpireAdvertisement";
        public const string ExpireAdvertisementsRel = "expire-advertisement";
        
        public const string RefreshAdvertisementName = "RefreshAdvertisement";
        public const string RefreshAdvertisementRel = "refresh-advertisement";
        
        public const string ReassignCatsToAdvertisementName = "ReassignCatsToAdvertisement";
        public const string ReassignCatsToAdvertisementRel = "reassign-cats-to-advertisement";
        
        public const string UpdateAdvertisementThumbnailName = "UpdateAdvertisementThumbnailRel";
        public const string UpdateAdvertisementThumbnailRel = "update-advertisement-thumbnail";
        
        public const string GetAdvertisementThumbnailName = "GetAdvertisementThumbnailRel";
        public const string GetAdvertisementThumbnailRel = "get-advertisement-thumbnail";
    }
}