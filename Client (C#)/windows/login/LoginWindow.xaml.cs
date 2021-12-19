using app.structure.animations.configs;
using app.structure.events;
using app.structure.services;
using app.structure.services.translation;
using app.structure.utils;
using app.windows.main;
using app.windows.login.components;
using System;
using System.Windows;
using app.windows.main.components.displays.actions.features;
using app.structure.models;
using app.structure.models.responses.login;
using app.structure.models.requests;
using app.structure.models.user;
using app.structure.models.error;

namespace app.windows.login
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string[] testAccount = new string[]
        {
            "", // Account Username or Email
            "" // Account Password
        };

        private AnimationService animation;
        private WindowService windowService;
        private ServerRequestService serverRequestService;
        private TranslationService translationService;

        private int responseTime = 0;
        private Timer timeTracker;

        public LoginWindow()
        {

            windowService = Services.getService<WindowService>();
            translationService = Services.getService<TranslationService>();

            windowService.init(this, "Login");

            InitializeComponent();

            schoolLogo.ImageSource = App.schoolLogo;

            header.setParentWindow(this);

            adjustUIOnLanguage(TranslationService.language);

            animation = Services.getService<AnimationService>();

            windowService.createWindowEventHandler(this).addEvent(EventType.CLICK, (e) => hiddenInput.Focus());

            ValueAnimationConfig<Thickness> originLocationConfig = new ValueAnimationConfig<Thickness>(TimeSpan.FromMilliseconds(650), new Thickness(0, 0, 0, 0));

            animation.movement.start(schoolLogoContainer, new ValueAnimationConfig<Thickness>(TimeSpan.FromMilliseconds(450), new Thickness(0, 25, 0, 0)));
            animation.movement.start(schoolName, new ValueAnimationConfig<Thickness>(TimeSpan.FromMilliseconds(650), new Thickness(0, 0, 0, 0)));
            animation.movement.start(usernameInput, originLocationConfig);
            animation.movement.start(passwordInput, originLocationConfig);
            animation.movement.start(loginBtn, originLocationConfig);
            
            serverRequestService = Services.getService<ServerRequestService>();

            loginBtn.button.events.addEvent(EventType.CLICK, (e) => requestLogin());

            timeTracker = new Timer((count, time) =>
            {
                ++responseTime; 
            }, TimeSpan.FromMilliseconds(1));

            usernameInput.KeyUp += onEnterKeyUp;
            passwordInput.KeyUp += onEnterKeyUp;

            rolePicker.backBtnEvents.addEvent(EventType.CLICK, (e) =>
            {
                if (!rolePicker.isLoading())
                {
                    usernameInput.clearContent();
                    passwordInput.clearContent();
                    hiddenInput.Focus();
                    note.hide();
                    inputsContainer.Visibility = Visibility.Visible;
                    rolePicker.Visibility = Visibility.Collapsed;
                    rolePicker.deselectAll();
                    usernameInput.applyFocus(true, true);
                }
            });

            TranslationService.changed += adjustUIOnLanguage;

            if (App.testMode == TestModes.BY_PASS_LOGIN)
            {
                passwordInput.applyFocus(true, true);
                requestLogin();

            } else
            {
                usernameInput.applyFocus(true, true);
            }

            Unloaded += onUnloaded;
        }

        private void onUnloaded(object sender, RoutedEventArgs e)
        {
            TranslationService.changed -= adjustUIOnLanguage;
        }

        private void adjustUIOnLanguage(Languages lang)
        {
            schoolName.Content = App.school.Equals(null) ? "" : lang == Languages.EN ? App.school.name.EN : App.school.name.AR;
            usernameInput.setInput(translationService.translate("username_or_email"), false, App.testMode == TestModes.BY_PASS_LOGIN ? testAccount[0] : null);
            passwordInput.setInput(translationService.translate("password"), true, App.testMode == TestModes.BY_PASS_LOGIN ? testAccount[1] : null);
        }

        private void onEnterKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                requestLogin();
            }
        }

        private async void requestLogin()
        {
            if (!loginBtn.button.isLoading)
            {
                note.hide();
                loginBtn.loading();
                usernameInput.IsEnabled = false;
                passwordInput.IsEnabled = false;
                usernameInput.applyFocus(false);
                passwordInput.applyFocus(false);

                timeTracker.start();
                await serverRequestService.post<LoginResponse>("login", new LoginRequestBody(usernameInput, passwordInput), onRequestLoginSuccess, onRequestFail).result;
                
                // Give some time for loading for user experience to notice loading is done 
                timeTracker.stop();

                if (responseTime < 18)
                {
                    new Timer((count, timer) =>
                    {
                        if (count == 18)
                        {
                            timer.stop();
                            stopLoading();
                        }
                    }, TimeSpan.FromMilliseconds(1)).start();
                } else
                {
                    stopLoading();
                }

                responseTime = 0;

            }
        }

        private void onRequestLoginSuccess (LoginResponse response)
        {
            bool hasSettings = response.data.account.settings != null;

            LoginDataAccountInformation information = response.data.account.information;
            UserAccountName name = new UserAccountName(information.name.first, information.name.last);
            UserAccountSettings settings = new UserAccountSettings(hasSettings ? response.data.account.settings.profile_image : null);
            App.user = new UserAccount(response.data.account.id, response.data.account.username, name, information.email, information.phone_number, settings);
            
            if (!response.data.session.require_role_index)
            {
                // Switch to Home Window (has one role)
                confirmLogin(response.data.session);
            }
            else
            {
                note.setConfig(AppColors.PurpleColor, new NoteTextTranslation("choose_a_role"), true);
                note.show();

                rolePicker.build(response.data.multipleRoles, (c, e) =>
                {
                    c.loading();
                    serverRequestService.post<LoginResponse>("login", new LoginRequestBody(usernameInput, passwordInput, e.getData<int>()), (sessionResponse) => confirmLogin(sessionResponse.data.session), (err) => {
                        c.loading(false);
                        onRequestFail(err);
                    });
                });
                timeTracker.stop();
                responseTime = 0;
                loginBtn.loading(false);
                inputsContainer.Visibility = Visibility.Collapsed;
                rolePicker.Visibility = Visibility.Visible;
            }
        }

        private void onRequestFail(ErrorState error)
        {
            bool isServerNotRunnig = error.code == ErrorCodes.REQUEST_FAILED;
            bool isUnauthorized = error.code == ErrorCodes.UNAUTHORIZED;
            if (isServerNotRunnig)
            {
                note.setConfig(AppColors.RedColor, new NoteTextTranslation(isServerNotRunnig ? "server_not_running" : "login_failed", isServerNotRunnig ? null : new string[] { "errorCode:" + error.code }) , true);
            } else if (isUnauthorized)
            {
                note.setConfig(AppColors.RedColor, new NoteTextTranslation("wrong_access"), true);
            }
            note.show();
        }

        private void stopLoading()
        {
            loginBtn.loading(false);
            usernameInput.IsEnabled = true;
            passwordInput.IsEnabled = true;

            if (usernameInput.isInputFocused())
            {
                usernameInput.applyFocus(true, true);
            }
            else if (passwordInput.isInputFocused())
            {
                passwordInput.applyFocus(true, true);
            }
        }

        private void confirmLogin(LoginDataSession session)
        {
            string profileImage = App.user.settings.profileImage;

            App.session = new Session(session.token, session.role, session.ownedFeatures, session.permissions, session.ownedTags);
            App.userProfileImage = profileImage != null ? ImageUtils.loadServerImage("accounts/" + profileImage) : null;

            Features.build();
            windowService.switchWindow<MainWindow, LoginWindow>();
        }

    }
}
