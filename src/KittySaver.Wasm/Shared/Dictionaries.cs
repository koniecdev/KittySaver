namespace KittySaver.Wasm.Shared;

public static class Dictionaries
{
    private const string BtnGreenClassName = "btn_green";
    private const string BtnDarkGreenClassName = "btn_darkgreen";
    private const string BtnOrangeClassName = "btn_orange";
    private const string BtnRedClassName = "btn_red";
    
    public static readonly Dictionary<bool, (string text, string className)> IsAssignedToAdvertisementDictionary = new()
    {
        { true, ("Tak - kliknij <strong>tutaj</strong> aby wyświetlić", BtnGreenClassName) },
        { false, ("Nie - należy go dodać", BtnRedClassName) }
    };
    
    public static readonly Dictionary<bool, (string text, string className)> IsCastratedDictionary = new()
    {
        { true, ("Tak", BtnGreenClassName) },
        { false, ("Nie - należy to zrobić", BtnRedClassName) }
    };

    public static readonly Dictionary<string, (string text, string className)> HealthStatusDictionary = new()
    {
        { "Good", ("Dobry", BtnGreenClassName) },
        { "Unknown", ("Nieznany", BtnDarkGreenClassName) },
        { "ChronicMinor", ("Przewlekle chory - stabilny", BtnOrangeClassName) },
        { "ChronicSerious", ("Poważnie przewlekle chory", BtnRedClassName) },
        { "Terminal", ("Nieuleczalnie chory", BtnRedClassName) }
    };

    public static readonly Dictionary<string, (string text, string className)> MedicalHelpUrgencyDictionary = new()
    {
        { "NoNeed", ("Kot nie potrzebuje wizyty", BtnGreenClassName)},
        { "ShouldSeeVet", ("Stan niepokojący - zalecana wizyta", BtnOrangeClassName)},
        { "HaveToSeeVet", ("Bardzo pilna - jak najszybsza", BtnRedClassName)}
    };
    
    public static readonly Dictionary<string, (string text, string className)> AgeCategoryDictionary = new()
    {
        { "Baby", ("Młody", BtnGreenClassName)},
        { "Adult", ("Dorosły", BtnGreenClassName)},
        { "Senior", ("Starszy", BtnGreenClassName)}
    };
    
    public static readonly Dictionary<string, (string text, string className)> BehaviourDictionary = new()
    {
        { "Friendly", ("Przyjazny dla ludzi", BtnGreenClassName)},
        { "Unfriendly", ("Płochliwy od ludzi", BtnOrangeClassName)},
        { "Aggressive", ("Agresywny", BtnRedClassName)}
    };
}