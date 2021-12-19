using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace app.structure.animations
{
    class HeightAnimation : DoubleValueAnimation
    {
        protected override DependencyProperty getProperty()
        {
            return FrameworkElement.HeightProperty;
        }

        protected override double getPropertyValue(FrameworkElement component)
        {
            return component.Height;
        }
    }
}
