using NAudio.Wave;

namespace EchoFix.services;

public class DistortionReducer : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly float _threshold;
    private readonly float _ratio;
    
    public DistortionReducer(ISampleProvider source, float threshold, float ratio)
    {
        this._source = source;
        this._threshold = threshold;
        this._ratio = ratio;
        WaveFormat = source.WaveFormat;
    }
    public WaveFormat WaveFormat { get; }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        for (int i = offset; i < offset + samplesRead; i++)
        {
            float sample = buffer[i];
            // Si le signal dépasse le seuil, on le compresse
            if (sample > _threshold)
                sample = _threshold + (sample - _threshold) / _ratio;
            else if (sample < -_threshold)
                sample = -_threshold + (sample + _threshold) / _ratio;
            buffer[i] = sample;
        }
        return samplesRead;
    }
    
}