using System.Text;

namespace EchoFix.Models;

public class AudioFile
{
    private const int WAV_HEADER_SIZE = 44;
    public string FilePath { get; set; }
    
    public AudioFile(string filePath)
    {
        FilePath = filePath;
    }
    
    public int getSampleRate(string filePath)
    {
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fileStream))
        {
            // Skip RIFF identifier and chunk size
            reader.ReadBytes(20);
            return reader.ReadInt32();
        }
    }
    
    public (float[], int, WaveFormat) ReadSamples()
    {
        using (var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fileStream))
        {
            // Lire l'en-tête WAV
            var riffId = new string(reader.ReadChars(4));
            if (riffId != "RIFF")
                throw new InvalidDataException("Not a valid WAV file");
                
            var fileSize = reader.ReadInt32();
            var waveId = new string(reader.ReadChars(4));
            if (waveId != "WAVE")
                throw new InvalidDataException("Not a valid WAV file");

            // Format chunk
            var fmtId = new string(reader.ReadChars(4));
            if (fmtId != "fmt ")
                throw new InvalidDataException("Invalid format chunk");
                
            var fmtSize = reader.ReadInt32();
            var audioFormat = reader.ReadInt16();
            var numChannels = reader.ReadInt16();
            var sampleRate = reader.ReadInt32();
            var byteRate = reader.ReadInt32();
            var blockAlign = reader.ReadInt16();
            var bitsPerSample = reader.ReadInt16();

            // Skip extra format bytes
            if (fmtSize > 16)
                reader.ReadBytes(fmtSize - 16);

            // Trouver le chunk "data"
            string chunkId;
            int chunkSize;
            do
            {
                chunkId = new string(reader.ReadChars(4));
                chunkSize = reader.ReadInt32();
                if (chunkId != "data")
                    reader.ReadBytes(chunkSize);
            } while (chunkId != "data" && fileStream.Position < fileStream.Length);

            if (chunkId != "data")
                throw new InvalidDataException("No data chunk found");

            // Lire les données audio
            var audioData = reader.ReadBytes(chunkSize);
            var sampleCount = audioData.Length / (bitsPerSample / 8);
            var samples = new float[sampleCount];

            // Convertir en float (support 16-bit PCM)
            for (int i = 0; i < sampleCount; i++)
            {
                var value = (short)((audioData[i * 2 + 1] << 8) | audioData[i * 2]);
                samples[i] = value / 32768f;
            }

            return (samples, sampleCount, new WaveFormat(sampleRate, bitsPerSample, numChannels));
        }
    }
    
    public static void WriteSamplesToWav(float[] samples, int count, string outputFile, WaveFormat format)
    {
        using (var fileStream = new FileStream(outputFile, FileMode.Create))
        using (var writer = new BinaryWriter(fileStream))
        {
            // RIFF Header
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write((int)(WAV_HEADER_SIZE + count * 2 - 8));
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            // Format chunk
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // Format chunk size
            writer.Write((short)1); // PCM format
            writer.Write((short)format.Channels);
            writer.Write(format.SampleRate);
            writer.Write(format.AverageBytesPerSecond);
            writer.Write((short)format.BlockAlign);
            writer.Write((short)format.BitsPerSample);

            // Data chunk
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(count * 2);

            // Écrire les échantillons
            for (int i = 0; i < count; i++)
            {
                var sample = (short)(samples[i] * 32767f);
                writer.Write(sample);
            }
        }
    }
}