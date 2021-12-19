using System.Windows.Media;

namespace app.structure.models
{
    public static class AppColors
    {
        public static Color BlueColor {get{return App.getResource<Color>("BlueColor");}}

        public static Color LightBlueColor { get { return App.getResource<Color>("LightBlueColor"); } }

        public static Color RedColor {get{return App.getResource<Color>("RedColor");}}

        public static Color OrangeColor { get{return App.getResource<Color>("OrangeColor");}}

        public static Color PurpleColor { get{return App.getResource<Color>("PurpleColor");}}

        public static Color PinkColor { get { return App.getResource<Color>("PinkColor"); } }

        public static Color LighterGreyColor { get { return App.getResource<Color>("LighterGreyColor"); } }

        public static Color LightGreyColor { get { return App.getResource<Color>("LightGreyColor"); } }

        public static Color GreyColor { get { return App.getResource<Color>("GreyColor"); } }

        public static Color DarkGreyColor { get{return App.getResource<Color>("DarkGreyColor");} }

        public static Color DarkerGreyColor { get { return App.getResource<Color>("DarkerGreyColor"); } }

        public static Color BlackColor { get { return App.getResource<Color>("BlackColor"); } }

    }
}
