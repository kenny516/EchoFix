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
    /// <summary>
    /// Applique une amplification en multipliant chaque échantillon par un facteur.
    /// </summary>
    public void Amplify(float[] samples, int count, float factor)
    {
        for (int i = 0; i < count; i++)
        {
            samples[i] *= factor;
            samples[i] = Math.Clamp(samples[i], -1.0f, 1.0f); // Empêcher le clipping.
        }
    }
}