using System.IO;
using Android.Graphics;
using Java.Lang;

namespace MobileEstimatorApp.Droid
{
    public static class Extensions
    {
        public static byte[] BitmapToBytes(this Bitmap myBitmapImage)
        {
            byte[] imageByteArray = null;
            try
            {
                var ms = new MemoryStream();
                myBitmapImage.Compress(Bitmap.CompressFormat.Png, 0, ms);
                imageByteArray = ms.ToArray();
                //return imageByteArray;
            }
            catch (Exception ex)
            {

            }
            return imageByteArray;

        }
    }
}