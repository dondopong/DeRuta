using System.IO;

namespace DeRuta
{
    public interface IFileService
    {
        void SavePicture(string name, Stream data, string location = "temp");
    }
}
