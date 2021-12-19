using System.Windows;

namespace app.structure.services.translation.content.startup
{
    public partial class StartupTranslationContent : TranslationContent
    {
        public StartupTranslationContent()
        {
            InitializeComponent();
        }

        public ResourceDictionary getTranslationDictionary()
        {
            return content;
        }
    }
}
