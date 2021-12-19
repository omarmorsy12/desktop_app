using app.structure.animations;
using app.structure.animations.configs;
using app.structure.events;
using app.structure.services;
using app.structure.services.translation;
using System;
using System.Windows;
using System.Windows.Controls;

namespace app.windows.login.components
{
    /// <summary>
    /// Interaction logic for InputComponent.xaml
    /// </summary>
    public partial class InputComponent : UserControl
    {
        public bool isPassword = false;
        OpacityAnimation opacityAnimation;
        
        public InputComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                opacityAnimation = Services.getService<AnimationService>().opacity;
                Events<TextBox> inputEvents = new Events<TextBox>(inputBox);
                Events<PasswordBox> passwordInputEvents = new Events<PasswordBox>(passwordInputBox);

                inputEvents
                    .addHoverEvent(getHoverAction<TextBox>())
                    .addFocusEvent(getFocusAction<TextBox>());

                passwordInputEvents
                    .addHoverEvent(getHoverAction<PasswordBox>())
                    .addFocusEvent(getFocusAction<PasswordBox>());

                inputBox.TextChanged += (s, e) => onTextChange();
                passwordInputBox.PasswordChanged += (s, e) => onTextChange();

                adjustUIOnLanguage(TranslationService.language);

                TranslationService.changed += adjustUIOnLanguage;

                Unloaded += onUnloaded;
            }
        }

        private void onUnloaded(object sender, RoutedEventArgs e)
        {
            TranslationService.changed -= adjustUIOnLanguage;
        }

        private void adjustUIOnLanguage(Languages language)
        {
            bool isArabic = language == Languages.AR;
            inputBox.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            placeholderLabel.HorizontalContentAlignment = isArabic ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            passwordInputBox.HorizontalAlignment = isArabic ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            passwordInputBox.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private void onTextChange()
        {
            int length = isPassword ? passwordInputBox.Password.Length : inputBox.Text.Length;
            bool isEmpty = length == 0;
            opacityAnimation.start(placeholderLabel, new ValueAnimationConfig<double>(TimeSpan.FromMilliseconds(isEmpty ? 200 : 1), isEmpty ? 1 : 0));
        }

        private EventMethods.ActionMethod<T> getHoverAction<T>() where T : FrameworkElement
        {
            return (e) =>
            {
                if (!e.component.IsFocused)
                {
                    opacityAnimation.start(inputBackground, new ValueAnimationConfig<double>(TimeSpan.FromMilliseconds(150), e.isOverComponent ? 0.75 : 0.4));
                }
            };
        }

        private EventMethods.ActionMethod<T> getFocusAction<T>() where T : FrameworkElement
        {
            return (e) => applyFocus(e.component.IsFocused);
        }

        public void setInput(string placeholder, bool isPassword = false, string inputText = null)
        {
            placeholderLabel.Content = placeholder;
            if (inputText != null && inputText != "")
            {
                if (isPassword)
                {
                    passwordInputBox.Password = inputText;
                }
                else
                {
                    inputBox.Text = inputText;
                }
            }

            this.isPassword = isPassword;

            passwordInputBox.Visibility = isPassword ? Visibility.Visible : Visibility.Collapsed;
            inputBox.Visibility = isPassword ? Visibility.Collapsed : Visibility.Visible;
        }


        public string getText()
        {
            return isPassword ? passwordInputBox.Password : inputBox.Text;
        }

        public void applyFocus(bool isFocused, bool withActualFocus = false)
        {
            if (withActualFocus)
            {
                if (isPassword)
                {
                    passwordInputBox.Focus();
                } else
                {
                    inputBox.Focus();
                }
            }
            
            opacityAnimation.start(inputBackground, new ValueAnimationConfig<double>(TimeSpan.FromMilliseconds(150), isFocused ? 1 : 0.4));
        }

        public bool isInputFocused()
        {
            return isPassword ? passwordInputBox.IsFocused : inputBox.IsFocused;
        }

        public void clearContent()
        {
            inputBox.Text = "";
            passwordInputBox.Password = "";
            placeholderLabel.Opacity = 1;
        }
    }
}
