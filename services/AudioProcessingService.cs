using EchoFix.Models;

namespace EchoFix.services;

public class AudioProcessingService
{
    public async Task<byte[]> ProcessAmplification(string inputPath, float amplificationLevel)
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

    public async Task<byte[]> ProcessDistortion(string inputPath, float threshold, float ratio)
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
                File.Delete(outputPath);
        }
    }

    public async Task<byte[]> ProcessNoiseReduction(string inputPath, float cutoffFrequency, float q)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_noise.wav");
        try
        {
            AudioFile audioFile = new AudioFile(inputPath);
            (var samples, var readSamples, WaveFormat waveFormat) = audioFile.ReadSamples();
            int sampleRate = audioFile.getSampleRate(inputPath);
            FixAudio.AntiNoise(samples, readSamples, cutoffFrequency, sampleRate);
            AudioFile.WriteSamplesToWav(samples, readSamples, outputPath, waveFormat);
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
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_combined.wav");
        try
        {
            AudioFile audioFile = new AudioFile(inputPath);
            (var samples, var readSamples, WaveFormat waveFormat) = audioFile.ReadSamples();
            int sampleRate = audioFile.getSampleRate(inputPath);
            
            // Appliquer les traitements dans l'ordre
            FixAudio.AntiNoise(samples, readSamples, cutoffFrequency, sampleRate);
            FixAudio.AntiDistort(samples, readSamples, threshold, ratio);
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
                File.Delete(outputPath);
        }
    }

    public async Task<byte[]> NoiseAudio(string inputPath, float cutoffFrequency)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_noise.wav");
        try
        {
            AudioFile audioFile = new AudioFile(inputPath);
            (var samples, var readSamples, WaveFormat waveFormat) = audioFile.ReadSamples();
            int sampleRate = audioFile.getSampleRate(inputPath);
            FixAudio.AntiNoise(samples, readSamples, cutoffFrequency, sampleRate);
            AudioFile.WriteSamplesToWav(samples, readSamples, outputPath, waveFormat);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    public async Task<byte[]> NoiseReductionWithReference(string inputPath, string noisePath, float noiseReductionFactor = 0.8f)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_noise_reduced.wav");
        try
        {
            AudioFile sourceFile = new AudioFile(inputPath);
            (var sourceSamples, var sourceReadSamples, WaveFormat sourceWaveFormat) = sourceFile.ReadSamples();

            AudioFile noiseFile = new AudioFile(noisePath);
            (var noiseSamples, var noiseReadSamples, _) = noiseFile.ReadSamples();
            
            int samplesToProcess = Math.Min(sourceReadSamples, noiseReadSamples);
            
            FixAudio.AntiNoiseWithReference(sourceSamples, noiseSamples, samplesToProcess, noiseReductionFactor);
            
            AudioFile.WriteSamplesToWav(sourceSamples, samplesToProcess, outputPath, sourceWaveFormat);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }
}