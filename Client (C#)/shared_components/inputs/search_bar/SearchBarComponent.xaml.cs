using app.structure.animations.configs;
using app.structure.events;
using app.structure.models;
using app.structure.services;
using app.structure.services.translation;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace app.shared_components.inputs
{
    /// <summary>
    /// Interaction logic for SearchBarComponent.xaml
    /// </summary>
    public partial class SearchBarComponent : UserControl
    {
        private TranslationService translation;
        private AnimationService animation;

        private string translationText = "quick_search";

        public delegate void SearchEvent(string term);
        public event SearchEvent onSearch;

        private Events<Rectangle> searchIconEvent;
        private bool searchOnTextChange;

        public SearchBarComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                translation = Services.getService<TranslationService>();
                animation = Services.getService<AnimationService>();

                new Events<Grid>(container).addHoverEvent((e) =>
                {
                    if (!textInput.IsFocused)
                    {
                        setContainerBackground(e.isOverComponent);
                    }
                });

                new Events<TextBox>(textInput)
                    .addFocusEvent((e) => {
                        if (e.component.IsFocused)
                        {
                            setContainerBackground(true);
                        } else if (!container.IsMouseOver)
                        {
                            setContainerBackground(false);
                        }
                        textInput.Foreground = new SolidColorBrush(e.component.IsFocused ? AppColors.BlueColor : AppColors.GreyColor);
                    });

                textInput.TextChanged += (sender, obj) => onTextChange();

                initSearchIconEvents();

                new Events<Rectangle>((Rectangle)clearIcon.Children[0]).addHoverEvent(applyIconHover).addEvent(EventType.CLICK, (e) => {
                    if (!string.IsNullOrEmpty(textInput.Text))
                    {
                        textInput.Text = "";
                    }
                });

                onLanguageChange(TranslationService.language);

                TranslationService.changed += onLanguageChange;

                Unloaded += onUnloaded;
            }
        }

        private void initSearchIconEvents()
        {
            if (searchIconEvent != null)
            {
                searchIconEvent.releaseAllEvents();
            } else
            {
                searchIconEvent = new Events<Rectangle>((Rectangle)searchIcon.Children[0]);
            }
            searchIconEvent.addHoverEvent(applyIconHover).addEvent(EventType.CLICK, (e) => {
                onSearch?.Invoke(textInput.Text);
            });
        }

        private void onUnloaded(object sender, RoutedEventArgs e)
        {
            TranslationService.changed -= onLanguageChange;
        }

        private void onLanguageChange(Languages lang)
        {
            bool isArabic = lang == Languages.AR;

            DockPanel.SetDock(searchIcon, isArabic ? Dock.Right : Dock.Left);
            DockPanel.SetDock(clearIcon, isArabic ? Dock.Left : Dock.Right);

            placeholderText.TextAlignment = isArabic ? TextAlignment.Right : TextAlignment.Left;

            textInput.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            placeholderText.Text = translation.translate(translationText);
        }

        private void onTextChange()
        {
            bool isEmpty = textInput.Text.Length == 0;
            animation.opacity.start(placeholderText, new ValueAnimationConfig<double>(TimeSpan.FromMilliseconds(isEmpty ? 200 : 1), isEmpty ? 1 : 0));
            if (searchOnTextChange)
            {
                onSearch?.Invoke(textInput.Text);
            }
        }

        private void setContainerBackground(bool on)
        {
            container.Background = new SolidColorBrush(on ? AppColors.BlueColor : AppColors.GreyColor);
        }

        private void applyIconHover(EventParams<Rectangle> eventParams)
        {
            Grid icon = (Grid)eventParams.component.Parent;
            icon.Children[1].Opacity = eventParams.isOverComponent ? 0 : 1;
            icon.Children[2].Opacity = eventParams.isOverComponent ? 1 : 0;

            eventParams.component.Fill = new SolidColorBrush(eventParams.isOverComponent ? AppColors.BlueColor : Colors.White);
        }

        private void setTranslationText(string text)
        {
            translationText = text;
            placeholderText.Text = translation.translate(translationText);
        }

        public void setSearchMode(SearchMode mode)
        {
            switch(mode)
            {
                case SearchMode.INSTANTLY:
                    searchIconEvent.releaseAllEvents();
                    searchIconEvent.addEvent(EventType.CLICK, (e) => textInput.Focus());
                    searchOnTextChange = true;
                    break;
                case SearchMode.DEFAULT:
                    initSearchIconEvents();
                    searchOnTextChange = false;
                    break;
            }
        }

        public enum SearchMode
        {
            INSTANTLY,
            DEFAULT
        }

    }
}
