using app.shared_components;
using app.structure.services;
using app.structure.services.translation;
using app.windows.login;
using app.windows.main.components.displays.actions.features;
using app.windows.main.sub_windows.communication;
using app.windows.main;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using app.structure.models;
using app.structure.models.responses.startup;
using app.structure.models.user;
using app.structure.models.responses;

namespace app
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public static TestModes testMode = TestModes.NONE;

        public static SchoolInformationData school;
        public static BitmapImage schoolLogo;
        public static UserAccount user;
        public static BitmapImage userProfileImage;
        public static Session session;

        public static string NAME = "Blocks";
        public static string VERSION = "1.0.520";

        private static string ConfigFilePath = "config.ac";

        public static T getResource<T>(string key)
        {
            return (T)Current.Resources[key];
        }

        public static void logout()
        {
            ServerRequestService.cancelPendingRequests();
            OnlineStorageService.cancelPendingRequests();

            Services.getService<ServerRequestService>().post<ApiResponse<object, object>>("logout", session);

            CommunicationWindow.reset();
            Feature.reset();

            windows.main.MainWindow.currentLocation = Location.ACTIONS;

            user = null;
            userProfileImage = null;
            session = null;

            Services.getService<WindowService>().openWindow<LoginWindow>(true);
        }

        public App()
        {
            StartupUri = new Uri("windows/" + (testMode == TestModes.TEST_WINDOW ? "test/TestWindow.xaml" : "startup/StartUpWindow.xaml"), UriKind.Relative);
            readConfig();
            isAlreadyRunning();
            Services.build();
        }

        private void readConfig()
        {
            bool hasConfig = File.Exists(ConfigFilePath);

            if (hasConfig)
            {
                try
                {
                    StreamReader sr = new StreamReader(ConfigFilePath);

                    string line = sr.ReadLine();
                    int i = 0;

                    while (line != null)
                    {
                        string value = line.Trim().ToLower();
                        if (i == 0)
                        {
                            TranslationService.language = value.Equals("en") ? Languages.EN : Languages.AR;

                        }
                        else if (i == 1)
                        {
                            ScreenModeComponent.isFullScreen = value.Equals("1");
                        }

                        line = sr.ReadLine();
                        i++;
                    }

                    sr.Close();
                }
                catch { }
            }
        }

        private void isAlreadyRunning()
        {
            string procName = Process.GetCurrentProcess().ProcessName;

            // get the list of all processes by the "procName"       
            Process[] processes = Process.GetProcessesByName(procName);

            if (processes.Length > 1)
            {
                MessageBox.Show("App already running", NAME);
                Current.Shutdown();
            }
        }
    }
}
