using System.Windows;

namespace app.structure.animations
{
    class WidthAnimation : DoubleValueAnimation
    {

        protected override DependencyProperty getProperty()
        {
            return FrameworkElement.WidthProperty;
        }

        protected override double getPropertyValue(FrameworkElement component)
        {
            return component.Width;
        }
    }
}
