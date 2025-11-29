using System.Globalization;
using System.Reflection;
using System.Windows;

namespace MailAgent;

internal static class LocalizationHelper
{
    private static readonly string[] AllowedCultures = ["en-US", "ru-RU"];
    public static event EventHandler? LanguageChanged;
    public static ResourceDictionary? CurrentDictionary;

    public static void SetLanguage(CultureInfo culture)
    {
        if (!AllowedCultures.Contains(culture.Name)) return;
        Thread.CurrentThread.CurrentUICulture = culture;
        CurrentDictionary = new ResourceDictionary
        {
            Source = GetDictionarySource(culture.Name)
        };
        LanguageChanged?.Invoke(Application.Current, EventArgs.Empty);
    }
    
    public static string GetString(string key)
    {
        if (CurrentDictionary?[key] != null) return CurrentDictionary[key]?.ToString() ?? key;
        return key;
    }
    
    private static Uri GetDictionarySource(string cultureName)
    {
        var moduleName =  Assembly.GetExecutingAssembly().GetName().Name;
        var uri = new Uri($"pack://application:,,,/{moduleName};component/Languages/lang.{cultureName}.xaml", UriKind.Absolute);
        try
        {
            // Проверяем, существует ли ресурс
            var dict = new ResourceDictionary();
            dict.Source = uri;
        }
        catch (Exception)
        {
            uri = new Uri($"pack://application:,,,/{moduleName};component/Languages/lang.xaml", UriKind.Absolute);
        }

        return uri;
    }

}