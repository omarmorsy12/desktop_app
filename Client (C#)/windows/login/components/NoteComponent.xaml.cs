using app.structure.animations;
using app.structure.services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using app.structure.animations.configs;
using app.structure.services.translation;

namespace app.windows.login.components
{
    /// <summary>
    /// Interaction logic for NoteComponent.xaml
    /// </summary>
    public partial class NoteComponent : UserControl
    {
        MovementAnimation movement;
        TranslationService translationService;

        NoteTextTranslation lastTranslation;

        Thickness showMargin;

        public NoteComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                showMargin = new Thickness(Margin.Left, Margin.Top, Margin.Right, Margin.Bottom);

                movement = Services.getService<AnimationService>().movement;
                translationService = Services.getService<TranslationService>();

                TranslationService.changed += onLanguageChanged;

                Unloaded += onUnloaded;
            }

        }

        private void onUnloaded(object sender, RoutedEventArgs e)
        {
            TranslationService.changed -= onLanguageChanged;
        }

        private void onLanguageChanged(Languages lang)
        {
            if (lastTranslation != null)
            {
                renderContent(lastTranslation);
            }
        }

        public void show()
        {
            animate(0);
        }

        private void animate(double to)
        {
            movement.start(container, new ValueAnimationConfig<Thickness>(TimeSpan.FromMilliseconds(350), new Thickness(to, 0, 0, 0)));
        }

        public void hide()
        {
            animate(-430);
        }

        public void setConfig(Color barColor, NoteTextTranslation translation, bool silentHide = false)
        {
            bar.Fill = new SolidColorBrush(barColor);
            renderContent(translation);

            if (silentHide)
            {
                container.Margin = new Thickness(-430, showMargin.Top, 0, 0);
            }
        }

        private void renderContent(NoteTextTranslation translation)
        {
            lastTranslation = translation;
            label.Content = translationService.translate(translation.key, translation.vars);
            Size size = Services.getService<ComponentService>().getComponentTextDimensions(label);
            bar.Width = size.Width + 44;
        }
    }

    public class NoteTextTranslation
    {
        public string key;
        public string[] vars;

        public NoteTextTranslation(string key, string[] vars = null)
        {
            this.key = key;
            this.vars = vars;
        }
    }
}
