namespace restaurant_API.Services
{
    public interface IFileService
    {
        Task DeleteFile(string fileName, string folderName);
        Task<string> UploadFile(string fileName, string folderName, IFormFile file);
    }
}
