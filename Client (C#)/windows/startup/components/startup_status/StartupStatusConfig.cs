using System.Windows.Media;

namespace app.windows.startup.components.startup_status
{
    public class StartupStatusConfig
    {
        public Color color { get; }
        public string text { get; }
        public string statusText { get; }

        public bool loadingAnimation { get; }

        public bool applyReloading { get; } = false;


        public StartupStatusConfig(Color color, string statusText, string text, bool loadingAnimation = false, bool applyReloading = false)
        {
            this.color = color;
            this.text = text;
            this.statusText = statusText;
            this.loadingAnimation = loadingAnimation;
            this.applyReloading = applyReloading;
        }

    }
}
