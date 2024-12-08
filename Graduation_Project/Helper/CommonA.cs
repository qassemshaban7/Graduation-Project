using static System.Net.Mime.MediaTypeNames;
using System.Net.NetworkInformation;

namespace Graduation_Project.Helper
{
    public static class CommonA
    {
        public static string GetCurrentDirectory()
        {
            var result = Directory.GetCurrentDirectory();
            return result;
        }
        public static string GetStaticContentDirectory()
        {
            var result = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images");
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }
        public static string GetFilePath(string FileName)
        {
            var _GetStaticContentDirectory = GetStaticContentDirectory();
            var result = Path.Combine(_GetStaticContentDirectory, FileName);
            return result;
        }
    }
}