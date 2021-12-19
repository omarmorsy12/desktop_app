using app.structure.events;
using app.structure.models;
using app.structure.services;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace app.shared_components.inputs
{
    /// <summary>
    /// Interaction logic for BackButtonComponent.xaml
    /// </summary>
    public partial class BackButtonComponent : UserControl
    {
        public Events<Rectangle> events;

        public BackButtonComponent()
        {
            InitializeComponent();
            if (ComponentService.isRuntimeMode)
            {
                events = new Events<Rectangle>(backBtn).addHoverEvent((e) => {
                    backBtn.Fill = new SolidColorBrush(e.isOverComponent ? AppColors.BlueColor : AppColors.DarkerGreyColor);
                });
            }
        }
    }
}
