using restaurant_API.Utility;

namespace restaurant_API.Services
{
    public class FileService : IFileService
    {
        public async Task DeleteFile(string fileName, string folderName)
        {
            var fullPath = Path.Combine(folderName, fileName);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
            }
        }
        
        public async Task<string> UploadFile(string fileName, string folderName, IFormFile file)
        {
            var fullPath = Path.Combine(folderName, fileName);

            Directory.CreateDirectory(folderName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }
    }
}
