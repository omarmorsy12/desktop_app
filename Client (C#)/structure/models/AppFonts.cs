using System.Windows.Media;

namespace app.structure.models
{
    public static class AppFonts
    {
        public static FontFamily HpSimplified { get { return App.getResource<FontFamily>("HpSimplified"); } }
        public static FontFamily Calibri { get { return App.getResource<FontFamily>("Calibri"); } }
    }
}
