using app.structure.events;
using app.structure.services;
using app.structure.services.translation;
using app.structure.utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace app.shared_components.inputs
{
    /// <summary>
    /// Interaction logic for ButtonComponent.xaml
    /// </summary>

    public partial class FixedButtonComponent : UserControl
    {
        public static Size MEDIUM_SIZE = new Size(273, 37);
        public static Size SMALL_SIZE = new Size(250, 35);

        public bool isLoading = false;
        public Events<Grid> events;

        private AnimationService animation;
        private TranslationService translation;

        private Timer loadingTimer;

        private string lastTranslationKey = "";
        private bool ignoreDefaultHoverEffect = false;

        public FixedButtonComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                animation = Services.getService<AnimationService>();
                translation = Services.getService<TranslationService>();

                text.Content = translation.translate(lastTranslationKey);

                loadingTimer = new Timer((tickCount, timer) =>
                {
                    RotateTransform rotate = loadingIcon.RenderTransform as RotateTransform;
                    double rotateValue = (rotate != null ? rotate.Angle : 0) + 10;
                    RotateTransform transform = new RotateTransform(rotateValue);
                    transform.CenterX = 19.0d / 2.0d;
                    transform.CenterY = 19.0d / 2.0d;
                    loadingIcon.RenderTransform = transform;
                }, TimeSpan.FromMilliseconds(0.5));

                events = new Events<Grid>(container).addHoverEvent((e) => {
                    if (!isLoading)
                    {
                        container.Opacity = e.isOverComponent ? 1 : 0.8;
                    }
                });

                TranslationService.changed += onLanguageChange;
                Unloaded += onUnloaded;
            }
        }

        private void onUnloaded(object sender, RoutedEventArgs e)
        {
            TranslationService.changed -= onLanguageChange;
        }

        private void onLanguageChange(Languages lang)
        {
            text.Content = translation.translate(lastTranslationKey);
        }

        public void disable()
        {
            container.IsHitTestVisible = false;
            if (!ignoreDefaultHoverEffect)
            {
                container.Opacity = 1;
            }
        }

        public void enable()
        {
            container.IsHitTestVisible = true;
            if (!ignoreDefaultHoverEffect)
            {
                container.Opacity = container.IsMouseOver ? 1 : 0.8;
            }
        }

        public void setText(string textTranslationKey)
        {
            lastTranslationKey = textTranslationKey;
            text.Content = translation.translate(lastTranslationKey);
        }

        public void setSize(Size size)
        {
            container.Width = size.Width;
            container.Height = size.Height;
        }

        public void setBackgroundColor(Color color)
        {
            background.Fill = new SolidColorBrush(color);
        }

        public void setConfig(string textTranslationKey, Color backgroundColor, Size size)
        {
            setText(textTranslationKey);
            setBackgroundColor(backgroundColor);
            setSize(size);
        }

        public void clearDefaultHoverEvent()
        {
            events.releaseEvents(EventType.HOVER_OFF);
            events.releaseEvents(EventType.HOVER_ON);
            container.Opacity = 1;
            ignoreDefaultHoverEffect = true;
        }

        public void loading(bool start = true)
        {
            isLoading = start;
            string translatedText = translation.translate(start ? "loading" : lastTranslationKey);

            if (start)
            {
                translatedText = translatedText.ToUpper();
            }

            text.Content = translatedText;


            loadingIcon.Opacity = start ? 1 : 0;
            container.Opacity = start || ignoreDefaultHoverEffect ? 1 : 0.8;

            if (start)
            {
                loadingIcon.Margin = new Thickness(0, 0, container.Width * 0.15, 0);
                loadingTimer.start();
            }
            else
            {
                loadingTimer.stop();
                loadingIcon.RenderTransform = new RotateTransform(0);
            }
        }
    }
}
