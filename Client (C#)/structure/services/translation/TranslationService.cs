using app.structure.services.translation;
using app.structure.services.translation.content;
using app.structure.services.translation.content.login;
using app.structure.services.translation.content.main;
using app.structure.services.translation.content.shared;
using app.structure.services.translation.content.startup;
using app.windows.login;
using app.windows.main;
using app.windows.startup;
using System.Collections.Generic;
using System.Windows;

namespace app.structure.services
{
    public class TranslationService : Services
    {
        private static Languages lang = Languages.EN;

        private TranslationContent content;
        private List<ResourceDictionary> sharedDictionaries;
        public delegate void LanguageChanged(Languages lang);

        public static event LanguageChanged changed;

        public static Languages language
        {
            get { return lang; }
            set
            {
                lang = value;
                changed?.Invoke(lang);
            }
        }

        public void loadTranslationContent(Window window = null)
        {
            SharedTranslationContent sharedContent = new SharedTranslationContent();

            content = null;
            changed = null;

            sharedDictionaries = sharedContent.getTranslationDictionaries(window);

            if (window is StartUpWindow)
            {
                content = new StartupTranslationContent();
            } else if (window is LoginWindow)
            {
                content = new LoginTranslationContent();
            } else if (window is MainWindow)
            {
                content = new MainTranslationContent();
            }
        }

        public string translate(string key, params string[] vars)
        {
            if (content == null && sharedDictionaries == null) 
            {
                return key;
            }

            string translatedString = content != null ? (string)getLanguageDictionary(content.getTranslationDictionary())[key] : null;
            string sharedTranslatedString = getSharedTranslation(key);

            string result = !string.IsNullOrEmpty(translatedString) ? translatedString : sharedTranslatedString;

            if (string.IsNullOrEmpty(result))
            {
                return key;
            }

            if (vars != null)
            {
                foreach(string var in vars)
                {
                    string[] splitter = var.Split(':');
                    string name = splitter[0];
                    string value = splitter[1];
                    string code = splitter.Length > 2 ? splitter[2] : "";
                    if (code == "N")
                    {
                        value = translateNumeric(value);
                    }
                    result = result.Replace("$" + name, value);
                }
            }

            return result;
        }

        public string translateNumeric(long value)
        {
            return translateNumeric(value + "");
        }

        public string translateNumeric(string value)
        {
            string shadow = value + "";
            string translated = value + "";

            for (int i = 0; i < 10; i++)
            {
                if (shadow == "")
                {
                    break;
                }

                string digit = i + "";

                if (translated.Contains(digit))
                {
                    translated = translated.Replace(digit, translate(digit));
                    shadow = shadow.Replace(digit, "");
                }
            }

            return translated;
        }

        public string translateByGender(string key, string gender, params string[] vars)
        {
            bool isFemale = gender == "female";
            string translatedText = translate(key, vars);

            if (translatedText.Contains("#"))
            {
                bool fullReplace = translatedText.Contains("##");

                if (fullReplace || !isFemale)
                {
                    return translatedText.Replace("##", "#").Split('#')[isFemale ? 1 : 0];
                }

                translatedText = translatedText.Replace("#", "");
            }

            return translatedText;
        }

        private string getSharedTranslation(string key)
        {
            if(sharedDictionaries == null)
            {
                return null;
            }

            ResourceDictionary dictionary = sharedDictionaries.Find((dic) => getLanguageDictionary(dic).Contains(key));

            if (dictionary != null)
            {
                return (string)getLanguageDictionary(dictionary)[key];
            }

            return null;
        }

        private ResourceDictionary getLanguageDictionary(ResourceDictionary translation)
        {
            object dictionary = null;
            switch (lang)
            {
                case Languages.AR:
                    dictionary = translation["Arabic"];
                    break;
                case Languages.EN:
                    dictionary = translation["English"];
                    break;
            }

            return (ResourceDictionary)dictionary;
        }
    }
}
