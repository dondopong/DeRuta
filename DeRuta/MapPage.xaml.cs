using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using DeRuta.Models;
using DeRuta.Constants;
using Xamarin.Forms.GoogleMaps.Clustering;
using Xamarin.Essentials;
using System.IO;
using DeRuta.Utils;
using System.Collections.ObjectModel;
using Foundation;

namespace DeRuta
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : TabbedPage
    {
        ILocation loc;
        double lat;
        double lng;
        bool moved = false;
        Pin myPin;
        List<Pin> amigos = new List<Pin>();
        List<Pin> lugares = new List<Pin>();
        byte[] resizedImage;

        public MapPage()
        {
            InitializeComponent();
            LoadPicture();
        }

        protected override void OnAppearing()
        {
            user.Text = AppConstants.loggedUser.username;
            loc = DependencyService.Get<ILocation>();
            loc.locationObtained += (object sender,
                ILocationEventArgs e) =>
            {
                lat = e.lat;
                lng = e.lng;
                if (myPin != null)
                {
                    map.Pins.Remove(myPin);
                }
                map.Pins.Add(myPin = new Pin()
                {
                    Type = PinType.Place,
                    Label = "You are here",
                    Rotation = 0f,
                    Position = new Position(lat, lng),
                    Tag = "id_here",
                    InfoWindowAnchor = new Point(0.5, 0)
                });
                if (!moved)
                {
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lng), Distance.FromMeters(5000)));
                    moved = true;
                }
                map.Cluster();
                UpdateCoords();
            };          
            loc.ObtainMyLocation();
            GetAndShowPlaces();
            GetContacts();
            map.Cluster();
        }

        private void LugaresButton_Clicked(object sender, EventArgs e)
        {
            lugaresButton.BackgroundColor = Color.LimeGreen;
            amigosButton.BackgroundColor = Color.LightGray;
            map.Pins.Clear();
            foreach (Pin pin in lugares)
            {
                map.Pins.Add(pin);
            }
            if (myPin != null)
            {
                map.Pins.Add(myPin);
            }
        }

        private void AmigosButton_Clicked(object sender, EventArgs e)
        {
            amigosButton.BackgroundColor = Color.LimeGreen;
            lugaresButton.BackgroundColor = Color.LightGray;
            map.Pins.Clear();
            foreach (Pin pin in amigos)
            {
                map.Pins.Add(pin);
            }
            if (myPin != null)
            {
                map.Pins.Add(myPin);
            }
        }
        
        private async void AgregarContactoButton_Clicked(object sender, EventArgs e)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://192.168.0.251:8080/user/addContact/" + contacto.Text),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.loggedUser.username + ":" + AppConstants.loggedUser.password)));
            var client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                contacto.Text = "";
            }
            GetContacts();
        }
        
        private async void GetPlaces()
        {
            lugares.Clear();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://192.168.0.251:8080/place"),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.loggedUser.username + ":" + AppConstants.loggedUser.password)));
            var client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<List<Place>>(content);
                foreach (Place place in resultado)
                {
                    lugares.Add(new Pin()
                    {
                        Position = new Position(place.coordinates.latitude, place.coordinates.longitude),
                        Label = place.name,
                        Address = place.country,
                        InfoWindowAnchor = new Point(0.5, 0)
                    });
                }
            }
        }

        private async void GetAndShowPlaces()
        {
            lugares.Clear();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://192.168.0.251:8080/place"),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.loggedUser.username + ":" + AppConstants.loggedUser.password)));
            var client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<List<Place>>(content);
                foreach (Place place in resultado)
                {
                    lugares.Add(new Pin()
                    {
                        Position = new Position(place.coordinates.latitude, place.coordinates.longitude),
                        Label = place.name,
                        Address = place.country,
                        InfoWindowAnchor = new Point(0.5, 0)
                    });
                }
                foreach (Pin pin in lugares)
                {
                    map.Pins.Add(pin);
                }
            }
        }

        private async void GetContacts()
        {
            amigos.Clear();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://192.168.0.251:8080/user/contacts"),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.loggedUser.username + ":" + AppConstants.loggedUser.password)));
            var client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<List<DataUser>>(content);
                this.contacts.ItemsSource = resultado;
                foreach (DataUser contact in resultado)
                {
                    if (contact.coordinates != null)
                    {
                        amigos.Add(new Pin()
                        {
                            Position = new Position(contact.coordinates.latitude, contact.coordinates.longitude),
                            Label = contact.username,
                            InfoWindowAnchor = new Point(0.5, 0)
                            //Icon = BitmapDescriptorFactory.FromBundle("image01.png")
                        });
                    }

                }
            }
        }
        
        private async void UpdateCoords()
        {
            Coordinates coordinates = new Coordinates()
            {
                latitude = lat,
                longitude = lng
            };
            var json = JsonConvert.SerializeObject(coordinates);
            var contentJson = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://192.168.0.251:8080/user/lastLocalization"),
                Method = HttpMethod.Post,
                Content = contentJson
            };
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.loggedUser.username + ":" + AppConstants.loggedUser.password)));
            var client = new HttpClient();
            await client.SendAsync(request);
        }
        
        private async void CambiarImagenButton_Clicked(object sender, EventArgs e)
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Seleccionar una imagen"
            });

            var stream = await result.OpenReadAsync();

            byte[] imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                imageData = ms.ToArray();
            }

            resizedImage = ImageResizer.ResizeImage(imageData, 400);

            avatar.Source = ImageSource.FromStream(() => new MemoryStream(resizedImage));
        }

        private async void TomarFotoButton_Clicked(object sender, EventArgs e)
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Seleccionar una imagen"
            });

            var stream = await result.OpenReadAsync();

            byte[] imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                imageData = ms.ToArray();
            }

            resizedImage = ImageResizer.ResizeImage(imageData, 400);

            avatar.Source = ImageSource.FromStream(() => new MemoryStream(resizedImage));




            //avatar.Source = ImageSource.FromStream(() => stream);
        }

        private async void GuardarFotoButton_Clicked(object sender, EventArgs e)
        {
            if (resizedImage != null)
            {
                
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.image4.io/v1.0/uploadImage"),
                    Method = HttpMethod.Post,
                    
                };
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.Image4ioApiKey + ":" + AppConstants.Image4ioApiSecret)));
                var content = new MultipartFormDataContent();
                content.Add(new StringContent("true"), "useFilename");
                content.Add(new StringContent("true"), "overwrite");
                content.Add(new StringContent("/avatars"), "path");
                content.Add(new StreamContent(new MemoryStream(resizedImage)), "image", $"{AppConstants.loggedUser.username}.png");
                request.Content = content;
                


                var client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    await DisplayAlert("Felicitaciones", "La imagen se ha guardado exitosamente", "Ok");
                    request = new HttpRequestMessage
                    {
                        RequestUri = new Uri("https://api.image4.io/v1.0/purge"),
                        Method = HttpMethod.Delete,
                    };
                    request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.Image4ioApiKey + ":" + AppConstants.Image4ioApiSecret)));
                    response = await client.SendAsync(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var q = 0;
                    }
                }
            }
        }

        private async void LoadPicture()
        {
            var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://cdn.image4.io/deruta/avatars/" + AppConstants.loggedUser.username + ".png"),
                Method = HttpMethod.Get
            };
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                avatar.Source = new UriImageSource { CachingEnabled = false, Uri = new Uri("https://cdn.image4.io/deruta/avatars/" + AppConstants.loggedUser.username + ".png") };
            }
        }
    }
}
