using System.Collections.Generic;
using System.Windows;

namespace app.structure.events
{
    public class EventElement <T> where T : FrameworkElement
    {
        public bool isEventsInitialized = false;
        public T component { get; }
        public Dictionary<EventType, List<EventAction<T>>> actions { get; } = new Dictionary<EventType, List<EventAction<T>>>();

        public EventElement(T component)
        {
            this.component = component;
        }
    }
}
