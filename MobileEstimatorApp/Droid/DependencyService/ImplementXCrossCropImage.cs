using System;

using Xamarin.Forms;

namespace MobileEstimatorApp.Droid
{
    public class ImplementXCrossCropImage : ContentPage
    {
        public ImplementXCrossCropImage()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

