using EchoFix.Models;
using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;

namespace EchoFix.services;

public class AudioProcessingService
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

    public async Task<byte[]> ProcessCombined(string inputPath, float cutoffFrequency, float q, float threshold,
        float ratio, float amplificationLevel)
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

    /// implementation 
    public async Task<byte[]> AmplifyAudio(string inputPath, float amplificationLevel)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_amplified.wav");
        try
        {
            AudioFile audioFile = new AudioFile(inputPath);
            (var samples, var readSamples, WaveFormat waveFormat) = audioFile.ReadSamples();
            FixAudio.Amplify(samples, readSamples, amplificationLevel);
            AudioFile.WriteSamplesToWav(samples, readSamples, outputPath, waveFormat);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    public async Task<byte[]> DistortionAudio(string inputPath, float threshold, float ratio)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_distortion.wav");
        try
        {
            AudioFile audioFile = new AudioFile(inputPath);
            (var samples, var readSamples, WaveFormat waveFormat) = audioFile.ReadSamples();
            FixAudio.AntiDistort(samples, readSamples, threshold, ratio);
            AudioFile.WriteSamplesToWav(samples, readSamples, outputPath, waveFormat);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }
    public async Task<byte[]> NoiseAudio(string inputPath, float cutoffFrequency)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_noise.wav");
        try
        {
            AudioFile audioFile = new AudioFile(inputPath);
            (var samples, var readSamples, WaveFormat waveFormat) = audioFile.ReadSamples();
            FixAudio.AntiNoise(samples, readSamples, cutoffFrequency, waveFormat.SampleRate);
            AudioFile.WriteSamplesToWav(samples, readSamples, outputPath, waveFormat);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    public async Task<byte[]> NoiseReductionWithReference(string inputPath, string noisePath, float noiseReductionFactor = 0.8f)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_noise_reduced.wav");
        try
        {
            // Lecture du fichier source
            AudioFile sourceFile = new AudioFile(inputPath);
            (var sourceSamples, var sourceReadSamples, WaveFormat sourceWaveFormat) = sourceFile.ReadSamples();

            // Lecture du fichier de référence de bruit
            AudioFile noiseFile = new AudioFile(noisePath);
            (var noiseSamples, var noiseReadSamples, _) = noiseFile.ReadSamples();

            // Calcul du nombre d'échantillons à traiter (minimum entre les deux fichiers)
            int samplesToProcess = Math.Min(sourceReadSamples, noiseReadSamples);

            // Application de la réduction de bruit
            FixAudio.AntiNoiseWithReference(sourceSamples, noiseSamples, samplesToProcess, noiseReductionFactor);

            // Écriture du résultat
            AudioFile.WriteSamplesToWav(sourceSamples, samplesToProcess, outputPath, sourceWaveFormat);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }
}