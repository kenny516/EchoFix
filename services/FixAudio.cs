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
            // 
            samples[i] = Math.Clamp(samples[i], -1.0f, 1.0f);
        }
    }
    
    /// <summary>
    /// argument: samples, count, threshold, ratio
    /// samples: tableau d'échantillons
    /// count: nombre d'échantillons à traiter
    /// threshold: seuil de distorsion max frequence
    /// ratio: ratio de distorsion
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

      /// <summary>
      /// Réduit le bruit en utilisant un fichier de référence de bruit
      /// sourceSamples: échantillons du fichier audio source
      /// noiseSamples: échantillons du fichier de référence de bruit
      /// count: nombre d'échantillons à traiter (doit être le minimum entre les deux tableaux)
      /// noiseReductionFactor: facteur de réduction du bruit (entre 0 et 1)
      /// </summary>
      public static void AntiNoiseWithReference(float[] sourceSamples, float[] noiseSamples, int count, float noiseReductionFactor = 0.8f)
      {
          if (sourceSamples == null || noiseSamples == null)
              throw new ArgumentNullException("Les tableaux d'échantillons ne peuvent pas être null");
              
          if (count <= 0 || count > Math.Min(sourceSamples.Length, noiseSamples.Length))
              throw new ArgumentOutOfRangeException("count", "Le nombre d'échantillons est invalide");
              
          if (noiseReductionFactor < 0 || noiseReductionFactor > 1)
              throw new ArgumentOutOfRangeException("noiseReductionFactor", "Le facteur de réduction doit être entre 0 et 1");
  
          // Soustraction spectrale simple
          for (int i = 0; i < count; i++)
          {
              // Soustrait le bruit pondéré du signal source
              sourceSamples[i] -= noiseSamples[i] * noiseReductionFactor;
              
              sourceSamples[i] = Math.Clamp(sourceSamples[i], -1.0f, 1.0f);
          }
          
          // Normalisation du signal
          float maxAmplitude = 0;
          for (int i = 0; i < count; i++)
          {
              maxAmplitude = Math.Max(maxAmplitude, Math.Abs(sourceSamples[i]));
          }
          
          if (maxAmplitude > 1.0f)
          {
              float normalizationFactor = 1.0f / maxAmplitude;
              for (int i = 0; i < count; i++)
              {
                  sourceSamples[i] *= normalizationFactor;
              }
          }
      }  
}