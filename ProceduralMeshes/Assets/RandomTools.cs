
using System;

public class RandomTools
{
  public static float Gaussian(float center, float min, float max, float stdDev)
  {
    float randNormal = -100;
    do
    {
      double u1 = 1 - UnityEngine.Random.value;
      double u2 = 1 - UnityEngine.Random.value;
      double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1))
                             * Math.Sin(2.0 * Math.PI * u2);
      randNormal = (float)(center + stdDev * randStdNormal);
    } while (randNormal < min || randNormal > max);
    return randNormal;
  }
}