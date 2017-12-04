using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileEstimatorApp
{
    public interface IXCrossCropImage
    {
        Task<byte[]> CropImageFromOriginalToBytes(string filePath);
    }

    public class CrossXMethod
    {
        private static readonly Lazy<IXCrossCropImage> Implementation = new Lazy<IXCrossCropImage>(CreateMedia, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        public static IXCrossCropImage Current
        {
            get
            {
                var ret = Implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        private static IXCrossCropImage CreateMedia()
        {
            return DependencyService.Get<IXCrossCropImage>();
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
