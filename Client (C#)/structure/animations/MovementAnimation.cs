using app.structure.animations.configs;
using System.Windows;
using System.Windows.Media.Animation;
using System;

namespace app.structure.animations
{
    public class MovementAnimation : Animation<ThicknessAnimation, Thickness, ValueAnimationConfig<Thickness>>
    {
        protected override void applyAnimation(FrameworkElement component, ValueAnimationConfig<Thickness> config)
        {
            animation.To = config.toValue;
            if (config.useFromValue)
            {
                animation.From = config.fromValue;
            }
        }

        protected override Thickness getPropertyValue(FrameworkElement component)
        {
            return component.Margin;
        }

        protected override DependencyProperty getProperty()
        {
            return FrameworkElement.MarginProperty;
        }

        protected override void initializeAnimation()
        {
            animation = new ThicknessAnimation();
        }

        protected override void onAnimationEnded(FrameworkElement component, ValueAnimationConfig<Thickness> config)
        {
            component.Margin = config.toValue;
        }
    }
}
