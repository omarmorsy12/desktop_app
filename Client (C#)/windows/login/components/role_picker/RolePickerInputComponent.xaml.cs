using app.structure.animations;
using app.structure.events;
using app.structure.models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace app.windows.login.components.role_picker
{
    /// <summary>
    /// Interaction logic for RolePickerInputComponent.xaml
    /// </summary>
    public partial class RolePickerInputComponent : UserControl
    {
        public bool isSelected = false;
        public int roleIndex;
        public Events<Ellipse> events;

        private bool allowHover = false;
        public bool isLoading = false;
        private RotateAnimation rotateAnimation;

        public RolePickerInputComponent()
        {
            InitializeComponent();

            rotateAnimation = new RotateAnimation(loader);

            events = new Events<Ellipse>(circle).addHoverEvent((e) => onHover(e.isOverComponent));

            new Events<Rectangle>(bar).addHoverEvent((e) =>
            {
                if (!isSelected)
                {
                    bar.Opacity = e.isOverComponent ? 0.75 : 0.4;
                }
            }).addEvent(EventType.CLICK, (e) => select());

        }

        public void select()
        {
            RolePickerComponent rolePicker = ((RolePickerComponent)((Grid)((Grid)Parent).Parent).Parent);
            if (!rolePicker.isLoading())
            {
                rolePicker.deselectAll();
                label.FontWeight = FontWeights.Bold;
                label.Foreground = new SolidColorBrush(AppColors.PurpleColor);
                bar.Opacity = 1;
                goTrigger.Visibility = Visibility.Visible;
                purpleArrow.Opacity = 1;
                allowHover = true;
                isSelected = true;
            }
        }

        public void deselect()
        {
            label.FontWeight = FontWeights.Normal;
            label.Foreground = new SolidColorBrush(AppColors.BlackColor);
            bar.Opacity = 0.4;
            goTrigger.Visibility = Visibility.Collapsed;
            purpleArrow.Opacity = 0;
            allowHover = false;
            isSelected = false;
        }

        public void loading(bool start = true)
        {
            isLoading = start;

            if (start)
            {
                allowHover = false;
                circle.Opacity = purpleArrow.Opacity = whiteArrow.Opacity = 0;
                loader.Opacity = 1;
                goTrigger.Visibility = Visibility.Visible;
                rotateAnimation.start();
            } else
            {
                rotateAnimation.stop();
                loader.Opacity = 0;
                select();
            }
        }

        private void onHover(bool on)
        {
            if (allowHover)
            {
                circle.Opacity = on ? 1 : 0;
                whiteArrow.Opacity = on ? 1 : 0;
                purpleArrow.Opacity = on ? 0 : 1;
            }
        }

    }
}
