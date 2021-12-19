using app.structure.animations.configs;
using System.Windows;
using System.Windows.Media.Animation;
using System;

namespace app.structure.animations
{
    abstract class DoubleValueAnimation : Animation<DoubleAnimation, double, ValueAnimationConfig<double>>
    {
        protected override void applyAnimation(FrameworkElement component, ValueAnimationConfig<double> config)
        {
            animation.To = config.toValue;
            if (config.useFromValue)
            {
                animation.From = config.fromValue;
            }
            
        }

        protected override void initializeAnimation()
        {
            animation = new DoubleAnimation();
        }

        protected override void onAnimationEnded(FrameworkElement component, ValueAnimationConfig<double> config)
        {
            component.SetValue(getProperty(), config.toValue);
        }
    }
}
