using System;
using Android.Widget;
using Xamarin.Forms;

namespace MathBattlegrounds
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        void OnPlayButtonClicked(object sender, EventArgs e)
        {
            //ServerInfo.GetSessionPort();
            Navigation.PushAsync(new GameplayPage());
        }

        void OnProfileButtonClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ProfilePage());
        }

        void OnPrivateButtonClicked(object sender, EventArgs e)
        {
            PromptPrivateRoom();
        }

        async void PromptPrivateRoom()
        {
            var action = await DisplayActionSheet("Приватное лобби:", "Отмена", null, "Создать", "Присоединиться");
            switch (action)
            {
                case "Создать":
                    await DisplayAlert("Код вашего лобби", ServerInfo.CreateRoom(), "OK");
                    await Navigation.PushAsync(new GameplayPage());
                    break;
                case "Присоединиться":
                    PromptCode();
                    break;
            }
        }

        async void PromptCode()
        {
            var code = await DisplayPromptAsync("Введите код приватного лобби", "");
            ServerInfo.GetRoom(code);
            await Navigation.PushAsync(new GameplayPage());
        }

    }
}