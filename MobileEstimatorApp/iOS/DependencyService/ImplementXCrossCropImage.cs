using System;
using System.Threading.Tasks;
using CoreGraphics;
using MobileEstimatorApp.iOS;
using UIKit;
using Wapps.TOCrop;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImplementXCrossCropImage))]
namespace MobileEstimatorApp.iOS
{
    public class ImplementXCrossCropImage : IXCrossCropImage
    {

        public class CropVcDelegate : TOCropViewControllerDelegate
        {
            private readonly WeakReference<TOCropViewController> _owner;

            public CropVcDelegate(TOCropViewController owner)
            {
                _owner = new WeakReference<TOCropViewController>(owner);
                _tcs = new TaskCompletionSource<byte[]>();
            }

            public override void DidCropImageToRect(TOCropViewController cropViewController, CGRect cropRect, nint angle)
            {
                cropViewController.PresentingViewController.DismissViewController(true, null);
                TOCropViewController owner;
                _tcs.SetResult(_owner.TryGetTarget(out owner) ? cropViewController.FinalImage.UIImageToBytes() : null);
            }

            public override void DidFinishCancelled(TOCropViewController cropViewController, bool cancelled)
            {
                cropViewController.PresentingViewController.DismissViewController(true, null);
                _tcs.SetResult(null);
            }

            public Task<byte[]> Task => _tcs.Task;

            private readonly TaskCompletionSource<byte[]> _tcs;
        }

        public Task<byte[]> CropImageFromOriginalToBytes(string filePath)
        {
            var image = UIImage.FromFile(filePath);
            var viewController = new TOCropViewController(TOCropViewCroppingStyle.Default, image);
            var ndelegate = new CropVcDelegate(viewController);

            viewController.Delegate = ndelegate;

            viewController.PresentUsingRootViewController();
            var result = ndelegate.Task.ContinueWith(t => t).Unwrap();
            return result;
        }
    }
}