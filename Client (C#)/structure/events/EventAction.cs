using System.Windows;
using static app.structure.events.EventMethods;

namespace app.structure.events
{
    public class EventAction<ElementType> where ElementType : FrameworkElement
    {
        public ActionMethod<ElementType> action { get; }
        public object data { get; }

        public EventAction(ActionMethod<ElementType> action, object data = null)
        {
            this.action = action;
            this.data = data;
        }
    }
}
