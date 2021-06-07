using System;
using System.Collections.Generic;
using System.Text;

namespace DeRuta.Models
{
    public class DataUser
    {
        public long id { get; set; }
        public string username { get; set; }
        public Coordinates coordinates { get; set; }
    }
}
