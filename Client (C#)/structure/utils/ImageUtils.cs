using app.structure.services;
using System;
using System.Windows.Media.Imaging;

namespace app.structure.utils
{
    public static class ImageUtils
    {
        public static BitmapImage loadImage(string uri)
        {
            BitmapImage image = new BitmapImage();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.BeginInit();
            image.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            image.EndInit();

            return image;
        }

        public static BitmapImage loadServerImage(string url)
        {
            return loadImage(ServerRequestService.BASE_URL + "app/resources/" + url + "?token=" + App.session);
        }
    }
}
