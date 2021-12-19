using app.structure.services;
using app.structure.services.translation;
using app.structure.utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace app.shared_components
{
    /// <summary>
    /// Interaction logic for ValuePreviewComponent.xaml
    /// </summary>
    public partial class ValuePreviewComponent : UserControl
    {
        private TranslationService translation;
        private string gender;
        private ValueType type;

        public ValuePreviewComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                translation = Services.getService<TranslationService>();
                onLanguageChanged(TranslationService.language);
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
            bool isArabic = lang == Languages.AR;
            TextAlignment alignment = isArabic ? TextAlignment.Right : TextAlignment.Left;

            label.TextAlignment = alignment;
            value.TextAlignment = alignment;
            
            value.Margin = new Thickness(isArabic ? 0 : 20, 20, isArabic ? 20 : 0, 10);

            if (label.Tag != null && value.Tag != null)
            {
                internalInit((string)label.Tag, value.Tag, type, gender);
            }

        }

        public void init(string label, string value, string gender = "")
        {
            internalInit(label, value, ValueType.TEXT, gender);
        }

        private void internalInit(string label, object value, ValueType type, string gender = "")
        {
            this.type = type;
            this.gender = gender;
            this.label.Tag = label;
            this.value.Tag = value;

            this.label.Text = translation.translate(label);

            if (type == ValueType.TEXT)
            {
                this.value.Text = string.IsNullOrEmpty(gender) ? translation.translate((string)value) : translation.translateByGender((string)value, gender);
            } else if (type == ValueType.DATE)
            {
                this.value.Text = DateUtils.getDateString((DateTime)value, translation);
            } else
            {
                this.value.Text = translation.translateNumeric((long)value);
            }
        }

        public void init(string label, DateTime value)
        {
            internalInit(label, value, ValueType.DATE);
        }

        public void init(string label, long value)
        {
            internalInit(label, value, ValueType.NUMERIC);
        }

        private enum ValueType
        {
            TEXT,
            NUMERIC,
            DATE
        }
    }

}
