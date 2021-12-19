using app.structure.events;
using app.structure.models;
using app.structure.services;
using app.structure.services.translation;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace app.shared_components
{
    /// <summary>
    /// Interaction logic for VerticalTabsComponent.xaml
    /// </summary>
    public partial class VerticalTabsComponent : UserControl
    {
        private Grid selected;

        private TranslationService translation;
        
        public delegate void TabChanged(string code, string previous);

        public event TabChanged changed;

        public VerticalTabsComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                translation = Services.getService<TranslationService>();

                TranslationService.changed += onTranslationChanged;

                Unloaded += onUnloaded;
            }
        }

        private void onUnloaded(object sender, RoutedEventArgs e)
        {
            TranslationService.changed -= onTranslationChanged;
        }

        private void onTranslationChanged(Languages lang)
        {
            foreach(Grid element in container.Children)
            {
                foreach(TextBlock textBlock in element.Children)
                {
                    string translatedLabel = translation.translate((string)element.Tag);
                    textBlock.Text = translatedLabel;
                }
            }
        }

        private Grid createTabElemen(string code, bool isSelected = false)
        {
            Grid element = new Grid();
            Thickness margin = container.Children.Count == 0 ? new Thickness(0) : new Thickness(0, 10, 0, 0);
            TextBlock textBlock = createTextBlock(code);

            element.Tag = code;
            element.Margin = margin;
            element.Children.Add(textBlock);
            element.Children.Add(createTextBlock(code, true));

            setSelection(element, isSelected);

            new Events<Grid>(element).addEvent(EventType.CLICK, (e) =>
            {
                if (selected != element)
                {
                    if (selected != null)
                    {
                        setSelection(selected, false);
                    }
                    string previous = selected != null ? (string)selected.Tag : null;
                    selected = element;
                    setSelection(selected, true);
                    changed?.Invoke(code, previous);
                }
            }).addHoverEvent((e) =>
            {
                if (selected != element)
                {
                    element.Background = new SolidColorBrush(e.isOverComponent ? AppColors.LightBlueColor : AppColors.LighterGreyColor);
                    textBlock.Foreground = new SolidColorBrush(e.isOverComponent ? Colors.White : AppColors.DarkGreyColor);
                }
            });

            if (isSelected)
            {
                selected = element;
            }

            return element;
        }

        private TextBlock createTextBlock(string code, bool isShadow = false)
        {
            string translatedLabel = translation.translate(code);

            TextBlock textBlock = new TextBlock();
            textBlock.FontFamily = AppFonts.HpSimplified;
            textBlock.FontSize = 16;
            textBlock.Padding = new Thickness(35, 15, 35, 15);
            textBlock.IsHitTestVisible = false;
            textBlock.Text = translatedLabel;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.Opacity = isShadow ? 0 : 1;
            textBlock.FontWeight = isShadow ? FontWeights.Bold : FontWeights.Normal;
            return textBlock;
        }

        public void init(List<string> codes)
        {
            container.Children.Clear();
            int index = 0;
            foreach(string code in codes)
            {
                container.Children.Add(createTabElemen(code, index == 0));
                ++index;
            }
        }

        public void setSelection(Grid element, bool selected)
        {
            TextBlock textBlock = (TextBlock)element.Children[0];
            textBlock.Foreground = new SolidColorBrush(selected ? Colors.White : AppColors.DarkGreyColor);
            textBlock.FontWeight = !selected ? FontWeights.Normal : FontWeights.Bold;

            element.Background = new SolidColorBrush(selected ? AppColors.BlueColor : AppColors.LighterGreyColor);
        }
    }
}
