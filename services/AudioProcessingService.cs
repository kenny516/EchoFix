using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;

namespace EchoFix.services;

public interface IAudioProcessingService
{
    Task<byte[]> ProcessAmplification(string inputPath, float amplificationLevel);
    Task<byte[]> ProcessDistortion(string inputPath, float threshold, float ratio);
    Task<byte[]> ProcessNoiseReduction(string inputPath, float cutoffFrequency, float q);
    Task<byte[]> ProcessCombined(string inputPath, float cutoffFrequency, float q, float threshold, float ratio, float amplificationLevel);
}

public class AudioProcessingService : IAudioProcessingService
{
    public async Task<byte[]> ProcessAmplification(string inputPath, float amplificationLevel)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_amplified.wav");
        try
        {
            var amplifier = new Amplifier();
            amplifier.AmplifyToFile(inputPath, outputPath, amplificationLevel);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    public async Task<byte[]> ProcessDistortion(string inputPath, float threshold, float ratio)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_distortion.wav");
        try
        {
            using var reader = new AudioFileReader(inputPath);
            var distortionReducer = new DistortionReducer(reader, threshold, ratio);
            WaveFileWriter.CreateWaveFile16(outputPath, distortionReducer);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    public async Task<byte[]> ProcessNoiseReduction(string inputPath, float cutoffFrequency, float q)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_noise.wav");
        try
        {
            using var reader = new AudioFileReader(inputPath);
            var noiseReducer = new NoiseReducer(reader, cutoffFrequency, q);
            WaveFileWriter.CreateWaveFile16(outputPath, noiseReducer);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    public async Task<byte[]> ProcessCombined(string inputPath, float cutoffFrequency, float q, float threshold, float ratio, float amplificationLevel)
    {
        var noisePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_noise.wav");
        var distortionPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_distortion.wav");
        var finalPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_final.wav");

        try
        {
            // 1. Noise reduction
            using (var reader = new AudioFileReader(inputPath))
            {
                var noiseReducer = new NoiseReducer(reader, cutoffFrequency, q);
                WaveFileWriter.CreateWaveFile16(noisePath, noiseReducer);
            }

            // 2. Distortion reduction
            using (var reader = new AudioFileReader(noisePath))
            {
                var distortionReducer = new DistortionReducer(reader, threshold, ratio);
                WaveFileWriter.CreateWaveFile16(distortionPath, distortionReducer);
            }

            // 3. Amplification
            var amplifier = new Amplifier();
            amplifier.AmplifyToFile(distortionPath, finalPath, amplificationLevel);

            return await File.ReadAllBytesAsync(finalPath);
        }
        finally
        {
            // Clean up temporary files
            foreach (var path in new[] { noisePath, distortionPath, finalPath })
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}