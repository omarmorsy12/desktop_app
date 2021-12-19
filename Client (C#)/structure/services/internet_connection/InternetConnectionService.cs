using NETWORKLIST;
using System.Runtime.InteropServices;
using System.Threading;

namespace app.structure.services
{
    public class InternetConnectionService : Services
    {
        public delegate void InternetConnectionChanged(bool isConnected);
        public static event InternetConnectionChanged changed;
        public bool isConnected;

        private Thread tracker;

        private bool isConnectedNow
        {
            get {
                return new NetworkListManager().IsConnectedToInternet;
            }
        }

        public InternetConnectionService()
        {
            isConnected = isConnectedNow;
        }

        public void startTracking()
        {
            if (tracker == null)
            {
                tracker = new Thread(() => {
                    while(true)
                    {
                        bool currentInternetConnection = isConnectedNow;
                        if (isConnected != currentInternetConnection)
                        {
                            isConnected = currentInternetConnection;
                            changed?.Invoke(currentInternetConnection);
                        }
                        Thread.Sleep(200);
                    }
                });
                tracker.IsBackground = true;
                tracker.Start();
            }
        }

        public void stopTracking()
        {
            if (tracker != null)
            {
                tracker.Abort();
                tracker = null;
            }
        }
    }
}
