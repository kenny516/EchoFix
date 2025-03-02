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
}