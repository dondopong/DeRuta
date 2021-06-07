using System;
using System.IO;
using System.Threading.Tasks;

using System.Drawing;
using UIKit;
using CoreGraphics;

using Android.Graphics;
using Xamarin.Forms;

namespace DeRuta.Utils
{
    public static class ImageResizer
    {
        static ImageResizer()
        {
        }

        public static byte[] ResizeImage(byte[] imageData, float maxSize)
        {
            if (Device.OS == TargetPlatform.iOS)
            {

                UIImage originalImage = ImageFromByteArray(imageData);

                int newWidth = (int)(originalImage.Size.Height > originalImage.Size.Width ? originalImage.Size.Width * maxSize / originalImage.Size.Height : maxSize);
                int newHeight = (int)(originalImage.Size.Height > originalImage.Size.Width ? maxSize : originalImage.Size.Height * maxSize / originalImage.Size.Width);

                UIImageOrientation orientation = originalImage.Orientation;

                //create a 24bit RGB image
                using (CGBitmapContext context = new CGBitmapContext(IntPtr.Zero,
                                                     newWidth, newHeight, 8,
                                                     4 * newWidth, CGColorSpace.CreateDeviceRGB(),
                                                     CGImageAlphaInfo.PremultipliedFirst))
                {

                    CGRect imageRect = new CGRect(0, 0, newWidth, newHeight);

                    // draw the image
                    context.DrawImage(imageRect, originalImage.CGImage);

                    UIKit.UIImage resizedImage = UIKit.UIImage.FromImage(context.ToImage(), 0, orientation);

                    // save the image as a jpeg
                    return resizedImage.AsJPEG().ToArray();
                }
            }

            if (Device.OS == TargetPlatform.Android)
            {
                // Load the bitmap
                Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

                int newWidth = (int)(originalImage.Height > originalImage.Width ? originalImage.Width * maxSize / originalImage.Height : maxSize);
                int newHeight = (int)(originalImage.Height > originalImage.Width ? maxSize : originalImage.Height * maxSize / originalImage.Width);

                Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, newWidth, newHeight, false);

                using (MemoryStream ms = new MemoryStream())
                {
                    resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                    return ms.ToArray();
                }
            }
            return null;
        }

        public static UIKit.UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            UIKit.UIImage image;
            try
            {
                image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
            }
            catch (Exception e)
            {
                Console.WriteLine("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }
    }
}


