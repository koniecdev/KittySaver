using KittySaver.Shared.Hateoas;

namespace KittySaver.Api.Infrastructure.Endpoints;

public static class EndpointNames
{
    public static class Discovery
    {
        public const string Group = "Discovery";
        private static class EndpointNames
        {
            public const string GetApiDiscoveryV1Name = "GetApiDiscoveryV1";
        }

        public static readonly EndpointInfo GetApiDiscoveryV1 = 
            new(EndpointNames.GetApiDiscoveryV1Name, EndpointRels.SelfRel, HttpVerbs.Get);
    }

    public static class Persons
    {
        public const string Group = "Persons";
        private static class EndpointNames
        {
            public const string CreatePersonName = "CreatePerson";
            public const string UpdatePersonName = "UpdatePerson";
            public const string DeletePersonName = "DeletePerson";
            public const string GetPersonName = "GetPerson";
            public const string GetPersonsName = "GetPersons";
        }

        public static readonly EndpointInfo Create = 
            new(EndpointNames.CreatePersonName, EndpointRels.Person.CreatePersonRel, HttpVerbs.Post);
        
        public static readonly EndpointInfo Update = 
            new(EndpointNames.UpdatePersonName, EndpointRels.Person.UpdatePersonRel, HttpVerbs.Put);
        
        public static readonly EndpointInfo Delete = 
            new(EndpointNames.DeletePersonName, EndpointRels.Person.DeletePersonRel, HttpVerbs.Delete);
        
        public static readonly EndpointInfo GetById = 
            new(EndpointNames.GetPersonName, EndpointRels.Person.GetPersonRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetAll = 
            new(EndpointNames.GetPersonsName, EndpointRels.Person.GetPersonsRel, HttpVerbs.Get);
    }

    public static class Cats
    {
        public const string Group = "Cats";
        private static class EndpointNames
        {
            public const string CreateCatName = "CreateCat";
            public const string UpdateCatName = "UpdateCat";
            public const string DeleteCatName = "DeleteCat";
            public const string GetCatName = "GetCat";
            public const string GetCatsName = "GetCats";
            public const string UpdateCatThumbnailName = "UpdateCatThumbnail";
            public const string GetCatThumbnailName = "GetCatThumbnail";
            public const string AddPicturesToCatGalleryName = "AddPicturesToCatGallery";
            public const string RemovePictureFromCatGalleryName = "RemovePictureFromCatGallery";
            public const string GetCatGalleryName = "GetCatGallery";
            public const string GetCatGalleryPictureName = "GetCatGalleryPicture";
        }

        public static readonly EndpointInfo Create = 
            new(EndpointNames.CreateCatName, EndpointRels.Cat.CreateCatRel, HttpVerbs.Post);
        
        public static readonly EndpointInfo Update = 
            new(EndpointNames.UpdateCatName, EndpointRels.Cat.UpdateCatRel, HttpVerbs.Put);
        
        public static readonly EndpointInfo Delete = 
            new(EndpointNames.DeleteCatName, EndpointRels.Cat.DeleteCatRel, HttpVerbs.Delete);
        
