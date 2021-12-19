using app.structure.animations;
using app.structure.animations.configs;
using app.structure.events;
using app.structure.models;
using app.structure.models.error;
using app.structure.models.responses;
using app.structure.models.responses.startup;
using app.structure.services;
using app.structure.utils;
using app.windows.login;
using app.windows.startup.components.startup_status;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace app.windows.startup.components
{
    /// <summary>
    /// Interaction logic for StartupLoading.xaml
    /// </summary>
    public partial class StartupStatusComponent : UserControl
    {
        private static StartupStatusConfig loadingConfig;
        private delegate void onStatusAnimationEnd();

        Timer frameAnimation;

        ComponentService componentService;
        WindowService windowService;
        ServerRequestService serverRequestService;
        TranslationService translationService;
        AnimationService animation;

        bool reloading = false;
        bool applyReloading = false;
        bool isAnimating = false;

        public StartupStatusComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                componentService = Services.getService<ComponentService>();
                windowService = Services.getService<WindowService>();
                serverRequestService = Services.getService<ServerRequestService>();
                translationService = Services.getService<TranslationService>();
                animation = Services.getService<AnimationService>();

                loadingConfig = new StartupStatusConfig(AppColors.PurpleColor, translationService.translate("loading"), translationService.translate("checking_license_validation"), true);

                text.Foreground = new SolidColorBrush(AppColors.DarkerGreyColor);
                reloadIcon.RenderTransformOrigin = new Point(0.5, 0.5);
            
                Events<Image> reloadIconEvents = new Events<Image>(reloadIcon);
                reloadIconEvents.addEvent(EventType.CLICK, (e) => {
                    if (applyReloading && !reloading)
                    {
                        reloading = true;

                        RotateAnimation rotation = new RotateAnimation(reloadIcon, true, 360);
                        rotation.onEnd = () => switchToStatus(loadingConfig, checkStatus);

                        rotation.start();
                    }
                });
                reloadIconEvents.addHoverEvent((e) => componentService.setComponentImage(reloadIcon, "resources/reload/" + (e.isOverComponent ? "highlight" : "static") + ".png"));
            }

        }

        private void animate(onStatusAnimationEnd onEnd = null)
        {
            if (!isAnimating)
            {
                isAnimating = true;
                bool moveToMiddle = rectangle.Margin.Bottom == 81;

                if (moveToMiddle)
                {
                    statusText.Opacity = 1;
                    animation.movement.start(rectangle, new ValueAnimationConfig<Thickness>(TimeSpan.FromMilliseconds(350), new Thickness(0, 0, 0, 0), (c) => {
                        frameAnimation?.start();
                        animation.opacity.start(text, new ValueAnimationConfig<double>(TimeSpan.FromMilliseconds(200), 1, (t) => {
                            isAnimating = false;
                            onEnd?.Invoke();
                        }));

                        if (applyReloading)
                        {
                            animation.opacity.start(reloadIcon, new ValueAnimationConfig<double>(TimeSpan.FromMilliseconds(200), 1));
                        }
                    }));
                } else
                {
                    reloadIcon.Opacity = 0;
                    animation.movement.start(rectangle, new ValueAnimationConfig<Thickness>(TimeSpan.FromMilliseconds(300), new Thickness(0, 0, 0, 81), (c) => {
                        isAnimating = false;
                        reloading = false;
                        onEnd?.Invoke();
                    }));
                    animation.opacity.start(text, new ValueAnimationConfig<double>(TimeSpan.FromMilliseconds(200), 0));
                    stopLoading();
                }
            }
        }

        private void stopLoading()
        {
            if (frameAnimation != null)
            {
                frameAnimation.stop();
                componentService.setComponentImage(loadingFrame, "resources/startup_loading_animation/1.png");
            }
        }

        private void animateConfig(StartupStatusConfig config, onStatusAnimationEnd onEnd = null)
        {
            rectangle.Fill = new SolidColorBrush(config.color);
            innerText.Text = config.text;
            statusText.Content = config.statusText;
            stopLoading();

            frameAnimation = config.loadingAnimation ? new Timer((tickCount, timer) =>
            {
                componentService.setComponentImage(loadingFrame, "resources/startup_loading_animation/" + tickCount + ".png");
            }, TimeSpan.FromMilliseconds(30), new TimerConfig().setTickCountRange(32)) : null;

            applyReloading = config.applyReloading;

            animate(onEnd);
        }

        private void switchToStatus(StartupStatusConfig switchConfig, onStatusAnimationEnd onEnd = null)
        {
            animate(() => animateConfig(switchConfig, onEnd));
        }

        public void startCheck()
        {
            animateConfig(loadingConfig, checkStatus);
        }

        private void checkStatus() {
            serverRequestService.get<StartupResponse>("app/startup", onRequestSuccess, onRequestFail);
        }

        private void onRequestSuccess(StartupResponse response)
        {
            switchToStatus(new StartupStatusConfig(AppColors.BlueColor, translationService.translate("done"), translationService.translate("license_verified")), () => {
                App.school = response.data;
                App.schoolLogo = response.data.logo == null ? App.getResource<BitmapImage>("DefaultSchoolLogo") : ImageUtils.loadServerImage("school/" + response.data.logo);
                new Timer((tickCount, t) => windowService.switchWindow<LoginWindow, StartUpWindow>(), TimeSpan.FromMilliseconds(500), new TimerConfig().setTickEndConfig(2)).start();
            });
        }

        private void onRequestFail(ErrorState err)
        {
            bool isServerNotRunning = err.code == ErrorCodes.REQUEST_FAILED;
            bool isLicenseExpired = err.code == ErrorCodes.LICENSE_EXPIRED;

            if (isServerNotRunning)
            {
                switchToStatus(new StartupStatusConfig(AppColors.RedColor, translationService.translate("error"), translationService.translate("server_not_running"), false, true));
            } else if (isLicenseExpired)
            {
                switchToStatus(new StartupStatusConfig(AppColors.OrangeColor, translationService.translate("problem"), translationService.translate("license_expired"), false, true));
            }
            else
            {
                switchToStatus(new StartupStatusConfig(AppColors.RedColor, translationService.translate("error"), translationService.translate("call_tech_team_err", new string[] { "errorCode:" + err.code }), false, true));
            }
        }
    }
}
