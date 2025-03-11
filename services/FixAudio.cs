namespace EchoFix.services;

public class FixAudio
{
    /// <summary>
    /// argument: samples, count, factor
    /// samples: tableau d'échantillons
    /// count: nombre d'échantillons à traiter
    /// factor: facteur d'amplification
    /// </summary>
    public static void Amplify(float[] samples, int count, float factor)
    {
        for (int i = 0; i < count; i++)
        {
            samples[i] *= factor;
            samples[i] = Math.Clamp(samples[i], -1.0f, 1.0f);
            
        }
    }
    
    /// <summary>
    /// argument: samples, count, threshold, ratio
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
    
    /// <summary>
    /// argument: samples, count, cutoffFrequency, sampleRate
    /// samples: tableau d'échantillons
    /// count: nombre d'échantillons à traiter
    /// cutoffFrequency: fréquence de coupure du filtre passe-bas
    /// sampleRate: fréquence d'échantillonnage
    /// </summary>
    public static void AntiNoise(float[] samples, int count, float cutoffFrequency, float sampleRate)
    {
        // Calcul de l'alpha pour le filtre RC
        float dt = 1.0f / sampleRate;
        float rc = 1.0f / (2 * (float)Math.PI * cutoffFrequency);
        float alpha = dt / (rc + dt);
        

        // Filtre passe-bas : y[n] = alpha * x[n] + (1 - alpha) * y[n-1]
        float previous = samples[0];
        for (int i = 1; i < count; i++)
        {
            samples[i] = alpha * samples[i] + (1 - alpha) * previous;
            previous = samples[i];
        }
    }
}