namespace DeRuta.Models
{
    public class DataUser
    {
        public long id { get; set; }
        public string username { get; set; }
        public Coordinates coordinates { get; set; }
        public bool pictureUpdated { get; set; }
    }
}
