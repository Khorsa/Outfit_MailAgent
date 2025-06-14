using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Collections.Concurrent;
using YamlDotNet.Core.Tokens;

namespace MailAgent
{
    internal class LocalizationHelper
    {
        public static readonly Dictionary<string, Language> Languages = new Dictionary<string, Language>();
        public static event EventHandler? LanguageChanged;

        public static ResourceDictionary CurrentDictionary;

        static LocalizationHelper()
        {
            Languages.Add("ru-RU", new Language("Русский", "ru-RU"));
            Languages.Add("en-US", new Language("English", "en-US"));

            LocalizationHelper.CurrentDictionary = new ResourceDictionary();
            LocalizationHelper.CurrentDictionary.Source = LocalizationHelper.Languages["ru-RU"].DictionarySource;
        }

        public static string GetString(string key)
        {
            if (LocalizationHelper.CurrentDictionary != null && LocalizationHelper.CurrentDictionary[key] != null)
            {
                return LocalizationHelper.CurrentDictionary[key].ToString() ?? key;
            }
            return key;
        }

        public static CultureInfo Language
        {
            get
            {
                return System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                if (LanguageChanged != null && LocalizationHelper.Languages.ContainsKey(value.Name))
                {
                    LocalizationHelper.CurrentDictionary = new ResourceDictionary();
                    LocalizationHelper.CurrentDictionary.Source = LocalizationHelper.Languages[value.Name].DictionarySource;

                    LanguageChanged(Application.Current, new EventArgs());
                }
            }
        }
    }

    public class Language(string Name, string CultureName)
    {
        public readonly string Name = Name;
        public readonly string CultureName = CultureName;

        public CultureInfo Culture
        {
            get
            {
                return new CultureInfo(CultureName);
            }
        }

        public Uri DictionarySource
        {
            get
            {
                var uri = new Uri(String.Format("pack://application:,,,/MailAgent;component/Languages/lang.{0}.xaml", CultureName), UriKind.Absolute);
                // Проверяем, существует ли ресурс
                try
                {
                    ResourceDictionary dict = new ResourceDictionary();
                    dict.Source = uri;
                }
                catch (Exception)
                {
                    uri = new Uri(String.Format("pack://application:,,,/MailAgent;component/Languages/lang.xaml", CultureName), UriKind.Absolute);
                }

                return uri;
            }
        }
    }
}
