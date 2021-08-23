using System;
using System.Collections.Generic;
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
using Xamarin.Essentials;
using System.IO;
using DeRuta.Utils;
using ImageCircle.Forms.Plugin.Abstractions;

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
        List<Pin> contactPins = new List<Pin>();
        List<Pin> placesPins = new List<Pin>();
        byte[] resizedImage;
        BitmapDescriptor myIcon;

public MapPage()
        {
            InitializeComponent();
            user.Text = AppConstants.loggedUser.username;
            avatar.Source = ImageSource.FromResource("DeRuta.Images.nopicture.png");
            LoadPicture();
            GetAndShowPlaces();
            GetContacts();
        }

        protected override void OnAppearing()
        {
            loc = DependencyService.Get<ILocation>();
            myPin = new Pin()
            {
                Type = PinType.Place,
                Label = "You are here",
                Rotation = 0f,
                Tag = "id_here",
                InfoWindowAnchor = new Point(0.5, 0),
                Icon = BitmapDescriptorFactory.FromView(new ContentView
                {
                    HeightRequest = 100,
                    WidthRequest = 100,
                    Content = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 100,
                        WidthRequest = 100,
                        Source = ImageSource.FromUri(new Uri("https://cdn.image4.io/deruta/c_fit,w_150,h_150/avatars/" + AppConstants.loggedUser.username + ".png"))
                    }
                })
                //Icon = await GetMyIcon()
            };
            loc.locationObtained += (object sender,
                ILocationEventArgs e) =>
            {
                lat = e.lat;
                lng = e.lng;
                if (myPin != null)
                {
                    map.Pins.Remove(myPin);
                }
                myPin.Position = new Position(lat, lng);
                map.Pins.Add(myPin);
                if (!moved)
                {
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lng), Distance.FromMeters(5000)));
                    moved = true;
                }
                UpdateCoords();
            };
            loc.ObtainMyLocation();
        }


        private void LugaresButton_Clicked(object sender, EventArgs e)
        {
            lugaresButton.BackgroundColor = Color.LimeGreen;
            amigosButton.BackgroundColor = Color.LightGray;
            map.Pins.Clear();
            foreach (Pin pin in placesPins)
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
            foreach (Pin pin in contactPins)
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

        private async Task<BitmapDescriptor> GetMyIcon()
        {
            if (myIcon == null)
            {
                HttpRequestMessage request = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.image4.io/v1.0/image?name=/avatars/" + AppConstants.loggedUser.username + ".png"),
                    Method = HttpMethod.Get
                };
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.Image4ioApiKey + ":" + AppConstants.Image4ioApiSecret)));
                var client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);
                WebClient wc = new WebClient();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //byte[] originalData = wc.DownloadData("https://cdn.image4.io/deruta/c_fit,w_150,h_150/avatars/" + AppConstants.loggedUser.username + ".png");
                    //MemoryStream stream = new MemoryStream(originalData);
                    myIcon = BitmapDescriptorFactory.FromView(new ContentView
                    {
                        HeightRequest = 100,
                        WidthRequest = 100,
                        Content = new Image
                        {
                            Aspect = Aspect.AspectFill,
                            HeightRequest = 100,
                            WidthRequest = 100,
                            Source = ImageSource.FromUri(new Uri("https://cdn.image4.io/deruta/c_fit,w_150,h_150/avatars/" + AppConstants.loggedUser.username + ".png"))
                        }
                    });
                }
                else
                {
                    byte[] originalData = wc.DownloadData("https://cdn.image4.io/deruta/c_fit,w_150,h_150/746615c2-4806-4651-bcdd-304051dc4c74.png");
                    MemoryStream stream = new MemoryStream(originalData);
                    myIcon = BitmapDescriptorFactory.FromStream(stream);
                }
            }
            return myIcon;
        }

        private async void GetAndShowPlaces()
        {
            placesPins.Clear();
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
                    placesPins.Add(new Pin()
                    {
                        Position = new Position(place.coordinates.latitude, place.coordinates.longitude),
                        Label = place.name,
                        Address = place.country,
                        InfoWindowAnchor = new Point(0.5, 0)
                    });
                }
                foreach (Pin pin in placesPins)
                {
                    map.Pins.Add(pin);
                }
            }
        }

        private async void GetContacts()
        {
            contactPins.Clear();
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
                this.contactsList.ItemsSource = resultado;
                foreach (DataUser contact in resultado)
                {
                    if (contact.coordinates != null)
                    {
                        Pin p = new Pin()
                        {
                            Position = new Position(contact.coordinates.latitude, contact.coordinates.longitude),
                            Label = contact.username,
                            InfoWindowAnchor = new Point(0.5, 0),
                        };
                        if (!contact.pictureUpdated)
                        {
                            request = new HttpRequestMessage
                            {
                                RequestUri = new Uri("https://api.image4.io/v1.0/image?name=/avatars/" + contact.username + ".png"),
                                Method = HttpMethod.Get
                            };
                            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.Image4ioApiKey + ":" + AppConstants.Image4ioApiSecret)));
                            response = await client.SendAsync(request);
                            WebClient wc = new WebClient();
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                byte[] originalData = wc.DownloadData("https://cdn.image4.io/deruta/c_fit,w_150,h_150/avatars/" + contact.username + ".png");
                                MemoryStream stream = new MemoryStream(originalData);

                                var documentsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "temp");
                                Directory.CreateDirectory(documentsPath);

                                byte[] bArray = new byte[stream.Length];
                                using (FileStream fs = new FileStream(Path.Combine(documentsPath, contact.username + ".png"), FileMode.OpenOrCreate))
                                {
                                    using (stream)
                                    {
                                        stream.Read(bArray, 0, (int)stream.Length);
                                    }
                                    int length = bArray.Length;
                                    fs.Write(bArray, 0, length);
                                }
                            }
                        }

                        var filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "temp", contact.username + ".png");
                        if (File.Exists(filePath))
                        {
                            p.Icon = BitmapDescriptorFactory.FromStream(File.OpenRead(filePath));
                        }
                        else
                        {
                            p.Icon = BitmapDescriptorFactory.FromBundle("DeRuta.Images.nopic150.png");
                        }

                        contactPins.Add(p);
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

            avatar.Source = Xamarin.Forms.ImageSource.FromStream(() => new MemoryStream(imageData));
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

            resizedImage = ImageConverter.ResizeImage(imageData, 400);

            avatar.Source = Xamarin.Forms.ImageSource.FromStream(() => new MemoryStream(resizedImage));

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
                RequestUri = new Uri("https://api.image4.io/v1.0/image?name=/avatars/" + AppConstants.loggedUser.username + ".png"),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(AppConstants.Image4ioApiKey + ":" + AppConstants.Image4ioApiSecret)));
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                avatar.Source = new UriImageSource { CachingEnabled = false, Uri = new Uri("https://cdn.image4.io/deruta/avatars/" + AppConstants.loggedUser.username + ".png") };
            }
        }
    }
}
