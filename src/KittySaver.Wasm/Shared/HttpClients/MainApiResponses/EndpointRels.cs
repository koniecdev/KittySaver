namespace KittySaver.Wasm.Shared.HttpClients.MainApiResponses;

public static class EndpointRels
{
    public const string SelfRel = "self";

    public static class Person
    {
        public const string CreatePersonRel = "create-person";
        public const string UpdatePersonRel = "update-person";
        public const string DeletePersonRel = "delete-person";
        public const string GetPersonRel = "get-person";
        public const string GetPersonsRel = "get-persons";
    }

    public static class Cat
    {
        public const string CreateCatRel = "create-cat";
        public const string UpdateCatRel = "update-cat";
        public const string DeleteCatRel = "delete-cat";
        public const string GetCatRel = "get-cat";
        public const string GetCatsRel = "get-cats";
    }

    public static class Advertisement
    {
        public const string CreateAdvertisementRel = "create-advertisement";
        public const string UpdateAdvertisementRel = "update-advertisement";
        public const string DeleteAdvertisementRel = "delete-advertisement";
        public const string GetAdvertisementRel = "get-advertisement";
        public const string GetAdvertisementsRel = "get-advertisements";
        public const string GetPersonAdvertisementsRel = "get-advertisements-by-personid";
        public const string GetPublicAdvertisementRel = "get-public-advertisement";
        public const string GetPublicAdvertisementsRel = "get-public-advertisements";
        public const string GetPublicPersonAdvertisementsRel = "get-public-advertisements-by-personid";
        public const string CloseAdvertisementRel = "close-advertisement";
        public const string ExpireAdvertisementsRel = "expire-advertisement";
        public const string RefreshAdvertisementRel = "refresh-advertisement";
        public const string ReassignCatsToAdvertisementRel = "reassign-cats-to-advertisement";
        public const string UpdateAdvertisementThumbnailRel = "update-advertisement-thumbnail";
        public const string GetAdvertisementThumbnailRel = "get-advertisement-thumbnail";
    }
}