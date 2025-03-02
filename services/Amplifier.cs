using NAudio.Wave.SampleProviders;

namespace EchoFix.services;

using NAudio.Wave;

public class Amplifier
{
    public void AmplifyToFile(string inputPath, string outputPath,float amplificationFactor = 1.0f)
    {
        using (var reader = new AudioFileReader(inputPath))
        {
            var volumeProvider = new VolumeSampleProvider(reader)
            {
                Volume = amplificationFactor
            };
            WaveFileWriter.CreateWaveFile16(outputPath, volumeProvider);
        }
    }
}