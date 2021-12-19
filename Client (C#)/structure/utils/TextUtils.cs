using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace app.structure.utils
{
    public static class TextUtils
    {
        public static Size getTextDimensions(string text, Typeface typeFace , double fontsize)
        {
            FormattedText formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeFace,
                fontsize,
                Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode.Display
            );

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
