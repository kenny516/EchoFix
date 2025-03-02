using NAudio.Dsp;
using NAudio.Wave;

namespace EchoFix.services;

public class NoiseReducer : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly BiQuadFilter _lowPassFilter;

    public NoiseReducer(ISampleProvider source, float cutoffFrequency, float q = 1)
    {
        this._source = source;
        WaveFormat = source.WaveFormat;
        // Créer un filtre passe-bas à la fréquence de coupure donnée
        _lowPassFilter = BiQuadFilter.LowPassFilter(WaveFormat.SampleRate, cutoffFrequency, q);
    }

    public WaveFormat WaveFormat { get; }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        for (int i = offset; i < offset + samplesRead; i++)
        {
            buffer[i] = _lowPassFilter.Transform(buffer[i]);
        }
        return samplesRead;
    }
}