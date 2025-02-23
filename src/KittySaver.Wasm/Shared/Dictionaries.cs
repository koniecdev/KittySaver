namespace KittySaver.Wasm.Shared;

public static class Dictionaries
{
    private const string BtnGreenClassName = "btn_green";
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
        { "Poor", ("Słaby", BtnOrangeClassName) },
        { "Critical", ("Krytyczny - potrzebna pomoc", BtnRedClassName) }
    };

    public static readonly Dictionary<string, (string text, string className)> MedicalHelpUrgencyDictionary = new()
    {
        { "NoNeed", ("Nie potrzebuje wizyty", BtnGreenClassName)},
        { "ShouldSeeVet", ("Należy udać się na wizytę", BtnOrangeClassName)},
        { "HaveToSeeVet", ("Bardzo pilna - jak najszybsza", BtnRedClassName)}
    };
    
    public static readonly Dictionary<string, (string text, string className)> AgeCategoryDictionary = new()
    {
        { "Baby", ("Młody", BtnGreenClassName)},
        { "Adult", ("Dorosły", BtnGreenClassName)},
        { "Senior", ("Starszy", BtnGreenClassName)}
    };
}