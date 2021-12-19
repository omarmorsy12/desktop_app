using app.windows.login;
using app.windows.main;
using app.windows.startup;
using System.Collections.Generic;
using System.Windows;

namespace app.structure.services.translation.content.shared
{
    public partial class SharedTranslationContent
    {

        public SharedTranslationContent()
        {
            InitializeComponent();
        }

        private string getViewerId(Window viewer)
        {
            if (viewer is LoginWindow)
            {
                return "login";
            } else if (viewer is StartUpWindow)
            {
                return "startup";
            } else if (viewer is MainWindow)
            {
                return "main";
            } else
            {
                return null;
            }
        }

        public List<ResourceDictionary> getTranslationDictionaries(Window viewer)
        {
            List<ResourceDictionary> dictionaries = new List<ResourceDictionary>();
            string viewerID = getViewerId(viewer);

            foreach (string name in content.Keys)
            {
                ResourceDictionary dictionary = (ResourceDictionary) content[name];

                bool isViewerIncluded = viewerID != null && dictionary["viewers"].ToString().Contains(viewerID);

                if (isViewerIncluded || dictionary["viewers"].ToString().Contains("all"))
                {
                    dictionaries.Add(dictionary);
                }
            }

            return dictionaries;
        }
    }
}
