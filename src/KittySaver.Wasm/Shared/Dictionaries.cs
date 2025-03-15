namespace KittySaver.Wasm.Shared;

public static class Dictionaries
{
    private const string BtnGreenClassName = "btn_green";
    private const string BtnDarkGreenClassName = "btn_darkgreen";
    private const string BtnOrangeClassName = "btn_orange";
    private const string BtnRedClassName = "btn_red";
    
    public static readonly Dictionary<AdvertisementStatus, (string text, string className)> AdvertisementStatusDictionary = new()
    {
        { AdvertisementStatus.Active, ("Aktywne", BtnGreenClassName) },
        { AdvertisementStatus.Closed, ("Zakończone", BtnDarkGreenClassName) },
        { AdvertisementStatus.Expired, ("Wygasłe", BtnRedClassName) },
        { AdvertisementStatus.ThumbnailNotUploaded, ("Brak wymaganej miniaturki", BtnRedClassName) },
    };
    public static readonly Dictionary<bool, (string text, string className)> IsAssignedToAdvertisementDictionary = new()
    {
        { true, ("Tak - kliknij <strong>tutaj</strong> aby wyświetlić", BtnGreenClassName) },
        { false, ("Nie - należy go dodać", BtnRedClassName) }
    };
    
    public static readonly Dictionary<bool, (string text, string className)> IsCastratedDictionary = new()
    {
        { true, ("Tak", "") },
        { false, ("Nie - należy to zrobić", BtnRedClassName) }
    };

    public static readonly Dictionary<string, (string text, string className)> HealthStatusDictionary = new()
    {
        { "Good", ("Dobry", "") },
        { "Unknown", ("Nieznany", "") },
        { "ChronicMinor", ("Przewlekle chory - stabilny", BtnOrangeClassName) },
        { "ChronicSerious", ("Poważnie przewlekle chory", BtnRedClassName) },
        { "Terminal", ("Nieuleczalnie chory", BtnRedClassName) }
    };

    public static readonly Dictionary<string, (string text, string className)> MedicalHelpUrgencyDictionary = new()
    {
        { "NoNeed", ("Kot nie potrzebuje wizyty", "")},
        { "ShouldSeeVet", ("Stan niepokojący - zalecana wizyta", BtnOrangeClassName)},
        { "HaveToSeeVet", ("Bardzo pilna - jak najszybsza", BtnRedClassName)}
    };
    
    public static readonly Dictionary<string, (string text, string className)> AgeCategoryDictionary = new()
    {
        { "Baby", ("Młody", "")},
        { "Adult", ("Dorosły", "")},
        { "Senior", ("Starszy", "")}
    };
    
    public static readonly Dictionary<string, (string text, string className)> BehaviourDictionary = new()
    {
        { "Friendly", ("Przyjazny dla ludzi", "")},
        { "Unfriendly", ("Płochliwy od ludzi", "")},
        { "Aggressive", ("Agresywny", "")}
    };
}