using EchoFix.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EchoFix.Controllers;

public class AudioProcessorController : Controller
{
    private readonly IAudioProcessingService _audioService;
    private readonly IAudioFileUtils _fileUtils;
    private const string AudioContentType = "audio/wav";

    public AudioProcessorController(
        IAudioProcessingService audioService,
        IAudioFileUtils fileUtils)
    {
        _audioService = audioService;
        _fileUtils = fileUtils;
    }

    public IActionResult Index() => View();
    public IActionResult Distortion() => View();
    public IActionResult Noise() => View();
    public IActionResult Combined() => View();

    [HttpPost]
    public async Task<IActionResult> ProcessAudio(IFormFile? file, float amplificationLevel)
    {
        var validationResult = await _fileUtils.ValidateAudioFile(file);
        if (validationResult != null) return validationResult;

        try
        {
            var (inputPath, _) = _fileUtils.CreateTempFilePaths("amplified");
            await using (var tempFile = new TempFile(inputPath))
            {
                await _fileUtils.SaveUploadedFile(file!, inputPath);
                var processedAudio = await _audioService.ProcessAmplification(inputPath, amplificationLevel);
                return File(processedAudio, AudioContentType, "amplified.wav");
            }
        }
        catch (Exception ex)
        {
            return _fileUtils.HandleProcessingError(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProcessDistortion(IFormFile? file, float threshold, float ratio)
    {
        var validationResult = await _fileUtils.ValidateAudioFile(file);
        if (validationResult != null) return validationResult;

        try
        {
            var (inputPath, _) = _fileUtils.CreateTempFilePaths("distortion");
            await using (var tempFile = new TempFile(inputPath))
            {
                await _fileUtils.SaveUploadedFile(file!, inputPath);
                var processedAudio = await _audioService.ProcessDistortion(inputPath, threshold, ratio);
                return File(processedAudio, AudioContentType, "processed.wav");
            }
        }
        catch (Exception ex)
        {
            return _fileUtils.HandleProcessingError(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProcessNoise(IFormFile? file, float cutoffFrequency, float q)
    {
        var validationResult = await _fileUtils.ValidateAudioFile(file);
        if (validationResult != null) return validationResult;

        try
        {
            var (inputPath, _) = _fileUtils.CreateTempFilePaths("noise");
            await using (var tempFile = new TempFile(inputPath))
            {
                await _fileUtils.SaveUploadedFile(file!, inputPath);
                var processedAudio = await _audioService.ProcessNoiseReduction(inputPath, cutoffFrequency, q);
                return File(processedAudio, AudioContentType, "noise_reduced.wav");
            }
        }
        catch (Exception ex)
        {
            return _fileUtils.HandleProcessingError(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProcessCombined(IFormFile? file, float cutoffFrequency, float q, float threshold, 
        float ratio, float amplificationLevel)
    {
        var validationResult = await _fileUtils.ValidateAudioFile(file);
        if (validationResult != null) return validationResult;

        try
        {
            var (inputPath, _) = _fileUtils.CreateTempFilePaths("combined");
            await using (var tempFile = new TempFile(inputPath))
            {
                await _fileUtils.SaveUploadedFile(file!, inputPath);
                var processedAudio = await _audioService.ProcessCombined(inputPath, cutoffFrequency, q, threshold, ratio, amplificationLevel);
                return File(processedAudio, AudioContentType, "processed_combined.wav");
            }
        }
        catch (Exception ex)
        {
            return _fileUtils.HandleProcessingError(ex);
        }
    }
}

// Classe utilitaire pour la gestion des fichiers temporaires
