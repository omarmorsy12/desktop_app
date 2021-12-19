using app.structure.events;
using app.structure.models;
using app.structure.services;
using app.structure.services.translation;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace app.shared_components.inputs
{
    /// <summary>
    /// Interaction logic for DropdownComponent.xaml
    /// </summary>
    public partial class DropdownComponent : UserControl
    {
        public delegate void OnValueChange();

        public class DropdownListItem
        {
            public string label { get; }
            public object value { get; }

            public DropdownListItem(string label, Type value)
            {
                this.label = label;

                this.value = value;
            }
        }

        public event OnValueChange valueChanged;

        public object selectedValue;
        TextBlock selectedElement;

        Dictionary<TextBlock, string> labels;
        
        AnimationService animation;
        TranslationService translation;

        Window parentWindow;
        
        Color staticColor = AppColors.DarkGreyColor;
        Color selectColor = AppColors.BlueColor;

        public DropdownComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                animation = Services.getService<AnimationService>();
                translation = Services.getService<TranslationService>();

                TranslationService.changed += onLanguageChange;

                new Events<Rectangle>(btn)
                    .addEvent(EventType.CLICK, (e) => toggle())
                    .addHoverEvent((e) => {
                        selectedDisplay.Foreground = new SolidColorBrush(e.isOverComponent ? selectColor : staticColor);
                    });

                new Events<Grid>(icons)
                    .addEvent(EventType.CLICK, (e) => toggle())
                    .addHoverEvent((e) => iconsHover(e.isOverComponent));

                DockPanel.SetDock(icons, TranslationService.language == Languages.AR ? Dock.Left : Dock.Right);

                Events.onMouseClick += onMouseClick;

                popup.Opened += onOpened;
                popup.Closed += onClosed;

                Loaded += onLoaded;

                Unloaded += onUnload;
            }
        }

        private void onLoaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(this);
            parentWindow.Deactivated += onLostFocus;
        }

        private void onLostFocus(object sender, EventArgs e)
        {
            popup.IsOpen = false;
        }

        private void onClosed(object sender, EventArgs e)
        {
            selectedDisplay.Foreground = new SolidColorBrush(btn.IsMouseOver ? selectColor : staticColor);
            icons.RenderTransform = null;
        }

        private void onOpened(object sender, EventArgs e)
        {
            RotateTransform transform = new RotateTransform(180);
            transform.CenterX = icons.ActualWidth / 2;
            transform.CenterY = icons.ActualHeight / 2;
            icons.RenderTransform = transform;
        }

        private void onMouseClick(FrameworkElement element)
        {
            if (element != icons && element != btn)
            {
                popup.IsOpen = false;
            }
        }

        private void onUnload(object sender, RoutedEventArgs e)
        {
            TranslationService.changed -= onLanguageChange;
            Events.onMouseClick -= onMouseClick;

            if (parentWindow != null)
            {
                parentWindow.Deactivated -= onLostFocus;
            }
        }

        private void onLanguageChange(Languages lang)
        {
            DockPanel.SetDock(icons, lang == Languages.AR ? Dock.Left : Dock.Right);

            foreach(TextBlock element in listContainer.Children)
            {
                string translatedText = translation.translate(labels[element]);
                element.Text = translatedText != null ? translatedText : labels[element];
            }
        }

        private void toggle()
        {
           popup.IsOpen = !popup.IsOpen;
        }

        private void iconsHover(bool isOn)
        {
            icons.Children[0].Opacity = isOn ? 0 : 1;
            icons.Children[1].Opacity = isOn ? 1 : 0;
            icons.Background = new SolidColorBrush(isOn ? AppColors.BlueColor : AppColors.LighterGreyColor);
        }
        
        public void init(DropdownListItem[] items = null)
        {
            bool hasList = items != null && items.Length > 0;

            listContainer.Children.Clear();

            labels = new Dictionary<TextBlock, string>();

            if (hasList)
            {
                foreach (DropdownListItem item in items)
                { 
                    TextBlock text = new TextBlock();

                    string label = item.label;

                    if (listContainer.Children.Count == 0)
                    {
                        text.Visibility = Visibility.Collapsed;
                        selectedElement = text;
                        selectedDisplay.Text = label;
                        selectedValue = item.value;
                    }

                    text.Padding = new Thickness(11);
                    text.TextTrimming = TextTrimming.WordEllipsis;
                    text.HorizontalAlignment = HorizontalAlignment.Center;
                    text.VerticalAlignment = VerticalAlignment.Center;
                    text.TextAlignment = TextAlignment.Center;
                    text.FontSize = 16;
                    text.FontFamily = AppFonts.HpSimplified;
                    text.Foreground = new SolidColorBrush(AppColors.GreyColor);
                    text.Text = label;
                    text.Background = new SolidColorBrush(staticColor);
                    
                    Binding binding = new Binding("ActualWidth");
                    binding.ElementName = "container";

                    text.SetBinding(WidthProperty, binding);

                    new Events<TextBlock>(text)
                        .addHoverEvent((e) => {
                            text.Foreground = new SolidColorBrush(e.isOverComponent ? Colors.White : AppColors.GreyColor);
                            text.Background = new SolidColorBrush(e.isOverComponent ? AppColors.BlueColor : AppColors.DarkGreyColor);
                        })
                        .addEvent(EventType.CLICK, (e) =>
                    {
                        selectedValue = item.value;
                        string translatedText = translation.translate(item.label);
                        selectedDisplay.Text = translatedText != null ? translatedText : item.label;
                        popup.IsOpen = false;
                        if (selectedElement != null)
                        {
                            selectedElement.Visibility = Visibility.Visible;
                        }
                        selectedElement = text;
                        selectedElement.Visibility = Visibility.Collapsed;
                        valueChanged?.Invoke();
                    });

                    labels.Add(text, item.label);
                    listContainer.Children.Add(text);
                }
            }
        }
        
    }
}