        public static readonly EndpointInfo GetById = 
            new(EndpointNames.GetCatName, EndpointRels.Cat.GetCatRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetAll = 
            new(EndpointNames.GetCatsName, EndpointRels.Cat.GetCatsRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo UpdateThumbnail = 
            new(EndpointNames.UpdateCatThumbnailName, EndpointRels.Cat.UpdateCatThumbnailRel, HttpVerbs.Put);
        
        public static readonly EndpointInfo GetThumbnail = 
            new(EndpointNames.GetCatThumbnailName, EndpointRels.Cat.GetCatThumbnailRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo AddPictures = 
            new(EndpointNames.AddPicturesToCatGalleryName, EndpointRels.Cat.AddPicturesToCatGalleryRel, HttpVerbs.Post);
        
        public static readonly EndpointInfo RemovePicture = 
            new(EndpointNames.RemovePictureFromCatGalleryName, EndpointRels.Cat.RemovePictureFromCatGalleryRel, HttpVerbs.Delete);
        
        public static readonly EndpointInfo GetGallery = 
            new(EndpointNames.GetCatGalleryName, EndpointRels.Cat.GetCatGalleryRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetGalleryPicture = 
            new(EndpointNames.GetCatGalleryPictureName, EndpointRels.Cat.GetCatGalleryPictureRel, HttpVerbs.Get);
    }

    public static class Advertisements
    {
        public const string Group = "Advertisements";
        private static class EndpointNames
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

        public static readonly EndpointInfo Create = 
            new(EndpointNames.CreateAdvertisementName, EndpointRels.Advertisement.CreateAdvertisementRel, HttpVerbs.Post);
        
        public static readonly EndpointInfo Update = 
            new(EndpointNames.UpdateAdvertisementName, EndpointRels.Advertisement.UpdateAdvertisementRel, HttpVerbs.Put);
        
        public static readonly EndpointInfo Delete = 
            new(EndpointNames.DeleteAdvertisementName, EndpointRels.Advertisement.DeleteAdvertisementRel, HttpVerbs.Delete);
        
        public static readonly EndpointInfo GetPublicById = 
            new(EndpointNames.GetPublicAdvertisementName, EndpointRels.Advertisement.GetPublicAdvertisementRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetPublicAll = 
            new(EndpointNames.GetPublicAdvertisementsName, EndpointRels.Advertisement.GetPublicAdvertisementsRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetPublicPersonAdvertisements = 
            new(EndpointNames.GetPublicAdvertisementsName, EndpointRels.Advertisement.GetPublicPersonAdvertisementsRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetById = 
            new(EndpointNames.GetAdvertisementName, EndpointRels.Advertisement.GetAdvertisementRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetAll = 
            new(EndpointNames.GetAdvertisementsName, EndpointRels.Advertisement.GetAdvertisementsRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetPersonAdvertisements = 
            new(EndpointNames.GetAdvertisementsName, EndpointRels.Advertisement.GetPersonAdvertisementsRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetPersonAdvertisement = 
            new(EndpointNames.GetAdvertisementName, EndpointRels.Advertisement.GetPersonAdvertisementRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo Close = 
            new(EndpointNames.CloseAdvertisementName, EndpointRels.Advertisement.CloseAdvertisementRel, HttpVerbs.Post);
        
        public static readonly EndpointInfo Expire = 
            new(EndpointNames.ExpireAdvertisementName, EndpointRels.Advertisement.ExpireAdvertisementsRel, HttpVerbs.Post);
        
        public static readonly EndpointInfo Refresh = 
            new(EndpointNames.RefreshAdvertisementName, EndpointRels.Advertisement.RefreshAdvertisementRel, HttpVerbs.Post);
        
        public static readonly EndpointInfo ReassignCats = 
            new(EndpointNames.ReassignCatsToAdvertisementName, EndpointRels.Advertisement.ReassignCatsToAdvertisementRel, HttpVerbs.Put);
        
        public static readonly EndpointInfo UpdateThumbnail = 
            new(EndpointNames.UpdateAdvertisementThumbnailName, EndpointRels.Advertisement.UpdateAdvertisementThumbnailRel, HttpVerbs.Put);
        
        public static readonly EndpointInfo GetThumbnail = 
            new(EndpointNames.GetAdvertisementThumbnailName, EndpointRels.Advertisement.GetAdvertisementThumbnailRel, HttpVerbs.Get);
        
        public static readonly EndpointInfo GetAdvertisementCats = 
            new(EndpointNames.GetAdvertisementCatsName, EndpointRels.Advertisement.GetAdvertisementCatsRel, HttpVerbs.Get);
    }
    
    private static class HttpVerbs
    {
        public const string Get = "GET";
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Delete = "DELETE";
    }
}