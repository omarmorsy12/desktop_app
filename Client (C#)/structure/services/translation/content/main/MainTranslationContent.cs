using System.Windows;

namespace app.structure.services.translation.content.main
{
    public partial class MainTranslationContent : TranslationContent
    {
        public MainTranslationContent()
        {
            InitializeComponent();
        }

        public ResourceDictionary getTranslationDictionary()
        {
            return content;
        }
    }
}
