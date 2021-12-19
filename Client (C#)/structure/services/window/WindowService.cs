using app.shared_components;
using app.structure.events;
using app.windows.main.sub_windows.communication;
using app.windows.main.sub_windows.help;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace app.structure.services
{
    public class WindowService : Services
    {
        private WindowRecord record = new WindowRecord(ScreenModeComponent.isFullScreen);

        public void init(Window window, string title)
        {
            TranslationService translationService = getService<TranslationService>();
            translationService.loadTranslationContent(window);

            Size defaultSize = new Size(740, 685);
            double screenWidth = SystemParameters.WorkArea.Width;
            double calcWidth = screenWidth * 0.5;
            double calcHeight = calcWidth - 100;
            window.Title = title;
            window.Icon = new BitmapImage(new Uri("pack://application:,,,/resources/icons/logo.ico"));
            window.MinWidth = defaultSize.Width;
            window.MinHeight = defaultSize.Height;

            if (record.isLocationChanged)
            {
                window.Left = record.left;
                window.Top = record.top;
            } else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                double[] center = calculateCenterLocation(Math.Max(calcWidth, defaultSize.Width), Math.Max(calcHeight, defaultSize.Height));
                record.updateLocation(center[0], center[1], true);
            }

            if (!record.isSizeChanged)
            {
                window.Width = calcWidth;
                window.Height = calcHeight;
            } else
            {
                window.Width = record.size.Width;
                window.Height = record.size.Height;
            }

            window.LocationChanged += (sender, e) => {
                if (ScreenModeComponent.isFullScreen == record.wasFullScreen)
                {
                    record.updateLocation(window.Left, window.Top);
                }
            };
            window.SizeChanged += (sender, e) => {
                if (!ScreenModeComponent.isFullScreen)
                {
                    record.size = e.NewSize;

                    if (record.wasFullScreen && !record.isLocationChanged)
                    {
                        window.Left = record.left;
                        window.Top = record.top;
                    }
                }
                record.wasFullScreen = ScreenModeComponent.isFullScreen;
            };
        }

        private double[] calculateCenterLocation(double width, double height)
        {
            return new double[] {
                (SystemParameters.WorkArea.Width - width) / 2 + SystemParameters.WorkArea.Left,
                (SystemParameters.WorkArea.Height - height) / 2 + SystemParameters.WorkArea.Top
            };
        }

        public T getWindow<T>() where T : Window
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w is T)
                {
                    return (T)w;
                }
            }

            return null;
        }

        public void switchWindow<open, close>(bool center = false) where open : Window where close : Window
        {
            Window window = (open)Activator.CreateInstance(typeof(open));
            Window previousWindow = getWindow<close>();

            ScreenModeComponent.setScreenModeFor(window);

            if (center)
            {
                centerWindow(window);
            } else
            {
                window.Left = previousWindow.Left;
                window.Top = previousWindow.Top;
            }

            previousWindow.Close();
            window.Show();
        }

        public void openWindow<type>(bool closeOthers = false) where type : Window
        {
            Window window = (type)Activator.CreateInstance(typeof(type));

            ScreenModeComponent.setScreenModeFor(window);

            if (closeOthers)
            {
                foreach (Window w in App.Current.Windows)
                {
                    if (w != window)
                    {
                        w.Close();
                    }
                }
            }

            window.Show();
        }

        public void openSubWindow<type>() where type : Window
        {
            Window window = getWindow<type>();

            if (window == null)
            {
                window = (Window)Activator.CreateInstance(typeof(type));

                centerWindow(window);

                window.Show();
            } else
            {
                window.Focus();
            }
        }

        public void centerWindow(Window window)
        {
            window.Left = (SystemParameters.WorkArea.Width - Math.Max(window.Width, window.MinWidth)) / 2 + SystemParameters.WorkArea.Left;
            window.Top = (SystemParameters.WorkArea.Height - Math.Max(window.Height, window.MinHeight)) / 2 + SystemParameters.WorkArea.Top;
        }

        public Events<Window> createWindowEventHandler(Window window)
        {
            Events<Window> windowEvents = new Events<Window>(window);
            return windowEvents.addEvent(EventType.MOUSE_DOWN, (e) => EventSpace.lastMouseDownElement = null);
        }

        public void closeAllSubWindows()
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w is CommunicationWindow || w is HelpWindow)
                {
                    w.Close();
                }
            }
        }

    }

    class WindowRecord
    {
        private bool _isLocationChanged = false;
        private bool _isSizeChanged = false;

        private double _left;
        private double _top;
        private Size _size;

        public bool wasFullScreen;

        public WindowRecord(bool wasFullScreen)
        {
            this.wasFullScreen = wasFullScreen;
        }

        public double left
        {
            get
            {
                return _left;
            }
        }

        public double top
        {
            get
            {
                return _top;
            }
        }

        public Size size
        {
            set
            {
                _size = value;
                _isSizeChanged = true;
            }
            get
            {
                return _size;
            }
        }

        public bool isSizeChanged
        {
            get
            {
                return _isSizeChanged;
            }
        }

        public bool isLocationChanged
        {
            get
            {
                return _isLocationChanged;
            }
        }

        public void updateLocation(double left, double top, bool ignoreChange = false)
        {
            _left = left;
            _top = top;
            if (!ignoreChange)
            {
                _isLocationChanged = true;
            }
        }
    }
}
