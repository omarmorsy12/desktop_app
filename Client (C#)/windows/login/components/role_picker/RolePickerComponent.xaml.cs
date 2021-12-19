using app.structure.events;
using app.structure.models.user;
using app.structure.services;
using app.structure.services.translation;
using app.windows.login.components.role_picker;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace app.windows.login.components
{
    /// <summary>
    /// Interaction logic for RolePickerComponent.xaml
    /// </summary>
    public partial class RolePickerComponent : UserControl
    {
        public delegate void OnStart(RolePickerInputComponent component, EventParams<Ellipse> eventParams);

        public Events<Rectangle> backBtnEvents;
        private RolePickerInputComponent[] controls;
        private TranslationService translationService;

        public RolePickerComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                translationService = Services.getService<TranslationService>();

                controls = new RolePickerInputComponent[] { _0, _1, _2, _3, _4, _5 };

                backBtnLabel.Content = translationService.translate("back");
                backBtnEvents = new Events<Rectangle>(bar);
                backBtnEvents.addHoverEvent((e) => bar.Opacity = e.isOverComponent && !isLoading() ? 1 : 0.8);

                TranslationService.changed += onLanguageChanged;

                Unloaded += onUnloaded;
            }
        }

        private void onUnloaded(object sender, RoutedEventArgs e)
        {
            TranslationService.changed -= onLanguageChanged;
        }

        private void onLanguageChanged(Languages lang)
        {

            bool isArabic = lang == Languages.AR;

            backBtnLabel.Content = translationService.translate("back");

            for (int index = 0; index < controls.Length; index++)
            {
                RolePickerInputComponent control = controls[index];
                if (control.Tag != null)
                {
                    control.label.Content = translationService.translate((string)control.Tag);
                }
            }
        }

        public void build(List<string> ownedRolesArray, OnStart onRoleStart = null)
        {
            List<string> unsortedOwnedRole = new List<string>(ownedRolesArray);
            List<string> ownedRoles = UserRoles.ALL_KEYS.FindAll((role) => ownedRolesArray.Contains(role));

            backBtnContainer.Margin = new Thickness(0);

            for (int index = 0; index < controls.Length; index++)
            {
                RolePickerInputComponent control = controls[index];
                if (index < ownedRoles.Count)
                {
                    control.Tag = ownedRoles[index];
                    control.label.Content = translationService.translate(ownedRoles[index]);

                    if (index == ownedRoles.Count - 1 && index % 2 == 0)
                    {
                        control.HorizontalAlignment = HorizontalAlignment.Center;
                    }

                    control.roleIndex = unsortedOwnedRole.FindIndex((roleCode) => roleCode.Equals(ownedRoles[index]));

                    if (onRoleStart != null)
                    {
                        control.events.releaseEvents(EventType.CLICK).addEvent(EventType.CLICK, (e) => onRoleStart(control, e), control.roleIndex);
                    }

                    control.Visibility = Visibility.Visible;

                    if (index % 2 == 1)
                    {
                        backBtnContainer.Margin = new Thickness(0, backBtnContainer.Margin.Top + 47, 0, 0);
                    }

                    continue;
                }

                control.Visibility = Visibility.Collapsed;
            }
        }

        public void deselectAll()
        {
            foreach(RolePickerInputComponent input in controls)
            {
                input.deselect();
            }
        }

        public bool isLoading()
        {
            return controls.Where((c) => c.isLoading).FirstOrDefault() != null;
        }
    }
}
