using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MathBattlegrounds
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        private Entry usernameEntry;
        private Entry passwordEntry;

        public ProfilePage()
        {
            InitializeComponent();
            usernameEntry = (Entry)FindByName("UsernameEntry");
            passwordEntry = (Entry)FindByName("PasswordEntry");
        }

        void OnLoginButtonClicked(object sender, EventArgs e)
        {
            ServerInfo.Authenticate(usernameEntry.Text, passwordEntry.Text);
            DisplayAlert("Login", ServerInfo.Token, "OK");
        }

        void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            ServerInfo.Register(usernameEntry.Text, passwordEntry.Text);
            DisplayAlert("Register", ServerInfo.Token, "OK");
        }
    }
}