using System;
using System.Net.Http;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using DeRuta.Models;
using System.Net;
using DeRuta.Constants;

namespace DeRuta
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void RegisterButton_Clicked(object sender, EventArgs e)
        {
            User user = new User()
            {
                username = username.Text,
                password = password.Text
            };
            Uri requestUri = new Uri("http://192.168.0.251:8080/user");
            var client = new HttpClient();
            var json = JsonConvert.SerializeObject(user);
            var contentJson = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUri, contentJson);
            if (response.StatusCode == HttpStatusCode.OK) {
                await DisplayAlert("Felicitaciones","El usuario se ha registrado correctamente","Ok");
            } else {
                await DisplayAlert("Oops", "Ha ocurrido un error", "Ok");
            }
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            User user = new User()
            {
                username = username.Text,
                password = password.Text
            };
            Uri requestUri = new Uri("http://192.168.0.251:8080/login");
            var client = new HttpClient();
            var json = JsonConvert.SerializeObject(user);
            var contentJson = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUri, contentJson);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                AppConstants.loggedUser = user;
                _ = Navigation.PushAsync(new MapPage());
            }
            else
            {
                await DisplayAlert("Oops", "Credenciales incorrectas", "Ok");
            }            
        }
    }
}
