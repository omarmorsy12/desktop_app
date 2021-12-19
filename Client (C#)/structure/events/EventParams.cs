using System.Windows;
using System.Windows.Input;

namespace app.structure.events
{
    public class EventParams<F> where F : FrameworkElement
    {
        public F component { get; }
        public MouseButtonEventArgs mouseButtonArgs { get; }
        public MouseEventArgs mouseArgs { get; }
        public RoutedEventArgs routeArgs { get; }
        public KeyEventArgs keyArgs { get; }

        public bool isOverComponent { get; }

        private object data;

        public EventParams(F component, bool isOverComponent = false)
        {
            this.isOverComponent = isOverComponent;
            this.component = component;
        }

        public EventParams(F component, bool isOverComponent, MouseButtonEventArgs mouseButtonArgs = null)
        {
            this.mouseButtonArgs = mouseButtonArgs;
            this.isOverComponent = isOverComponent;
            this.component = component;
        }

        public EventParams(F component, bool isOverComponent, RoutedEventArgs routeArgs = null)
        {
            this.routeArgs = routeArgs;
            this.isOverComponent = isOverComponent;
            this.component = component;
        }

        public EventParams(F component, bool isOverComponent, MouseEventArgs mouseArgs = null)
        {
            this.mouseArgs = mouseArgs;
            this.isOverComponent = isOverComponent;
            this.component = component;
        }

        public EventParams(F component, bool isOverComponent, KeyEventArgs keyArgs = null)
        {
            this.isOverComponent = isOverComponent;
            this.keyArgs = keyArgs;
            this.component = component;
        }

        public void setData(object data)
        {
            this.data = data;
        }

        public T getData<T>()
        {
            return (T)data;
        }

        public void handled(bool value = true)
        {
            if (mouseArgs != null)
            {
                mouseArgs.Handled = value;
            }
            else if (mouseButtonArgs != null)
            {
                mouseButtonArgs.Handled = value;
            }
            else if (routeArgs != null)
            {
                routeArgs.Handled = value;
            }
        }
    }

}
