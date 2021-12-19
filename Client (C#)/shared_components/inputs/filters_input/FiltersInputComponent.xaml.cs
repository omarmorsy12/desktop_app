using app.structure.events;
using app.structure.models;
using app.structure.services;
using app.structure.services.translation;
using app.structure.utils;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace app.shared_components.inputs
{
    /// <summary>
    /// Interaction logic for PillSelectionComponent.xaml
    /// </summary>
    public partial class FiltersInputComponent : UserControl
    {
        public class FilterConfig
        {
            public Color? color { get; }
            public string label { get; }
            
            public FilterConfig(string label, Color color)
            {
                this.label = label;
                this.color = color;
            }

            public FilterConfig(string label)
            {
                this.label = label;
            }

        }

        private class Filter
        {
            public FilterConfig config;
            public double width = 0;
            public Border element;

            public Filter(FilterConfig config)
            {
                this.config = config;
            }
            
            public Filter(FilterConfig config, Border element)
            {
                this.config = config;
                this.element = element;
                updateWidth();
            }

            public void updateWidth()
            {
                TextBlock text = (TextBlock)element.Child;
                Typeface typeFace = new Typeface(text.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                width = TextUtils.getTextDimensions(text.Text, typeFace, text.FontSize).Width + 40;
            }

        }

        private FontFamily defaultFont;

        private Color defaultColor;

        private string headerLabel;

        private double originWidth;

        private Filter more;

        private List<Filter> shownFilters = new List<Filter>();
        private List<Filter> hiddenFilters = new List<Filter>();
        
        private ComponentService componentService;
        private TranslationService translation;

        public FiltersInputComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                defaultFont = AppFonts.HpSimplified;

                componentService = Services.getService<ComponentService>();
                translation = Services.getService<TranslationService>();

                TranslationService.changed += onTranslationChange;
                Loaded += onLoaded; ;
                Unloaded += onUnloaded;
                setContentDock(TranslationService.language);
                
                new Events(filterIcon).addHoverEvent((e) =>
                {
                    filterIcon.Children[1].Opacity = e.isOverComponent ? 1 : 0;
                    filterIcon.Children[0].Opacity = e.isOverComponent ? 0 : 1;

                    filterIcon.Background = new SolidColorBrush(e.isOverComponent ? AppColors.BlueColor : AppColors.LighterGreyColor);
                });
            }
        }

        private void onLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)Parent;
            element.SizeChanged += onParentSizeChange;
        }

        private bool isShown(Filter filter)
        {
            return pillsContainer.Children.Contains(filter.element);
        }

        private void show(Filter filter, int? position = null)
        {
            hiddenFilters.Remove(filter);

            if (position != null)
            {
                filter.element.Margin = new Thickness(position == 0 ? 0 : 5, 0, 0, 0);
                pillsContainer.Children.Insert((int)position, filter.element);
                shownFilters.Insert((int)position, filter);
            }
            else
            {
                filter.element.Margin = new Thickness(pillsContainer.Children.Count == 0 ? 0 : 5, 0, 0, 0);
                pillsContainer.Children.Add(filter.element);
                shownFilters.Add(filter);
            }
        }

        private void hide(Filter filter)
        {
            shownFilters.Remove(filter);
            if (filter != more)
            {
                hiddenFilters.Add(filter);
            }
            pillsContainer.Children.Remove(filter.element);
        }

        private void onParentSizeChange(object sender, SizeChangedEventArgs e)
        {
            adjustContent(e.NewSize.Width);
        }

        public void adjustContent(double parentWidth)
        {
            double avaliableSpace = parentWidth - originWidth - Margin.Left - Margin.Right;

            shownFilters.ForEach(wrapper => avaliableSpace -= wrapper.width + 5);

            bool isMoreShown = isShown(more);

            if (avaliableSpace < 0 && shownFilters.Count > 1)
            {
                for (int index = shownFilters.Count - (isMoreShown ? 2 : 1); index >= 0; index--)
                {
                    Filter filter = shownFilters[index];

                    hide(filter);

                    if (avaliableSpace + filter.width - (isMoreShown ? 0 : more.width + 5) >= 0)
                    {
                        break;
                    }
                    else
                    {
                        avaliableSpace += filter.width;
                    }
                }
                if (!isMoreShown)
                {
                    show(more);
                }
            }
            else if (avaliableSpace > 0 && hiddenFilters.Count > 0)
            {

                for (int index = hiddenFilters.Count - 1; index >= 0; index--)
                {
                    Filter filter = hiddenFilters[index];
                    double addMoreWidth = hiddenFilters.Count == 1 ? more.width + 5 : 0;

                    if (avaliableSpace + addMoreWidth - filter.width - 5 >= 0)
                    {
                        show(filter, isMoreShown ? (int?)(shownFilters.Count - 1) : null);
                        avaliableSpace -= filter.width;
                    }
                    else
                    {
                        break;
                    }
                }

                if (isMoreShown && hiddenFilters.Count == 0)
                {
                    hide(more);
                }
            }
        }

        private void calculateWidth()
        {
            double labelWidth = componentService.getComponentTextDimensions(label).Width + 28;

            originWidth = filterIcon.Width + labelWidth + (pillsContainer.Children.Count != 0 ? 5 : 0);

            shownFilters.ForEach(config => config.updateWidth());
            hiddenFilters.ForEach(config => config.updateWidth());
        }

        private void onUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)Parent;
            TranslationService.changed -= onTranslationChange;
            element.SizeChanged -= onParentSizeChange; 
        }

        public void init(string headerLabel, Color defaultColor)
        {
            this.defaultColor = defaultColor;
            this.headerLabel = headerLabel;
            label.Text = translation.translate(headerLabel);

            FilterConfig moreConfig = new FilterConfig("...");
            more = new Filter(moreConfig, createFilter(moreConfig));

            calculateWidth();
        }

        private void onTranslationChange(Languages lang)
        {
            bool isArabic = lang == Languages.AR;

            label.Text = translation.translate(headerLabel);

            int index = 0;

            foreach (Border border in pillsContainer.Children)
            {
                TextBlock textBlock = (TextBlock)border.Child;
                string configLabel = shownFilters[index].config.label;
                string translated = translation.translate(configLabel);
                textBlock.Text = translated != null ? translated : configLabel;

                ++index;
            }

            setContentDock(lang);
            calculateWidth();
        }

        private void setContentDock(Languages lang)
        {
            bool isArabic = lang == Languages.AR;

            DockPanel.SetDock(label, isArabic ? Dock.Right : Dock.Left);
            DockPanel.SetDock(filterIcon, isArabic ? Dock.Left : Dock.Right);
            DockPanel.SetDock(pillsContainer, isArabic ? Dock.Right : Dock.Left);
        }

        public void addFilter(FilterConfig config)
        {
            bool isFirst = pillsContainer.Children.Count == 0;
            Border border = createFilter(config);

            if (isFirst)
            {
                pillsContainer.Margin = new Thickness(5);
                originWidth += 5;
            }

            Filter wrapper = new Filter(config, border);
            show(wrapper);
        }

        private Border createFilter(FilterConfig config)
        {
            string translatedLabel = translation.translate(config.label);

            Border border = new Border();
            border.CornerRadius = new CornerRadius(14);
            border.Background = new SolidColorBrush(config.color != null ? (Color)config.color : defaultColor);

            TextBlock textBlock = new TextBlock();
            textBlock.FontSize = 14;
            textBlock.FontFamily = defaultFont;
            textBlock.Margin = new Thickness(20, 0, 20, 0);
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.Text = translatedLabel != null ? translatedLabel : config.label;

            border.Child = textBlock;
            return border;
        }
    }

}
