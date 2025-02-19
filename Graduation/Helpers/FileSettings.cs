using System.Diagnostics.Contracts;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace Graduation.Helpers
{
    public class FileSettings
    {
        public static string UploadFile(IFormFile file,string folderName)
        {
            string folderPath=Path.Combine(Directory.GetCurrentDirectory(), "Files", folderName);
            string fileName =$"{Guid.NewGuid()}{file.FileName}";
            string filePath=Path.Combine(folderPath, fileName);
            FileStream fileStream =new FileStream(filePath,FileMode.Create);
            file.CopyTo(fileStream);
            return fileName;

        }


        public static string deleteFile(string fileName, string folderName)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", folderName,fileName);
            if(File.Exists(folderPath))
            {
                File.Delete(folderPath);
                return "done";
            }
            return null;

        }

       
        

    }
}
