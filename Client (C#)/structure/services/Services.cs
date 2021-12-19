using System;
using System.Collections.Generic;

namespace app.structure.services
{
    public class Services
    {
        static Dictionary<Type, Services> instances = new Dictionary<Type, Services>();

        public Services()
        {
            instances.Add(GetType(), this);
        }

        public static T getService<T>() where T : Services
        {
            return (T)instances[typeof(T)];
        }

        public static void build()
        {
            Type[] services = new Type[]
            {
                typeof(ServerRequestService),
                typeof(TranslationService),
                typeof(ComponentService),
                typeof(WindowService),
                typeof(AnimationService),
                typeof(InternetConnectionService),
                typeof(OnlineStorageService)
            };

            foreach (Type type in services)
            {
                Activator.CreateInstance(type);
            }
        }
    }
}
