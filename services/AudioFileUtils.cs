using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EchoFix.services;

public interface IAudioFileUtils
{
    Task<IActionResult?> ValidateAudioFile(IFormFile? file);
    Task SaveUploadedFile(IFormFile file, string path);
    (string inputPath, string outputPath) CreateTempFilePaths(string suffix);
    IActionResult HandleProcessingError(Exception ex);
}

public class AudioFileUtils : IAudioFileUtils
{
    private readonly string _tempPath;
    private readonly ILogger<AudioFileUtils> _logger;
    private const int MaxFileSizeMB = 50;

    public AudioFileUtils(ILogger<AudioFileUtils> logger)
    {
        _logger = logger;
        _tempPath = Path.Combine(Path.GetTempPath(), "EchoFix");
        if (!Directory.Exists(_tempPath))
        {
            Directory.CreateDirectory(_tempPath);
        }
    }

    public async Task<IActionResult?> ValidateAudioFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Tentative d'upload d'un fichier vide ou nul");
            return new BadRequestObjectResult("No file uploaded");
        }

        if (!file.FileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning($"Type de fichier non supporté: {file.FileName}");
            return new BadRequestObjectResult("Only WAV files are supported");
        }

        if (file.Length > MaxFileSizeMB * 1024 * 1024)
        {
            _logger.LogWarning($"Fichier trop volumineux: {file.Length / (1024 * 1024)}MB > {MaxFileSizeMB}MB");
            return new BadRequestObjectResult($"File size must be less than {MaxFileSizeMB}MB");
        }

        return null;
    }

    public async Task SaveUploadedFile(IFormFile file, string path)
    {
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
    }

    public (string inputPath, string outputPath) CreateTempFilePaths(string suffix)
    {
        var inputPath = Path.Combine(_tempPath, $"{Guid.NewGuid()}.wav");
        var outputPath = Path.Combine(_tempPath, $"{Guid.NewGuid()}_{suffix}.wav");
        return (inputPath, outputPath);
    }

    public IActionResult HandleProcessingError(Exception ex)
    {
        _logger.LogError(ex, "Une erreur s'est produite lors du traitement audio");
        
        var errorMessage = ex switch
        {
            IOException => "Error accessing the audio file",
            OutOfMemoryException => "File is too large to process",
            InvalidOperationException => "Invalid audio file format",
            _ => "An unexpected error occurred while processing the audio"
        };

        return new StatusCodeResult(500);
    }
}

public class TempFile : IAsyncDisposable
{
    private readonly string[] _paths;

    public TempFile(params string[] paths)
    {
        _paths = paths;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var path in _paths.Where(File.Exists))
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                // Ignorer les erreurs de suppression
            }
        }
        await ValueTask.CompletedTask;
    }
}