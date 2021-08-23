using System;
using CoreLocation;
using DeRuta.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(GetMyLocation))]
namespace DeRuta.iOS
{//---event arguments containing lat and lng---
    public class LocationEventArgs : EventArgs, ILocationEventArgs
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class GetMyLocation : ILocation
    {
        CLLocationManager lm;
        //---an EventHandler delegate that is called when a
        // location is obtained---
        public event EventHandler<ILocationEventArgs>
            locationObtained;
        //---custom event accessor when client subscribes
        // to the event---
        event EventHandler<ILocationEventArgs>
            ILocation.locationObtained
        {
            add
            {
                locationObtained += value;
            }
            remove
            {
                locationObtained -= value;
            }
        }
        //---method to call to start getting location---
        public void ObtainMyLocation()
        {
            lm = new CLLocationManager
            {
                DesiredAccuracy = CLLocation.AccuracyBest,
                DistanceFilter = CLLocationDistance.FilterNone
            };
            //---fired whenever there is a change in location---
            lm.LocationsUpdated +=
                (object sender, CLLocationsUpdatedEventArgs e) => {
                    var locations = e.Locations;
                    var strLocation =
                        locations[locations.Length - 1].
                            Coordinate.Latitude.ToString();
                    strLocation = strLocation + "," +
                        locations[locations.Length - 1].
                            Coordinate.Longitude.ToString();
                    LocationEventArgs args = new LocationEventArgs
                    {
                        lat = locations[locations.Length - 1].
                        Coordinate.Latitude,
                        lng = locations[locations.Length - 1].
                        Coordinate.Longitude
                    };
                    locationObtained(this, args);
                };
            lm.AuthorizationChanged += (object sender,
                CLAuthorizationChangedEventArgs e) => {
                    if (e.Status ==
                        CLAuthorizationStatus.AuthorizedWhenInUse)
                    {
                        lm.StartUpdatingLocation();
                    }
                };
            lm.RequestWhenInUseAuthorization();
        }
        //---stop the location update when the object is set to
        // null---
        ~GetMyLocation()
        {
            lm.StopUpdatingLocation();
        }
    }
}