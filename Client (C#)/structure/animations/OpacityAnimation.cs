using System.Windows;

namespace app.structure.animations
{
    class OpacityAnimation : DoubleValueAnimation
    {

        protected override DependencyProperty getProperty()
        {
            return UIElement.OpacityProperty;
        }

        protected override double getPropertyValue(FrameworkElement component)
        {
            return component.Opacity;
        }
    }
}
