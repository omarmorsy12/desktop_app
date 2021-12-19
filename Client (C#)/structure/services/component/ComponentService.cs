using app.structure.services.translation;
using app.structure.utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace app.structure.services
{
    public class ComponentService : Services
    {
        public static bool isDesignMode {
            get {
                return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            }
        }

        public static bool isRuntimeMode
        {
            get
            {
                return LicenseManager.UsageMode == LicenseUsageMode.Runtime;
            }
        }

        public void setComponentImage(Image component, string uri)
        {
            BitmapImage image = ImageUtils.loadImage(uri);
            component.Source = image;
        }

        public Size getComponentTextDimensions(Label component)
        {
            Typeface typeFace = new Typeface(component.FontFamily, component.FontStyle, component.FontWeight, component.FontStretch);
            return TextUtils.getTextDimensions(component.Content.ToString(), typeFace, component.FontSize);
        }

        public Size getComponentTextDimensions(TextBlock component)
        {
            Typeface typeFace = new Typeface(component.FontFamily, component.FontStyle, component.FontWeight, component.FontStretch);
            return TextUtils.getTextDimensions(component.Text, typeFace, component.FontSize);
        }

        public void updateScrollViewer(ScrollViewer scrollArea, Languages lang)
        {
            bool isArabic = lang == Languages.AR;

            Grid grid = scrollArea.Template.FindName("grid", scrollArea) as Grid;

            if (grid == null)
            {
                scrollArea.ApplyTemplate();
                grid = scrollArea.Template.FindName("grid", scrollArea) as Grid;
            }
            
            ColumnDefinition scrollAreaContent = new ColumnDefinition();
            scrollAreaContent.Width = new GridLength(1, GridUnitType.Star);

            ColumnDefinition scrollAreaBar = new ColumnDefinition();
            scrollAreaBar.Width = new GridLength(1, GridUnitType.Auto);

            grid.ColumnDefinitions.Clear();

            grid.ColumnDefinitions.Add(isArabic ? scrollAreaBar : scrollAreaContent);
            grid.ColumnDefinitions.Add(isArabic ? scrollAreaContent : scrollAreaBar);

            ScrollBar bar = scrollArea.Template.FindName("PART_VerticalScrollBar", scrollArea) as ScrollBar;

            Grid.SetColumn((FrameworkElement)scrollArea.Template.FindName("content", scrollArea), isArabic ? 1 : 0);
            Grid.SetColumn(bar, isArabic ? 0 : 1);

        }
    }
}
