using NAudio.Wave;

namespace EchoFix.Models;

public class AudioFile
{
    public string FilePath { get; set; }
    
    public AudioFile(string filePath)
    {
        FilePath = filePath;
    }
    
    public int getSampleRate(string filePath)
    {
        using (var render = new WaveFileReader(filePath))
        {
            int sampleRate = render.WaveFormat.SampleRate;
            return sampleRate;
        }
    }
    
    /// <summary>
    /// Convertit les échantillons float en bytes pour les écrire en 16-bit PCM.
    /// </summary>
    public static void  WriteSamplesToWav(float[] samples, int count, string outputFile, WaveFormat inputFormat)
    {
        // On souhaite écrire en 16-bit PCM, on crée donc un format adapté.
        WaveFormat outFormat = WaveFormat.CreateIeeeFloatWaveFormat(inputFormat.SampleRate, inputFormat.Channels);
        using (var writer = new WaveFileWriter(outputFile, new WaveFormat(inputFormat.SampleRate, 16, inputFormat.Channels)))
        {
            byte[] buffer = new byte[count * 2]; // 2 bytes par échantillon 16-bit.
            for (int i = 0; i < count; i++)
            {
                // Conversion d'un float (entre -1 et 1) en short (16-bit PCM)
                short s = (short)(samples[i] * short.MaxValue);
                buffer[i * 2] = (byte)(s & 0xFF);
                buffer[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
            }
            writer.Write(buffer, 0, buffer.Length);
        }
    }
}