using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace MobileEstimatorApp
{
    public partial class VehicleInfoPageThree : ContentPage
    {
        byte[] cropedBytes;
        List<byte> imglist;
        ImageSource imgProfileImageSource;
        byte[] imgdata;


        public VehicleInfoPageThree()
        {
            InitializeComponent();
            imgPicture.HeightRequest = 70;
            imgPicture.WidthRequest = 70;
        }

        void BackTapped(object sender, EventArgs e)
        {
            try
            {
                Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        void NextTapped(object sender, EventArgs e)
        {
            try
            {
                Navigation.PushModalAsync(new VehicleInfoPageFour());
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        async void SelectImageTapped(object sender, EventArgs e)
        {
            try
            {
                IList<String> buttons = new List<String>();
                buttons.Add(ChooseImageFrom.Gallery.ToString());
                buttons.Add(ChooseImageFrom.Camera.ToString());

                var action = await DisplayActionSheet("Choose photo from", "Cancel", null, buttons.ToArray());

                if (action == ChooseImageFrom.Camera.ToString())
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await CrossMedia.Current.Initialize();
                        if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                        {
                            return;
                        }

                        MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                        {
                            SaveToAlbum = true,
                            DefaultCamera = CameraDevice.Rear,
                            Directory = "OnePosInventory",
                            Name = "Media.jpg"
                        });

                        if (file == null)
                        {
                            return;
                        }

                        imglist = new List<byte>();

                        cropedBytes = await CrossXMethod.Current.CropImageFromOriginalToBytes(file.Path);

                        if (cropedBytes != null)
                        {
                            foreach (var item in cropedBytes)
                            {
                                imglist.Add(item); ;
                            }
                        }

                        if (cropedBytes != null)
                        {
                            imgProfileImageSource = ImageSource.FromStream(() =>
                            {
                                var cropedImage = new MemoryStream(cropedBytes);
                                file.Dispose();
                                return cropedImage;
                            });
                            selectImage.Source = imgProfileImageSource;
                        }
                        else
                        {
                            file.Dispose();
                            if (imgdata == null)
                            {
                                selectImage.Source = "profileOne.png";
                            }
                            else
                            {
                                selectImage.Source = ImageSource.FromStream(() => new MemoryStream(imgdata));
                            }
                        }
                    });
                }

                else if (action == ChooseImageFrom.Gallery.ToString())
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await CrossMedia.Current.Initialize();
                        if (!CrossMedia.Current.IsPickPhotoSupported)
                        {
                            return;
                        }
                        MediaFile file = await CrossMedia.Current.PickPhotoAsync();

                        if (file == null)
                        {
                            return;
                        }
                        imglist = new List<byte>();

                        cropedBytes = await CrossXMethod.Current.CropImageFromOriginalToBytes(file.Path);

                        if (cropedBytes != null)
                        {
                            foreach (var item in cropedBytes)
                            {
                                imglist.Add(item);
                            }
                        }

                        if (cropedBytes != null)
                        {
                            imgProfileImageSource = ImageSource.FromStream(() =>
                            {
                                var cropedImage = new MemoryStream(cropedBytes);
                                file.Dispose();
                                return cropedImage;
                            });
                            selectImage.Source = imgProfileImageSource;
                        }
                        else
                        {
                            file.Dispose();
                            if (imgdata == null)
                            {
                                selectImage.Source = "profileOne.png";
                            }
                            else
                            {
                                selectImage.Source = ImageSource.FromStream(() => new MemoryStream(imgdata));
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
    }
    public enum ChooseImageFrom
    {
        Camera,
        Gallery
    }
}
