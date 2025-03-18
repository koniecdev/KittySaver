using KittySaver.Shared.Hateoas;

namespace KittySaver.Api.Shared.Endpoints;

public static class EndpointNames
{
    public static readonly EndpointInfo GetApiDiscoveryV1 = 
        new(ApiDiscovery.GetApiDiscoveryV1Name, EndpointRels.SelfRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo CreatePerson = 
        new(Person.CreatePersonName, EndpointRels.Person.CreatePersonRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo UpdatePerson = 
        new (Person.UpdatePersonName, EndpointRels.Person.UpdatePersonRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo DeletePerson = 
        new (Person.DeletePersonName, EndpointRels.Person.DeletePersonRel, HttpVerbs.Delete);
    
    public static readonly EndpointInfo GetPerson = 
        new (Person.GetPersonName, EndpointRels.Person.GetPersonRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetPersons = 
        new (Person.GetPersonsName, EndpointRels.Person.GetPersonsRel, HttpVerbs.Get);
    
        
    public static readonly EndpointInfo CreateCat = 
        new (Cat.CreateCatName, EndpointRels.Cat.CreateCatRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo UpdateCat = 
        new (Cat.UpdateCatName, EndpointRels.Cat.UpdateCatRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo DeleteCat = 
        new (Cat.DeleteCatName, EndpointRels.Cat.DeleteCatRel, HttpVerbs.Delete);
    
    public static readonly EndpointInfo GetCat = 
        new (Cat.GetCatName, EndpointRels.Cat.GetCatRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetCats = 
        new (Cat.GetCatsName, EndpointRels.Cat.GetCatsRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo UpdateCatThumbnail = 
        new (Cat.UpdateCatThumbnailName, EndpointRels.Cat.UpdateCatThumbnailRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo GetCatThumbnail = 
        new (Cat.GetCatThumbnailName, EndpointRels.Cat.GetCatThumbnailRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo AddPicturesToCatGallery = 
        new (Cat.AddPicturesToCatGalleryName, EndpointRels.Cat.AddPicturesToCatGalleryRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo RemovePictureFromCatGallery = 
        new (Cat.RemovePictureFromCatGalleryName, EndpointRels.Cat.RemovePictureFromCatGalleryRel, HttpVerbs.Delete);
    
    public static readonly EndpointInfo GetCatGallery = 
        new (Cat.GetCatGalleryName, EndpointRels.Cat.GetCatGalleryRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetCatGalleryPicture = 
        new (Cat.GetCatGalleryPictureName, EndpointRels.Cat.GetCatGalleryPictureRel, HttpVerbs.Get);
    
    
    public static readonly EndpointInfo CreateAdvertisement = 
        new (Advertisement.CreateAdvertisementName, EndpointRels.Advertisement.CreateAdvertisementRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo UpdateAdvertisement = 
        new (Advertisement.UpdateAdvertisementName, EndpointRels.Advertisement.UpdateAdvertisementRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo DeleteAdvertisement = 
        new (Advertisement.DeleteAdvertisementName, EndpointRels.Advertisement.DeleteAdvertisementRel, HttpVerbs.Delete);
    
    public static readonly EndpointInfo GetPublicAdvertisement = 
        new (Advertisement.GetPublicAdvertisementName, EndpointRels.Advertisement.GetPublicAdvertisementRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetPublicAdvertisements = 
        new (Advertisement.GetPublicAdvertisementsName, EndpointRels.Advertisement.GetPublicAdvertisementsRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetPublicPersonAdvertisements = 
        new (Advertisement.GetPublicAdvertisementsName, EndpointRels.Advertisement.GetPublicPersonAdvertisementsRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetAdvertisement = 
        new (Advertisement.GetAdvertisementName, EndpointRels.Advertisement.GetAdvertisementRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetAdvertisements = 
        new (Advertisement.GetAdvertisementsName, EndpointRels.Advertisement.GetAdvertisementsRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetPersonAdvertisements = 
        new (Advertisement.GetAdvertisementsName, EndpointRels.Advertisement.GetPersonAdvertisementsRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetPersonAdvertisement = 
        new (Advertisement.GetAdvertisementName, EndpointRels.Advertisement.GetPersonAdvertisementRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo CloseAdvertisement = 
        new (Advertisement.CloseAdvertisementName, EndpointRels.Advertisement.CloseAdvertisementRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo ExpireAdvertisement = 
        new (Advertisement.ExpireAdvertisementName, EndpointRels.Advertisement.ExpireAdvertisementsRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo RefreshAdvertisement = 
        new (Advertisement.RefreshAdvertisementName, EndpointRels.Advertisement.RefreshAdvertisementRel, HttpVerbs.Post);
    
    public static readonly EndpointInfo ReassignCatsToAdvertisement = 
        new (Advertisement.ReassignCatsToAdvertisementName, EndpointRels.Advertisement.ReassignCatsToAdvertisementRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo UpdateAdvertisementThumbnail = 
        new (Advertisement.UpdateAdvertisementThumbnailName, EndpointRels.Advertisement.UpdateAdvertisementThumbnailRel, HttpVerbs.Put);
    
    public static readonly EndpointInfo GetAdvertisementThumbnail = 
        new (Advertisement.GetAdvertisementThumbnailName, EndpointRels.Advertisement.GetAdvertisementThumbnailRel, HttpVerbs.Get);
    
    public static readonly EndpointInfo GetAdvertisementCats = 
        new (Advertisement.GetAdvertisementCatsName, EndpointRels.Advertisement.GetAdvertisementCatsRel, HttpVerbs.Get);

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
        public const string UpdatePersonName = "UpdatePerson";
        public const string DeletePersonName = "DeletePerson";
        public const string GetPersonName = "GetPerson";
        public const string GetPersonsName = "GetPersons";
    }
    private static class Cat
    {
        public const string CreateCatName = "CreateCat";
        public const string UpdateCatName = "UpdateCat";
        public const string DeleteCatName = "DeleteCat";
        public const string GetCatName = "GetCat";
        public const string GetCatsName = "GetCats";
        public const string UpdateCatThumbnailName = "UpdateCatThumbnail";
        public const string AddPicturesToCatGalleryName = "AddPicturesToCatGallery";
        public const string RemovePictureFromCatGalleryName = "RemovePictureFromCatGallery";
        public const string GetCatThumbnailName = "GetCatThumbnail";
        public const string GetCatGalleryName = "GetCatGallery";
        public const string GetCatGalleryPictureName = "GetCatGalleryPicture";
    }
    private static class Advertisement
    {
        public const string CreateAdvertisementName = "CreateAdvertisement";
        public const string UpdateAdvertisementName = "UpdateAdvertisement";
        public const string DeleteAdvertisementName = "DeleteAdvertisement";
        public const string GetAdvertisementName = "GetAdvertisement";
        public const string GetAdvertisementCatsName = "GetAdvertisementCats";
        public const string GetAdvertisementsName = "GetAdvertisements";
        public const string GetPublicAdvertisementName = "GetPublicAdvertisement";
        public const string GetPublicAdvertisementsName = "GetPublicAdvertisements";
        public const string CloseAdvertisementName = "CloseAdvertisement";
        public const string ExpireAdvertisementName = "ExpireAdvertisement";
        public const string RefreshAdvertisementName = "RefreshAdvertisement";
        public const string ReassignCatsToAdvertisementName = "ReassignCatsToAdvertisement";
        public const string UpdateAdvertisementThumbnailName = "UpdateAdvertisementThumbnail";
        public const string GetAdvertisementThumbnailName = "GetAdvertisementThumbnail";
    }
}