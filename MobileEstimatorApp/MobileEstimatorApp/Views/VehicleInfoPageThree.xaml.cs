using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace MobileEstimatorApp
{
    public partial class VehicleInfoPageThree : ContentPage
    {
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
    }
}
