using System.Windows;

namespace app.structure.services.translation.content.login
{
    public partial class LoginTranslationContent : TranslationContent
    {
        public LoginTranslationContent()
        {
            InitializeComponent();
        }

        public ResourceDictionary getTranslationDictionary()
        {
            return content;
        }
    }
}
