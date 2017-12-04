using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace MobileEstimatorApp
{
    public partial class VehicleInfoPageTwo : ContentPage
    {
        public VehicleInfoPageTwo()
        {
            InitializeComponent();
            imgSettings.HeightRequest = 70;
            imgSettings.WidthRequest = 70;
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
                Navigation.PushModalAsync(new VehicleInfoPageThree());

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
    }
}
