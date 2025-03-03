namespace EchoFix.services;

public class FixAudio
{
    /// <summary>
    /// args: samples, count, factor
    /// samples: tableau d'échantillons
    /// count: nombre d'échantillons à traiter
    /// factor: facteur d'amplification
    /// </summary>
    public static void Amplify(float[] samples, int count, float factor)
    {
        for (int i = 0; i < count; i++)
        {
            samples[i] *= factor;
            // Empêcher le clipping.
            samples[i] = Math.Clamp(samples[i], -1.0f, 1.0f);
        }
    }
    
    /// <summary>
    /// args: samples, count, threshold, ratio
    /// samples: tableau d'échantillons
    /// count: nombre d'échantillons à traiter
    /// threshold: seuil de distorsion
    /// </summary>
    public static void AntiDistort(float[] samples, int count, float threshold, float ratio)
    {
        for (int i = 0; i < count; i++)
        {
            float sample = samples[i];
            if (sample > threshold)
                sample = threshold + (sample - threshold) / ratio;
            else if (sample < -threshold)
                sample = -threshold + (sample + threshold) / ratio;
            samples[i] = sample;
        }
    }
}