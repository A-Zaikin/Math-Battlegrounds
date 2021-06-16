using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MathBattlegrounds
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        private readonly Entry usernameEntry;
        private readonly Entry passwordEntry;

        public ProfilePage()
        {
            InitializeComponent();
            usernameEntry = (Entry)FindByName("UsernameEntry");
            passwordEntry = (Entry)FindByName("PasswordEntry");
        }

        void OnLoginButtonClicked(object sender, EventArgs e)
        {
            ServerInfo.Authenticate(usernameEntry.Text, passwordEntry.Text);
            Toast.MakeText(Android.App.Application.Context, "Login successful", ToastLength.Short)?.Show();
        }

        void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            ServerInfo.Register(usernameEntry.Text, passwordEntry.Text);
            Toast.MakeText(Android.App.Application.Context, "Registration successful", ToastLength.Short)?.Show();
        }
    }
}