namespace EduVoice.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string fileUrl);
    Task<string> GetPresignedUrlAsync(string fileName, int expiryMinutes = 60);
}
