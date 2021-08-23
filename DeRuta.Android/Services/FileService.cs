using DeRuta.Droid.Services;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileService))]
namespace DeRuta.Droid.Services
{
    class FileService : IFileService
    {
        public void SavePicture(string name, Stream data, string location = "temp")
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            documentsPath = Path.Combine(documentsPath, location);
            Directory.CreateDirectory(documentsPath);

            string filePath = Path.Combine(documentsPath, name);

            byte[] bArray = new byte[data.Length];
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (data)
                {
                    data.Read(bArray, 0, (int)data.Length);
                }
                int length = bArray.Length;
                fs.Write(bArray, 0, length);
            }
        }
        /*
        public Stream GetPicture(string name, string location = "temp")
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(documentsPath, location, name);
            if (File.Exists(filePath))
            {
                return File.OpenRead(filePath);
            }
        }*/
    }
}