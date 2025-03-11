namespace EchoFix.Models;

public class WaveFormat
{
    public int SampleRate { get; }
    public int BitsPerSample { get; }
    public int Channels { get; }
    public int BlockAlign => (BitsPerSample * Channels) / 8;
    public int AverageBytesPerSecond => SampleRate * BlockAlign;

    public WaveFormat(int sampleRate, int bitsPerSample, int channels)
    {
        SampleRate = sampleRate;
        BitsPerSample = bitsPerSample;
        Channels = channels;
    }
}