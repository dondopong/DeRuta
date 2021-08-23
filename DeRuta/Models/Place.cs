namespace DeRuta.Models
{
    class Place
    {
        public long id { get; set; }
        public string name { get; set; }
        public Coordinates coordinates { get; set; }
        public string country { get; set; }
    }

}
