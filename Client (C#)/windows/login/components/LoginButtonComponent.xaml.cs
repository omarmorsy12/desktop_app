using app.structure.animations.configs;
using app.structure.services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace app.windows.login.components
{
    /// <summary>
    /// Interaction logic for LoginButtonComponent.xaml
    /// </summary>
    public partial class LoginButtonComponent : UserControl
    {
        private AnimationService animation;
        private bool isMouseOverDirection = false;

        public LoginButtonComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                animation = Services.getService<AnimationService>();

                button.setText("login");
                button.clearDefaultHoverEvent();
                
                double startPoint = Services.getService<ComponentService>().getComponentTextDimensions(button.text).Width + 12;
                directionArrowIcon.Margin = new Thickness(startPoint, 0, 0, 0);

                button.events.addHoverEvent((e) => {
                    isMouseOverDirection = e.isOverComponent;
                    if (button.isLoading)
                    {
                        return;
                    }
                    directionArrowIcon.Opacity = e.isOverComponent ? 1 : 0;
                    animation.movement.stop(directionArrowIcon);
                    if (e.isOverComponent)
                    {
                        animation.movement.start(directionArrowIcon, new ValueAnimationConfig<Thickness>(TimeSpan.FromMilliseconds(120), new Thickness(160, 0,0,0), new Thickness(startPoint, 0,0,0)));
                    }
                    animation.opacity.start(directionArrowIcon, new ValueAnimationConfig<double>(TimeSpan.FromMilliseconds(25), e.isOverComponent ? 1 : 0));
                });
            }
        }

        public void loading(bool start = true)
        {
            if (start)
            {
                directionArrowIcon.Opacity = 0;
            } else if (isMouseOverDirection)
            {
                directionArrowIcon.Opacity = 1;
            }
            button.loading(start);
        }
    }
}
