using app.structure.animations;
using System.Windows;
using System.Windows.Controls;

namespace app.shared_components
{
    /// <summary>
    /// Interaction logic for LoadingComponent.xaml
    /// </summary>
    public partial class LoadingComponent : UserControl
    {
        private RotateAnimation rotateAnimation;

        public bool isLoading {
            get
            {
                return rotateAnimation.isRotating;
            }
        }

        public LoadingComponent()
        {
            InitializeComponent();
            rotateAnimation = new RotateAnimation(container);
        }

        public void start()
        {
            Visibility = Visibility.Visible;
            rotateAnimation.start();
        }

        public void stop()
        {
            Visibility = Visibility.Collapsed;
            rotateAnimation.stop();
        }
    }
}
