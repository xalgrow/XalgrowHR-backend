using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

public class FileStorageService
{
    private readonly string _uploadFolder;
    public int MaxFileSizeMB { get; private set; }

    public FileStorageService(IConfiguration configuration)
    {
        // Ensure upload folder and max file size are read from configuration
        _uploadFolder = configuration["FileStorage:UploadFolder"] 
                        ?? throw new ArgumentNullException(nameof(_uploadFolder)); // Throw if the folder is not set
        MaxFileSizeMB = int.Parse(configuration["FileStorage:MaxFileSizeMB"] ?? "10"); // Default to 10MB if not provided
    }

    // Method to save the file
    public async Task<string> SaveFileAsync(IFormFile file)
    {
        // Check if the file exceeds the maximum allowed size
        if (file.Length > MaxFileSizeMB * 1024 * 1024)
        {
            throw new Exception("File exceeds the maximum allowed size.");
        }

        // Generate file path
        var filePath = Path.Combine(_uploadFolder, file.FileName);

        // Ensure the upload folder exists
        if (!Directory.Exists(_uploadFolder))
        {
            Directory.CreateDirectory(_uploadFolder);
        }

        // Save the file asynchronously
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath; // Return the file path of the saved file
    }
}
