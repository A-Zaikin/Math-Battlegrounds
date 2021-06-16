using System;
using System.Threading;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MathBattlegrounds
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GameplayPage : ContentPage
    {
        private readonly Label questionLabel;
        private readonly Entry answerEntry;
        private readonly Label timeLabel;
        private DateTime startTime;
        private bool timerEnabled;
        private readonly StackLayout playerStack;
        private readonly Thread gameLogicThread;
        private bool gameIsOn = true;
        private RoundData roundData;
        private int playerId;

        public GameplayPage()
        {
            InitializeComponent();
            questionLabel = (Label)FindByName("QuestionLabel");
            timeLabel = (Label)FindByName("TimeLabel");
            answerEntry = (Entry)FindByName("AnswerEntry");
            playerStack = (StackLayout)FindByName("PlayerStack");
            ServerInfo.StartStreamThread();
            Toast.MakeText(Android.App.Application.Context, "Подождите других игроков", ToastLength.Long)?.Show();
            gameLogicThread = new Thread(() =>
            {
                ServerInfo.SendNewPlayerInfo("Alexey");
                while (ServerInfo.CurrentPlayerId == null)
                {
                    Thread.Sleep(100);
                }
                playerId = (int)ServerInfo.CurrentPlayerId;
                ServerInfo.CurrentPlayerId = 0;
                while (gameIsOn)
                {
                    while (ServerInfo.CurrentRoundData == null)
                    {
                        Thread.Sleep(100);
                    }
                    roundData = ServerInfo.CurrentRoundData;
                    ServerInfo.CurrentRoundData = null;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        questionLabel.Text = roundData.Problem.Text;
                        UpdatePlayerStack();
                    });
                    startTime = DateTime.Now;
                    timerEnabled = true;
                    Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                    {
                        if (timerEnabled)
                        {
                            var elapsedTime = DateTime.Now - startTime;
                            if (elapsedTime > roundData.Problem.Time)
                            {
                                ServerInfo.SendAnswerData(new AnswerData("no answer", DateTime.Now - startTime));
                                return false;
                            }
                            var timeLeft = (roundData.Problem.Time - elapsedTime);
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                timeLabel.Text = $"{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
                            });
                            return true;
                        }
                        return false;
                    });
                    while (ServerInfo.CurrentRoundResult == null)
                    {
                        Thread.Sleep(100);
                    }
                    var roundResult = ServerInfo.CurrentRoundResult;
                    ServerInfo.CurrentRoundResult = null;
                    if (roundResult.PlayerPlace != 0)
                    {
                        var gameEndMessage = roundResult.PlayerPlace == 1 
                            ? "Вы выиграли!" 
                            : $"Вы заняли место №{roundResult.PlayerPlace}";
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            DisplayAlert("Игра окончена", gameEndMessage, "OK");
                            //Toast.MakeText(Android.App.Application.Context, gameEndMessage, ToastLength.Long)?.Show();
                            Navigation.PopToRootAsync();
                        });
                        gameLogicThread.Abort();
                        ServerInfo.StreamEnabled = false;
                        return;
                    }
                    var message = "";
                    switch (roundResult.PlayerResult)
                    {
                        case PlayerRoundResult.Win:
                            message = "Вы выиграли раунд!";
                            break;
                        case PlayerRoundResult.Loss:
                            message = "Вы проиграли раунд";
                            break;
                        case PlayerRoundResult.Tie:
                            message = "Раунд закончился ничьёй";
                            break;
                    }
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long)?.Show();
                        UpdatePlayerStack();
                        answerEntry.Text = "";
                    });
                }
            });
            gameLogicThread.Start();
        }

        void OnSubmitButtonClicked(object sender, EventArgs e)
        {
            Toast.MakeText(Android.App.Application.Context, "Подождите ответа вашего оппонента", ToastLength.Short)?.Show();
            ServerInfo.SendAnswerData(new AnswerData(answerEntry.Text, DateTime.Now - startTime));
            timerEnabled = false;
        }

        void UpdatePlayerStack()
        {
            playerStack.Children.Clear();
            foreach (var playerData in roundData.Pairs.Values)
            {
                var color = Color.Gray;
                if (playerData.Id == playerId)
                    color = Color.Green;
                if (playerData.Id == roundData.Pairs[playerId].OpponentId)
                    color = Color.Red;
                var label = new Label
                {
                    Text = $"{playerData.Name}  --  {playerData.Health} ХП",
                    FontSize = 20,
                    TextColor = color
                };
                playerStack.Children.Add(label);
            }
        }
    }
}