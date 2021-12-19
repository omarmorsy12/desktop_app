using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using static app.structure.events.EventMethods;
using System;

namespace app.structure.events
{
    public class Events : Events<FrameworkElement>
    {
        public Events(FrameworkElement element) : base(element)
        {
        }
    }

    public class Events<ElementType> where ElementType : FrameworkElement
    {
        public delegate void OnMouseClick(FrameworkElement element);

        public static event OnMouseClick onMouseClick;

        private EventElement<ElementType> element;

        public Events(ElementType element) {
            this.element = new EventElement<ElementType>(element);
        }

        private void onHoverOff(object sender, MouseEventArgs e)
        {
            eventTrigger(EventType.HOVER_OFF, new EventParams<ElementType>((ElementType)sender, false, e));
        }

        private void onHoverOn(object sender, MouseEventArgs e)
        {
            eventTrigger(EventType.HOVER_ON, new EventParams<ElementType>((ElementType)sender, true, e));
        }

        private void onMouseUp(object sender, MouseButtonEventArgs e)
        {
            ElementType element = (ElementType)sender;

            eventTrigger(EventType.MOUSE_UP, new EventParams<ElementType>(element, true, e));
            if (EventSpace.lastMouseDownElement == element)
            {
                EventSpace.lastMouseDownElement = null;
                Events.onMouseClick?.Invoke(element);
                eventTrigger(EventType.CLICK, new EventParams<ElementType>(element, true, e));
            }
        }

        private void onMouseDown(object sender, MouseButtonEventArgs e)
        {
            EventParams<ElementType> eventParams = new EventParams<ElementType>((ElementType)sender, true, e);
            eventTrigger(EventType.MOUSE_DOWN, eventParams);
            EventSpace.lastMouseDownElement = (FrameworkElement) sender;
        }

        private void eventTrigger (EventType type, EventParams<ElementType> eventParams)
        {
            if (type != EventType.FOCUS && type != EventType.BLUR)
            {
                eventParams.handled();
            }

            for (int i = 0; hasEvent(type) && i < element.actions[type].Count; i++)
            {
                eventParams.setData(element.actions[type][i].data);
                element.actions[type][i].action(eventParams);
            }
        }

        public Events<ElementType> addEvent(EventType type, ActionMethod<ElementType> action, object data = null)
        {
            if (!element.isEventsInitialized)
            {
                element.component.MouseDown += onMouseDown;
                element.component.MouseUp += onMouseUp;
                element.component.MouseEnter += onHoverOn;
                element.component.MouseLeave += onHoverOff;
                element.component.GotFocus += onFocus;
                element.component.LostFocus += onBlur;
                element.component.KeyDown += onKeyDown;
                element.component.KeyUp += onKeyUp; 
                element.isEventsInitialized = true;
            }

            if (!element.actions.ContainsKey(type))
            {
                element.actions.Add(type, new List<EventAction<ElementType>>());
            }

            element.actions[type].Add(new EventAction<ElementType>(action, data));

            return this;
        }

        public Events<ElementType> releaseEvents(EventType type)
        {
            if (element.actions.ContainsKey(type))
            {
                element.actions.Remove(type);
            }
            return this;
        }

        public Events<ElementType> releaseAllEvents()
        {
            element.actions.Clear();
            return this;
        }

        public bool hasEvent(EventType type)
        {
            return element.actions.ContainsKey(type);
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            EventParams<ElementType> eventParams = new EventParams<ElementType>((ElementType)sender, false, e);
            eventTrigger(EventType.KEY_UP, eventParams);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            EventParams<ElementType> eventParams = new EventParams<ElementType>((ElementType)sender, false, e);
            eventTrigger(EventType.KEY_DOWN, eventParams);
        }

        private void onBlur(object sender, RoutedEventArgs e)
        {
            EventParams<ElementType> eventParams = new EventParams<ElementType>((ElementType)sender, false, e);
            eventTrigger(EventType.BLUR, eventParams);
        }

        private void onFocus(object sender, RoutedEventArgs e)
        {
            EventParams<ElementType> eventParams = new EventParams<ElementType>((ElementType)sender, true, e);
            eventTrigger(EventType.FOCUS, eventParams);
        }

        public Events<ElementType> addHoverEvent(ActionMethod<ElementType> action, object data = null)
        {
            addEvent(EventType.HOVER_ON, action, data);
            addEvent(EventType.HOVER_OFF, action, data);
            return this;
        }
        
        public Events<ElementType> addFocusEvent(ActionMethod<ElementType> action, object data = null)
        {
            addEvent(EventType.FOCUS, action, data);
            addEvent(EventType.BLUR, action, data);
            return this;
        }   
    }

    public enum EventType
    {
        CLICK,
        HOVER_ON,
        HOVER_OFF,
        MOUSE_DOWN,
        MOUSE_UP,
        KEY_DOWN,
        KEY_UP,
        FOCUS,
        BLUR
    }

    public static class EventMethods
    {
        public delegate void ActionMethod<F>(EventParams<F> eventParams) where F : FrameworkElement;
    }

    public static class EventSpace
    {
        public static FrameworkElement lastMouseDownElement = null;
    }
}
