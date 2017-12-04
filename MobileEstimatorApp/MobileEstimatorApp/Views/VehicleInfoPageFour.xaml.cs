using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace MobileEstimatorApp
{
    public partial class VehicleInfoPageFour : ContentPage
    {
        public VehicleInfoPageFour()
        {
            InitializeComponent();
            imgDescription.HeightRequest = 70;
            imgDescription.WidthRequest = 70;
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
                //Navigation.PushModalAsync(new VehicleInfoPageTwo());
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
    }
}
