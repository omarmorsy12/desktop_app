using app.shared_components;
using app.structure.events;
using app.structure.services;
using app.structure.utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace app.windows.startup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartUpWindow : Window
    {
        ComponentService componentService;
        public StartUpWindow()
        {
            Services.getService<WindowService>().init(this, "Startup");

            InitializeComponent();

            screenMode.setScreenModeConfig(this);

            ScreenModeComponent.setScreenModeFor(this);

            componentService = Services.getService<ComponentService>();

            new Events<Label>(upperCorner).addHoverEvent((e) =>
            {
                componentService.setComponentImage(upperCornerContainer, getResourceUri("upper-corner", e.isOverComponent));
                screenMode.onHover(e.isOverComponent);
            }).addEvent(EventType.CLICK, (e) => {
                componentService.setComponentImage(upperCornerContainer, getResourceUri("upper-corner", upperCorner.IsMouseOver));
                screenMode.onClick();
            });

            startLogoAnimation();

            Services.getService<WindowService>().createWindowEventHandler(this);
        }

        public string getResourceUri(string resourceName, bool highlight = false)
        {
            return "resources/" + resourceName + "/" + (highlight ? "highlight" : "static") + ".png";
        }

        private void onTick(int tickCount, Timer timer)
        {
            if (tickCount == 1)
            {
                timer.changeTickDelay(TimeSpan.FromMilliseconds(0.5));
            }
            componentService.setComponentImage(logo, "resources/logo_animation/" + tickCount + ".png");
        }

        private void startLogoAnimation()
        {
            Timer timer = new Timer(onTick, TimeSpan.FromSeconds(1), new TimerConfig().setTickEndConfig(32, status.startCheck));
            timer.start();
        }
    }
}
